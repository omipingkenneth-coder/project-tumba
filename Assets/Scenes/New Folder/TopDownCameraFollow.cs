using UnityEngine;

public class TopDownCameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;
    public float topdownRotationSpeed = 360f; // Degrees per second    // The player or object to follow

    [Header("Camera Settings")]
    public float height = 10f;     // Height above the target (for top-down)
    public float distance = 5f;    // Distance behind the target (for top-down)
    public float followSpeed = 5f; // How smoothly the camera follows
    public float rotationAngle = 45f; // Vertical tilt angle (X axis)

    [Header("Auto Rotation Settings")]
    public float idleDelay = 2f;          // Seconds before auto-rotate starts
    public float autoRotateSpeed = 20f;   // Degrees per second
    public bool enableAutoRotate = true;  // Toggle for feature
    public float rotationDelay = 1f;      // Delay before camera rotates towards target's facing direction

    [Header("First-Person Settings")]
    public float forwardOffset = 0.3f;
    public bool enableFirstPerson = false;  // Toggle for first-person mode
    public float firstPersonHeight = 1.7f;  // The height of the camera in first-person mode (typically eye level)
    public float firstPersonFov = 90f;      // Field of view for first-person mode
    public float firstPersonSensitivity = 2f; // Camera rotation sensitivity in first-person mode
    private float yaw;   // Horizontal rotation
    private float pitch; // Vertical rotation

    [Header("First-Person Rotation Limits")]
    public float minVerticalAngle = -80f; // Minimum vertical look angle
    public float maxVerticalAngle = 80f;  // Maximum vertical look angle
    public float horizontalSensitivity = 2f; // Horizontal mouse sensitivity
    public float verticalSensitivity = 2f;   // Vertical mouse sensitivity

    private Vector3 lastTargetPosition;
    private float idleTimer = 0f;
    private bool isIdle = false;
    private bool isWaitingForRotation = false; // Flag to wait for rotation delay
    private float currentYaw = 0f; // Y rotation around target
    private float rotationDelayTimer = 0f; // Timer for custom rotation delay

    [Header("Joystick Settings")]
    public float joystickSensitivity = 100f;
    public Vector2 cameraJoystickInput; // Assign from UI joystick script
    private Vector2 currentRotation;
    private Vector2 rotationSmoothVelocity;
    [Header("Smoothness")]
    public float rotationSmoothTime = 0.05f;
    public float transitionSpeed = 5f;
    [Header("Joystick Settings")]
    public bool invertY = true; // Set in Inspector

    private Camera cam; // Camera component

    void Start()
    {
        if (target != null)
            lastTargetPosition = target.position;

        cam = GetComponent<Camera>(); // Get the camera component
    }

    void Update()
    {
        HandleCameraToggle();
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        HandleIdleCheck();
        UpdateCameraPosition();
    }

    private void HandleIdleCheck()
    {
        // Detect if target moved
        if (Vector3.Distance(target.position, lastTargetPosition) > 0.05f)
        {
            // Player moved — reset idle timer and stop waiting for rotation
            idleTimer = 0f;
            isIdle = false;
            isWaitingForRotation = false; // Reset rotation waiting
            rotationDelayTimer = 0f; // Reset delay timer
            lastTargetPosition = target.position;
        }
        else
        {
            // Player stationary
            idleTimer += Time.deltaTime;
            if (enableAutoRotate && idleTimer >= idleDelay)
                isIdle = true;
        }

        // If the player is idle and auto-rotate is enabled, check if we should wait before rotating
        if (isIdle && enableAutoRotate && !isWaitingForRotation)
        {
            rotationDelayTimer += Time.deltaTime;
            if (rotationDelayTimer >= rotationDelay)
            {
                isWaitingForRotation = true; // Start rotating after delay
            }
        }
    }

    private void UpdateCameraPosition()
    {
        if (enableFirstPerson)
        {
            // First-person mode
            FirstPersonMode();
        }
        else
        {
            // Top-down camera mode
            TopDownMode();
        }
    }

    private void TopDownMode()
    {
        // If idle, rotate automatically around the target after the delay
        if (isIdle && isWaitingForRotation)
        {
            // Rotate the camera yaw based on idle time
            currentYaw += autoRotateSpeed * Time.deltaTime;
        }

        // Calculate desired camera offset based on currentYaw rotation
        Quaternion yawRotation = Quaternion.Euler(rotationAngle, currentYaw, 0);
        Vector3 offset = yawRotation * new Vector3(0, 0, -distance);
        Vector3 desiredPos = target.position + offset + Vector3.up * height;

        // Smooth camera movement
        transform.position = Vector3.Lerp(transform.position, desiredPos, followSpeed * Time.deltaTime);

        // Ensure camera always looks at the target's position
        transform.LookAt(target.position + Vector3.up * (height * 0.2f)); // Slight look offset for better framing

        // Optionally, match the camera rotation to the target's rotation after the delay
        if (isWaitingForRotation)
        {
            float targetYaw = target.eulerAngles.y; // The target's facing direction (yaw)
            currentYaw = Mathf.LerpAngle(currentYaw, targetYaw, Time.deltaTime * autoRotateSpeed);
        }
    }

    private void FirstPersonMode()
    {
        // --- ROTATION FROM JOYSTICK ---
        float inputX = cameraJoystickInput.x * joystickSensitivity * Time.deltaTime;
        float inputY = cameraJoystickInput.y * joystickSensitivity * Time.deltaTime;

        yaw += inputX;
        pitch += invertY ? -inputY : inputY; // Fix vertical inversion
        pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);

        Vector2 targetRotation = new Vector2(pitch, yaw);
        currentRotation = Vector2.SmoothDamp(currentRotation, targetRotation, ref rotationSmoothVelocity, rotationSmoothTime);

        // --- CAMERA POSITION WITH FORWARD OFFSET ---
        Vector3 forward = target.forward * forwardOffset;
        Vector3 targetPos = target.position + Vector3.up * firstPersonHeight + forward;
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * transitionSpeed);

        // --- CAMERA ROTATION ---
        transform.rotation = Quaternion.Euler(currentRotation.x, currentRotation.y, 0f);

        // --- ROTATE PLAYER HORIZONTALLY ---
        target.rotation = Quaternion.Euler(0f, yaw, 0f);

        // --- FOV ---
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, firstPersonFov, Time.deltaTime * 5f);
    }

    /// <summary>
    /// Rotates the target transform based on joystick input (for Top-Down mode).
    /// </summary>
    private void RotateTargetFromJoystick(Vector2 joystickInput)
    {
        if (target == null || enableFirstPerson || joystickInput.sqrMagnitude < 0.01f)
            return;

        // Convert joystick input to rotation angle
        float targetAngle = Mathf.Atan2(joystickInput.x, joystickInput.y) * Mathf.Rad2Deg;

        // Smoothly rotate target based on speed
        float rotationSpeed = topdownRotationSpeed; // Custom rotation speed
        Quaternion desiredRotation = Quaternion.Euler(0f, targetAngle, 0f);

        target.rotation = Quaternion.RotateTowards(
            target.rotation,
            desiredRotation,
            rotationSpeed * Time.deltaTime
        );
    }


    // --- This method will be called by the joystick output event ---
    public void ReceiveJoystickInput(Vector2 input)
    {
        cameraJoystickInput = input;
        if (!enableFirstPerson)
        {
            RotateTargetFromJoystick(input);
        }
    }

    private void HandleCameraToggle()
    {
        if (Input.GetKeyDown(KeyCode.F)) // Toggle camera mode with the 'F' key
        {
            enableFirstPerson = !enableFirstPerson;
            if (!enableFirstPerson)
            {
                // Reset FOV when switching back to top-down mode
                cam.fieldOfView = 60f;
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (target != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, target.position);
        }
    }
#endif
}
