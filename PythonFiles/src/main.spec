# -*- mode: python ; coding: utf-8 -*-
import sys
import os
from PyInstaller.utils.hooks import collect_data_files, collect_submodules, collect_dynamic_libs

# Collect MediaPipe properly - these return lists, not tuples
mediapipe_datas = collect_data_files('mediapipe')
print(f"MediaPipe data files: {len(mediapipe_datas)} found")

mediapipe_binaries = collect_dynamic_libs('mediapipe')
print(f"MediaPipe binaries: {len(mediapipe_binaries)} found")

mediapipe_hidden = collect_submodules('mediapipe')
print(f"MediaPipe hidden imports: {len(mediapipe_hidden)} found")

# Your project files
added_files = [
	('models', 'models'),
	('calibration', 'calibration'),
    ('gestures', 'gestures'),
    ('face_landmarks.py', '.'),
    ('utilities.py', '.'),
    ('video_segment_recorder.py', '.'),
]

# Add video_segments directory if it exists
if os.path.exists('video_segments'):
    added_files.append(('video_segments', 'video_segments'))

# Add any model files from gestures directory
if os.path.exists('gestures'):
    for file in os.listdir('gestures'):
        if file.endswith(('.h5', '.tflite', '.pb', '.pth', '.pt', '.onnx', '.json', '.bin')):
            added_files.append((os.path.join('gestures', file), 'gestures'))
            print(f"Found model file: {file}")

# Combine all data files
all_datas = added_files + mediapipe_datas

a = Analysis(
    ['main.py'],
    pathex=[],
    binaries=mediapipe_binaries,
    datas=all_datas,
    hiddenimports=[
        'cv2',
        'numpy',
        'mediapipe',
        'mediapipe.python.solutions.face_mesh',
        'mediapipe.python.solutions.hands',
        'mediapipe.python.solutions.drawing_utils',
        'mediapipe.python.solutions.face_detection',
        'mediapipe.python.solutions.hands_connections',
        'mediapipe.python.solutions.face_mesh_connections',
        'gestures',
        'gestures.gesture_recognizer',
    ] + mediapipe_hidden,
    hookspath=[],
    hooksconfig={},
    runtime_hooks=[],
    excludes=[],
    noarchive=False,
)

pyz = PYZ(a.pure)

exe = EXE(
    pyz,
    a.scripts,
    a.binaries,
    a.datas,
    [],
    name='face_gesture_server',
    debug=False,
    bootloader_ignore_signals=False,
    strip=False,
    upx=True,
    upx_exclude=[],
    runtime_tmpdir=None,
    console=True,
    disable_windowed_traceback=False,
    argv_emulation=False,
    target_arch=None,
    codesign_identity=None,
    entitlements_file=None,
)