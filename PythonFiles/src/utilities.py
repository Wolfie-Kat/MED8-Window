

class Utilities:
    @staticmethod
    def get_face_center(landmarks):
        """Get normalized face center directly from the nose tip landmark."""
        if landmarks is None:
            return None
        # Average of left eye (159) and right eye (386) inner landmarks
        return (landmarks[159].x + landmarks[386].x) / 2, (landmarks[159].y + landmarks[386].y) / 2
    
