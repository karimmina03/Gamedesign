using UnityEngine;
using Cinemachine;

public class CameraSwitcher : MonoBehaviour
{
    [Header("Cameras")]
    public CinemachineVirtualCamera thirdPersonCam;
    public CinemachineVirtualCamera firstPersonCam;
    public CinemachineVirtualCamera finishCam;
    public Camera CarCam; // regular camera (child of the car)

    private bool isFirstPerson = false;

    void Start()
    {
        // Default: third person active
        SetActiveCamera(thirdPersonCam);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetActiveCamera(firstPersonCam);
            isFirstPerson = true;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetActiveCamera(thirdPersonCam);
            isFirstPerson = false;
        }
    }

    public void ActivateFinishCam()
    {
        // disable the CarCam (since it’s not Cinemachine)
        if (CarCam != null)
            CarCam.enabled = false;

        SetActiveCamera(finishCam);
        isFirstPerson = false;

        Debug.Log("Switched to Finish Camera!");
    }

    public bool IsFirstPersonActive()
    {
        return isFirstPerson;
    }

    private void SetActiveCamera(CinemachineVirtualCamera activeCam)
    {
        // disable CarCam to avoid conflict with Cinemachine
        if (CarCam != null)
            CarCam.enabled = false;

        if (thirdPersonCam != null) thirdPersonCam.Priority = 0;
        if (firstPersonCam != null) firstPersonCam.Priority = 0;
        if (finishCam != null) finishCam.Priority = 0;

        if (activeCam != null)
        {
            activeCam.Priority = 10;
            Debug.Log("Active Cinemachine cam: " + activeCam.name);
        }
    }
}
