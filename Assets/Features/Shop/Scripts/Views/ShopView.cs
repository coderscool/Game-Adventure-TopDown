using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Game.Features.Character.Systems;
using Game.Features.Inventory;
using Game.Features.Shop.Data;
using Game.Features.Shop.Services;

namespace Game.Features.Shop.Views
{
    /// <summary>
    /// The one shared shop window (lives in the persistent UI). A map's ShopTrigger tells it which
    /// ShopData to show. Put this on an object that is always active, not on the panel it shows.
    /// </summary>
    public class ShopView : MonoBehaviour
    {
        public static ShopView Instance { get; private set; }

        [SerializeField] private GameObject _panel;
        [SerializeField] private Transform _listRoot;
        [SerializeField] private ShopListItemView _listItemPrefab;

        [Header("Optional texts")]
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _goldText;
        [SerializeField] private TMP_Text _messageText;

        [Header("Messages")]
        [SerializeField] private string _successMessage = "Mua thành công";
        [SerializeField] private string _notEnoughGoldMessage = "Không đủ vàng";
        [SerializeField] private string _inventoryFullMessage = "Túi đồ đã đầy";

        private readonly List<ShopListItemView> _itemViews = new List<ShopListItemView>();
        private ShopData _currentShop;

        public bool IsOpenFor(ShopData shop)
        {
            return _panel.activeSelf && _currentShop == shop;
        }

        private void Awake()
        {
            Instance = this;
            _panel.SetActive(false);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void Open(ShopData shop)
        {
            if (shop == null)
            {
                Debug.LogWarning("ShopView.Open: shop is null.");
                return;
            }

            _currentShop = shop;
            SetText(_titleText, shop.ShopName);
            SetText(_messageText, string.Empty);

            BuildList(shop.Entries);
            RefreshGold();
            _panel.SetActive(true);
        }

        // Can be bound to a close button's UnityEvent - keep the name.
        public void Close()
        {
            _currentShop = null;
            _panel.SetActive(false);
        }

        // Rows are reused between shops instead of destroyed and recreated.
        private void BuildList(IReadOnlyList<ShopEntry> entries)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (i == _itemViews.Count)
                    _itemViews.Add(Instantiate(_listItemPrefab, _listRoot));

                ShopListItemView view = _itemViews[i];
                bool isValid = entries[i] != null && entries[i].Item != null;

                view.gameObject.SetActive(isValid);
                if (isValid)
                    view.Bind(entries[i], OnBuyClicked);
            }

            for (int i = entries.Count; i < _itemViews.Count; i++)
                _itemViews[i].gameObject.SetActive(false);
        }

        private void OnBuyClicked(ShopEntry entry)
        {
            BuyResult result = ShopService.TryBuy(Player.Instance, InventoryController.Instance, entry);

            SetText(_messageText, MessageFor(result));
            RefreshGold();
        }

        private string MessageFor(BuyResult result)
        {
            switch (result)
            {
                case BuyResult.Success: return _successMessage;
                case BuyResult.NotEnoughGold: return _notEnoughGoldMessage;
                case BuyResult.InventoryFull: return _inventoryFullMessage;
                default: return string.Empty;
            }
        }

        private void RefreshGold()
        {
            Player player = Player.Instance;
            if (player != null)
                SetText(_goldText, player.Gold.ToString());
        }

        private static void SetText(TMP_Text text, string value)
        {
            if (text != null)
                text.text = value;
        }
    }
}
