using UnityEngine;

/// <summary>
/// Handles ghost preview, click placement, and cancellation for warehouse rack placement.
/// </summary>
public class BuildingPlacementController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WarehouseGrid warehouseGrid;
    [SerializeField] private GridSelector gridSelector;

    [Header("Prefabs / Materials")]
    [SerializeField] private GameObject rackPrefab;
    [SerializeField] private Material previewMaterial;

    [Header("Behavior")]
    [SerializeField] private bool startInPlacementMode = true;
    [SerializeField] private bool keepPlacementModeAfterBuild = true;
    [SerializeField] private Color previewValidColor = new Color(0.1f, 1f, 0.1f, 0.45f);
    [SerializeField] private Color previewInvalidColor = new Color(1f, 0.1f, 0.1f, 0.45f);

    private GameObject previewInstance;
    private Renderer[] previewRenderers;
    private readonly MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();

    private bool placementModeActive;

    private void Start()
    {
        if (startInPlacementMode)
        {
            EnterPlacementMode();
        }
    }

    private void Update()
    {
        if (!placementModeActive || warehouseGrid == null || gridSelector == null)
        {
            return;
        }

        UpdatePreviewTransformAndColor();

        if (Input.GetMouseButtonDown(0))
        {
            TryPlaceSelectedCell();
        }
        else if (Input.GetMouseButtonDown(1))
        {
            CancelPlacementMode();
        }
    }

    public void EnterPlacementMode()
    {
        if (placementModeActive)
        {
            return;
        }

        placementModeActive = true;
        SpawnPreviewIfNeeded();
    }

    public void CancelPlacementMode()
    {
        placementModeActive = false;

        if (previewInstance != null)
        {
            Destroy(previewInstance);
            previewInstance = null;
            previewRenderers = null;
        }
    }

    private void SpawnPreviewIfNeeded()
    {
        if (rackPrefab == null || previewInstance != null)
        {
            return;
        }

        previewInstance = Instantiate(rackPrefab);
        previewRenderers = previewInstance.GetComponentsInChildren<Renderer>(true);

        Collider[] colliders = previewInstance.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }

        if (previewMaterial != null)
        {
            for (int i = 0; i < previewRenderers.Length; i++)
            {
                Material[] mats = previewRenderers[i].sharedMaterials;
                for (int j = 0; j < mats.Length; j++)
                {
                    mats[j] = previewMaterial;
                }

                previewRenderers[i].sharedMaterials = mats;
            }
        }
    }

    private void UpdatePreviewTransformAndColor()
    {
        if (previewInstance == null)
        {
            SpawnPreviewIfNeeded();
        }

        if (previewInstance == null)
        {
            return;
        }

        if (!gridSelector.TryGetCurrentCellCenter(out Vector3 center))
        {
            previewInstance.SetActive(false);
            return;
        }

        previewInstance.SetActive(true);
        previewInstance.transform.position = center;

        bool canPlace = warehouseGrid.IsCellFree(gridSelector.CurrentX, gridSelector.CurrentY);
        SetPreviewColor(canPlace ? previewValidColor : previewInvalidColor);
    }

    private void SetPreviewColor(Color color)
    {
        propertyBlock.SetColor("_Color", color);
        propertyBlock.SetColor("_BaseColor", color);

        for (int i = 0; i < previewRenderers.Length; i++)
        {
            previewRenderers[i].SetPropertyBlock(propertyBlock);
        }
    }

    private void TryPlaceSelectedCell()
    {
        if (!gridSelector.HasValidCell || rackPrefab == null)
        {
            return;
        }

        bool placed = warehouseGrid.PlaceObject(rackPrefab, gridSelector.CurrentX, gridSelector.CurrentY);
        if (placed && !keepPlacementModeAfterBuild)
        {
            CancelPlacementMode();
        }
    }
}
