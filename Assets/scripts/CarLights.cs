using UnityEngine;
using UnityEngine.InputSystem;

public class CarLights : MonoBehaviour
{
    public Light[] headlights; // assign your Headlights and Headlights 1 here
    private bool lightsOn = false;

    void Update()
    {
        if (Keyboard.current.lKey.wasPressedThisFrame) // press L to toggle
        {
            lightsOn = !lightsOn;
            foreach (var light in headlights)
            {
                light.enabled = lightsOn;
            }
        }
    }
}
