using UnityEngine;

public class UIInputController : MonoBehaviour
{
    private const KeyCode InventoryKey = KeyCode.I;
    private const KeyCode SystemKey = KeyCode.O;

    private void Update()
    {
        if (UIManager.Instance == null)
            return;

        if (Input.GetKeyDown(InventoryKey))
            UIManager.Instance.ToggleInventory();

        if (Input.GetKeyDown(SystemKey))
            UIManager.Instance.ToggleSystem();
    }
}
