using System;
using System.Numerics;
using UnityEngine;
using Unity.Cinemachine;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;


[RequireComponent(typeof(CharacterController))]
public class FPController : MonoBehaviour
{
    [Header("Movement Parameters")]
    public float MaxSpeed => SprintInput ? SprintSpeed : WalkSpeed;
    public float Acceleration = 15f;
    
    [SerializeField] float WalkSpeed = 3.5f;
    [SerializeField] float SprintSpeed = 8f;

    
    [Tooltip("This is how high the character can jump")]
    [SerializeField] float JumpHeight = 2f;


    public bool Sprinting
    {
        get
        {
            return SprintInput && CurrentSpeed > 0.1f;
        }
    }
    

    [Header("Looking Parameters")]
    public Vector2 LookSensitivity = new Vector2(0.1f, 0.1f);
    
    public float PitchLimit = 85f;

    [SerializeField] private float currentPitch = 0f;

    public float CurrentPitch
    {
        get => currentPitch;
        
        set
        {
            currentPitch = Math.Clamp(value, -PitchLimit, PitchLimit);
        }
    }

    [Header("Camera Parameters")] 
    [SerializeField] private float CameraNormalFOV = 60f;

    [SerializeField] private float CameraSprintFOV = 80f;
    [SerializeField] private float CameraFOVSmoothing = 1f;

    float TargetCameraPOV
    {
        get
        {
            return Sprinting ? CameraSprintFOV : CameraNormalFOV;
        }
    }


    [Header("Physics Parameters")] 
    [SerializeField] private float GravityScale = 3f;

    public float VerticalVelocity = 0f;
    
    public Vector3 CurrentVelocity { get; private set; }
    public float CurrentSpeed { get; private set; }
    
    public bool IsGrounded => _characterController.isGrounded;

    [Header("Inputs")] 
    public Vector2 MoveInput;
    public Vector2 LookInput;
    public bool SprintInput;
    
    
    [Header("Components")]
    [SerializeField] CinemachineCamera _fpCamera;
    [SerializeField] private CharacterController _characterController;

    #region Unity Methods
    
    
    private void OnValidate()
    {
        if (_characterController == null)
        {
            _characterController = GetComponent<CharacterController>();
        }
    }

    private void Update()
    {
        MoveUpdate();
        LookUpdate();
        CameraUpdate();
    }

    #endregion
    
    #region Controller Methods

    public void TryJump()
    {
        if (IsGrounded == false)
        {
            return;
        }
        VerticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Physics.gravity.y * GravityScale);
    }
    

    private void MoveUpdate()
    {
        Vector3 motion = transform.forward * MoveInput.y + transform.right * MoveInput.x;
        motion.y = 0f;
        motion.Normalize();

        if (motion.sqrMagnitude >= 0.01f)
        {
            CurrentVelocity = Vector3.MoveTowards(CurrentVelocity, motion * MaxSpeed, Acceleration * Time.deltaTime);   
            
        }
        else
        {
            CurrentVelocity = Vector3.MoveTowards(CurrentVelocity, Vector3.zero, Acceleration * Time.deltaTime);
        }

        if (IsGrounded && VerticalVelocity <= 0.01f)
        {
            VerticalVelocity = -3f;
        }
        else
        {
            VerticalVelocity += Physics.gravity.y * GravityScale * Time.deltaTime;
        }
        
        Vector3 fullVelocity = new Vector3(CurrentVelocity.x, VerticalVelocity, CurrentVelocity.z);
        
        
        _characterController.Move(fullVelocity * Time.deltaTime);
        
        // Update Speed
        CurrentSpeed = CurrentVelocity.magnitude;
    }

    private void LookUpdate()
    {
        Vector2 input = new Vector2(LookInput.x * LookSensitivity.x, LookInput.y * LookSensitivity.y);

        // Look up and down
        CurrentPitch -= input.y;

        _fpCamera.transform.localRotation = Quaternion.Euler(CurrentPitch, 0f, 0f);
        
        // Look left and right
        transform.Rotate(Vector3.up * input.x);
        
    }

    void CameraUpdate()
    {
        float targetFOV = CameraNormalFOV;

        if (Sprinting)
        {
            float speedRatio = CurrentSpeed / SprintSpeed;

            targetFOV = Mathf.Lerp(CameraNormalFOV, CameraSprintFOV, speedRatio);
        }
        
        _fpCamera.Lens.FieldOfView = Mathf.Lerp(_fpCamera.Lens.FieldOfView, targetFOV,CameraFOVSmoothing * Time.deltaTime);
    }
    

    #endregion
    
}
