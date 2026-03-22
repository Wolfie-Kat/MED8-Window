"""Capture a single gesture image and save it to gesture_dataset/<gesture_name>/."""

import argparse
import os

import cv2 as cv

DATASET_DIR = os.path.join(os.path.dirname(__file__), "gesture_dataset")


def next_filename(folder):
    """Return the next available filename like 0001.jpg, 0002.jpg, ..."""
    existing = [f for f in os.listdir(folder) if f.endswith(".jpg")]
    index = len(existing) + 1
    return f"{index:04d}.jpg"


def main():
    parser = argparse.ArgumentParser(description="Capture a gesture image.")
    parser.add_argument("gesture", help="Gesture name (subfolder in gesture_dataset)")
    args = parser.parse_args()

    gesture_dir = os.path.join(DATASET_DIR, args.gesture)
    os.makedirs(gesture_dir, exist_ok=True)

    cap = cv.VideoCapture(0)
    if not cap.isOpened():
        print("Error: cannot open camera.")
        return

    print(f"Saving to: {gesture_dir}")
    print("Press SPACE to capture, Q to quit.")

    while True:
        ret, frame = cap.read()
        if not ret:
            print("Error: failed to read frame.")
            break

        cv.imshow("Capture Gesture", frame)
        key = cv.waitKey(1) & 0xFF

        if key == ord(" "):
            filename = next_filename(gesture_dir)
            path = os.path.join(gesture_dir, filename)
            cv.imwrite(path, frame)
            print(f"Saved: {path}")
        elif key == ord("q"):
            break

    cap.release()
    cv.destroyAllWindows()


if __name__ == "__main__":
    main()
