# WH3D_Main

## Isometric Camera System (Unity)

This repository now includes a Factorio/city-builder style isometric camera controller:

- Script: `Assets/Scripts/Camera/IsometricCameraController.cs`
- Projection: Perspective (handled in Unity Camera component)
- Rig-based motion: move/rotate rig, keep camera pitch stable

### Setup Instructions

1. Create a `CameraRig` GameObject at scene origin.
2. Make `Main Camera` a child of `CameraRig`.
3. Set `Main Camera` projection to **Perspective**.
4. Attach `IsometricCameraController` to `CameraRig`.
5. Assign the child `Main Camera` to `Camera Transform` in the inspector (auto-detect works too).
6. Start with these values:
   - `Pitch Angle`: `40`
   - `Move Speed`: `25`
   - `Movement Smoothness`: `0.12`
   - `Min Zoom`: `8`
   - `Max Zoom`: `40`
   - `Zoom Step`: `5`
   - `Keyboard Rotation Speed`: `90`
7. Optionally enable edge scrolling and tune `Edge Size`.

### Controls

- **Move**: `WASD` or Arrow Keys
- **Rotate**: `Q / E` or hold middle mouse and drag horizontally
- **Zoom**: Mouse wheel
- **Edge Scroll**: Move mouse near screen edge (if enabled)

### Notes on Grid Alignment

- Movement is flattened to world XZ (Y is always ignored).
- Rig rotates only around Y.
- Camera keeps a fixed local pitch (35–45° recommended).
- Zoom changes local camera distance, not FOV.

This keeps the viewpoint stable and consistent for logistics/grid gameplay.
