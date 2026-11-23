using UnityEngine;
using Game.Features.Shop.Data;
using Game.Features.Shop.Views;
using Game.Shared;

namespace Game.Features.Shop.Systems
{
    /// <summary>
    /// Put on a shop counter/NPC in a map. Press E near it to open or close its shop.
    /// The game keeps running while the shop is open; walking away closes it.
    /// </summary>
    public class ShopTrigger : MonoBehaviour
    {
        private const KeyCode InteractKey = KeyCode.E;

        [SerializeField] private ShopData _shopData;

        private bool _isPlayerNear;

        private void Update()
        {
            if (!_isPlayerNear || !Input.GetKeyDown(InteractKey))
                return;

            ShopView view = ShopView.Instance;
            if (view == null)
            {
                Debug.LogWarning("ShopTrigger: no ShopView in the scene.");
                return;
            }

            if (view.IsOpenFor(_shopData))
                view.Close();
            else
                view.Open(_shopData);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag(GameTags.Player))
                _isPlayerNear = true;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag(GameTags.Player))
                return;

            _isPlayerNear = false;
            CloseIfShowingThisShop();
        }

        // Also covers leaving the map while the shop is open.
        private void OnDisable()
        {
            _isPlayerNear = false;
            CloseIfShowingThisShop();
        }

        private void CloseIfShowingThisShop()
        {
            ShopView view = ShopView.Instance;
            if (view != null && view.IsOpenFor(_shopData))
                view.Close();
        }
    }
}
