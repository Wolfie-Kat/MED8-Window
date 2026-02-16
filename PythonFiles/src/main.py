import os

import cv2 as cv
from utilities import Utilities
from face_landmarks import FaceLandmarker
from face_recognition import FaceDetector
from socket import *
import struct

def render_video(cv, frame, bbox):
    if bbox is not None:
        x, y, w, h = bbox
        cv.rectangle(frame, (x, y), (x + w, y + h), color=(0, 255, 0), thickness=4)
    cv.imshow('frame', frame)   
    return frame


def main():
    cap = cv.VideoCapture(0)
    cap.set(cv.CAP_PROP_FRAME_WIDTH, 1920)
    cap.set(cv.CAP_PROP_FRAME_HEIGHT, 1080)

    clientSocket = socket(AF_INET, SOCK_DGRAM)
    address = ("127.0.0.1", 8888)

    detector = FaceDetector()
    landmarker = FaceLandmarker()
    utilities = Utilities()

    if not cap.isOpened():
        print("Cannot open camera")
        exit()

    while True:
        ret, frame = cap.read()
        screen_y, screen_x = frame.shape[:2]
        
        if not ret:
            print("Can't receive frame (stream end?). Exiting ...")
            break

        # Returns exact bounding box of a face on the screen
        bbox = detector.detect_faces(frame)
        
        #Sends face positions to udp socket for unity
        if bbox != None:
            face_x, face_y = utilities.normalize_face_position(screen_x, screen_y, bbox)
            message = struct.pack('ff', face_x, face_y)
            clientSocket.sendto(message, address)
        
        
        # render_video(cv, frame, bbox)
        if cv.waitKey(1) == ord('q'):
            break
    
    cap.release()
    cv.destroyAllWindows()


if __name__ == "__main__":
    main()