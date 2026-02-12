import cv2 as cv
from face_recognition import FaceDetector
 
cap = cv.VideoCapture(0)
cap.set(cv.CAP_PROP_FRAME_WIDTH, 1920)
cap.set(cv.CAP_PROP_FRAME_HEIGHT, 1080)

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
    
    print(f"Face position on screen: ({face_x:.2f}, {face_y:.2f})")
    
    cv.imshow('frame', frame_with_face_boxes)
    if cv.waitKey(1) == ord('q'):
        break
 
cap.release()
cv.destroyAllWindows()