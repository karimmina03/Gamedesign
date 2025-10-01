using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public int speed;
    public GameObject cars;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        speed += 5;
        cars.SetActive(false);  // hides the car

    }
}
