using UnityEngine;

public class HideBodyInFirstPerson : MonoBehaviour
{
    public GameObject bodyMesh; // assign your player's visible mesh (not root)
    private CameraSwitcher camSwitch;

    void Start()
    {
        camSwitch = FindObjectOfType<CameraSwitcher>();
        if (camSwitch == null)
            Debug.LogWarning("CameraSwitcher not found!");
    }

    void LateUpdate()
    {
        if (camSwitch == null || bodyMesh == null) return;

        // hide mesh in first person
        bodyMesh.SetActive(!camSwitch.IsFirstPersonActive());
    }
}
