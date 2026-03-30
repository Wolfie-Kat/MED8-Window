import cv2 as cv
import os
from datetime import datetime

class VideoSegmentRecorder:

    def __init__(self, output_dir="Recordings", fps=30.0, frame_size=(720, 1280)):
        """
        Initialize the video segment recorder

        Args:
            output_dir: Directory to save recordings
            fps: Frames per second for output video
            frame_size: Size of video frames (width, height)
        """
        self.output_dir = output_dir
        self.fps = fps
        self.frame_size = frame_size
        self.current_segment_frames = []
        self.is_recording = True # Always records to buffer
        self.frame_count = 0
        self.segment_count = 0

        # Create output directory if it doesn't exist
        if not os.path.exists(output_dir):
            os.makedirs(output_dir)

    def add_frame(self, frame):
            """Add a frame to the current segment buffer."""
            if frame is not None:
                # Resize frame if needed
                if frame.shape[1] != self.frame_size[0] or frame.shape[0] != self.frame_size[1]:
                    frame = cv.resize(frame, self.frame_size)
                self.current_segment_frames.append(frame.copy())
                self.frame_count += 1
    
    def save_current_segment(self, custom_name=None):
        """
        Save the current video segment to a file.
        
        Args:
            custom_name: Optional custom name for the video file
        """
        if len(self.current_segment_frames) == 0:
            print("No frames to save")
            return None
        
        # Generate filename
        if custom_name:
            filename = f"{custom_name}.mp4"
        else:
            timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
            filename = f"segment_{self.segment_count:04d}_{timestamp}.mp4"

        filepath = os.path.join(self.output_dir, filename)

        # Create video writer
        fourcc = cv.VideoWriter_fourcc(*'mp4v')
        out = cv.VideoWriter(filepath, fourcc, self.fps, self.frame_size)

        # Write all frames
        for frame in self.current_segment_frames:
            out.write(frame)

        out.release()

        # Get segment duration
        duration = len(self.current_segment_frames) / self.fps
        print(f"Saved: {filepath} ({len(self.current_segment_frames)} frames, {duration:.2f} seconds)")

        # Clear the buffer for next segment
        self.current_segment_frames = []
        self.segment_count += 1
        return filepath
    
    def reset_segment(self):
        """Reset the current segment without saving."""
        self.current_segment_frames = []
    
    def get_segment_info(self):
        """Get information about the current segment."""
        return {
            'frame_count': len(self.current_segment_frames),
            'duration': len(self.current_segment_frames) / self.fps,
            'is_recording': self.is_recording
        }
    
    def get_total_segments(self):
        """Return the total number of segments saved."""
        return self.segment_count
        