

class Utilities:
    @staticmethod
    def normalize_face_position(screen_x, screen_y, bbox):
        if bbox is None:
            return 0.5, 0.5
        
        x, y, w, h = bbox
        face_x = x + w / 2
        face_y = y + h / 2
        
        normalized_x = face_x / screen_x
        normalized_y = face_y / screen_y
        return normalized_x, normalized_y
    
    @staticmethod
    def get_bbox_from_landmarks(landmarks, screen_x, screen_y):
        if landmarks is not None:
            h, w = screen_y, screen_x
            
            x1, y1 = int(landmarks[10].x * w), int(landmarks[10].y * h)   # forehead
            x2, y2 = int(landmarks[152].x * w), int(landmarks[152].y * h) # chin
            x3, y3 = int(landmarks[234].x * w), int(landmarks[234].y * h) # left ear
            x4, y4 = int(landmarks[454].x * w), int(landmarks[454].y * h) # right ear
            
            x = min(x1, x2, x3, x4)
            y = min(y1, y2, y3, y4)
            width = max(x1, x2, x3, x4) - x
            height = max(y1, y2, y3, y4) - y
            
            return (x, y, width, height)
        
        return None
    
    @staticmethod
    def get_bbox_from_detections(detections, screen_x, screen_y):
        if len(detections) > 0:
            detection = detections[0]
            bbox = detection.bounding_box
            x = int(bbox.origin_x)
            y = int(bbox.origin_y)
            width = int(bbox.width)
            height = int(bbox.height)
            return (x, y, width, height)
        
        return None
