using UnityEngine;

/// <summary>
/// Factorio/city-builder style isometric camera controller.
/// Attach to a Camera Rig object and keep the Main Camera as a child.
/// The rig handles planar movement + yaw rotation, while the child camera keeps a fixed pitch.
/// </summary>
public class IsometricCameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraTransform;

    [Header("Movement")]
    [SerializeField, Min(0.1f)] private float moveSpeed = 25f;
    [SerializeField, Min(0.01f)] private float movementSmoothness = 0.12f;

    [Header("Zoom")]
    [SerializeField, Min(1f)] private float minZoom = 8f;
    [SerializeField, Min(1f)] private float maxZoom = 40f;
    [SerializeField, Min(0.01f)] private float zoomStep = 5f;
    [SerializeField, Min(0.01f)] private float zoomSmoothness = 0.12f;

    [Header("Rotation")]
    [SerializeField, Min(1f)] private float keyboardRotationSpeed = 90f;
    [SerializeField, Min(1f)] private float mouseRotationSpeed = 180f;
    [SerializeField] private KeyCode rotateLeftKey = KeyCode.Q;
    [SerializeField] private KeyCode rotateRightKey = KeyCode.E;

    [Header("Isometric Angle")]
    [Tooltip("Fixed vertical look angle. Typical isometric values are ~35-45 degrees.")]
    [SerializeField, Range(35f, 45f)] private float pitchAngle = 40f;

    [Header("Edge Scrolling")]
    [SerializeField] private bool enableEdgeScrolling = true;
    [SerializeField, Min(1f)] private float edgeSize = 16f;
    [SerializeField, Range(0f, 1f)] private float edgeMoveContribution = 1f;

    private Vector3 _targetRigPosition;
    private Vector3 _rigVelocity;

    private float _targetDistance;
    private float _smoothedDistance;
    private float _zoomVelocity;

    private float _targetYaw;

    private void Reset()
    {
        // Auto-wire camera reference when script is added.
        if (cameraTransform == null && transform.childCount > 0)
        {
            cameraTransform = transform.GetChild(0);
        }
    }

    private void Awake()
    {
        if (cameraTransform == null)
        {
            Camera mainCam = GetComponentInChildren<Camera>();
            if (mainCam != null)
            {
                cameraTransform = mainCam.transform;
            }
        }

        if (cameraTransform == null)
        {
            Debug.LogError("IsometricCameraController requires a child camera reference.", this);
            enabled = false;
            return;
        }

        _targetRigPosition = transform.position;
        _targetYaw = transform.eulerAngles.y;

        // Infer current zoom distance from child local position.
        float initialDistance = cameraTransform.localPosition.magnitude;
        _targetDistance = Mathf.Clamp(initialDistance > 0.001f ? initialDistance : minZoom, minZoom, maxZoom);
        _smoothedDistance = _targetDistance;

        ApplyCameraLocalTransformImmediate();
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        Vector2 planarInput = ReadPlanarInput();
        ApplyMovementInput(planarInput, dt);

        float zoomInput = Input.mouseScrollDelta.y;
        ApplyZoomInput(zoomInput);

        float rotateInput = ReadRotationInput();
        ApplyRotationInput(rotateInput, dt);

        ApplySmoothing(dt);
    }

    private Vector2 ReadPlanarInput()
    {
        // Keyboard input supports both WASD and arrow keys via Unity input axes.
        Vector2 keyboard = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (!enableEdgeScrolling)
        {
            return Vector2.ClampMagnitude(keyboard, 1f);
        }

        // Edge scrolling emits a normalized direction based on mouse proximity to screen edges.
        Vector2 edge = Vector2.zero;
        Vector3 mouse = Input.mousePosition;

        if (mouse.x >= 0f && mouse.y >= 0f && mouse.x <= Screen.width && mouse.y <= Screen.height)
        {
            if (mouse.x <= edgeSize) edge.x = -1f;
            else if (mouse.x >= Screen.width - edgeSize) edge.x = 1f;

            if (mouse.y <= edgeSize) edge.y = -1f;
            else if (mouse.y >= Screen.height - edgeSize) edge.y = 1f;
        }

        Vector2 combined = keyboard + edge * edgeMoveContribution;
        return Vector2.ClampMagnitude(combined, 1f);
    }

    private float ReadRotationInput()
    {
        float keyboardYaw = 0f;
        if (Input.GetKey(rotateLeftKey)) keyboardYaw -= 1f;
        if (Input.GetKey(rotateRightKey)) keyboardYaw += 1f;

        // Optional middle mouse drag for intuitive orbiting around Y axis.
        float mouseYaw = 0f;
        if (Input.GetMouseButton(2))
        {
            mouseYaw = Input.GetAxis("Mouse X") * mouseRotationSpeed / Mathf.Max(1f, keyboardRotationSpeed);
        }

        return keyboardYaw + mouseYaw;
    }

    private void ApplyMovementInput(Vector2 planarInput, float dt)
    {
        // Keep movement parallel to the grid plane (XZ): ignore camera pitch by flattening the rig axes.
        Vector3 forward = transform.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 right = transform.right;
        right.y = 0f;
        right.Normalize();

        Vector3 moveDirection = (right * planarInput.x + forward * planarInput.y);
        if (moveDirection.sqrMagnitude > 1f)
        {
            moveDirection.Normalize();
        }

        _targetRigPosition += moveDirection * (moveSpeed * dt);
    }

    private void ApplyZoomInput(float scrollInput)
    {
        if (Mathf.Abs(scrollInput) < 0.001f)
        {
            return;
        }

        _targetDistance = Mathf.Clamp(_targetDistance - scrollInput * zoomStep, minZoom, maxZoom);
    }

    private void ApplyRotationInput(float rotateInput, float dt)
    {
        if (Mathf.Abs(rotateInput) < 0.001f)
        {
            return;
        }

        _targetYaw += rotateInput * keyboardRotationSpeed * dt;
    }

    private void ApplySmoothing(float dt)
    {
        // Smoothly damp rig translation for stable panning across large maps.
        transform.position = Vector3.SmoothDamp(
            transform.position,
            _targetRigPosition,
            ref _rigVelocity,
            movementSmoothness,
            Mathf.Infinity,
            dt);

        // Smooth yaw rotation around global up axis; keeps camera level and grid alignment intact.
        float currentYaw = transform.eulerAngles.y;
        float smoothedYaw = Mathf.LerpAngle(currentYaw, _targetYaw, 1f - Mathf.Exp(-dt / movementSmoothness));
        transform.rotation = Quaternion.Euler(0f, smoothedYaw, 0f);

        // Smooth zoom by changing camera local offset (distance), not FOV.
        _smoothedDistance = Mathf.SmoothDamp(_smoothedDistance, _targetDistance, ref _zoomVelocity, zoomSmoothness, Mathf.Infinity, dt);

        ApplyCameraLocalTransformImmediate();
    }

    private void ApplyCameraLocalTransformImmediate()
    {
        // Camera local rotation remains fixed isometric pitch: no roll, no changing tilt during gameplay.
        Quaternion localRotation = Quaternion.Euler(pitchAngle, 0f, 0f);
        cameraTransform.localRotation = localRotation;

        // Place camera on the local -Z axis at the selected distance, then pitch it downward.
        // This keeps zoom behavior stable and always centered around the rig pivot.
        Vector3 localOffset = localRotation * (Vector3.back * _smoothedDistance);
        cameraTransform.localPosition = localOffset;
    }
}
