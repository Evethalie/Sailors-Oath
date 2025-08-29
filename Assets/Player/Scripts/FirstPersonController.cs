using System;
using System.Collections;
using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    #region References
    private InputReader inputReader;
    private Camera playerCamera;
    private CharacterController controller;
    #endregion

    #region Properties
    public bool CanMove { get; private set; } = true;
    private bool IsSprinting => canSprint && inputReader.sprintPressed;
    private bool ShouldCrouch =>  !inCrouchAnimation && controller.isGrounded;
    #endregion

    #region Toggle Options
    [Header("Toggle Options")] 
    [SerializeField]private bool canSprint = true;
    [SerializeField] private bool canJump = true;
    [SerializeField] private bool canCrouch = true;
    [SerializeField] private bool canUseHeadbob = true;
    [SerializeField] private bool slideOnSlopes = true;
    #endregion

    #region Movement Parameters
    [Header("Movement Parameters")] 
    [SerializeField] private float walkSpeed = 3.0f;
    [SerializeField] private float crouchSpeed = 1.5f;
    [SerializeField] private float sprintSpeed = 6.0f;
    [SerializeField] private float slopeSpeed = 8.0f;
    #endregion

    #region Look Parameters
    [Header("Look Parameters")] 
    [SerializeField, Range(1, 10)] private float horizontalSensitivity = 2.0f;
    [SerializeField, Range(1, 10)] private float verticalSensitivity = 2.0f;
    [SerializeField, Range(1, 180)] private float upperLookLimit = 80.0f;
    [SerializeField, Range(1, 180)] private float lowerLookLimit = 80.0f;
    #endregion
    
    #region Jump Parameters
    [Header("Jump Parameters")] [SerializeField]
    private float jumpForce = 8.0f;
    [SerializeField] private float gravity = 30.0f;
    #endregion
    
    #region Crouch Parameters
    [Header("Crouch Parameters")] 
    [SerializeField] private float crouchHeight = 0.5f;
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float timeToCrouch = 0.25f;
    [SerializeField] private Vector3 crouchCenter = new Vector3(0, 0.5f, 0);
    [SerializeField] private Vector3 standingCenter = new Vector3(0, 0, 0);
    private bool isCrouching = false;
    private bool inCrouchAnimation = false;
    #endregion
    
    #region Headbob Parameters
    [Header("Headbob Parameters")]
    [SerializeField] private float walkBobSpeed = 14f;
    [SerializeField] private float walkBobAmount = 0.05f;
    [SerializeField] private float crouchBobSpeed = 8f;
    [SerializeField] private float crouchBobAmount = 0.025f;
    [SerializeField] private float sprintBobSpeed = 18f;
    [SerializeField] private float sprintBobAmount = 0.1f;
    private float defaultYPos = 0;
    private float timer;
    #endregion

    #region Sliding Parameters

    private Vector3 hitPointNormal;

    private bool IsSliding
    {
        get
        {
            if (controller.isGrounded && Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 2f))
            {
                hitPointNormal = hit.normal;
                return Vector3.Angle(hit.normal, Vector3.up) > controller.slopeLimit;
            }
            else
            {
                return false;
            }
        }
    }

    #endregion
    
    private Vector3 moveDirection;
    private Vector2 currentInput;

    private float rotationX = 0;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        inputReader = GetComponent<InputReader>();
        playerCamera = Camera.main;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        defaultYPos = playerCamera.transform.localPosition.y;
    }

    private void OnEnable()
    {
        inputReader.JumpPressed += HandleJump;
        inputReader.CrouchPressed += HandleCrouch;
    }

    private void OnDisable()
    {
        inputReader.JumpPressed -= HandleJump;
        inputReader.CrouchPressed -= HandleCrouch;
    }


    private void Update()
    {
        if (CanMove)
        {
            HandleMovementInput();
            HandleMouseLook();
            ApplyFinalMovements();
            if (canUseHeadbob)
            {
                HandleHeadbob();
            }
            
        }
    }

    private void HandleMovementInput()
    {
        currentInput = new Vector2((isCrouching ? crouchSpeed : IsSprinting ? sprintSpeed : walkSpeed) * inputReader.moveInput.y,
            (isCrouching ? crouchSpeed : IsSprinting ? sprintSpeed : walkSpeed) * inputReader.moveInput.x);
        float moveDirY = moveDirection.y;
        moveDirection = (transform.TransformDirection(Vector3.forward) * currentInput.x) +
                        (transform.TransformDirection(Vector3.right) * currentInput.y);
        moveDirection.y = moveDirY;
    }

    private void HandleMouseLook()
    {
        //looking up and down
        rotationX -= inputReader.lookInput.y * verticalSensitivity * Time.deltaTime;
        rotationX = Mathf.Clamp(rotationX, -upperLookLimit, lowerLookLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);

        //looking left and right
        float yaw = inputReader.lookInput.x * horizontalSensitivity * Time.deltaTime;
        transform.Rotate(Vector3.up * yaw, Space.World);

    }

    private void HandleJump()
    {
        if(!controller.isGrounded || !canJump) return;
        moveDirection.y = jumpForce;
    }

    private void HandleCrouch()
    {
       if(!ShouldCrouch) return;
       if(!canCrouch) return;
       StartCoroutine(CrouchStand());
    }

    private IEnumerator CrouchStand()
    {
        if(isCrouching && Physics.Raycast(playerCamera.transform.position, Vector3.up, 1f)) yield break;
        inCrouchAnimation = true;
        float timeElapsed = 0;
        float targetHeight = isCrouching ? standingHeight : crouchHeight;
        float currentHeight = controller.height;
        Vector3 targetCenter = isCrouching ? standingCenter : crouchCenter;
        Vector3 currentCenter = controller.center;
        isCrouching = !isCrouching;
        while (timeElapsed < timeToCrouch)
        {
            timeElapsed += Time.deltaTime;
            controller.height = Mathf.Lerp(currentHeight, targetHeight, timeElapsed / timeToCrouch);
            controller.center = Vector3.Lerp(currentCenter, targetCenter, timeElapsed / timeToCrouch);
            yield return null;
        }
        controller.height = targetHeight;
        controller.center = targetCenter;
      
        inCrouchAnimation = false;
    }

    private void HandleHeadbob()
    {
        if(!controller.isGrounded) return;
        if (!(Mathf.Abs(moveDirection.x) > 0.1f) || !(Mathf.Abs(moveDirection.z) > 0.1f)) return;
        timer += Time.deltaTime * (isCrouching ? crouchBobSpeed : IsSprinting ? sprintBobSpeed : walkBobSpeed);
        playerCamera.transform.localPosition = new Vector3(playerCamera.transform.localPosition.x,
            defaultYPos + Mathf.Sin(timer) *
            (isCrouching ? crouchBobAmount : IsSprinting ? sprintBobAmount : walkBobAmount),
            playerCamera.transform.localPosition.z);
    }

    private void ApplyFinalMovements()
    {
        if (!controller.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }
        
        if (controller.velocity.y < -1f && controller.isGrounded)
        {
            moveDirection.y = 0;
        }
        if (slideOnSlopes && IsSliding)
        {
            moveDirection += new Vector3(hitPointNormal.x, -hitPointNormal.y, hitPointNormal.z) * slopeSpeed;
        }
        controller.Move(moveDirection * Time.deltaTime);
    }


}
