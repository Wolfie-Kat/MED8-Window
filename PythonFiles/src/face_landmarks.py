import pickle

import cv2 as cv
import numpy as np
import mediapipe as mp
from mediapipe.tasks import python
from mediapipe.tasks.python import vision
from utilities import Utilities
import os

path= os.path.join(os.path.dirname(os.path.dirname(__file__)),'face_landmarker.task')
base_options = python.BaseOptions(model_asset_path=path)

CALIBRATION_PATH = os.path.join(
    os.path.dirname(os.path.dirname(__file__)),'src', 'calibration', 'output', 'calibration_data.pkl'
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

class FaceLandmarker:
    def __init__(self):
        self.face_landmarker = vision.FaceLandmarker.create_from_options(options)
        self.frame_timestamp = 0
        self.fx = self.load_focal_length()

    def load_focal_length(self):
        with open(CALIBRATION_PATH, 'rb') as f:
            data = pickle.load(f)
            # fx from the camera matrix
            return data['camera_matrix'][0, 0]
    
    def detect_landmarks(self, frame):
        rgb_frame = cv.cvtColor(frame, cv.COLOR_BGR2RGB)
        mp_image = mp.Image(image_format=mp.ImageFormat.SRGB, data=rgb_frame)
        
        self.frame_timestamp += 1
        detection_result = self.face_landmarker.detect_for_video(mp_image, self.frame_timestamp)
        
        if len(detection_result.face_landmarks) > 0:
            return detection_result.face_landmarks[0]
        
        return None

    def estimate_distance(self, landmarks, frame_width, frame_height):
        left = np.array([landmarks[234].x * frame_width, landmarks[234].y * frame_height])
        right = np.array([landmarks[454].x * frame_width, landmarks[454].y * frame_height])
        face_width_px = np.linalg.norm(left - right)

        if face_width_px < 1.0:
            return 0

        return (REAL_FACE_WIDTH_CM * self.fx) / face_width_px

    def detect_faces(self, frame):
        landmarks = self.detect_landmarks(frame)

        if landmarks is not None:
            h, w = frame.shape[:2]
            bbox = Utilities.get_bbox_from_landmarks(landmarks, w, h)
            distance = self.estimate_distance(landmarks, w, h)
            return bbox, distance

        return None, None
    
