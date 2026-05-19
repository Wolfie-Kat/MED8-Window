import os
os.environ['OPENCV_VIDEOIO_MSMF_ENABLE_HW_TRANSFORMS'] = '0'

import cv2 as cv
from socket import *
import struct
import sys
import traceback
import time
from enum import Enum

from gestures.gesture_recognizer import GestureRecognizer
from face_landmarks import FaceLandmarker
from utilities import calculate_camera_fov
from video_segment_recorder import VideoSegmentRecorder

class TrackingMode(Enum):
    FACE_TRACKING = 0
    OPEN_TRACK = 1

def render_video(cv, frame, face_center, fov=None, segment_info=None, tracking_mode=None):
    if face_center is not None:
        h, w = frame.shape[:2]
        cx = int(face_center[0] * w)
        cy = int(face_center[1] * h)
        cv.circle(frame, (cx, cy), 8, color=(0, 255, 0), thickness=-1)
    
    if fov is not None:
        cv.putText(frame, f"FOV: {fov:.1f} deg", (10, 80),
                   cv.FONT_HERSHEY_SIMPLEX, 0.8, (0, 255, 255), 2)
    
    # Display tracking mode
    if tracking_mode is not None:
        mode_text = f"Mode: {tracking_mode.value}"
        cv.putText(frame, mode_text, (10, 40),
                   cv.FONT_HERSHEY_SIMPLEX, 0.8, (0, 255, 0), 2)
    
    # Display segment info if available
    if segment_info is not None:
        cv.putText(frame, f"Segment: {segment_info['frame_count']} frames ({segment_info['duration']:.1f}s)", 
                   (10, 120), cv.FONT_HERSHEY_SIMPLEX, 0.6, (0, 255, 255), 2)
        cv.putText(frame, "Press 't' to toggle tracking mode | 's' to save | 'r' to reset | 'q' to quit", 
                   (10, frame.shape[0] - 10), cv.FONT_HERSHEY_SIMPLEX, 0.5, (0, 255, 0), 1)
    
    return frame

def gesture_to_code(gesture):
    if gesture == "none" or gesture == "hold":
        return 0.0
    elif gesture == "fist_left":
        return 1.0
    elif gesture == "fist_middle":
        return 2.0
    elif gesture == "fist_right":
        return 3.0
    elif gesture == "palm":
        return 4.0
    else:
        return -1.0

class TrackingSystem:
    def __init__(self):
        self.mode = TrackingMode.FACE_TRACKING
        self.cap = None
        self.landmarker = None
        self.fov = None
        self.aspect_ratio = None
        self.frame_width = None
        self.frame_height = None
        self.fps = None
        
        # Socket for sending data
        self.clientSocket = socket(AF_INET, SOCK_DGRAM)
        self.addressOut = ("127.0.0.1", 8888)
        
        # Socket for receiving OpenTrack data
        self.opentrack_socket = socket(AF_INET, SOCK_DGRAM)
        self.opentrack_addressIn = ("127.0.0.1", 4242)
        
    def init_camera(self):
        """Initialize or reinitialize the camera"""
        if self.cap is not None:
            self.cap.release()
        
        self.cap = cv.VideoCapture(0)
        if not self.cap.isOpened():
            print("Cannot open camera")
            return False
        
        # Get camera properties
        self.frame_width = int(self.cap.get(3))
        self.frame_height = int(self.cap.get(4))
        self.fps = self.cap.get(cv.CAP_PROP_FPS)
        if self.fps <= 0:
            self.fps = 30.0
        
        # Calculate FOV and aspect ratio
        self.fov = calculate_camera_fov(self.frame_width)
        self.aspect_ratio = self.frame_width / self.frame_height
        
        # Initialize face landmarker
        self.landmarker = FaceLandmarker()
        
        print(f"Camera initialized: {self.frame_width}x{self.frame_height} @ {self.fps}fps")
        print(f"FOV: {self.fov:.1f}°, Aspect ratio: {self.aspect_ratio:.2f}")
        
        return True
    
    def release_camera(self):
        """Release camera resources"""
        if self.cap is not None:
            self.cap.release()
            self.cap = None
            print("Camera released")
    
    def get_face_tracking_data(self):
        """Get tracking data from face detection"""
        if self.cap is None or not self.cap.isOpened():
            print("Camera not available, reinitializing...")
            if not self.init_camera():
                return None, None, None
        
        ret, frame = self.cap.read()
        if not ret:
            print("Failed to read frame")
            return None, None, None
        
        face_center, distance = self.landmarker.detect_faces(frame)
        
        # Return frame for optional display
        return face_center, distance, frame
    
    def get_opentrack_data(self):
        """Get tracking data from OpenTrack"""
        try:
            # Set socket to non-blocking with timeout
            self.opentrack_socket.settimeout(0.1)
            data, addr = self.opentrack_socket.recvfrom(48)
            
            # Parse OpenTrack data (assuming 6 doubles: x, y, z, yaw, pitch, roll)
            opentrack_data = struct.unpack('dddddd', data)
            
            # Return position data (x, y, z) - adjust based on actual OpenTrack output
            return {
                'x': opentrack_data[0],
                'y': opentrack_data[1],
                'z': opentrack_data[2]
            }
        except timeout:
            return None
        except Exception as e:
            print(f"Error receiving OpenTrack data: {e}")
            return None
    
    def send_tracking_data(self, x, y, z):
        """Send tracking data to target application"""
        if x is not None and y is not None and z is not None:
            message = struct.pack('ffff', x, y, self.aspect_ratio, z)
            self.clientSocket.sendto(message, self.addressOut)
            return True
        return False
    
    def switch_mode(self):
        """Switch between tracking modes"""
        if self.mode == TrackingMode.FACE_TRACKING:
            print("Switching to OpenTrack mode...")
            self.release_camera()
            self.mode = TrackingMode.OPEN_TRACK
            # Bind OpenTrack socket
            try:
                self.opentrack_socket.bind(self.opentrack_addressIn)
                print(f"OpenTrack socket bound to {self.opentrack_addressIn}")
            except Exception as e:
                print(f"Failed to bind OpenTrack socket: {e}")
        else:
            print("Switching to Face Tracking mode...")
            self.mode = TrackingMode.FACE_TRACKING
            # Initialize camera
            if not self.init_camera():
                print("Failed to initialize camera, staying in OpenTrack mode")
                self.mode = TrackingMode.OPEN_TRACK
                return False
        
        print(f"Switched to {self.mode.value} mode")
        return True

def main():
    # Setup error logging
    error_log_path = os.path.join(os.path.dirname(sys.executable) if getattr(sys, 'frozen', False) else ".", "error_log.txt")
    
    try:
        # Optional: Redirect stderr to file for debugging
        # sys.stderr = open(error_log_path, 'w')
        
        print(f"Starting application...")
        print(f"Python version: {sys.version}")
        print(f"Working directory: {os.getcwd()}")
        
        # Initialize tracking system
        tracker = TrackingSystem()
        
        # Start with face tracking
        if not tracker.init_camera():
            print("Failed to initialize camera, switching to OpenTrack mode")
            tracker.mode = TrackingMode.OPEN_TRACK
        
        # Initialize video segment recorder (optional)
        if tracker.frame_width and tracker.frame_height:
            video_recorder = VideoSegmentRecorder(
                output_dir="video_segments", 
                fps=tracker.fps, 
                frame_size=(tracker.frame_width, tracker.frame_height)
            )
            currently_recording = False
        else:
            video_recorder = None
            currently_recording = False
        
        # Create hidden window for keyboard input
        cv.namedWindow('Control', cv.WINDOW_GUI_NORMAL)
        cv.resizeWindow('Control', 1, 1)
        cv.moveWindow('Control', -100, -100)
        
        print("\n=== Controls ===")
        print("Press 't' - Toggle between Face Tracking and OpenTrack")
        print("Press 's' - Start/Stop video recording")
        print("Press 'r' - Reset current recording segment")
        print("Press 'q' - Quit application")
        print("================\n")
        
        frame_count = 0
        last_status_print = time.time()
        
        while True:
            # Handle keyboard input
            key = cv.waitKey(1) & 0xFF
            
            if key == ord('t'):
                tracker.switch_mode()
                
            elif key == ord('s') and video_recorder:
                currently_recording = not currently_recording
                status = "started" if currently_recording else "stopped"
                print(f"Video recording {status}")
                
            elif key == ord('r') and video_recorder:
                if len(video_recorder.current_segment_frames) > 0:
                    video_recorder.reset_segment()
                    print("Recording segment reset")
                else:
                    print("Nothing to reset")
                    
            elif key == ord('q'):
                print("\nQuitting...")
                break
            
            # Get tracking data based on current mode
            tracking_data = None
            display_frame = None
            
            if tracker.mode == TrackingMode.FACE_TRACKING:
                face_center, distance, frame = tracker.get_face_tracking_data()
                
                if face_center is not None:
                    tracking_data = {
                        'x': face_center[0],
                        'y': face_center[1],
                        'z': distance
                    }
                    display_frame = frame
                    
                    # Send data
                    tracker.send_tracking_data(face_center[0], face_center[1], distance)
                    
                    # Print status periodically
                    print(f"Face Tracking - Position: ({face_center[0]:.2f}, {face_center[1]:.2f}), Distance: {distance:.1f}cm")
                else:
                    print("Face Tracking - No face detected")
                    
            else:  # OpenTrack mode
                opentrack_data = tracker.get_opentrack_data()
                
                if opentrack_data:
                    tracking_data = opentrack_data
                    
                    # Send data
                    tracker.send_tracking_data(
                        opentrack_data['x'], 
                        opentrack_data['y'], 
                        opentrack_data['z']
                    )
                    
                    # Print status periodically
                    print(f"OpenTrack - Position: ({opentrack_data['x']:.2f}, {opentrack_data['y']:.2f}, {opentrack_data['z']:.2f})")
                else:
                    print("OpenTrack - No data received")
            
            # Optional: Display video frame (comment out if not needed)
            if display_frame is not None and False:  # Set to True to enable display
                segment_info = None
                if video_recorder and currently_recording:
                    video_recorder.add_frame(display_frame)
                    segment_info = video_recorder.get_segment_info()
                
                render_video(cv, display_frame, face_center if tracker.mode == TrackingMode.FACE_TRACKING else None, 
                           tracker.fov, segment_info, tracker.mode)
                cv.imshow('Face Tracking', display_frame)
            
            frame_count += 1
    
    except KeyboardInterrupt:
        print("\nApplication interrupted by user")
    
    except Exception as e:
        print(f"FATAL ERROR: {str(e)}")
        print(traceback.format_exc())
        with open(error_log_path, 'a') as f:
            f.write(f"\nFATAL ERROR: {str(e)}\n")
            f.write(traceback.format_exc())
    
    finally:
        # Cleanup
        if 'tracker' in locals():
            tracker.release_camera()
            tracker.clientSocket.close()
            tracker.opentrack_socket.close()
        
        cv.destroyAllWindows()
        
        if 'video_recorder' in locals() and video_recorder:
            print(f"\nTotal segments saved: {video_recorder.get_total_segments_saved()}")
            print(f"Videos saved in: {video_recorder.output_dir}/")
        
        print("Application closed")

if __name__ == "__main__":
    main()