using UnityEngine;
using Cinemachine;

public  class camdebug
{
    public static void PrintAllCamerasStatus()
    {
        if (Camera.main == null)
        {
            Debug.LogWarning("Camera.main is null. Cannot print camera status.");
            return;
        }

        CinemachineVirtualCamera[] cameras = GameObject.FindObjectsOfType<CinemachineVirtualCamera>(true);
        Debug.Log("---------- Camera Debug ----------");

        foreach (var cam in cameras)
        {
            string enabledState = cam.enabled ? "ENABLED" : "DISABLED";
            string activeState = cam.gameObject.activeSelf ? "Active" : "Inactive";
            Debug.Log($"Camera: {cam.name}, Priority: {cam.Priority}, {enabledState}, {activeState}");

            // Highlight the cameras you care about
            if (cam.name.ToLower() == "vcam1" || cam.name.ToLower() == "first person cam")
            {
                Debug.Log($"  IMPORTANT CAMERA: {cam.name} detected! Follow: {(cam.Follow != null ? cam.Follow.name : "null")}, LookAt: {(cam.LookAt != null ? cam.LookAt.name : "null")}");
            }
        }

        var brain = Camera.main.GetComponent<CinemachineBrain>();
        if (brain != null && brain.ActiveVirtualCamera != null)
        {
            Debug.Log($"Active camera in Brain: {brain.ActiveVirtualCamera.Name}");
        }
        else
        {
            Debug.Log("No active camera in Brain");
        }

        Debug.Log("---------------------------------");
    }

    

    public static void PrintAllCameraStatus()
    {
        Debug.Log("=========================================================");
        Debug.Log("             CAMERA DEBUG STATUS (O Pressed)            ");
        Debug.Log("=========================================================");

        // --- Unity Cameras ---
        Camera[] cameras = GameObject.FindObjectsOfType<Camera>(true);
        Debug.Log("------ Unity Cameras ------");
        foreach (Camera cam in cameras)
        {
            Debug.Log($"UnityCam: {cam.name} | enabled={cam.enabled} | activeSelf={cam.gameObject.activeSelf}");
        }

        // --- Cinemachine Virtual Cameras ---
        CinemachineVirtualCamera[] vcams = GameObject.FindObjectsOfType<CinemachineVirtualCamera>(true);
        Debug.Log("------ Cinemachine Virtual Cameras ------");

        foreach (var vcam in vcams)
        {
            string enabledState = vcam.enabled ? "ENABLED" : "DISABLED";

            Debug.Log(
                $"VCAM: {vcam.name}\n" +
                $"   Priority = {vcam.Priority}\n" +
                $"   Enabled = {enabledState}\n" +
                $"   Follow = {(vcam.Follow ? vcam.Follow.name : "NULL")}\n" +
                $"   LookAt = {(vcam.LookAt ? vcam.LookAt.name : "NULL")}\n" +
                $"   ActiveSelf = {vcam.gameObject.activeSelf}\n"
            );
        }

        // --- Active camera from CinemachineBrain ---
        CinemachineBrain brain = Camera.main?.GetComponent<CinemachineBrain>();
        if (brain && brain.ActiveVirtualCamera != null)
        {
            Debug.Log("------ Brain Active Camera ------");
            Debug.Log($"Brain Active VCam: {brain.ActiveVirtualCamera.Name}");
        }
        else
        {
            Debug.Log("Brain has NO active virtual camera!");
        }

        Debug.Log("=========================================================");
    }
}
