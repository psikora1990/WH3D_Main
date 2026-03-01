using UnityEngine;

/// <summary>
/// Runtime-generated warehouse grid for top-down logistics gameplay.
/// Grid is aligned to XZ plane where Y is height.
/// </summary>
public class WarehouseGrid : MonoBehaviour
{
    [Header("Grid Dimensions")]
    [Min(1)] [SerializeField] private int width = 20;
    [Min(1)] [SerializeField] private int height = 20;
    [Min(0.1f)] [SerializeField] private float cellSize = 1f;

    [Header("Debug")]
    [SerializeField] private bool drawDebugGrid = true;
    [SerializeField] private float gizmoYOffset = 0.02f;

    private GridCell[,] gridCells;

    public int Width => width;
    public int Height => height;
    public float CellSize => cellSize;

    private void Start()
    {
        GenerateGrid();
    }

    /// <summary>
    /// Creates a new grid and initializes every cell with coordinate and world position.
    /// </summary>
    public void GenerateGrid()
    {
        gridCells = new GridCell[width, height];

        Vector3 origin = transform.position;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 worldPosition = origin + new Vector3(x * cellSize, 0f, y * cellSize);
                gridCells[x, y] = new GridCell(x, y, worldPosition);
            }
        }
    }

    /// <summary>
    /// Returns true when a coordinate is inside the warehouse grid.
    /// </summary>
    public bool IsInBounds(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    /// <summary>
    /// Returns whether a cell is inside bounds and currently empty.
    /// </summary>
    public bool IsCellFree(int x, int y)
    {
        return IsInBounds(x, y) && !gridCells[x, y].IsOccupied;
    }

    /// <summary>
    /// Places a GameObject into a specific grid cell and snaps it to the cell world position.
    /// </summary>
    public bool PlaceObject(GameObject obj, int x, int y)
    {
        if (obj == null || !IsCellFree(x, y))
        {
            return false;
        }

        GridCell cell = gridCells[x, y];
        cell.IsOccupied = true;
        cell.OccupiedObject = obj;

        // Snap object to exact cell coordinate on the XZ plane.
        obj.transform.position = cell.WorldPosition;
        return true;
    }

    /// <summary>
    /// Removes the current object from a cell if any object is present.
    /// </summary>
    public void RemoveObject(int x, int y)
    {
        if (!IsInBounds(x, y))
        {
            return;
        }

        GridCell cell = gridCells[x, y];
        cell.IsOccupied = false;
        cell.OccupiedObject = null;
    }

    /// <summary>
    /// Returns the world-space origin position of a cell.
    /// </summary>
    public Vector3 GetWorldPosition(int x, int y)
    {
        if (!IsInBounds(x, y))
        {
            return Vector3.positiveInfinity;
        }

        return gridCells[x, y].WorldPosition;
    }

    /// <summary>
    /// Converts a world position into grid coordinates for mouse/placement logic.
    /// </summary>
    public bool TryGetGridPosition(Vector3 worldPos, out int x, out int y)
    {
        Vector3 local = worldPos - transform.position;
        x = Mathf.FloorToInt(local.x / cellSize);
        y = Mathf.FloorToInt(local.z / cellSize);

        return IsInBounds(x, y);
    }

    /// <summary>
    /// Gets a cell reference when coordinates are valid.
    /// </summary>
    public bool TryGetCell(int x, int y, out GridCell cell)
    {
        if (IsInBounds(x, y))
        {
            cell = gridCells[x, y];
            return true;
        }

        cell = null;
        return false;
    }

    /// <summary>
    /// Draws the full grid in editor and during play mode.
    /// Green = free, Red = occupied.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!drawDebugGrid)
        {
            return;
        }

        int drawWidth = Mathf.Max(1, width);
        int drawHeight = Mathf.Max(1, height);
        float drawCellSize = Mathf.Max(0.1f, cellSize);

        bool hasRuntimeGrid = gridCells != null &&
                              gridCells.GetLength(0) == width &&
                              gridCells.GetLength(1) == height;

        Vector3 origin = transform.position + new Vector3(0f, gizmoYOffset, 0f);

        for (int x = 0; x < drawWidth; x++)
        {
            for (int y = 0; y < drawHeight; y++)
            {
                Vector3 cellOrigin = origin + new Vector3(x * drawCellSize, 0f, y * drawCellSize);

                bool occupied = hasRuntimeGrid && gridCells[x, y] != null && gridCells[x, y].IsOccupied;
                Gizmos.color = occupied ? Color.red : Color.green;

                Vector3 a = cellOrigin;
                Vector3 b = cellOrigin + new Vector3(drawCellSize, 0f, 0f);
                Vector3 c = cellOrigin + new Vector3(drawCellSize, 0f, drawCellSize);
                Vector3 d = cellOrigin + new Vector3(0f, 0f, drawCellSize);

                Gizmos.DrawLine(a, b);
                Gizmos.DrawLine(b, c);
                Gizmos.DrawLine(c, d);
                Gizmos.DrawLine(d, a);
            }
        }
    }
}
