import cv2
import os

# Source: https://github.com/niconielsen32/camera-calibration/blob/main/capture_calibration_images.py

# Capture parameters
CAMERA_ID = 0  # Camera ID (usually 0 for built-in webcam)
CHESSBOARD_SIZE = (8, 5)  # Inner corners = squares - 1 in each dimension
OUTPUT_DIRECTORY = os.path.join(os.path.dirname(os.path.dirname(__file__)), 'calibration', 'calibration_images')  # Directory to save calibration images

IMAGE_RES = (1280,720)



def capture_calibration_images():
    """
    Capture images of a chessboard pattern for camera calibration.
    Run in an infinite loop until user presses 'q' or Escape to quit.
    Press 'c' to capture an image.
    """
    # Create output directory if it doesn't exist
    if not os.path.exists(OUTPUT_DIRECTORY):
        os.makedirs(OUTPUT_DIRECTORY)
    
    # Open camera
    cap = cv2.VideoCapture(CAMERA_ID)

    # Set width and height
    cap.set(cv2.CAP_PROP_FRAME_WIDTH, IMAGE_RES[0])
    cap.set(cv2.CAP_PROP_FRAME_HEIGHT, IMAGE_RES[1])
    
    if not cap.isOpened():
        print(f"Error: Could not open camera {CAMERA_ID}")
        return
    
    # Get camera resolution
    width = int(cap.get(cv2.CAP_PROP_FRAME_WIDTH))
    height = int(cap.get(cv2.CAP_PROP_FRAME_HEIGHT))
    print(f"Camera resolution: {width}x{height}")
    
    # Counter for captured images
    img_counter = 0
    
    print("Press 'c' to capture an image")
    print("Press 'q' or Escape to quit")
    print(f"Images will be saved to {OUTPUT_DIRECTORY}")
    
    while True:
        # Capture frame
        ret, frame = cap.read()
        
        if not ret:
            print("Error: Failed to capture image")
            break
        
        # Convert to grayscale for chessboard detection
        gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
        
        # Find chessboard corners
        ret_chess, corners = cv2.findChessboardCorners(gray, CHESSBOARD_SIZE, None)
        
        # Draw corners on a copy for display only (keep frame clean for saving)
        display = frame.copy()
        if ret_chess:
            cv2.drawChessboardCorners(display, CHESSBOARD_SIZE, corners, ret_chess)
            cv2.putText(display, "Chessboard detected!", (50, 50),
                        cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 2)

        cv2.putText(display, f"Captured: {img_counter}", (50, height - 50),
                    cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 2)

        cv2.imshow('Camera Calibration', display)

        key = cv2.waitKey(1) & 0xFF

        if key == ord('q') or key == 27:
            print("Exiting...")
            break

        # 'c' to capture — saves the clean frame without overlays
        elif key == ord('c'):
            img_name = os.path.join(OUTPUT_DIRECTORY, f"calibration_{img_counter:02d}.jpg")
            cv2.imwrite(img_name, frame)
            print(f"Captured {img_name}")
            
            img_counter += 1
    
    # Release camera and close windows
    cap.release()
    cv2.destroyAllWindows()
    
    print(f"Captured {img_counter} images for calibration")

if __name__ == "__main__":
    capture_calibration_images()