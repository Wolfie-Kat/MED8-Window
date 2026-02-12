import cv2 as cv


class FaceDetector:
    def __init__(self):
        self.face_classifier = cv.CascadeClassifier(
            cv.data.haarcascades + "haarcascade_frontalface_default.xml"
        )

        self.last_face_x = 0
        self.last_face_y = 0
    
    def detect_faces(self, frame, min_confidence=3):
        gray_image = cv.cvtColor(frame, cv.COLOR_BGR2GRAY)

        faces =  self.face_classifier.detectMultiScale(
            gray_image,
            scaleFactor=1.05,
            minNeighbors=6,
            minSize=(80, 80),
        )
        return faces

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
