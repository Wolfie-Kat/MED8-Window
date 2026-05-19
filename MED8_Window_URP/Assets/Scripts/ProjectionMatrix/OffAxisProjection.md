# Off-Axis Projection System

## Overview

This system implements a generalized off-axis perspective projection in Unity. It simulates looking through a physical window: the monitor acts as the window, and the viewer's head position determines what they see through it. As the viewer moves, the perspective shifts naturally — just like looking through a real window.

The system has four components, each with a single responsibility.

---

## Components

### ProjectionPlane

**File:** `ProjectionPlane.cs`

**Responsibility:** Defines a rectangle in 3D space that represents the physical monitor.

**What it computes each frame:**
- Four corner positions in world space (BottomLeft, BottomRight, TopLeft, TopRight) from the GameObject's transform and `SizeInMeters`
- Three orthonormal axes: DirRight, DirUp, DirNormal — the projection plane's local coordinate system
- Matrix M — a rotation matrix that converts world directions into projection-plane-local directions

**Configuration:**
- `SizeInMeters` — physical width and height of the monitor in meters (e.g. 0.60 x 0.34 for a 27" monitor)
- `ShowWindowFrame` — toggles a thick frame around the projection plane to sell the window illusion
- `FrameDepth` — wall thickness in meters (how deep the frame extends into the scene)
- `FrameBorder` — how far the wall extends beyond the screen edges

**Important:** The projection plane does not touch the camera. It is purely a data source that describes where the monitor is, how big it is, and how it's oriented.

---

### TestEyeController

**File:** `TestEyeController.cs`

**Responsibility:** Moves an empty transform with keyboard input. Stands in for real head tracking.

**Controls:**
- A/D — left/right
- W/S — toward/away from projection plane
- Q/E — up/down
- R — reset to start position
- 1/2/3 — preset positions (center, left, right)
- Shift — fast mode

**Configuration:**
- `MoveSpeed` — movement speed in meters per second
- `MaxOffsetX` — max lateral offset (realistic: 0.15m)
- `MaxOffsetY` — max vertical offset (realistic: 0.10m)
- `MinDistanceZ` — closest distance to projection plane (realistic: 0.45m)
- `MaxDistanceZ` — farthest distance from projection plane (realistic: 0.75m)

**Important:** This component lives on a separate GameObject from the camera. Any system that moves this transform works — keyboard, webcam, OpenTrack, etc. The OffAxisCamera reads its position.

---

### OffAxisCamera

**File:** `OffAxisCamera.cs`

**Responsibility:** Reads the projection plane and eye position, computes and applies the off-axis projection.

The script runs in `LateUpdate` and performs four steps:

**Step 1 — Eye distance:**
Computes the perpendicular distance from the eye to the projection plane. Takes the vector from the eye to any corner, dots it with the plane normal. This distance is the foundation — the frustum extents are scaled relative to it.

**Step 2 — Frustum extents:**
From the eye, computes how far the projection plane edges are in each direction (left, right, bottom, top). This is done by projecting the eye-to-corner vectors onto the projection plane's right and up axes. The results are then scaled by `nearClip / eyeDistance` because `Matrix4x4.Frustum` expects extents at the near clip plane, not at the projection plane distance.

**Step 3 — Projection matrix:**
Feeds the four extents into `Matrix4x4.Frustum(left, right, bottom, top, near, far)`. This creates an asymmetric frustum — the pyramid of vision is skewed so it passes exactly through the projection plane edges, regardless of where the eye is.

**Step 4 — View matrix:**
Overrides the camera's view matrix with `M * T`, where:
- M is the rotation matrix from ProjectionPlane (aligns the camera to the projection plane's axes)
- T is a translation by negative eye position (moves the world so the eye is at the origin)

This means the camera always faces perpendicular to the projection plane. The camera GameObject's own rotation is ignored.

**Configuration:**
- `projectionPlane` — reference to the ProjectionPlane
- `eyeTransform` — reference to the eye position transform
- `nearClip` / `farClip` — near and far clip plane distances

**Debug values visible in inspector:**
- `eyeDistance` — perpendicular distance from eye to projection plane
- `frustumLeft/Right/Bottom/Top` — the four frustum extents at the near clip plane

---

### ProjectionTestScene

**File:** `ProjectionTestScene.cs`

**Responsibility:** Generates diagnostic geometry for visually verifying the projection is correct.

**What it generates:**
- **Screen plane markers (z=0):** Colored cubes at each corner (red, green, blue, yellow), edge midpoints, center, and a white border frame. These sit exactly on the projection plane.
- **Behind projection plane (z=1w, 2w, 4w):** Row of pillars, a blue archway, a checkerboard back wall. Objects at increasing depths.
- **In front of projection plane (negative z):** A red sphere and magenta cube. These "pop out" of the screen.
- **Ground grid:** Floor with grid lines extending into depth. Tests that perspective lines stay straight.
- **Reference objects:** RGB axis indicator, sphere ladder at increasing depth+height, yellow wireframe tunnel receding into the distance.

**Configuration:**
- `ScreenSize` — must match `ProjectionPlane.SizeInMeters` for the geometry to align with the projection

---

### ProjectionValidator

**File:** `ProjectionValidator.cs`

**Responsibility:** Quantitative test that verifies the projection math is correct by comparing theoretical predictions against actual rendered positions.

**How it works:**
1. Places invisible test points at known depths behind the projection plane
2. For each point, computes the **theoretical** pixel position using the parallax formula
3. Gets the **actual** pixel position from `camera.WorldToScreenPoint()`
4. Compares them — error must be less than 1 pixel to pass

**The parallax formula:**
```
shift_on_plane = eye_offset * depth / (depth + eye_distance)
```

- At depth 0 (on the projection plane): shift = 0 (zero parallax)
- At depth = eye_distance: shift = half the eye offset
- At depth = infinity: shift approaches the eye offset

**Usage:** Press V during play mode. A table appears showing theory vs actual for each test depth, with PASS/FAIL per row.

**Configuration:**
- `testDepths` — array of depths behind the projection plane to test (meters)

---

## Unit System

Everything is in meters. 1 Unity unit = 1 meter.

- `ProjectionPlane.SizeInMeters` — monitor dimensions in meters (27" monitor ≈ 0.60 x 0.34)
- Eye transform position — in meters. Position (0, 0, -0.6) means 60cm in front of the projection plane
- `ProjectionTestScene.ScreenSize` — must match `SizeInMeters`
- All frustum extents, clip distances, and object positions inherit this convention. There is no unit conversion anywhere.

---

## Scene Setup

1. **ProjectionPlane** GameObject at (0, 0, 0) — add `ProjectionPlane` component, set `SizeInMeters`
2. **Eye** GameObject at (0, 0, -0.6) — add `TestEyeController` component
3. **Camera** GameObject — add `OffAxisCamera` component, assign projection plane and eye references
4. **TestScene** GameObject — add `ProjectionTestScene` component, set `ScreenSize` to match `SizeInMeters`
5. **Validator** GameObject (optional) — add `ProjectionValidator` component, assign references

---

## How the Data Flows

```
ProjectionPlane                    TestEyeController
(corners, axes, M)                 (eye position)
        |                                |
        +---------- OffAxisCamera -------+
                         |
              projection matrix (Step 1-3)
              view matrix (Step 4)
                         |
                    Camera renders
```

---

## Visual Tests

**Zero parallax:** Move the eye with A/D and Q/E. The colored corner cubes on the projection plane must not move relative to the screen edges.

**Parallax direction:** Move eye right — objects behind the projection plane shift left. Move eye up — objects behind shift down.

**Negative parallax:** The red sphere and magenta cube in front of the projection plane shift with your movement (same direction), not against it.

**Straight lines:** Grid lines on the floor must remain straight regardless of eye position.

**Symmetric when centered:** With the eye centered, frustumLeft and frustumRight should be equal magnitude (opposite sign). Same for frustumBottom and frustumTop.

---

## Realistic Movement Ranges

In a real head-tracking setup, the viewer sits in front of a monitor. Physical head movement is limited:

| Parameter | Realistic value | Why |
|-----------|----------------|-----|
| Lateral offset | ±0.15m | Head sways ~15cm side to side max |
| Vertical offset | ±0.10m | Head bobs ~10cm up/down max |
| Min distance | 0.45m | Closest comfortable sitting distance |
| Max distance | 0.75m | Leaning back in a chair |
| Max horizontal angle | ~17° | atan(0.15 / 0.50) |
| Max vertical angle | ~9° | atan(0.10 / 0.60) |

Within these ranges the frustum never becomes degenerate. The projection math does not need clamping — the physical world already limits it.

---

## Window Frame

The window illusion is significantly more convincing with a visible frame around the projection plane. The frame provides occlusion-based depth cues: as you move your head, the frame edges hide and reveal parts of the scene. This is the single most effective technique for selling the illusion.

Toggle `ShowWindowFrame` on the ProjectionPlane component. Adjust `FrameDepth` (wall thickness) and `FrameBorder` (wall extent) to taste.
