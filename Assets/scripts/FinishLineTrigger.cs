using UnityEngine;
using Cinemachine;
using System.Collections;

public class FinishLineTrigger : MonoBehaviour
{
    [Header("Assign Cameras")]
    public CinemachineVirtualCamera finishCam;
    public CinemachineVirtualCamera vcam1;          // walking cam
    public CinemachineVirtualCamera firstPersonCam; // 1st person
    public Camera CarCam;         // car cam

    private CinemachineBrain brain;

    private void Start()
    {
        brain = Camera.main.GetComponent<CinemachineBrain>();
        if (brain == null)
            Debug.LogError("[FinishLineTrigger] No CinemachineBrain found on Main Camera!");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Car"))
        {
            Debug.Log("[FinishLineTrigger] Player crossed finish line!");

            // Stop all gameplay cameras
            DisableGameplayCameras();

            // Activate the finish cam
            StartCoroutine(ActivateFinishCamRoutine());
        }
    }

    private IEnumerator ActivateFinishCamRoutine()
    {
        Debug.Log("[FinishLineTrigger] Activating finishCam...");

        // Lower all gameplay cam priorities
        if (vcam1) vcam1.Priority = 0;
        if (firstPersonCam) firstPersonCam.Priority = 0;
        CarCam.enabled = false;

        // Activate and raise finish cam
        if (finishCam)
        {
            finishCam.enabled = true;
            finishCam.Priority = 500;
            Debug.Log("[FinishLineTrigger] FinishCam '" + finishCam.name + "' ENABLED and priority = 500");
        }

        // Wait a bit for CinemachineBrain to process
        yield return new WaitForSeconds(0.2f);

        ForceBrainUpdate();

        PrintAllVcamStatus();

        yield return null;
    }

    private void DisableGameplayCameras()
    {
        if (CarCam)
        {
            CarCam.enabled = false;
            Debug.Log("[FinishLineTrigger] Disabled CarCam '" + CarCam.name + "'");
        }

        if (firstPersonCam)
        {
            firstPersonCam.enabled = false;
            Debug.Log("[FinishLineTrigger] Disabled FirstPersonCam '" + firstPersonCam.name + "'");
        }

        if (vcam1)
        {
            vcam1.enabled = false;
            Debug.Log("[FinishLineTrigger] Disabled ThirdPersonCam '" + vcam1.name + "'");
        }
    }

    private void ForceBrainUpdate()
    {
        if (brain)
        {
            brain.m_DefaultBlend.m_Time = 0f; // instant switch
            brain.ManualUpdate(); // force update
            Debug.Log("[FinishLineTrigger] CinemachineBrain manual update called.");
        }
    }

    private void PrintAllVcamStatus()
    {
        CinemachineVirtualCamera[] vcams = FindObjectsOfType<CinemachineVirtualCamera>(true);
        Debug.Log("-----------------------------------------------------");
        Debug.Log("[FinishLineTrigger] Active VCam Status Report:");
        foreach (var cam in vcams)
        {
            string state = cam.enabled ? "ENABLED" : "DISABLED";
            Debug.Log("-> " + cam.name + ": Priority=" + cam.Priority + ", " + state + ", ActiveSelf=" + cam.gameObject.activeSelf);
        }

        if (brain && brain.ActiveVirtualCamera != null)
            Debug.Log("[FinishLineTrigger] CinemachineBrain Active Cam: " + brain.ActiveVirtualCamera.Name);
        else
            Debug.Log("[FinishLineTrigger] No active camera in brain!");
        Debug.Log("-----------------------------------------------------");
    }
}
