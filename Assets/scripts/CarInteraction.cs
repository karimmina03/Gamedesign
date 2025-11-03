using StarterAssets;
using UnityEngine;
using System.Collections;

public class CarInteraction : MonoBehaviour
{
    public Camera playerCamera;
    public GameObject player;       // the player GameObject
    public Camera carCamera;
    public CarController carController;

    private ThirdPersonController playerMovement;
    private bool isDriving = false;
    private bool isFirstPerson = false;
    public Transform DriverSeat;

    public float interactDistance = 7f; // distance in meters
    public Transform ExitPoint;
    public Vector3 exitOffset = new Vector3(-1.5f, 0f, 0f);
    void Start()
    {
        // Replace "PlayerMovement" with the actual name of your player movement script
        playerMovement = player.GetComponent<ThirdPersonController>();

        if (carController != null)
            carController.enabled = false; // start disabled
    }

    void Update()
    {
        float distance = Vector3.Distance(player.transform.position, transform.position);

        // Enter/exit car
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!isDriving && distance <= interactDistance)
                EnterCar();
            else
                ExitCar();
        }

        // Switch car camera view
        if (isDriving && Input.GetKeyDown(KeyCode.V))
            SwitchCarView();

        // Camera follow
        if (isDriving)
            FollowCarDirection();
    }
    private System.Collections.IEnumerator PlacePlayerNextFrame(Vector3 pos, Quaternion rot)
    {
        // wait one frame so other activation logic (if any) runs first
        yield return null;

        // If player has a CharacterController, temporarily disable it to set position safely
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
            player.transform.position = pos;
            player.transform.rotation = rot;
            cc.enabled = true;

            // zero any legacy velocity if your character controller stores it
            // (StarterAssets usually handles this internally once enabled)
        }
        else
        {
            // If the player has a Rigidbody, zero velocity
            Rigidbody prb = player.GetComponent<Rigidbody>();
            if (prb != null)
            {
                prb.linearVelocity = Vector3.zero;
                prb.angularVelocity = Vector3.zero;
            }

            player.transform.position = pos;
            player.transform.rotation = rot;
        }
    }
    private CharacterController playerController;

    void EnterCar()
    {
        isDriving = true;

        // Snap player to driver seat position and rotation
        if (DriverSeat != null)
        {
            player.transform.position = DriverSeat.position;
            player.transform.rotation = DriverSeat.rotation;
        }

        // Disable player movement
        if (playerMovement != null)
            playerMovement.enabled = false;

        // Disable player CharacterController to prevent collisions
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null)
            cc.enabled = false;

        // Hide player visually
        foreach (Renderer r in player.GetComponentsInChildren<Renderer>())
            r.enabled = false;

        // Enable car movement
        if (carController != null)
            carController.enabled = true;

        // Enable car camera
        if (carCamera != null)
            carCamera.enabled = true;

        // Play engine sound
        AudioSource engineAudio = carController.GetComponent<AudioSource>();
        if (engineAudio != null)
            engineAudio.Play();

        // Start in third-person
        isFirstPerson = false;
        SetCarCameraPosition();
        if (GameState.hasSpeedBoost && carController != null)
        {
            // Apply boost to the car immediately when entering
            carController.maxSpeed *= 2f;     // same boostAmount
            carController.motorPower *= 2f;
            StartCoroutine(RemoveCarBoostAfterTime(500f)); // same boostDuration
        }
    }

    private IEnumerator RemoveCarBoostAfterTime(float duration)
    {
        yield return new WaitForSeconds(duration);
        if (carController != null)
        {
            carController.maxSpeed /= 2f;
            carController.motorPower /= 2f;
        }
        GameState.hasSpeedBoost = false;
    }
    void ExitCar()
    {
        isDriving = false;

        // Compute where the player should appear
        Vector3 exitPosition;
        Quaternion exitRotation;

        if (ExitPoint != null)
        {
            exitPosition = ExitPoint.position;
            exitRotation = ExitPoint.rotation;
        }
        else
        {
            // transform.TransformPoint uses car's local space; adjust x to choose driver side
            exitPosition = transform.TransformPoint(exitOffset);
            // face away from the car
            exitRotation = Quaternion.LookRotation(-transform.forward, Vector3.up);
        }

        // Show player visually now
        foreach (Renderer r in player.GetComponentsInChildren<Renderer>())
            r.enabled = true;

        // Enable player movement
        if (playerMovement != null)
            playerMovement.enabled = true;

        // Enable CharacterController
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = true;

            // Reset velocity if Rigidbody is attached to prevent residual movement
            Rigidbody prb = player.GetComponent<Rigidbody>();
            if (prb != null)
            {
                prb.linearVelocity = Vector3.zero;
                prb.angularVelocity = Vector3.zero;
            }
        }

        // Disable car movement & camera & engine
        if (carController != null)
            carController.enabled = false;
        if (carCamera != null)
            carCamera.enabled = false;

        AudioSource engineAudio = carController != null ? carController.GetComponent<AudioSource>() : null;
        if (engineAudio != null)
            engineAudio.Stop();

        // Ensure player is not parented to car
        if (player.transform.parent == transform)
            player.transform.parent = null;

        // Place player after one frame
        StartCoroutine(PlacePlayerNextFrame(exitPosition, exitRotation));
    }



    void SwitchCarView()
    {
        isFirstPerson = !isFirstPerson;
        SetCarCameraPosition();
    }

    void SetCarCameraPosition()
    {
        if (carCamera == null) return;

        if (isFirstPerson)
        {
            carCamera.transform.localPosition = new Vector3(0f, 1.2f, 0.2f);
            carCamera.transform.localRotation = Quaternion.Euler(5f, 0f, 0f);
        }
        else
        {
            carCamera.transform.localPosition = new Vector3(0f, 2f, -5f);
            carCamera.transform.localRotation = Quaternion.Euler(10f, 0f, 0f);
        }
    }

    void FollowCarDirection()
    {
        if (carCamera == null) return;

        // Smooth rotation
        Quaternion desiredRotation = Quaternion.LookRotation(transform.forward);
        carCamera.transform.rotation = Quaternion.Lerp(
            carCamera.transform.rotation,
            desiredRotation,
            Time.deltaTime * 5f
        );

        // Smooth position follow only in 3rd person
        if (!isFirstPerson)
        {
            Vector3 desiredPosition = transform.TransformPoint(new Vector3(0, 2, -5));
            carCamera.transform.position = Vector3.Lerp(
                carCamera.transform.position,
                desiredPosition,
                Time.deltaTime * 5f
            );
        }
    }
}
