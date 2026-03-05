import os
os.environ['OPENCV_VIDEOIO_MSMF_ENABLE_HW_TRANSFORMS'] = '0'
import cv2 as cv
import math
from utilities import Utilities
from face_landmarks import FaceLandmarker
from face_recognition import FaceDetector
from socket import *
import struct

IMAGE_RES = (1280,720)

def render_video(cv, frame, bbox):
    if bbox is not None:
        x, y, w, h = bbox
        cv.rectangle(frame, (x, y), (x + w, y + h), color=(0, 255, 0), thickness=4)
    cv.imshow('frame', frame)   
    return frame

def AspectRatioCalculator (width, height):
        ratio = width / height
        return ratio

def main():
    cap = cv.VideoCapture(0)
    cap.set(cv.CAP_PROP_FRAME_WIDTH, IMAGE_RES[0])
    cap.set(cv.CAP_PROP_FRAME_HEIGHT, IMAGE_RES[1])

    clientSocket = socket(AF_INET, SOCK_DGRAM)
    address = ("127.0.0.1", 8888)

    detector = FaceDetector()
    landmarker = FaceLandmarker()
    utilities = Utilities()
    aspect_ratio = AspectRatioCalculator(IMAGE_RES[0], IMAGE_RES[1])

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
        bbox,distance = landmarker.detect_faces(frame)
        print(f"Estimated distance: {distance:.2f} cm" if distance is not None else "Distance estimation failed")
        #Sends face positions to udp socket for unity
        if bbox != None:
            face_x, face_y = utilities.normalize_face_position(screen_x, screen_y, bbox)
            message = struct.pack('ffff', face_x, face_y, aspect_ratio,distance )
            clientSocket.sendto(message, address)
        
        
        #render_video(cv, frame, bbox)
        if cv.waitKey(1) == ord('q'):
            break
    
    cap.release()
    cv.destroyAllWindows()


if __name__ == "__main__":
    main()