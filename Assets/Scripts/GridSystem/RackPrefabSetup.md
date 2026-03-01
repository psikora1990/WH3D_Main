# Rack Prefab Setup (Example)

1. Create a GameObject named **Rack** with mesh (or a model import) and scale that fits one grid tile.
2. Position the mesh pivot at the base center so snapping to cell center aligns correctly on the XZ plane.
3. Add a collider (BoxCollider recommended) sized to the rack footprint.
4. Save the object as a prefab in `Assets/Prefabs/Rack.prefab`.
5. Assign this prefab to `RackPrefab` on `BuildingPlacementController`.
6. Create a transparent material (URP/Lit or Standard with Rendering Mode = Transparent) and assign it to `PreviewMaterial`.
7. Optional: add a child quad with transparent unlit material as `HighlightPrefab` and assign it on `GridSelector`.

This setup gives Factorio-style ghost placement, color validation, and snapped placement behavior.
