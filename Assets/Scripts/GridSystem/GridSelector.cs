using UnityEngine;

/// <summary>
/// Detects the grid cell under the mouse cursor and manages a single tile highlight object.
/// </summary>
public class GridSelector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WarehouseGrid warehouseGrid;
    [SerializeField] private Camera worldCamera;
    [SerializeField] private GameObject highlightPrefab;

    [Header("Raycast")]
    [SerializeField] private LayerMask raycastMask = ~0;

    [Header("Highlight")]
    [SerializeField] private Color validColor = new Color(0.1f, 1f, 0.1f, 0.5f);
    [SerializeField] private Color invalidColor = new Color(1f, 0.1f, 0.1f, 0.5f);
    [SerializeField] private float highlightYOffset = 0.02f;

    private Plane fallbackPlane;
    private GameObject highlightInstance;
    private Renderer[] highlightRenderers;
    private readonly MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();

    private int currentX = -1;
    private int currentY = -1;
    private bool hasValidCell;

    public bool HasValidCell => hasValidCell;
    public int CurrentX => currentX;
    public int CurrentY => currentY;

    private void Awake()
    {
        if (worldCamera == null)
        {
            worldCamera = Camera.main;
        }

        float yLevel = warehouseGrid != null ? warehouseGrid.transform.position.y : 0f;
        fallbackPlane = new Plane(Vector3.up, new Vector3(0f, yLevel, 0f));

        if (highlightPrefab != null)
        {
            highlightInstance = Instantiate(highlightPrefab);
            highlightInstance.SetActive(false);
            highlightRenderers = highlightInstance.GetComponentsInChildren<Renderer>(true);
        }
    }

    private void Update()
    {
        UpdateCurrentCell();
        UpdateHighlight();
    }

    /// <summary>
    /// Raycasts from current mouse position using Camera.ScreenPointToRay and returns a grid coordinate when valid.
    /// </summary>
    public bool TryGetMouseGridPosition(out int x, out int y)
    {
        x = -1;
        y = -1;

        if (warehouseGrid == null || worldCamera == null)
        {
            return false;
        }

        Ray ray = worldCamera.ScreenPointToRay(Input.mousePosition);
        Vector3 worldPoint;

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, raycastMask, QueryTriggerInteraction.Ignore))
        {
            worldPoint = hit.point;
        }
        else if (fallbackPlane.Raycast(ray, out float enter))
        {
            worldPoint = ray.GetPoint(enter);
        }
        else
        {
            return false;
        }

        return warehouseGrid.TryGetGridPosition(worldPoint, out x, out y);
    }

    /// <summary>
    /// Gets center point in world space for the currently hovered tile.
    /// </summary>
    public bool TryGetCurrentCellCenter(out Vector3 center)
    {
        center = Vector3.zero;
        if (!hasValidCell || warehouseGrid == null)
        {
            return false;
        }

        center = warehouseGrid.GetCellCenterWorldPosition(currentX, currentY);
        return true;
    }

    private void UpdateCurrentCell()
    {
        hasValidCell = TryGetMouseGridPosition(out currentX, out currentY);
    }

    private void UpdateHighlight()
    {
        if (highlightInstance == null)
        {
            return;
        }

        if (!hasValidCell || warehouseGrid == null)
        {
            highlightInstance.SetActive(false);
            return;
        }

        bool isFree = warehouseGrid.IsCellFree(currentX, currentY);
        Vector3 center = warehouseGrid.GetCellCenterWorldPosition(currentX, currentY);
        center.y += highlightYOffset;

        highlightInstance.SetActive(true);
        highlightInstance.transform.position = center;

        Color targetColor = isFree ? validColor : invalidColor;
        propertyBlock.SetColor("_Color", targetColor);
        propertyBlock.SetColor("_BaseColor", targetColor);

        for (int i = 0; i < highlightRenderers.Length; i++)
        {
            highlightRenderers[i].SetPropertyBlock(propertyBlock);
        }
    }
}
