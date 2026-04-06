using UnityEngine;

public class ShopTrigger : MonoBehaviour
{
    public GameObject shopUI;
    private bool isPlayerNear = false;

    void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("ok");
            bool isActive = shopUI.activeSelf;
            shopUI.SetActive(!isActive);
            Time.timeScale = 0f; // pause game
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            Debug.Log("Press E to open shop");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
        }
    }
}