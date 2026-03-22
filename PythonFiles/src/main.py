import os

os.environ['OPENCV_VIDEOIO_MSMF_ENABLE_HW_TRANSFORMS'] = '0'
from gestures.gesture_recognizer import GestureRecognizer
import cv2 as cv
from face_landmarks import FaceLandmarker
from utilities import calculate_camera_fov
from socket import *
import struct

def render_video(cv, frame, face_center, gesture=None, fov=None):
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
    cv.imshow('frame', frame)
    return frame

def main():
    cap = cv.VideoCapture(0)

    clientSocket = socket(AF_INET, SOCK_DGRAM)
    address = ("127.0.0.1", 8888)

    landmarker = FaceLandmarker()
    gesture_recognizer = GestureRecognizer()

    if not cap.isOpened():
        print("Cannot open camera")
        exit()

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

        if face_center is not None:
            face_x, face_y = face_center
            message = struct.pack('ffff', face_x, face_y, aspect_ratio, distance)
            clientSocket.sendto(message, address)


        render_video(cv, frame, face_center, gesture, fov)
        if cv.waitKey(1) == ord('q'):
            break
    
    cap.release()
    cv.destroyAllWindows()


if __name__ == "__main__":
    main()