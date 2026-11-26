using UnityEngine;

namespace Game.Features.UI
{
    public class UIInputController : MonoBehaviour
    {
        private const KeyCode InventoryKey = KeyCode.I;
        private const KeyCode SystemKey = KeyCode.O;
        private const KeyCode MapKey = KeyCode.M;
        private const KeyCode QuestKey = KeyCode.J;

        private void Update()
        {
            if (UIManager.Instance == null)
                return;

            if (Input.GetKeyDown(InventoryKey))
                UIManager.Instance.ToggleInventory();

            if (Input.GetKeyDown(SystemKey))
                UIManager.Instance.ToggleSystem();

            if (Input.GetKeyDown(MapKey))
                UIManager.Instance.ToggleMap();

            if (Input.GetKeyDown(QuestKey))
                UIManager.Instance.ToggleQuest();
        }
    }
}
