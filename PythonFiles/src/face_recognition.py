import cv2 as cv
import mediapipe as mp
from mediapipe.tasks import python
from mediapipe.tasks.python import vision
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
        
        for detection in detection_result.detections:
            bbox = detection.bounding_box
            x = int(bbox.origin_x)
            y = int(bbox.origin_y)
            width = int(bbox.width)
            height = int(bbox.height)
            return [(x, y, width, height)]
        
        return []
        
    def draw_bounding_boxes(self, frame, faces, color=(0, 255, 0), thickness=4):
        for (x, y, w, h) in faces:
            cv.rectangle(frame, (x, y), (x + w, y + h), color, thickness)
        return frame, faces


    def get_normalized_face_screen_positions(self,frame, faces):
        if (len(faces) == 0):
            return self.normalize_face_position(frame, self.last_face_x, self.last_face_y)
        
        face_x, face_y = 0, 0
        for (x, y, w, h) in faces:
            face_x = x + w / 2
            face_y = y + h / 2
        
        self.last_face_x = face_x
        self.last_face_y = face_y
        
        return self.normalize_face_position(frame, face_x, face_y)

    def normalize_face_position(self, frame, face_x, face_y):
        frame_width = frame.shape[1]
        frame_height = frame.shape[0]
        
        normalized_x = face_x / frame_width
        normalized_y = face_y / frame_height

        return normalized_x, normalized_y
    
    def detect_and_draw(self, frame):
        faces = self.detect_faces(frame)
        return self.draw_bounding_boxes(frame, faces)
    
    def __del__(self):
        if hasattr(self, 'face_detector'):
            self.face_detector.close()
