using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Game.Features.Items;
using Game.Features.Shop.Data;

namespace Game.Features.Shop.Views
{
    /// <summary>One row of the shop list: icon, name, price and a buy button.</summary>
    public class ShopListItemView : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _priceText;
        [SerializeField] private Button _buyButton;

        private ShopEntry _entry;
        private Action<ShopEntry> _onBuy;

        private void Awake()
        {
            _buyButton.onClick.AddListener(OnBuyClicked);
        }

        private void OnDestroy()
        {
            if (_buyButton != null)
                _buyButton.onClick.RemoveListener(OnBuyClicked);
        }

        public void Bind(ShopEntry entry, Action<ShopEntry> onBuy)
        {
            _entry = entry;
            _onBuy = onBuy;

            ItemData item = entry.Item;
            _icon.sprite = item.Icon;
            _icon.enabled = item.Icon != null;
            _nameText.text = item.ItemName;
            _priceText.text = entry.Price.ToString();
        }

        private void OnBuyClicked()
        {
            _onBuy?.Invoke(_entry);
        }
    }
}
