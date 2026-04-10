import math
import cv2 as cv
import mediapipe as mp
from mediapipe.tasks import python
from mediapipe.tasks.python import vision
import os


GESTURE_MODEL_PATH = os.path.join(
    os.path.dirname(os.path.dirname(os.path.dirname(__file__))), 'models', 'gesture_recognizer_new.task'
)  

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
    