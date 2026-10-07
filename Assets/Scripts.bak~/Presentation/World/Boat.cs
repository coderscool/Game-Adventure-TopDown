using UnityEngine;

public class Boat : MonoBehaviour
{
    public Transform seatPoint; 

    private bool playerInRange = false;
    private GameObject player;

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (!Player.Instance.isOnBoat)
                MountBoat();
            else
                DismountBoat();
        }
    }

    void MountBoat()
    {
        Player.Instance.isOnBoat = true;

        player.transform.SetParent(transform); 
        player.transform.position = seatPoint.position;

        Debug.Log("Lên tàu");
    }

    void DismountBoat()
    {
        Player.Instance.isOnBoat = false;

        player.transform.SetParent(null); 

        player.transform.position = transform.position + Vector3.left * 2f;

        Debug.Log("Xuống tàu");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            player = other.gameObject;

            Debug.Log("Press E to enter boat");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}
