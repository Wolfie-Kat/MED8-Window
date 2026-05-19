# -*- mode: python ; coding: utf-8 -*-
from PyInstaller.utils.hooks import collect_all

datas = [('C:\\Users\\katri\\Documents\\GitHub\\MED8-Window\\PythonFiles\\src\\models\\face_landmarker.task', '.'), ('C:\\Users\\katri\\Documents\\GitHub\\MED8-Window\\PythonFiles\\src\\models\\gesture_recognizer.task', '.'), ('C:\\Users\\katri\\Documents\\GitHub\\MED8-Window\\PythonFiles\\src\\models\\nodrag_gesture.task', '.'), ('C:\\Users\\katri\\Documents\\GitHub\\MED8-Window\\PythonFiles\\src\\calibration', 'calibration/')]
binaries = []
hiddenimports = ['cv2', 'numpy', 'mediapipe.tasks.python', 'mediapipe.tasks.python.vision']
tmp_ret = collect_all('mediapipe')
datas += tmp_ret[0]; binaries += tmp_ret[1]; hiddenimports += tmp_ret[2]


a = Analysis(
    ['C:\\Users\\katri\\Documents\\GitHub\\MED8-Window\\PythonFiles\\src\\main.py'],
    pathex=[],
    binaries=binaries,
    datas=datas,
    hiddenimports=hiddenimports,
    hookspath=[],
    hooksconfig={},
    runtime_hooks=[],
    excludes=[],
    noarchive=False,
    optimize=0,
)
pyz = PYZ(a.pure)

exe = EXE(
    pyz,
    a.scripts,
    a.binaries,
    a.datas,
    [],
    name='LaunchScript',
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
