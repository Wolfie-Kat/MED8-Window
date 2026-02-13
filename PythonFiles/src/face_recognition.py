import cv2 as cv
import mediapipe as mp
from mediapipe.tasks import python
from mediapipe.tasks.python import vision
from utilities import Utilities
import os




model_path = os.path.join(os.path.dirname(os.path.dirname(__file__)),'blaze_face_short_range.tflite')
base_options = python.BaseOptions(model_asset_path=model_path)

options = vision.FaceDetectorOptions(
    base_options=base_options,
    min_detection_confidence=0.7
)

class FaceDetector:
    def __init__(self):
        self.face_detector = vision.FaceDetector.create_from_options(options)
        self.last_face_x = 0
        self.last_face_y = 0
    
    def detect_faces(self, frame):
        rgb_frame = cv.cvtColor(frame, cv.COLOR_BGR2RGB)
        mp_image = mp.Image(image_format=mp.ImageFormat.SRGB, data=rgb_frame)
        
        detection_result = self.face_detector.detect(mp_image)
        screen_y, screen_x = frame.shape[:2]
        return Utilities.get_bbox_from_detections(detection_result.detections, screen_x, screen_y)
    
        
    def __del__(self):
        if hasattr(self, 'face_detector'):
            self.face_detector.close()
