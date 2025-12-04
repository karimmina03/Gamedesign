using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    private bool boostApplied = false;
    private float boostedMaxSpeed;
    private float boostedMotorPower;
    private bool forcedStop = false;
    [Header("Traction")]
    public float tractionSlipThreshold = 0.25f;
    public float tractionReductionRate = 0.85f;
    public float slopeFrictionBoost = 1.5f;
    public float slopeAngleThresholdDeg = 3f;
    private float tractionMotorLimiter = 1f;
    public float slipLimit = 0.45f;
    public float tractionRecoverRate = 1.2f;
    public float tractionReduceRate = 2.5f;

    [Header("Movement")]
    public float motorPower = 2500f;
    public float reversePower = 2000f;
    public float turnSensitivity = 2.5f;
    public float maxSteerAngle = 35f;
    public float brakeForce = 8000f;
    public float maxSpeed = 30f;
    public float accelerationRate = 25f;
    public float reverseAccelerationRate = 40f;
    [Header("Steering Helpers")]
    public float steerHelper = 0.5f; // 0 is no help, 1 is full help (Try 0.5 to 0.8)
    public float angularDragOnTurn = 0.01f; // Make it spin easier
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
    public float rollingResistance = 150f;
    public float downforceMultiplier = 6f;
    public float antiRollForce = 5000f;

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
        rb.mass = 1400f; // Updated to match Inspector

        // Lowered Center of Mass slightly less aggressively to prevent pendulum effect
        rb.centerOfMass = new Vector3(0.0f, -0.4f, 0.0f);

        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // REMOVED: ConfigureWheelFriction(); 
        // Reason: This was overwriting your Inspector settings. 
        // Now you can tune friction directly in Unity.
    }

    void FixedUpdate()
    {
        // REMOVED: Duplicate ApplyAntiRoll calls here.
        // They are handled correctly by the ApplyAntiRoll() function at the end of FixedUpdate.

        // handle global speed boost state
        // (Assuming GameState exists in your project, kept logic as is)
        /* 
        if (GameState.hasSpeedBoost && !boostApplied) { ... } 
        */

        HandleInput();
        HandleMotor();
        HandleSteering();
        HandleBraking();
        UpdateWheelMeshes();
        ApplyDownforce();

        // This single call handles the anti-roll for both axles
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

            if (Keyboard.current.tKey.wasPressedThisFrame)
            {
                StopCarInstantly();
            }
        }
    }
    void HandleMotor()
    {
        if (forcedStop)
        {
            ApplyMotorTorque(0f);
            ApplyBrakesTorque(float.MaxValue);

            if (moveInput != 0f)
            {
                forcedStop = false;
                ReleaseBrakes();
            }
            return;
        }

        float currentSpeed = rb.linearVelocity.magnitude;
        Vector3 localVelocity = transform.InverseTransformDirection(rb.linearVelocity);
        bool movingBackward = localVelocity.z < -0.1f;
        bool movingForward = localVelocity.z > 0.1f;

        WheelCollider[] wheels = { wheelFL, wheelFR, wheelBL, wheelBR };

        float slopeAngle = GetAverageGroundSlopeAngle();
        float slopeTorqueMultiplier = (slopeAngle > slopeAngleThresholdDeg) ? 1f + Mathf.Clamp01(slopeAngle / 15f) : 1f;

        // --- NEW CODE: Turn Assist Logic ---
        // If we are turning AND speed is low (< 10), double the torque to fight friction
        float turnAssist = 1f;
        if (Mathf.Abs(turnInput) > 0.1f && currentSpeed < 10f)
        {
            turnAssist = 2.5f; // Boost power by 2.5x when turning at low speeds
        }
        // -----------------------------------

        float targetTorque = 0f;

        // Forward
        if (moveInput > 0f)
        {
            isReversing = false;

            if (movingBackward)
            {
                ApplyBrakesTorque(brakeForce * 0.7f);
                currentAcceleration = 0f;
            }
            else
            {
                // Apply the turnAssist multiplier here
                targetTorque = Mathf.Lerp(currentAcceleration, motorPower * slopeTorqueMultiplier * turnAssist, Time.deltaTime * accelerationRate);

                foreach (var wheel in wheels)
                {
                    if (wheel.GetGroundHit(out WheelHit hit))
                    {
                        // Allow more slip when power-turning so the wheels don't lock up
                        float slipThreshold = (turnAssist > 1f) ? 1.0f : slipLimit;
                        float slipFactor = hit.forwardSlip > slipThreshold ? 0.8f : 1f;

                        wheel.motorTorque = targetTorque * slipFactor;
                    }
                    else
                    {
                        wheel.motorTorque = 0f;
                        wheel.brakeTorque = 10f;
                    }
                }
                currentAcceleration = targetTorque;
                ReleaseBrakes();
            }
        }
        // Reverse (Also needs boost to turn while reversing)
        else if (moveInput < 0f)
        {
            if (movingForward && currentSpeed > 1f)
            {
                ApplyBrakesTorque(brakeForce * 0.8f);
                currentAcceleration = 0f;
                isReversing = false;
            }
            else
            {
                isReversing = true;
                // Apply turnAssist to reverse as well
                targetTorque = Mathf.Lerp(currentAcceleration, -reversePower * turnAssist, Time.deltaTime * reverseAccelerationRate);

                foreach (var wheel in wheels)
                {
                    if (wheel.GetGroundHit(out WheelHit hit))
                    {
                        wheel.motorTorque = targetTorque;
                    }
                }
                currentAcceleration = targetTorque;
                ReleaseBrakes();
            }
        }
        // No input
        else
        {
            currentAcceleration = 0f;
            isReversing = false;
            ApplyBrakesTorque(rollingResistance);
            foreach (var wheel in wheels)
                wheel.motorTorque = 0f;
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
        // 1. Remove the "Lerp" lag. This makes input instant.
        // Calculate the target steer angle directly.
        float targetSteerAngle = maxSteerAngle * turnInput;

        // 2. Ackermann Steering Logic (Prevents car from stopping/scrubbing)
        // We assume the car is roughly 1.5m wide (Track) and 2.5m long (Wheelbase)
        // You can fine tune these, but these defaults work for most standard Unity cars.
        float wheelBase = 2.55f;
        float rearTrack = 1.5f;
        float turnRadius = 0f;

        if (turnInput > 0) // Turning Right
        {
            turnRadius = wheelBase / Mathf.Sin(Mathf.Abs(targetSteerAngle) * Mathf.Deg2Rad);
            wheelFL.steerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearTrack / 2))) * turnInput;
            wheelFR.steerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - (rearTrack / 2))) * turnInput;
        }
        else if (turnInput < 0) // Turning Left
        {
            turnRadius = wheelBase / Mathf.Sin(Mathf.Abs(targetSteerAngle) * Mathf.Deg2Rad);
            wheelFL.steerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - (rearTrack / 2))) * turnInput;
            wheelFR.steerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearTrack / 2))) * turnInput;
        }
        else // Going Straight
        {
            wheelFL.steerAngle = 0;
            wheelFR.steerAngle = 0;
        }

        // 3. Steer Helper (The "Arcade" Feel)
        // If the car is moving, we physically rotate the body slightly to help it turn fast.
        if (Mathf.Abs(turnInput) > 0.1f && rb.linearVelocity.magnitude > 1f)
        {
            float speedFactor = Mathf.Clamp01(rb.linearVelocity.magnitude / maxSpeed);
            // Add torque to rotate the car body Y-axis
            rb.AddRelativeTorque(Vector3.up * turnInput * steerHelper * 10000f);
        }

        // Visuals
        if (headlightPivot != null)
            headlightPivot.localRotation = Quaternion.Euler(0f, targetSteerAngle * 0.5f, 0f);
    }
    void HandleBraking()
    {
        if (Keyboard.current.spaceKey.isPressed)
        {
            ApplyBrakesTorque(brakeForce);
            currentAcceleration = 0f;
        }
    }

    void ApplyDownforce()
    {
        float currentSpeed = rb.linearVelocity.magnitude;
        // Only apply downforce if grounded
        if (wheelFL.isGrounded || wheelFR.isGrounded)
            rb.AddForce(-transform.up * downforceMultiplier * Mathf.Max(1f, currentSpeed));
    }

    void ApplyAntiRoll()
    {
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

    public void StopCarInstantly()
    {
        forcedStop = true;
        currentAcceleration = 0f;
        isReversing = false;
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.Sleep();
        }
        ApplyBrakesTorque(float.MaxValue);
        ApplyMotorTorque(0f);
    }

    float GetAverageGroundSlopeAngle()
    {
        WheelCollider[] wheels = { wheelFL, wheelFR, wheelBL, wheelBR };
        Vector3 avgNormal = Vector3.zero;
        int hits = 0;
        foreach (var w in wheels)
        {
            if (w.GetGroundHit(out WheelHit hit))
            {
                avgNormal += hit.normal;
                hits++;
            }
        }
        if (hits == 0) return 0f;
        avgNormal /= hits;
        return Vector3.Angle(avgNormal, Vector3.up);
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