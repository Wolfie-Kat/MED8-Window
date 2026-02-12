import cv2 as cv
from face_recognition import FaceDetector
from socket import *
import struct


def main():
    cap = cv.VideoCapture(0)
    cap.set(cv.CAP_PROP_FRAME_WIDTH, 1920)
    cap.set(cv.CAP_PROP_FRAME_HEIGHT, 1080)

    clientSocket = socket(AF_INET, SOCK_DGRAM)
    address = ("127.0.0.1", 8888)

    detector = FaceDetector()

    if not cap.isOpened():
        print("Cannot open camera")
        exit()

    while True:
        ret, frame = cap.read()
        if not ret:
            print("Can't receive frame (stream end?). Exiting ...")
            break


        frame_with_face_boxes, faces = detector.detect_and_draw(frame)
        face_x, face_y = detector.get_normalized_face_screen_positions(frame, faces)
        
        message = struct.pack('ff', face_x, face_y)
        encoded_message = message
        clientSocket.sendto(encoded_message, address)
        
        # cv.imshow('frame', frame_with_face_boxes)
        if cv.waitKey(1) == ord('q'):
            break
    
    cap.release()
    cv.destroyAllWindows()


if __name__ == "__main__":
    main()