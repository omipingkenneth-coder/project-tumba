using System.Collections;
using UnityEngine;
using Mirror;



[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(NetworkIdentity))]
public class NetworkPlayerController : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 2f;
    public float runSpeed = 5f;
    public float rotationSpeed = 10f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;

    [Header("References")]
    public Animation animationComponent; // legacy Animation
    public Transform cameraTransform;

    [Header("Joystick / Mobile")]
    public bool useJoystick = true;
    private Vector2 joystickInput = Vector2.zero;
    private bool isMobileRunning = false;

    private CharacterController controller;
    private Vector3 velocity;

    [SyncVar(hook = nameof(OnAnimationStateChanged))]
    public AnimState currentAnim = AnimState.Idle;

    [SyncVar] public bool isBusy = false; // locks movement/other actions
    private AnimState lastSentAnim;

    [Header("Pick & Throw Settings")]
    public Transform holdPoint;
    public LayerMask pickableLayer;
    public float pickUpRange = 3f;
    public float throwForce = 10f;
    public float pickAnimDuration = 1.0f;
    public float throwAnimDuration = 0.6f;

    private GameObject heldObject;

    #region Initialization
    void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (animationComponent == null)
            animationComponent = GetComponentInChildren<Animation>();
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    public override void OnStartLocalPlayer()
    {
        controller.enabled = true;
        Debug.Log("[LocalPlayer] NetworkPlayerController started for local player");
    }
    #endregion

    void Update()
    {
        if (!isLocalPlayer) return;

        if (isBusy || currentAnim == AnimState.Pick || currentAnim == AnimState.Throw) return;

        HandleMovement();
        HandleJumpInput();
    }

    #region Mobile / Joystick Input
    public void OnJoystickInput(Vector2 input)
    {
        joystickInput = input;
        Debug.Log($"[Input] Joystick input: {input}");
    }

    public void OnRunPressed()
    {
        isMobileRunning = true;
        Debug.Log("[Input] Run pressed (mobile)");
    }

    public void OnRunReleased()
    {
        isMobileRunning = false;
        Debug.Log("[Input] Run released (mobile)");
    }

    public void MobilePickup()
    {
        if (!isBusy && heldObject == null)
        {
            Debug.Log("[Input] Mobile pickup pressed");
            LocalPickObject();
        }
    }

    public void MobileThrow()
    {
        if (!isBusy && heldObject != null)
        {
            Debug.Log("[Input] Mobile throw pressed");
            LocalThrowObject();
        }
    }
    #endregion

    #region Movement
    void HandleMovement()
    {
        float inputX = useJoystick ? joystickInput.x : Input.GetAxis("Horizontal");
        float inputZ = useJoystick ? joystickInput.y : Input.GetAxis("Vertical");

        bool isRunning = Input.GetButton("Fire3") || isMobileRunning;
        float speed = isRunning ? runSpeed : walkSpeed;

        Vector3 inputDir = new Vector3(inputX, 0f, inputZ);
        Vector3 moveDir = Vector3.zero;

        if (inputDir.sqrMagnitude > 0.01f)
        {
            if (cameraTransform != null)
            {
                Vector3 camForward = cameraTransform.forward;
                Vector3 camRight = cameraTransform.right;
                camForward.y = 0; camRight.y = 0;
                camForward.Normalize(); camRight.Normalize();
                moveDir = (camForward * inputZ + camRight * inputX).normalized;
            }
            else moveDir = inputDir.normalized;

            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDir), rotationSpeed * Time.deltaTime);
            controller.Move(moveDir * speed * Time.deltaTime);

            PlayAnimationLocally(isRunning ? AnimState.Run : AnimState.Walk);
            CmdSetAnimationStateSafe(isRunning ? AnimState.Run : AnimState.Walk);

            Debug.Log($"[Movement] Moving {(isRunning ? "running" : "walking")} dir {moveDir}");
        }
        else
        {
            PlayAnimationLocally(AnimState.Idle);
            CmdSetAnimationStateSafe(AnimState.Idle);
        }

        // Gravity
        if (controller.isGrounded && velocity.y < 0) velocity.y = -2f;
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleJumpInput()
    {
        if (!controller.isGrounded) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            PlayAnimationLocally(AnimState.Jump);
            CmdSetAnimationStateSafe(AnimState.Jump);
            Debug.Log("[Action] Jump triggered");
        }
    }
    #endregion

    #region Animation
    private void PlayAnimationLocally(AnimState state)
    {
        if (animationComponent == null) return;

        string clipName = state switch
        {
            AnimState.Walk => "Walking",
            AnimState.Run => "RunningAnimation",
            AnimState.Jump => "Jump",
            AnimState.Pick => "Pick Object",
            AnimState.Throw => "Throw Object",
            _ => "Idle"
        };

        if (animationComponent[clipName] != null)
        {
            animationComponent.Stop();
            animationComponent.CrossFade(clipName, 0.1f);
            Debug.Log($"[Animation] Local animation: {clipName}");
        }
        else Debug.LogWarning($"[Animation] Clip '{clipName}' not found on {animationComponent.name}");
    }

    [Command]
    void CmdSetAnimationStateSafe(AnimState state)
    {
        if (state == lastSentAnim) return;
        lastSentAnim = state;
        currentAnim = state;
        Debug.Log($"[Network] Animation state set on server: {state}");
    }

    void OnAnimationStateChanged(AnimState oldState, AnimState newState)
    {
        if (animationComponent == null || isLocalPlayer) return;

        Debug.Log($"[Network] Animation state changed: {oldState} -> {newState}");
        PlayAnimationLocally(newState);
    }
    #endregion

    #region Pick & Throw
    private void LocalPickObject()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, pickUpRange, pickableLayer)) return;

        isBusy = true;
        PlayAnimationLocally(AnimState.Pick);
        currentAnim = AnimState.Pick;
        Debug.Log($"[Action] Local pick object triggered on {hit.collider.name}");
        CmdPickObject(hit.collider.gameObject);
    }

    private void LocalThrowObject()
    {
        if (heldObject == null) return;

        isBusy = true;
        PlayAnimationLocally(AnimState.Throw);
        currentAnim = AnimState.Throw;
        Debug.Log("[Action] Local throw object triggered");
        CmdThrowObject();
    }

    [Command]
    void CmdPickObject(GameObject target)
    {
        if (target == null) return;

        RpcPlayAnimation(AnimState.Pick);
        StartCoroutine(ServerPickRoutine(target));
        Debug.Log($"[Network] CmdPickObject called on server for {target.name}");
    }

    IEnumerator ServerPickRoutine(GameObject target)
    {
        yield return new WaitForSeconds(pickAnimDuration);

        if (target != null)
        {
            Rigidbody rb = target.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            target.transform.SetParent(holdPoint);
            target.transform.localPosition = Vector3.zero;
            target.transform.localRotation = Quaternion.identity;

            heldObject = target;
            Debug.Log($"[Network] Object {target.name} picked up and parented to hold point");
        }

        currentAnim = AnimState.Idle;
        isBusy = false;
        RpcPlayAnimation(AnimState.Idle);
        Debug.Log("[Network] Pick animation finished, returning to idle");
    }

    [Command]
    void CmdThrowObject()
    {
        if (heldObject == null) return;

        RpcPlayAnimation(AnimState.Throw);
        StartCoroutine(ServerThrowRoutine());
        Debug.Log("[Network] CmdThrowObject called on server");
    }

    IEnumerator ServerThrowRoutine()
    {
        yield return new WaitForSeconds(throwAnimDuration);

        if (heldObject != null)
        {
            heldObject.transform.SetParent(null);
            Rigidbody rb = heldObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.AddForce(transform.forward * throwForce, ForceMode.VelocityChange);
            }

            Debug.Log($"[Network] Object {heldObject.name} thrown with force {throwForce}");
            heldObject = null;
        }

        currentAnim = AnimState.Idle;
        isBusy = false;
        RpcPlayAnimation(AnimState.Idle);
        Debug.Log("[Network] Throw animation finished, returning to idle");
    }

    [ClientRpc]
    void RpcPlayAnimation(AnimState state)
    {
        if (animationComponent == null || isLocalPlayer) return;
        PlayAnimationLocally(state);
        Debug.Log($"[Network] RPC Animation played on client: {state}");
    }
    #endregion
}
