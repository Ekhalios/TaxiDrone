using UnityEngine;

public class Client : MonoBehaviour
{

    public bool isWaiting = true;

    public bool InDrone = false;

    public GameObject body;

    private Drone drone;

    private bool land = false;

    public Transform point;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        drone = FindObjectOfType<Drone>();
    }

    // Update is called once per frame
    void Update()
    {
        if (land)
        {
            transform.position = Vector3.MoveTowards(transform.position, drone.transform.position, 2 * Time.deltaTime);
            float distance = Vector3.Distance(new Vector3(transform.position.x, transform.position.y, transform.position.z), new Vector3(drone.gameObject.transform.position.x, drone.gameObject.transform.position.y, drone.gameObject.transform.position.z));
            if (distance < 0.1f)
            {
                isWaiting = false;
                InDrone = true;
                body.SetActive(false);
                drone.GetClient(point.position);
            }
        }
    }

    public void droneLand()
    {
        if (InDrone)
        {
            Arrived();
            return;
        }
        land = true;
    }


    public void Arrived()
    {
        transform.position = drone.gameObject.transform.position;
        body.SetActive(true);
    }
}
