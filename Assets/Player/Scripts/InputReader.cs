using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class InputReader : MonoBehaviour, InputSystem_Actions.IPlayerActions
{
    private InputSystem_Actions controls;
    
    public Vector2 moveInput;
    public Vector2 lookInput;

    public bool sprintPressed;
    public event Action CrouchPressed;
    public event Action JumpPressed;
    private void OnEnable()
    {
        if (controls == null)
        {
            controls = new InputSystem_Actions();
            controls.Player.SetCallbacks(this);
        }

        controls.Player.Enable();
    }

   
    public void OnDisable()
    {
        controls.Player.Disable();
    }
    public void OnMove(InputAction.CallbackContext context)
    {
       moveInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
       if(!context.performed) return;
       CrouchPressed?.Invoke();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if(!context.performed) return;
        JumpPressed?.Invoke();
    }

    public void OnPrevious(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    public void OnNext(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            sprintPressed = true;
        }
        else if (context.canceled)
        {
            sprintPressed = false;
        }
    }
}
