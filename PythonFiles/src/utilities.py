import math
import pickle
import os
import sys

# Fix path handling for both development and frozen exe
def get_base_path():
    """Get the correct base path whether running as script or frozen exe"""
    if getattr(sys, 'frozen', False):
        return sys._MEIPASS
    else:
        return os.path.dirname(os.path.dirname(os.path.abspath(__file__)))

BASE_PATH = get_base_path()

def find_calibration_file():
    """Find the calibration data file"""
    # Possible filenames
    possible_names = ['calibration_data.pkl', 'calibration_data.json', 'calibration.pkl']
    
    # Possible locations
    possible_paths = []
    for name in possible_names:
        possible_paths.extend([
            os.path.join(BASE_PATH, name),                                    # Root
            os.path.join(BASE_PATH, 'src', 'calibration', 'output', name),   # Original path
            os.path.join(BASE_PATH, 'calibration', name),                    # calibration/
            os.path.join(BASE_PATH, 'calibration', 'output', name),          # calibration/output/
        ])
    
    # Also search everywhere for the file
    for root, dirs, files in os.walk(BASE_PATH):
        for file in files:
            if file in possible_names:
                found_path = os.path.join(root, file)
                possible_paths.insert(0, found_path)
                print(f"Found calibration file: {found_path}")
                return found_path
    
    # Not found - show debug info
    pkl_files = []
    for root, dirs, files in os.walk(BASE_PATH):
        for file in files:
            if file.endswith('.pkl') or file.endswith('.json'):
                pkl_files.append(os.path.join(root, file))
    
    raise FileNotFoundError(
        f"Calibration file not found!\n"
        f"Base path: {BASE_PATH}\n"
        f"Searched locations:\n" + '\n'.join(f"  - {p}" for p in possible_paths) + 
        f"\nAvailable .pkl/.json files: {pkl_files if pkl_files else 'None'}"
    )

CALIBRATION_PATH = find_calibration_file()
print(f"Loading calibration from: {CALIBRATION_PATH}")

def load_calibration_data():
    # Handle both pickle and JSON formats
    if CALIBRATION_PATH.endswith('.json'):
        import json
        with open(CALIBRATION_PATH, 'r') as f:
            return json.load(f)
    else:
        with open(CALIBRATION_PATH, 'rb') as f:
            return pickle.load(f)


def calculate_camera_fov(frame_width):
    data = load_calibration_data()
    fx = data['camera_matrix'][0, 0]
    cal_w = data.get('image_width', frame_width)
    # Scale fx to match current frame resolution
    scale = frame_width / cal_w
    fx_scaled = fx * scale
    return 2 * math.degrees(math.atan(frame_width / (2 * fx_scaled)))


class Utilities:
    @staticmethod
    def get_face_center(landmarks):
        """Get normalized face center directly from the nose tip landmark."""
        if landmarks is None:
            return None
        # Average of left eye (159) and right eye (386) inner landmarks
        return (landmarks[159].x + landmarks[386].x) / 2, (landmarks[159].y + landmarks[386].y) / 2