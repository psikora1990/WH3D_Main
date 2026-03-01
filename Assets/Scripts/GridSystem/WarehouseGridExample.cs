using UnityEngine;

/// <summary>
/// Example script showing how to raycast from mouse position onto a ground plane,
/// convert to grid coordinates, and place/remove objects.
/// </summary>
public class WarehouseGridExample : MonoBehaviour
{
    [SerializeField] private WarehouseGrid warehouseGrid;
    [SerializeField] private GameObject placeablePrefab;
    [SerializeField] private Camera topDownCamera;

    private Plane placementPlane;

    private void Awake()
    {
        // Top-down plane at the grid object Y level.
        float yLevel = warehouseGrid != null ? warehouseGrid.transform.position.y : 0f;
        placementPlane = new Plane(Vector3.up, new Vector3(0f, yLevel, 0f));
    }

    private void Update()
    {
        if (warehouseGrid == null || topDownCamera == null)
        {
            return;
        }

        if (!TryGetMouseCell(out int x, out int y))
        {
            return;
        }

        // Left click places one prefab instance if cell is free.
        if (Input.GetMouseButtonDown(0) && placeablePrefab != null)
        {
            GameObject instance = Instantiate(placeablePrefab);
            bool placed = warehouseGrid.PlaceObject(instance, x, y);
            if (!placed)
            {
                Destroy(instance);
            }
        }

        // Right click removes object reference from selected cell.
        if (Input.GetMouseButtonDown(1))
        {
            if (warehouseGrid.TryGetCell(x, y, out GridCell cell) && cell.OccupiedObject != null)
            {
                Destroy(cell.OccupiedObject);
            }

            warehouseGrid.RemoveObject(x, y);
        }
    }

    private bool TryGetMouseCell(out int x, out int y)
    {
        Ray ray = topDownCamera.ScreenPointToRay(Input.mousePosition);
        x = -1;
        y = -1;

        if (!placementPlane.Raycast(ray, out float enter))
        {
            return false;
        }

        Vector3 worldPoint = ray.GetPoint(enter);
        return warehouseGrid.TryGetGridPosition(worldPoint, out x, out y);
    }
}
