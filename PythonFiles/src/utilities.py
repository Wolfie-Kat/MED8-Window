import math
import pickle
import os

CALIBRATION_PATH = os.path.join(
    os.path.dirname(os.path.dirname(__file__)), 'src', 'calibration', 'output', 'calibration_data.pkl'
)


def calculate_camera_fov(frame_width):
    with open(CALIBRATION_PATH, 'rb') as f:
        data = pickle.load(f)
        fx = data['camera_matrix'][0, 0]
    return 2 * math.degrees(math.atan(frame_width / (2 * fx)))


class Utilities:
    @staticmethod
    def get_face_center(landmarks):
        """Get normalized face center directly from the nose tip landmark."""
        if landmarks is None:
            return None
        # Average of left eye (159) and right eye (386) inner landmarks
        return (landmarks[159].x + landmarks[386].x) / 2, (landmarks[159].y + landmarks[386].y) / 2
    
