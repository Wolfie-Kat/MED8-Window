import cv2 as cv
import mediapipe as mp
from mediapipe.tasks import python
from mediapipe.tasks.python import vision
from utilities import Utilities
import os


base_options = python.BaseOptions(model_asset_path='face_landmarker.task')

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
    
    def detect_landmarks(self, frame):
        rgb_frame = cv.cvtColor(frame, cv.COLOR_BGR2RGB)
        mp_image = mp.Image(image_format=mp.ImageFormat.SRGB, data=rgb_frame)
        
        self.frame_timestamp += 1
        detection_result = self.face_landmarker.detect_for_video(mp_image, self.frame_timestamp)
        
        if len(detection_result.face_landmarks) > 0:
            return detection_result.face_landmarks[0]
        
        return None

    def detect_faces(self, frame):
        landmarks = self.detect_landmarks(frame)
        
        if landmarks is not None:
            bbox = Utilities.get_bbox_from_landmarks(landmarks, frame.shape[1], frame.shape[0])
            return bbox if bbox is not None else None
        
        return None
    
