using Cinemachine;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishLineTrigger : MonoBehaviour
{
    [Header("Assign Cameras")]
    public CinemachineVirtualCamera finishCam;
    public CinemachineVirtualCamera vcam1;          // walking cam
    public CinemachineVirtualCamera firstPersonCam; // 1st person
    public Camera CarCam;         // car cam
    public LevelTimer levelTimer;

    private CinemachineBrain brain;
    public float finishCamDuration = 5f; // time to wait before transition
    public string level1Transition = "Level 1 Transition";
    public string level2Transition = "Level 2 Transition";
    public string level3Transition = "Ending";
    private void Start()
    {
        brain = Camera.main.GetComponent<CinemachineBrain>();
        if (brain == null)
            Debug.LogError("[FinishLineTrigger] No CinemachineBrain found on Main Camera!");
    }



    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Car")) return;

        // Stop the timer and save
        LevelTimer timer = FindObjectOfType<LevelTimer>();
        if (timer != null)
        {
            string currentLevel = SceneManager.GetActiveScene().name;
            timer.StopAndSaveTime(currentLevel);
        }
        StartCoroutine(DelayedTransition());

        Debug.Log("Level finished! Timer stopped.");

        // Start the finish cam routine and delayed transition
        StartCoroutine(ActivateFinishCamRoutine());
    }

    private IEnumerator DelayedTransition()
    {
        // Wait for finish cam / birdseye view duration
        yield return new WaitForSeconds(finishCamDuration);

        // Determine current level and transition scene
        string currentLevel = SceneManager.GetActiveScene().name;
        string transitionScene = "";

        switch (currentLevel)
        {
            case "Level 1":
                transitionScene = level1Transition;
                break;
            case "Level 2":
                transitionScene = level2Transition;
                break;
            case "Level 3":
                transitionScene = level3Transition;
                break;
        }

        if (!string.IsNullOrEmpty(transitionScene))
        {
            SceneManager.LoadScene(transitionScene);
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

        yield return new WaitForSeconds(0.2f);
        ForceBrainUpdate();

        GameObject car = GameObject.FindWithTag("Car");
        CarController cc = car.GetComponent<CarController>();
        yield return new WaitForSeconds(3.0f);

        cc.StopCarInstantly();
        cc.StopCarInstantly();
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

  




}
