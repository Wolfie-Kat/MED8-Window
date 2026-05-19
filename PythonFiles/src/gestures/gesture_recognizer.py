import math
import cv2 as cv
import mediapipe as mp
from mediapipe.tasks import python
from mediapipe.tasks.python import vision
import os
import sys

# Fix path handling for both development and frozen exe
def get_base_path():
    """Get the correct base path whether running as script or frozen exe"""
    if getattr(sys, 'frozen', False):
        # Running as PyInstaller bundle
        return sys._MEIPASS
    else:
        # Running as normal Python script
        # Go up 3 levels from gestures/gesture_recognizer.py to project root
        return os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

BASE_PATH = get_base_path()

# Find the gesture model
def find_model():
    possible_paths = [
        os.path.join(BASE_PATH, 'models', 'nodrag_gesture.task'),
        os.path.join(BASE_PATH, 'nodrag_gesture.task'),
    ]
    
    # Also search recursively
    for root, dirs, files in os.walk(BASE_PATH):
        for file in files:
            if file == 'nodrag_gesture.task':
                possible_paths.insert(0, os.path.join(root, file))
    
    for path in possible_paths:
        if os.path.exists(path):
            return path
    
    # Debug info
    task_files = []
    for root, dirs, files in os.walk(BASE_PATH):
        for file in files:
            if file.endswith('.task'):
                task_files.append(os.path.join(root, file))
    
    raise FileNotFoundError(
        f"nodrag_gesture.task not found!\n"
        f"Base path: {BASE_PATH}\n"
        f"Available .task files: {task_files}"
    )

GESTURE_MODEL_PATH = find_model()
print(f"Loading gesture model from: {GESTURE_MODEL_PATH}")

gesture_options = vision.GestureRecognizerOptions(
    base_options=python.BaseOptions(model_asset_path=GESTURE_MODEL_PATH),
    running_mode=mp.tasks.vision.RunningMode.VIDEO
)


GRAB_ENTER = 0.04
GRAB_EXIT = 0.07


class GestureRecognizer:
    def __init__(self):
        self.recognizer = vision.GestureRecognizer.create_from_options(gesture_options)
        self.frame_timestamp = 0
        self.is_grabbing = False

    def _update_grab(self, hand_landmarks):
        thumb_tip = hand_landmarks[4]
        index_pip = hand_landmarks[6]
        distance = math.hypot(thumb_tip.x - index_pip.x, thumb_tip.y - index_pip.y)
        threshold = GRAB_EXIT if self.is_grabbing else GRAB_ENTER
        self.is_grabbing = distance < threshold
        return self.is_grabbing

    def recognize_gesture(self, frame):
        rgb_frame = cv.cvtColor(frame, cv.COLOR_BGR2RGB)
        mp_image = mp.Image(image_format=mp.ImageFormat.SRGB, data=rgb_frame)
        self.frame_timestamp += 1
        result = self.recognizer.recognize_for_video(mp_image, self.frame_timestamp)

        if result.gestures:
            gesture = result.gestures[0][0].category_name
            if gesture != "None":
                return gesture

        return None

    def get_gesture_position(self, frame):
        """
        Get the position of the hand gesture.
        Returns a tuple (x, y, z) normalized coordinates or None if no hand is detected
        """
        rgb_frame = cv.cvtColor(frame, cv.COLOR_BGR2RGB)
        mp_image = mp.Image(image_format=mp.ImageFormat.SRGB, data=rgb_frame)
        self.frame_timestamp += 1
        result = self.recognizer.recognize_for_video(mp_image, self.frame_timestamp)

        # Check if hand landmarks are available
        if result.hand_landmarks and len(result.hand_landmarks) > 0:
            # Get first hand detected
            hand_landmarks = result.hand_landmarks[0]

            # Get wrist position (landmark 0) as reference point
            wrist = hand_landmarks[0]

            position = (wrist.x, wrist.y, wrist.z)

            self._update_grab(hand_landmarks)

            return position
        
        return (-1.0, -1.0, -1.0)

    def normalize_vertical_movement(self, current_y, start_y, max_distance=0.5):
        """
        Normalize vertical movement (up/down) from start position to 0-1 range.
    
        Args:
            current_y: Current y coordinate (0-1)
            start_y: Starting y coordinate (0-1)
            max_distance: Maximum expected movement distance (default 0.5)
        
        Returns:
            Float in [0, 1] where:
            0 = max movement up from start
            0.5 = at start position
            1 = max movement down from start
        """
        y_offset = current_y - start_y
        normalized = (y_offset / max_distance + 1) / 2
        return max(0.0, min(1.0, normalized))