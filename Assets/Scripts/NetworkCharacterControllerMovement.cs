using System.Collections;
using UnityEngine;
using Mirror;

public enum AnimState
{
    Idle,
    Walk,
    Run,
    Jump,
    Pick,
    Throw,
    PutCan
}

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(NetworkIdentity))]
public class NetworkCharacterControllerMovement : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 2f;
    public float runSpeed = 5f;
    public float rotationSpeed = 10f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;

    [Header("References")]
    public Animation animationComponent;
    public Transform cameraTransform;

    [Header("Mobile / Joystick")]
    public bool useJoystick = true;
    private Vector2 joystickInput = Vector2.zero;
    private bool isMobileRunning = false;

    private CharacterController controller;
    private Vector3 velocity;

    [SyncVar(hook = nameof(OnAnimationStateChanged))]
    public AnimState currentAnim = AnimState.Idle;

    private AnimState lastSentAnim;

    [SyncVar] public bool isPickingUp = false;
    public bool isThrowing = false;

    // -----------------------------
    // Initialization
    // -----------------------------
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
        controller.enabled = true; // Only the local player moves
    }

    public override void OnStopLocalPlayer()
    {
        controller.enabled = false;
    }

    void Update()
    {
        if (!isLocalPlayer) return;
        // Local Player uses Pick + Throw
        // Non-player uses Pick + PutCan
        bool blockMovement =
            gameObject.CompareTag("Player")
                ? (currentAnim == AnimState.Pick || currentAnim == AnimState.Throw)
                : (currentAnim == AnimState.Pick || currentAnim == AnimState.PutCan);

        if (blockMovement) return;

        HandleMovement();
        HandleJumpInput();

    }

    // -----------------------------
    // Input
    // -----------------------------
    public void OnJoystickInput(Vector2 input)
    {
        joystickInput = input;
    }

    public void MobileRunInput()
    {
        isMobileRunning = !isMobileRunning;
    }

    // -----------------------------
    // Pick / Throw Animations
    // -----------------------------
    public IEnumerator PlayPickAnimation()
    {
        if (!isLocalPlayer || animationComponent == null) yield break;

        animationComponent.Stop();
        animationComponent.CrossFade("Pick Object", 0.1f);
        float length = GetAnimationClipLength("Pick Object", 1.0f);

        CmdNotifyPickAnimation(length);
        yield return new WaitForSeconds(length);
    }

    public IEnumerator PlayThrowAnimation()
    {
        if (!isLocalPlayer || animationComponent == null) yield break;

        animationComponent.Stop();
        animationComponent.CrossFade("Throw Object", 0.1f);
        float length = GetAnimationClipLength("Throw Object", 0.6f);

        CmdNotifyThrowAnimation(length);
        yield return new WaitForSeconds(length);
    }

    [Command]
    void CmdNotifyPickAnimation(float duration)
    {
        StartCoroutine(ServerAnimRoutine(AnimState.Pick, duration));
    }

    [Command]
    void CmdNotifyThrowAnimation(float duration)
    {
        StartCoroutine(ServerAnimRoutine(AnimState.Throw, duration));
    }

    IEnumerator ServerAnimRoutine(AnimState state, float duration)
    {
        currentAnim = state;
        if (state == AnimState.Pick) isPickingUp = true;
        yield return new WaitForSeconds(duration);
        if (state == AnimState.Pick) isPickingUp = false;
        currentAnim = AnimState.Idle;
    }

    // -----------------------------
    // Movement & Local Gravity
    // -----------------------------
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
            // Camera-relative movement
            if (cameraTransform != null)
            {
                Vector3 camForward = cameraTransform.forward;
                Vector3 camRight = cameraTransform.right;
                camForward.y = 0;
                camRight.y = 0;
                camForward.Normalize();
                camRight.Normalize();
                moveDir = (camForward * inputZ + camRight * inputX).normalized;
            }
            else
            {
                moveDir = inputDir.normalized;
            }

            // Rotate toward movement direction
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);

            // Move
            controller.Move(moveDir * speed * Time.deltaTime);

            // Local immediate animation
            if (animationComponent != null)
            {
                animationComponent.CrossFade(isRunning ? "RunningAnimation" : "Walking", 0.15f);
            }

            CmdSetAnimationStateSafe(isRunning ? AnimState.Run : AnimState.Walk);
        }
        else
        {
            if (animationComponent != null)
                animationComponent.CrossFade("Idle", 0.15f);

            CmdSetAnimationStateSafe(AnimState.Idle);
        }

        // Gravity applied locally
        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleJumpInput()
    {
        if (!isLocalPlayer || !controller.isGrounded) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            if (animationComponent != null)
                animationComponent.CrossFade("Jump", 0.15f);

            CmdSetAnimationStateSafe(AnimState.Jump);
        }
    }

    // -----------------------------
    // Networked animation helpers
    // -----------------------------
    [Command]
    void CmdSetAnimationStateSafe(AnimState state)
    {
        if (state == lastSentAnim) return;
        lastSentAnim = state;
        currentAnim = state;
    }

    void OnAnimationStateChanged(AnimState oldState, AnimState newState)
    {
        if (animationComponent == null) return;

        switch (newState)
        {
            case AnimState.Walk: animationComponent.CrossFade("Walking", 0.15f); break;
            case AnimState.Run: animationComponent.CrossFade("RunningAnimation", 0.15f); break;
            case AnimState.Jump: animationComponent.CrossFade("Jump", 0.15f); break;
            case AnimState.Pick: animationComponent.CrossFade("Pick Object", 0.1f); break;
            case AnimState.Throw: animationComponent.CrossFade("Throw Object", 0.1f); break;
            default: animationComponent.CrossFade("Idle", 0.15f); break;
        }
    }

    // -----------------------------
    // Helpers
    // -----------------------------
    float GetAnimationClipLength(string clipName, float fallback)
    {
        if (animationComponent == null) return fallback;
        if (animationComponent[clipName] != null)
            return animationComponent[clipName].length;
        return fallback;
    }
}
