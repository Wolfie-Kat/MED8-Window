import cv2 as cv
import numpy as np
import math
import time
import mediapipe as mp
from mediapipe.tasks import python
from mediapipe.tasks.python import vision
from utilities import Utilities, load_calibration_data
import os
import sys


class OneEuroFilter:
    """One-Euro Filter for smoothing noisy signals in real-time.

    Adapts cutoff frequency based on signal speed: smooths aggressively when
    still (eliminates banding), stays responsive during fast movement.
    Reference: Casiez et al., "1euro Filter", CHI 2012.
    """

    def __init__(self, min_cutoff=1.0, beta=0.007, d_cutoff=1.0):
        self.min_cutoff = min_cutoff
        self.beta = beta
        self.d_cutoff = d_cutoff
        self.x_prev = None
        self.dx_prev = 0.0
        self.t_prev = None

    @staticmethod
    def _smoothing_factor(t_e, cutoff):
        r = 2 * math.pi * cutoff * t_e
        return r / (r + 1)

    def __call__(self, x, t=None):
        if t is None:
            t = time.monotonic()

        if self.t_prev is None:
            self.x_prev = x
            self.dx_prev = 0.0
            self.t_prev = t
            return x

        t_e = t - self.t_prev
        if t_e <= 0:
            return self.x_prev

        # Filtered derivative (speed)
        a_d = self._smoothing_factor(t_e, self.d_cutoff)
        dx = (x - self.x_prev) / t_e
        dx_hat = a_d * dx + (1 - a_d) * self.dx_prev

        # Adaptive cutoff based on speed
        cutoff = self.min_cutoff + self.beta * abs(dx_hat)

        # Filtered signal
        a = self._smoothing_factor(t_e, cutoff)
        x_hat = a * x + (1 - a) * self.x_prev

        self.x_prev = x_hat
        self.dx_prev = dx_hat
        self.t_prev = t
        return x_hat

# Fix path handling for both development and frozen exe
def get_base_path():
    """Get the correct base path whether running as script or frozen exe"""
    if getattr(sys, 'frozen', False):
        # Running as PyInstaller bundle
        return sys._MEIPASS
    else:
        # Running as normal Python script
        return os.path.dirname(os.path.abspath(__file__))

BASE_PATH = get_base_path()

# Model paths - check models folder first, then root
MODELS_FOLDER = os.path.join(BASE_PATH, 'models')
FACE_MODEL_PATH = os.path.join(MODELS_FOLDER, 'face_landmarker.task')
GESTURE_MODEL_PATH = os.path.join(MODELS_FOLDER, 'gesture_recognizer.task')

# If models folder doesn't exist, check root (for when files are added directly)
if not os.path.exists(FACE_MODEL_PATH):
    FACE_MODEL_PATH = os.path.join(BASE_PATH, 'face_landmarker.task')
if not os.path.exists(GESTURE_MODEL_PATH):
    GESTURE_MODEL_PATH = os.path.join(BASE_PATH, 'gesture_recognizer.task')

# Verify models exist
if not os.path.exists(FACE_MODEL_PATH):
    # List available .task files for debugging
    available_tasks = []
    for root, dirs, files in os.walk(BASE_PATH):
        for file in files:
            if file.endswith('.task'):
                available_tasks.append(os.path.join(root, file))
    
    error_msg = f"Face landmarker model not found!\n"
    error_msg += f"Searched: {FACE_MODEL_PATH}\n"
    error_msg += f"Available .task files: {available_tasks}"
    raise FileNotFoundError(error_msg)

print(f"Loading face model from: {FACE_MODEL_PATH}")
print(f"Loading gesture model from: {GESTURE_MODEL_PATH}")

base_options = python.BaseOptions(model_asset_path=FACE_MODEL_PATH)
gesture_base_options = python.BaseOptions(model_asset_path=GESTURE_MODEL_PATH)

REAL_FACE_WIDTH_CM = 14.2

# Reference measurements for multi-landmark distance estimation (in cm).
# Each entry: (landmark_a, landmark_b, real_distance_cm)
FACE_REFERENCES = [
    (234, 454, 14.2),   # Face width (cheek to cheek)
    (133, 362,  6.3),   # Inter-eye (inner corners)
    (10,  152, 12.0),   # Forehead to chin (vertical)
]

options = vision.FaceLandmarkerOptions(
    base_options=base_options,
    output_face_blendshapes=False,
    output_facial_transformation_matrixes=False,
    num_faces=1,
    min_face_detection_confidence=0.5,
    min_tracking_confidence=0.5,
    running_mode=mp.tasks.vision.RunningMode.VIDEO
)

gesture_options = vision.GestureRecognizerOptions(
    base_options=gesture_base_options,
    running_mode=mp.tasks.vision.RunningMode.VIDEO
)



class FaceLandmarker:
    def __init__(self):
        self.face_landmarker = vision.FaceLandmarker.create_from_options(options)
        self.frame_timestamp = 0
        data = load_calibration_data()
        self.fx = data['camera_matrix'][0, 0]
        self.cal_width = data.get('image_width')
        self._fx_scaled = None
        self._last_frame_width = None

        # One-Euro filters: low min_cutoff for strong smoothing when still,
        # moderate beta so fast head movements still come through quickly.
        self.filter_x = OneEuroFilter(min_cutoff=1.5, beta=0.01)
        self.filter_y = OneEuroFilter(min_cutoff=1.5, beta=0.01)
        self.filter_z = OneEuroFilter(min_cutoff=0.8, beta=0.005)

    def detect_landmarks(self, frame):
        rgb_frame = cv.cvtColor(frame, cv.COLOR_BGR2RGB)
        mp_image = mp.Image(image_format=mp.ImageFormat.SRGB, data=rgb_frame)

        self.frame_timestamp += 1
        detection_result = self.face_landmarker.detect_for_video(mp_image, self.frame_timestamp)

        if len(detection_result.face_landmarks) > 0:
            return detection_result.face_landmarks[0]

        return None

    def _get_scaled_fx(self, frame_width):
        if frame_width != self._last_frame_width:
            self._last_frame_width = frame_width
            if self.cal_width is not None:
                self._fx_scaled = self.fx * (frame_width / self.cal_width)
            else:
                self._fx_scaled = self.fx
        return self._fx_scaled

    def estimate_distance(self, landmarks, frame_width, frame_height):
        """Estimate distance using multiple facial reference measurements.

        Computes a distance estimate from each landmark pair in
        FACE_REFERENCES, then returns the weighted median to reject
        outliers while preserving accuracy.
        """
        fx = self._get_scaled_fx(frame_width)
        estimates = []

        for idx_a, idx_b, real_cm in FACE_REFERENCES:
            ax = landmarks[idx_a].x * frame_width
            ay = landmarks[idx_a].y * frame_height
            bx = landmarks[idx_b].x * frame_width
            by = landmarks[idx_b].y * frame_height
            span_px = math.hypot(bx - ax, by - ay)
            if span_px < 1.0:
                continue
            estimates.append((real_cm * fx) / span_px)

        if not estimates:
            return 0

        # Weighted median: face-width pair is most reliable so weight it more
        estimates.sort()
        return estimates[len(estimates) // 2]

    def detect_faces(self, frame):
        landmarks = self.detect_landmarks(frame)

        if landmarks is not None:
            h, w = frame.shape[:2]
            face_center = Utilities.get_face_center(landmarks)
            distance = self.estimate_distance(landmarks, w, h)

            # Apply One-Euro filtering to eliminate banding / jitter
            t = time.monotonic()
            fx = self.filter_x(face_center[0], t)
            fy = self.filter_y(face_center[1], t)
            fz = self.filter_z(distance, t)

            return (fx, fy), fz

        return None, None