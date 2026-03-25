import math
import pickle
import os

CALIBRATION_PATH = os.path.join(
    os.path.dirname(os.path.dirname(__file__)), 'src', 'calibration', 'output', 'calibration_data.pkl'
)


def load_calibration_data():
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
    
