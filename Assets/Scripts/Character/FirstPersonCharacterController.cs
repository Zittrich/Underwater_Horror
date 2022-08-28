using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(AudioSource))]
public class FirstPersonCharacterController : MonoBehaviour
{
    private Rigidbody _rigidBody;
    private AudioSource _stepAudioSource;


    public Camera PlayerCamera;

    [Title("Mouse Settings")]
    public float Fov = 60f;
    public float MouseSensitivity = 2f;
    public float MaxLookAngle = 50f;

    [Title("Jump Settings")]
    public KeyCode JumpKey = KeyCode.Space;
    public float JumpPower = 5f;

    [Title("Movement Settings")]
    public AudioClip[] StepClips;
    public float WalkSpeed = 5f;
    public float MaxVelocityChange = 10f;

    private bool _isWalking;
    private float _yaw = 0.0f;
    private float _pitch = 0.0f;
    private bool _isGrounded = false;

    [Title("Headbob Settings")]
    public Transform Joint;
    public float BobSpeed = 10f;
    public Vector3 BobAmount = new Vector3(.15f, .05f, 0f);

    private Vector3 _jointOriginalPos;
    private float _timer = 0;

    private System.Random _random = new System.Random();

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _rigidBody = GetComponent<Rigidbody>();
        _stepAudioSource = GetComponent<AudioSource>();

        PlayerCamera.fieldOfView = Fov;
        _jointOriginalPos = Joint.localPosition;
    }

    void FixedUpdate()
    {
        Vector3 targetVelocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));


        targetVelocity = transform.TransformDirection(targetVelocity) * WalkSpeed;

        Vector3 velocity = _rigidBody.velocity;
        Vector3 velocityChange = (targetVelocity - velocity);
        velocityChange.x = Mathf.Clamp(velocityChange.x, -MaxVelocityChange, MaxVelocityChange);
        velocityChange.z = Mathf.Clamp(velocityChange.z, -MaxVelocityChange, MaxVelocityChange);
        velocityChange.y = 0;

        _rigidBody.AddForce(velocityChange, ForceMode.VelocityChange);

        if (targetVelocity.x != 0 || targetVelocity.z != 0 && _isGrounded)
        {
            _isWalking = true;
            if (!_stepAudioSource.isPlaying)
            {
                _stepAudioSource.clip = StepClips[_random.Next(0, StepClips.Length)];
                _stepAudioSource.Play();
            }
        }

        else
        {
            _isWalking = false;
            targetVelocity = transform.TransformDirection(targetVelocity) * WalkSpeed;

            velocity = _rigidBody.velocity;
            velocityChange = (targetVelocity - velocity);
            velocityChange.x = Mathf.Clamp(velocityChange.x, -MaxVelocityChange, MaxVelocityChange);
            velocityChange.z = Mathf.Clamp(velocityChange.z, -MaxVelocityChange, MaxVelocityChange);
            velocityChange.y = 0;

            _rigidBody.AddForce(velocityChange, ForceMode.VelocityChange);
        }
    }
    private void Update()
    {

        _yaw = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * MouseSensitivity;

        _pitch -= MouseSensitivity * Input.GetAxis("Mouse Y");

        _pitch = Mathf.Clamp(_pitch, -MaxLookAngle, MaxLookAngle);

        transform.localEulerAngles = new Vector3(0, _yaw, 0);
        PlayerCamera.transform.localEulerAngles = new Vector3(_pitch, 0, 0);

        if (Input.GetKeyDown(JumpKey) && _isGrounded)
        {
            Jump();
        }

        CheckGround();

        HeadBob();

    }

    private void Jump()
    {
        if (_isGrounded)
        {
            _rigidBody.AddForce(0f, JumpPower, 0f, ForceMode.Impulse);
            _isGrounded = false;
        }
    }
    private void HeadBob()
    {
        if (_isWalking)
        {
            _timer += Time.deltaTime * BobSpeed;
            Joint.localPosition = new Vector3(_jointOriginalPos.x + Mathf.Sin(_timer) * BobAmount.x, _jointOriginalPos.y + Mathf.Sin(_timer) * BobAmount.y, _jointOriginalPos.z + Mathf.Sin(_timer) * BobAmount.z);
        }
        else
        {
            _timer = 0;
            Joint.localPosition = new Vector3(Mathf.Lerp(Joint.localPosition.x, _jointOriginalPos.x, Time.deltaTime * BobSpeed), Mathf.Lerp(Joint.localPosition.y, _jointOriginalPos.y, Time.deltaTime * BobSpeed), Mathf.Lerp(Joint.localPosition.z, _jointOriginalPos.z, Time.deltaTime * BobSpeed));
        }
    }
    private void CheckGround()
    {
        Vector3 origin = new Vector3(transform.position.x, transform.position.y - (transform.localScale.y * 0.5f), transform.position.z);
        Vector3 direction = transform.TransformDirection(Vector3.down);
        float distance = 0.75f;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, distance))
        {
            Debug.DrawRay(origin, direction * distance, Color.red);
            _isGrounded = true;
        }
        else
        {
            _isGrounded = false;
        }
    }
}
