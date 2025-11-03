using StarterAssets;
using UnityEngine;

public class SpeedBoost : MonoBehaviour
{
    public float boostAmount = 2f;
    public float boostDuration = 500f;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Triggered by: " + other.name);

        // Player pickup
        if (other.CompareTag("Player"))
        {
            ThirdPersonController controller = other.GetComponent<ThirdPersonController>();
            if (controller != null)
            {
                GameState.hasSpeedBoost = true; // remember pickup
                controller.StartCoroutine(BoostPlayerSpeed(controller));
            }

            gameObject.SetActive(false);
        }

        // Car pickup (optional: if the car directly hits the can)
        if (other.CompareTag("Car"))
        {
            CarController car = other.GetComponent<CarController>();
            if (car != null)
            {
                GameState.hasSpeedBoost = true;
                car.StartCoroutine(BoostCarSpeed(car));
            }

            gameObject.SetActive(false);
        }
    }

    private System.Collections.IEnumerator BoostPlayerSpeed(ThirdPersonController controller)
    {
        float originalMoveSpeed = controller.MoveSpeed;
        float originalSprintSpeed = controller.SprintSpeed;

        controller.MoveSpeed *= boostAmount;
        controller.SprintSpeed *= boostAmount;

        yield return new WaitForSeconds(boostDuration);

        controller.MoveSpeed = originalMoveSpeed;
        controller.SprintSpeed = originalSprintSpeed;
        GameState.hasSpeedBoost = false;
    }

    private System.Collections.IEnumerator BoostCarSpeed(CarController car)
    {
        float originalMaxSpeed = car.maxSpeed;
        float originalPower = car.motorPower;

        car.maxSpeed *= boostAmount;
        car.motorPower *= boostAmount;

        yield return new WaitForSeconds(boostDuration);

        car.maxSpeed = originalMaxSpeed;
        car.motorPower = originalPower;
        GameState.hasSpeedBoost = false;
    }
}
