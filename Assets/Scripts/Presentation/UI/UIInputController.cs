using UnityEngine;

public class UIInputController : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            UIManager.Instance.Open(UIManager.Instance.inventoryCanvas);
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            UIManager.Instance.Open(UIManager.Instance.systemCanvas);
        }
    }
}
