using UnityEngine;
using TMPro;

public class TimeCanPickup : MonoBehaviour
{
    public float timeBonus = 5f;
    public GameObject pressGText;
    public float startDelayBonus = 5f;

    private bool playerNearby = false;

    void Start()
    {
        if (pressGText != null)
            pressGText.SetActive(false);
    }

    void Update()
    {
        if (playerNearby && Input.GetKeyDown(KeyCode.G))
        {
            PickupCan();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;

            if (pressGText != null)
                pressGText.SetActive(true);    // show "Press G"
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;

            if (pressGText != null)
                pressGText.SetActive(false);
        }
    }

    private void PickupCan()
    {
        // Give timer bonus
        LevelTimer timer = FindObjectOfType<LevelTimer>();
        if (timer != null)
        {
            timer.SetStartDelay(startDelayBonus);
        }

        // Hide UI text
        if (pressGText != null)
            pressGText.SetActive(false);

        // Remove can from world
        gameObject.SetActive(false);
    }
}
