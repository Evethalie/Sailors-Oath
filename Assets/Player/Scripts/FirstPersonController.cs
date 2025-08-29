using System;
using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    public bool CanMove { get; private set; } = true;
    [SerializeField] private float walkSpeed = 3;
    [SerializeField] private float gravity = 30.0f;
    
    [Header("Look Parameters")]
    [SerializeField, Range(1,10)]private float horizontalSensitivity = 2.0f;
    [SerializeField, Range(1,10)]private float verticalSensitivity = 2.0f;
    [SerializeField, Range(1,180)]private float upperLookLimit = 80.0f;
    [SerializeField, Range(1,180)]private float lowerLookLimit = 80.0f;
    
    private Camera playerCamera;
    private CharacterController controller;
    private InputReader inputReader;
    
    private Vector3 moveDirection;
    private Vector2 currentInput;
    
    private float rotationX = 0;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = Camera.main;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (CanMove)
        {
            HandleMovementInput();
            HandleMouseLook();
            ApplyFinalMovements();
        }
    }
    private void HandleMovementInput()
    {
        currentInput = new Vector2(walkSpeed * inputReader.moveInput.y, walkSpeed * inputReader.moveInput.x);
        float moveDirY = moveDirection.y;
        moveDirection = (transform.TransformDirection(Vector3.forward) * currentInput.x) + (transform.TransformDirection(Vector3.right) * currentInput.y);
        moveDirection.y = moveDirY;
    }
    private void HandleMouseLook()
    {
      
    }

    private void ApplyFinalMovements()
    {
        if (!controller.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        controller.Move(moveDirection * Time.deltaTime);
    }


}
