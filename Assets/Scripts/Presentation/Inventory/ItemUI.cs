using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ItemUI : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public static ItemUI Instance;

    [Header("Data")]
    public ItemData data;

    [HideInInspector] public Slot currentSlot;

    [Header("UI (Assign in Inspector)")]
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text amountText;

    RectTransform rect;
    RectTransform amountRect;
    CanvasGroup canvasGroup;

    Canvas rootCanvas;
    Transform dragLayer;
    Transform originalParent;

    Vector2 cachedSize;

    void Awake()
    {
        Instance = this;
        rect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (icon == null)
            icon = GetComponent<Image>();

        if (amountText != null)
            amountRect = amountText.rectTransform;
    }

    public void Init(ItemData newData, int amount)
    {
        data = newData;

        icon.sprite = data.icon;
        amountText.text = amount.ToString();
    }

    public void SetRootCanvas(Canvas canvas)
    {
        rootCanvas = canvas;
    }

    public void SetDragLayer(Transform layer)
    {
        dragLayer = layer;
    }

    public void RefreshUI()
    {
        if (icon != null && data != null)
            icon.sprite = data.icon;
    }

    public void SetAmount(int amount)
    {
        if (amountText == null || data == null)
            return;

        if (!data.stackable)
        {
            amountText.gameObject.SetActive(false);
            return;
        }

        amountText.gameObject.SetActive(amount > 1);
        amountText.text = amount.ToString();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (data == null)
            return;

        Debug.Log("ok");
        ItemInfo.Instance.Show(this);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (rootCanvas == null)
        {
            Debug.LogError("RootCanvas chưa được gán cho ItemUI!");
            return;
        }

        //InventoryController.Instance.scrollRect.enabled = false;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.9f;

        originalParent = transform.parent;

        cachedSize = rect.rect.size;

        if (dragLayer == null)
        {
            Debug.LogError("Drag layer has not been assigned to ItemUI.");
            return;
        }

        transform.SetParent(dragLayer, false);
        transform.SetAsLastSibling();

        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);

        rect.sizeDelta = cachedSize;
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas.transform as RectTransform,
            eventData.position,
            rootCanvas.worldCamera,
            out Vector2 localPos
        );

        rect.anchoredPosition = localPos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //InventoryController.Instance.scrollRect.enabled = true;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        if (currentSlot != null)
        {
            transform.SetParent(currentSlot.transform, false);
            rect.anchoredPosition = Vector2.zero;

            FixRootRect();
            FixAmountRect();
        }
        else
        {
            transform.SetParent(originalParent, false);
            rect.anchoredPosition = Vector2.zero;

            FixRootRect();
        }
    }

    void FixRootRect()
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
    }

    void FixAmountRect()
    {
        if (amountRect == null) return;

        amountRect.anchorMin = new Vector2(1, 0);
        amountRect.anchorMax = new Vector2(1, 0);
        amountRect.pivot = new Vector2(1, 0);
        amountRect.anchoredPosition = new Vector2(-6, 6);
        amountRect.localScale = Vector3.one;
    }
}

