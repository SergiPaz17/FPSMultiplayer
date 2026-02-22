using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(FPController))]
public class FPPlayer : MonoBehaviour
{
    [Header("Components")] 
    [SerializeField] FPController fpController;

    private FPController test;
    
    #region Input Handling

    void OnMove(InputValue value)
    {
        fpController.MoveInput = value.Get<Vector2>();
    }

    void OnLook(InputValue value)
    {
        fpController.LookInput = value.Get<Vector2>();
    }

    void OnSprint(InputValue value)
    {
        fpController.SprintInput = value.isPressed;
    }

    void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            fpController.TryJump();
        }
    }
    
    

    #endregion

    #region Unity Methods

    private void OnValidate()
    {
        if (fpController == null) fpController = GetComponent<FPController>();
    }

    private void Start()
    {
         
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    #endregion
}
