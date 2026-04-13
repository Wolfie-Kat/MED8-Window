import os

os.environ['OPENCV_VIDEOIO_MSMF_ENABLE_HW_TRANSFORMS'] = '0'
from gestures.gesture_recognizer import GestureRecognizer
import cv2 as cv
from face_landmarks import FaceLandmarker
from utilities import calculate_camera_fov
from socket import *
import struct
from video_segment_recorder import VideoSegmentRecorder

def render_video(cv, frame, face_center, gesture=None, fov=None, segment_info=None):
    if face_center is not None:
        h, w = frame.shape[:2]
        cx = int(face_center[0] * w)
        cy = int(face_center[1] * h)
        cv.circle(frame, (cx, cy), 8, color=(0, 255, 0), thickness=-1)
    if gesture is not None:
        cv.putText(frame, f"Gesture: {gesture}", (10, 40),
                   cv.FONT_HERSHEY_SIMPLEX, 1.2, (0, 255, 0), 3)
    if fov is not None:
        cv.putText(frame, f"FOV: {fov:.1f} deg", (10, 80),
                   cv.FONT_HERSHEY_SIMPLEX, 0.8, (0, 255, 255), 2)
    
        # Display segment info if available
    if segment_info is not None:
        cv.putText(frame, f"Segment: {segment_info['frame_count']} frames ({segment_info['duration']:.1f}s)", 
                   (10, 120), cv.FONT_HERSHEY_SIMPLEX, 0.6, (0, 255, 255), 2)
        cv.putText(frame, "Press 's' to save segment | 'r' to reset | 'q' to quit", 
                   (10, frame.shape[0] - 10), cv.FONT_HERSHEY_SIMPLEX, 0.5, (0, 255, 0), 1)
        
    cv.imshow('frame', frame)
    return frame

def AspectRatioCalculator(width, height):
        ratio = width / height
        return ratio

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
    
def main():
    cap = cv.VideoCapture(0)

    # Properties
    frame_width = int(cap.get(3))
    frame_height = int(cap.get(4))
    fps = cap.get(cv.CAP_PROP_FPS)

    # Use actual camera FPS or default to 30 if not available
    if fps <= 0:
        fps = 30.0

    # Initialize the video segment recorder
    video_recorder = VideoSegmentRecorder(output_dir="video_segments", fps=fps, frame_size=(frame_width, frame_height))

    currently_recording = False

    # Writer
    #fourcc = cv.VideoWriter_fourcc(*'MPEG')
    #out = cv.VideoWriter('output.avi', fourcc, fps, (frame_width, frame_height))

    clientSocket = socket(AF_INET, SOCK_DGRAM)
    address = ("127.0.0.1", 8888)

    landmarker = FaceLandmarker()
    gesture_recognizer = GestureRecognizer()

    if not cap.isOpened():
        print("Cannot open camera")
        exit()

    gesture_start_position = (-1.0, -1.0, -1.0)
    fov = None
    aspect_ratio = None

    while True:
        ret, frame = cap.read()

        if not ret:
            print("Can't receive frame (stream end?). Exiting ...")
            break

        if fov is None:
            h, w = frame.shape[:2]
            fov = calculate_camera_fov(w)
            aspect_ratio = w / h

        face_center, distance = landmarker.detect_faces(frame)
        gesture = gesture_recognizer.recognize_gesture(frame)
        gesture_position = gesture_recognizer.get_gesture_position(frame)

        if gesture == "fist_left":
            if gesture_start_position is (-1.0, -1.0, -1.0):
                gesture_start_position = gesture_position
        elif gesture == "fist_middle":
            if gesture_start_position is (-1.0, -1.0, -1.0):
                gesture_start_position = gesture_position
        elif gesture == "fist_right":
            if gesture_start_position is (-1.0, -1.0, -1.0):
                gesture_start_position = gesture_position
        elif gesture == "palm":
            if gesture_start_position is (-1.0, -1.0, -1.0):
                gesture_start_position = gesture_position
        else:
            gesture_start_position = (-1.0, -1.0, -1.0)
        

        if face_center is not None:
            face_x, face_y = face_center
            gesture_code = gesture_to_code(gesture)
            print (f"Gesture start position: {round(gesture_start_position[0], 2), round(gesture_start_position[1], 2)}, Gesture position: {round(gesture_position[0], 2), round(gesture_position[1], 2)}")
            message = struct.pack('fffffffff', 
                                  face_x, 
                                  face_y, 
                                  aspect_ratio, 
                                  distance, 
                                  gesture_code, 
                                  gesture_start_position[0], 
                                  gesture_position[0], 
                                  gesture_start_position[1], 
                                  gesture_position[1])
            clientSocket.sendto(message, address)

        # Add frame to the segment recorder
        if currently_recording == True:
            video_recorder.add_frame(frame)

            # Get segment info for display
            segment_info = video_recorder.get_segment_info()

        if currently_recording == True:
            # Render video with segment info
            render_video(cv, frame, face_center, gesture, fov, segment_info)
        else:
            render_video(cv, frame, face_center, gesture, fov)

        key = cv.waitKey(1) & 0xFF

        #out.write(frame)
        if key == ord('s'):
            # Save current segment
            if currently_recording == True:
                print(f"\nSaving segment with {segment_info['frame_count']} frames...")
                video_recorder.save_current_segment()
                currently_recording = False
            else:
                currently_recording = True
            
        elif key == ord('r'):
            # Reset current segment without saving
            if len(video_recorder.current_segment_frames) > 0:
                print(f"\nResetting segment (discarding {segment_info['frame_count']} frames)")
                video_recorder.reset_segment()
            else:
                print("\nNothing to reset")
                
        elif key == ord('q'):
            print("\nQuitting...")
            break

    
    cap.release()
    #out.release()
    cv.destroyAllWindows()

    # Print summary of saved segments
    print(f"\nTotal segments saved: {video_recorder.get_total_segments_saved()}")
    print(f"Videos saved in: {video_recorder.output_dir}/")


if __name__ == "__main__":
    main()