using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class Drone : MonoBehaviour
{
    public float speed = 5f;
    private Vector3 target;
    private Vector3 targetPoint;

    private Client nearestClient;

    private bool goToClient = false;
    private bool goToPoint = false;

    private bool isLanding = false;
    private float flyingAltitude = 15f;
    private float landingAltitude = 0.6f;

    private bool pointLand = false;

    public List<GameObject> helices = new List<GameObject>();
    void Start()
    {
        FindNearestWaitingClient();
    }

    void AnimateHelices()
    {
        foreach (GameObject helix in helices)
        {
            if (helix != null)
            {
                helix.transform.Rotate(Vector3.up * 60 * Time.deltaTime, Space.Self);
            }
        }
    }
    void Update()
    {
        AnimateHelices();
        if (goToClient)
        {
            Vector3 targetPosition = new Vector3(target.x, transform.position.y, target.z);
            Vector3 direction = targetPosition - transform.position;
            direction.y = 0f; // Ignore la rotation verticale
            if (direction.magnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.deltaTime);
            }
            // Étape 1 : Monter à l'altitude de vol si pas encore atteint
            if (transform.position.y < flyingAltitude)
            {
                Vector3 targetAltitude = new Vector3(transform.position.x, flyingAltitude, transform.position.z);
                transform.position = Vector3.MoveTowards(transform.position, targetAltitude, speed * Time.deltaTime);
                return;
            }

            // Étape 2 : Aller vers la position devant le client (en ignorant Y)
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

            // Étape 3 : Vérifier si on est assez proche pour atterrir
            float distance = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(target.x, 0, target.z));
            if (distance < 0.1f)
            {
                goToClient = false; // Stop horizontal movement
                isLanding = true;   // Begin landing
                pointLand = false;
            }
        }
        Debug.Log(goToPoint);
        if (goToPoint)
        {
            Vector3 targetPosition1 = new Vector3(targetPoint.x, transform.position.y, targetPoint.z);
            Vector3 direction1 = targetPosition1 - transform.position;
            direction1.y = 0f; // Ignore la rotation verticale
            if (direction1.magnitude > 0.01f)
            {
                Quaternion targetRotation1 = Quaternion.LookRotation(direction1);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation1, 5f * Time.deltaTime);
            }
            // Étape 1 : Monter à l'altitude de vol si pas encore atteint
            if (transform.position.y < flyingAltitude)
            {
                Vector3 targetAltitude1 = new Vector3(transform.position.x, flyingAltitude, transform.position.z);
                transform.position = Vector3.MoveTowards(transform.position, targetAltitude1, speed * Time.deltaTime);
                return;
            }

            // Étape 2 : Aller vers la position devant le client (en ignorant Y)
            transform.position = Vector3.MoveTowards(transform.position, targetPosition1, speed * Time.deltaTime);

            // Étape 3 : Vérifier si on est assez proche pour atterrir
            float distance1 = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(targetPoint.x, 0, targetPoint.z));
            Debug.Log(distance1);
            if (distance1 < 0.1f)
            {
                isLanding = true;
                goToPoint = false;   // Begin landing
                pointLand = true;
            }
        }

        // Étape 4 : Atterrissage
        if (isLanding)
        {
            Vector3 landingPos = new Vector3(transform.position.x, landingAltitude, transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, landingPos, speed * Time.deltaTime);

            if (Mathf.Abs(transform.position.y - landingAltitude) < 0.05f)
            {
                isLanding = false;
                StartCoroutine(WaitBeforeLandingActions());
            }
        }
    }

    private IEnumerator WaitBeforeLandingActions()
    {
        yield return new WaitForSeconds(3f); // ⏱️ délai de 3 secondes
        if (nearestClient != null)
        {
            nearestClient.droneLand();
        }
        if (pointLand)
        {
            FindNearestWaitingClient();
            pointLand = false;
        }
    }

    void FindNearestWaitingClient()
    {
        Client[] clients = FindObjectsOfType<Client>();
        nearestClient = null;
        float minDistance = Mathf.Infinity;

        foreach (Client client in clients)
        {
            if (!client.isWaiting) continue;

            Vector3 dronePos = new Vector3(transform.position.x, 0, transform.position.z);
            Vector3 clientPos = new Vector3(client.transform.position.x, 0, client.transform.position.z);
            float dist = Vector3.Distance(dronePos, clientPos);

            if (dist < minDistance)
            {
                minDistance = dist;
                nearestClient = client;
            }
        }

        if (nearestClient != null)
        {
            // Calcul d’un point devant le client (pas sur lui)
            Vector3 forwardOffset = nearestClient.transform.forward * 1f; // 1.5 unités devant lui
            Vector3 destination = nearestClient.transform.position + forwardOffset;
            destination.y = transform.position.y; // Ignore Y

            target = destination;
            goToClient = true;
        }
    }

    public void GetClient(Vector3 tragetClient)
    {
        targetPoint = tragetClient;
        goToPoint = true;
    }
}
