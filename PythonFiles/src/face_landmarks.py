import cv2 as cv
import numpy as np
import mediapipe as mp
from mediapipe.tasks import python
from mediapipe.tasks.python import vision
from utilities import Utilities, load_calibration_data
import os

path= os.path.join(os.path.dirname(os.path.dirname(__file__)),'models', 'face_landmarker.task')
base_options = python.BaseOptions(model_asset_path=path)

GESTURE_MODEL_PATH = os.path.join(
    os.path.dirname(os.path.dirname(__file__)), 'models', 'gesture_recognizer.task'
)
REAL_FACE_WIDTH_CM = 14.2

options = vision.FaceLandmarkerOptions(
    base_options=base_options,
    output_face_blendshapes=False,
    output_facial_transformation_matrixes=False,
    num_faces=1,
    min_face_detection_confidence=0.5,
    min_tracking_confidence=0.5,
    running_mode=mp.tasks.vision.RunningMode.VIDEO
)

gesture_options = vision.GestureRecognizerOptions(
    base_options=python.BaseOptions(model_asset_path=GESTURE_MODEL_PATH),
    running_mode=mp.tasks.vision.RunningMode.VIDEO
)



class FaceLandmarker:
    def __init__(self):
        self.face_landmarker = vision.FaceLandmarker.create_from_options(options)
        self.frame_timestamp = 0
        data = load_calibration_data()
        self.fx = data['camera_matrix'][0, 0]
        self.cal_width = data.get('image_width')
        self._fx_scaled = None
        self._last_frame_width = None
    
    def detect_landmarks(self, frame):
        rgb_frame = cv.cvtColor(frame, cv.COLOR_BGR2RGB)
        mp_image = mp.Image(image_format=mp.ImageFormat.SRGB, data=rgb_frame)
        
        self.frame_timestamp += 1
        detection_result = self.face_landmarker.detect_for_video(mp_image, self.frame_timestamp)
        
        if len(detection_result.face_landmarks) > 0:
            return detection_result.face_landmarks[0]
        
        return None

    def _get_scaled_fx(self, frame_width):
        if frame_width != self._last_frame_width:
            self._last_frame_width = frame_width
            if self.cal_width is not None:
                self._fx_scaled = self.fx * (frame_width / self.cal_width)
            else:
                self._fx_scaled = self.fx
        return self._fx_scaled

    def estimate_distance(self, landmarks, frame_width, frame_height):
        left = np.array([landmarks[234].x * frame_width, landmarks[234].y * frame_height])
        right = np.array([landmarks[454].x * frame_width, landmarks[454].y * frame_height])
        face_width_px = np.linalg.norm(left - right)

        if face_width_px < 1.0:
            return 0

        fx = self._get_scaled_fx(frame_width)
        return (REAL_FACE_WIDTH_CM * fx) / face_width_px

    def detect_faces(self, frame):
        landmarks = self.detect_landmarks(frame)

        if landmarks is not None:
            h, w = frame.shape[:2]
            face_center = Utilities.get_face_center(landmarks)
            distance = self.estimate_distance(landmarks, w, h)
            return face_center, distance

        return None, None
    
