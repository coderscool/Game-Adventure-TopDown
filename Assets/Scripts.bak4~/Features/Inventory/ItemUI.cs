using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Game.Features.Items;

namespace Game.Features.Inventory
{
    public class ItemUI : MonoBehaviour,
        IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        public static event Action<ItemUI> Clicked;

        private const float DragAlpha = 0.9f;

        private static readonly Vector2 Center = new Vector2(0.5f, 0.5f);
        private static readonly Vector2 BottomRight = new Vector2(1f, 0f);
        private static readonly Vector2 AmountOffset = new Vector2(-6f, 6f);

        [Header("Data")]
        [FormerlySerializedAs("data")]
        [SerializeField] private ItemData _data;

        [Header("UI (Assign in Inspector)")]
        [FormerlySerializedAs("icon")]
        [SerializeField] private Image _icon;

        [FormerlySerializedAs("amountText")]
        [SerializeField] private TMP_Text _amountText;

        private RectTransform _rect;
        private RectTransform _amountRect;
        private CanvasGroup _canvasGroup;

        private Canvas _rootCanvas;
        private Transform _dragLayer;
        private Transform _originalParent;
        private bool _isDragging;

        public ItemData Data => _data;
        public Slot CurrentSlot { get; set; }

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();

            if (_icon == null)
                _icon = GetComponent<Image>();

            if (_amountText != null)
                _amountRect = _amountText.rectTransform;
        }

        public void Init(ItemData newData, int amount)
        {
            _data = newData;

            _icon.sprite = newData.Icon;
            _amountText.text = amount.ToString();
        }

        public void SetRootCanvas(Canvas canvas)
        {
            _rootCanvas = canvas;
        }

        public void SetDragLayer(Transform layer)
        {
            _dragLayer = layer;
        }

        public void SetAmount(int amount)
        {
            if (_amountText == null || _data == null)
                return;

            if (!_data.Stackable)
            {
                _amountText.gameObject.SetActive(false);
                return;
            }

            _amountText.gameObject.SetActive(amount > 1);
            _amountText.text = amount.ToString();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_data == null)
                return;

            Clicked?.Invoke(this);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_rootCanvas == null)
            {
                Debug.LogError("RootCanvas chưa được gán cho ItemUI!");
                return;
            }

            if (_dragLayer == null)
            {
                Debug.LogError("Drag layer has not been assigned to ItemUI.");
                return;
            }

            _isDragging = true;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.alpha = DragAlpha;

            _originalParent = transform.parent;
            Vector2 size = _rect.rect.size;

            transform.SetParent(_dragLayer, false);
            transform.SetAsLastSibling();

            _rect.anchorMin = _rect.anchorMax = Center;
            _rect.pivot = Center;
            _rect.sizeDelta = size;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging)
                return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _rootCanvas.transform as RectTransform,
                eventData.position,
                _rootCanvas.worldCamera,
                out Vector2 localPosition
            );

            _rect.anchoredPosition = localPosition;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_isDragging)
                return;

            _isDragging = false;
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.alpha = 1f;

            if (CurrentSlot != null)
            {
                transform.SetParent(CurrentSlot.transform, false);
                _rect.anchoredPosition = Vector2.zero;

                FixRootRect();
                FixAmountRect();
            }
            else
            {
                transform.SetParent(_originalParent, false);
                _rect.anchoredPosition = Vector2.zero;

                FixRootRect();
            }
        }

        private void FixRootRect()
        {
            _rect.anchorMin = Vector2.zero;
            _rect.anchorMax = Vector2.one;
            _rect.offsetMin = Vector2.zero;
            _rect.offsetMax = Vector2.zero;
            _rect.localScale = Vector3.one;
        }

        private void FixAmountRect()
        {
            if (_amountRect == null)
                return;

            _amountRect.anchorMin = BottomRight;
            _amountRect.anchorMax = BottomRight;
            _amountRect.pivot = BottomRight;
            _amountRect.anchoredPosition = AmountOffset;
            _amountRect.localScale = Vector3.one;
        }
    }
}
