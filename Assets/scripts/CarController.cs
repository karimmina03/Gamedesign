using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    private bool boostApplied = false;
    private float boostedMaxSpeed;
    private float boostedMotorPower;

    [Header("Movement")]
    public float motorPower = 2500f;           // increased from 1500
    public float reversePower = 2000f;         // separate reverse power
    public float turnSensitivity = 2.5f;
    public float maxSteerAngle = 35f;
    public float brakeForce = 8000f;           // increased from 5000
    public float maxSpeed = 30f;
    public float accelerationRate = 25f;       // increased from 10 for faster response
    public float reverseAccelerationRate = 40f; // faster reverse engagement

    [Header("Wheels")]
    public WheelCollider wheelFL;
    public WheelCollider wheelFR;
    public WheelCollider wheelBL;
    public WheelCollider wheelBR;

    public Transform wheelFLMesh;
    public Transform wheelFRMesh;
    public Transform wheelBLMesh;
    public Transform wheelBRMesh;

    private float originalMaxSpeed;
    private float originalMotorPower;

    [SerializeField] private float boostMultiplier = 2f;

    [Header("Headlights")]
    public Transform headlightPivot;

    [Header("Advanced Settings")]
    public float rollingResistance = 150f;     // reduced for less drag
    public float downforceMultiplier = 2f;     // helps with traction
    public float antiRollForce = 5000f;        // prevents flipping

    private Rigidbody rb;
    private float moveInput;
    private float turnInput;
    private float currentAcceleration;
    private bool isReversing = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        originalMaxSpeed = maxSpeed;
        originalMotorPower = motorPower;
        rb.mass = 1000f;
        rb.centerOfMass = new Vector3(0.0f, -0.5f, 0f);
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // Improve wheel friction for better traction
        ConfigureWheelFriction();
    }

    void ConfigureWheelFriction()
    {
        WheelCollider[] wheels = { wheelFL, wheelFR, wheelBL, wheelBR };
        foreach (var wheel in wheels)
        {
            WheelFrictionCurve forwardFriction = wheel.forwardFriction;
            forwardFriction.stiffness = 1.5f;  // better grip
            wheel.forwardFriction = forwardFriction;

            WheelFrictionCurve sidewaysFriction = wheel.sidewaysFriction;
            sidewaysFriction.stiffness = 2f;   // better lateral grip
            wheel.sidewaysFriction = sidewaysFriction;
        }
    }

    void FixedUpdate()

    {
        // handle global speed boost state
        if (GameState.hasSpeedBoost && !boostApplied)
        {
            maxSpeed = originalMaxSpeed * boostMultiplier;
            motorPower = originalMotorPower * boostMultiplier;
            accelerationRate *= 2.0f; // quicker acceleration during boost
            boostApplied = true;
            Debug.Log("[CarController] BOOST APPLIED: maxSpeed=" + maxSpeed + " motorPower=" + motorPower);
        }
        else if (!GameState.hasSpeedBoost && boostApplied)
        {
            // restore originals
            maxSpeed = originalMaxSpeed;
            motorPower = originalMotorPower;
            boostApplied = false;
            Debug.Log("[CarController] BOOST RESTORED: maxSpeed=" + maxSpeed + " motorPower=" + motorPower);
        }


        HandleInput();
        HandleMotor();
        HandleSteering();
        HandleBraking();
        UpdateWheelMeshes();
        ApplyDownforce();
        ApplyAntiRoll();
    }

    void HandleInput()
    {
        moveInput = 0f;
        turnInput = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
                moveInput = 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
                moveInput = -1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                turnInput = -1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                turnInput = 1f;
        }
    }

    void HandleMotor()
    {
        float currentSpeed = rb.linearVelocity.magnitude;
        Vector3 localVelocity = transform.InverseTransformDirection(rb.linearVelocity);
        bool movingForward = localVelocity.z > 0.1f;
        bool movingBackward = localVelocity.z < -0.1f;

        // Forward acceleration
        if (moveInput > 0f)
        {
            isReversing = false;

            // If moving backward and trying to go forward, apply brakes first
            if (movingBackward)
            {
                ApplyBrakesTorque(brakeForce * 0.7f);
                currentAcceleration = 0f;
            }
            else if (currentSpeed < maxSpeed)
            {
                // Quick acceleration with instant response
                currentAcceleration = Mathf.Lerp(currentAcceleration, motorPower, Time.deltaTime * accelerationRate);
                ApplyMotorTorque(currentAcceleration);
                ReleaseBrakes();
            }
            else
            {
                ReleaseBrakes();
            }
        }
        // Reverse/Brake
        else if (moveInput < 0f)
        {
            // If moving forward and trying to reverse, brake first
            if (movingForward && currentSpeed > 1f)
            {
                ApplyBrakesTorque(brakeForce * 0.8f);
                currentAcceleration = 0f;
                isReversing = false;
            }
            else
            {
                // Now engage reverse with faster response
                isReversing = true;
                currentAcceleration = Mathf.Lerp(currentAcceleration, -reversePower, Time.deltaTime * reverseAccelerationRate);
                ApplyMotorTorque(currentAcceleration);
                ReleaseBrakes();
            }
        }
        // No input - coast with mild resistance
        else
        {
            currentAcceleration = 0f;
            isReversing = false;
            ApplyBrakesTorque(rollingResistance);
        }
    }

    void ApplyMotorTorque(float torque)
    {
        wheelFL.motorTorque = torque;
        wheelFR.motorTorque = torque;
        wheelBL.motorTorque = torque;
        wheelBR.motorTorque = torque;
    }

    void ApplyBrakesTorque(float brake)
    {
        wheelFL.brakeTorque = brake;
        wheelFR.brakeTorque = brake;
        wheelBL.brakeTorque = brake;
        wheelBR.brakeTorque = brake;
    }

    void ReleaseBrakes()
    {
        wheelFL.brakeTorque = 0f;
        wheelFR.brakeTorque = 0f;
        wheelBL.brakeTorque = 0f;
        wheelBR.brakeTorque = 0f;
    }

    void HandleSteering()
    {
        float currentSpeed = rb.linearVelocity.magnitude;
        float speedFactor = Mathf.Clamp01(currentSpeed / maxSpeed);

        // Reduce steering at high speeds
        float adjustedSteerAngle = maxSteerAngle * Mathf.Lerp(1f, 0.4f, speedFactor);

        // Apply steering
        float steerAngle = turnInput * adjustedSteerAngle * turnSensitivity;
        wheelFL.steerAngle = steerAngle;
        wheelFR.steerAngle = steerAngle;

        if (headlightPivot != null)
            headlightPivot.localRotation = Quaternion.Euler(0f, steerAngle * 0.5f, 0f);
    }

    void HandleBraking()
    {
        // Space bar for handbrake
        if (Keyboard.current.spaceKey.isPressed)
        {
            ApplyBrakesTorque(brakeForce);
            currentAcceleration = 0f;
        }
    }

    void ApplyDownforce()
    {
        // Add downward force at speed for better traction
        float currentSpeed = rb.linearVelocity.magnitude;
        rb.AddForce(-transform.up * currentSpeed * downforceMultiplier);
    }

    void ApplyAntiRoll()
    {
        // Anti-roll bars to prevent flipping
        ApplyAntiRollBar(wheelFL, wheelFR);
        ApplyAntiRollBar(wheelBL, wheelBR);
    }

    void ApplyAntiRollBar(WheelCollider leftWheel, WheelCollider rightWheel)
    {
        WheelHit hit;
        float travelL = 1.0f;
        float travelR = 1.0f;

        bool groundedL = leftWheel.GetGroundHit(out hit);
        if (groundedL)
            travelL = (-leftWheel.transform.InverseTransformPoint(hit.point).y - leftWheel.radius) / leftWheel.suspensionDistance;

        bool groundedR = rightWheel.GetGroundHit(out hit);
        if (groundedR)
            travelR = (-rightWheel.transform.InverseTransformPoint(hit.point).y - rightWheel.radius) / rightWheel.suspensionDistance;

        float antiRollForceAmount = (travelL - travelR) * antiRollForce;

        if (groundedL)
            rb.AddForceAtPosition(leftWheel.transform.up * -antiRollForceAmount, leftWheel.transform.position);
        if (groundedR)
            rb.AddForceAtPosition(rightWheel.transform.up * antiRollForceAmount, rightWheel.transform.position);
    }

    void UpdateWheelMeshes()
    {
        UpdateWheelMesh(wheelFL, wheelFLMesh);
        UpdateWheelMesh(wheelFR, wheelFRMesh);
        UpdateWheelMesh(wheelBL, wheelBLMesh);
        UpdateWheelMesh(wheelBR, wheelBRMesh);
    }

    void UpdateWheelMesh(WheelCollider collider, Transform mesh)
    {
        if (mesh == null || collider == null) return;
        Vector3 pos;
        Quaternion rot;
        collider.GetWorldPose(out pos, out rot);
        mesh.position = pos;
        mesh.rotation = rot;
    }
}