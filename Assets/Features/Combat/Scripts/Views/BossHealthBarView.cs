using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Features.Combat.Systems;

namespace Game.Features.Combat.Views
{
    /// <summary>
    /// Boss health bar at the top of the screen. It builds itself and stays hidden until a boss fight starts,
    /// so no scene or prefab setup is needed.
    /// </summary>
    public class BossHealthBarView : MonoBehaviour
    {
        private const int CanvasSortOrder = 50;

        private static readonly Color BackColor = new Color(0f, 0f, 0f, 0.7f);
        private static readonly Color FillColor = new Color(0.8f, 0.15f, 0.15f, 1f);

        private GameObject _panel;
        private Image _fill;
        private TMP_Text _nameText;
        private BossController _boss;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Create()
        {
            if (FindFirstObjectByType<BossHealthBarView>() != null)
                return;

            var root = new GameObject("BossHealthBar");
            DontDestroyOnLoad(root);
            root.AddComponent<BossHealthBarView>();
        }

        private void Awake()
        {
            Build();
            _panel.SetActive(false);
        }

        private void OnEnable()
        {
            BossController.FightStarted += OnFightStarted;
            BossController.FightEnded += OnFightEnded;
        }

        private void OnDisable()
        {
            BossController.FightStarted -= OnFightStarted;
            BossController.FightEnded -= OnFightEnded;
            Unbind();
        }

        private void OnFightStarted(BossController boss)
        {
            Unbind();
            _boss = boss;
            _boss.BossHealth.Changed += Refresh;

            _nameText.text = boss.EnemyInfo != null ? boss.EnemyInfo.DisplayName : "Boss";
            _panel.SetActive(true);
            Refresh();
        }

        private void OnFightEnded(BossController boss)
        {
            if (boss != _boss)
                return;

            Unbind();
            _panel.SetActive(false);
        }

        private void Unbind()
        {
            if (_boss != null && _boss.BossHealth != null)
                _boss.BossHealth.Changed -= Refresh;

            _boss = null;
        }

        private void Refresh()
        {
            if (_boss == null)
                return;

            _fill.fillAmount = _boss.BossHealth.Max > 0f ? _boss.BossHealth.Current / _boss.BossHealth.Max : 0f;
        }

        private void Build()
        {
            var canvasObject = new GameObject("Canvas");
            canvasObject.transform.SetParent(transform, false);

            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = CanvasSortOrder;

            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            _panel = new GameObject("Panel", typeof(RectTransform));
            _panel.transform.SetParent(canvasObject.transform, false);

            var panelRect = (RectTransform)_panel.transform;
            panelRect.anchorMin = panelRect.anchorMax = new Vector2(0.5f, 1f);
            panelRect.pivot = new Vector2(0.5f, 1f);
            panelRect.anchoredPosition = new Vector2(0f, -40f);
            panelRect.sizeDelta = new Vector2(720f, 28f);

            Image back = _panel.AddComponent<Image>();
            back.sprite = Systems.PlaceholderSprites.Square;
            back.color = BackColor;
            back.raycastTarget = false;

            var fillObject = new GameObject("Fill", typeof(RectTransform));
            fillObject.transform.SetParent(_panel.transform, false);

            var fillRect = (RectTransform)fillObject.transform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(3f, 3f);
            fillRect.offsetMax = new Vector2(-3f, -3f);

            _fill = fillObject.AddComponent<Image>();
            _fill.sprite = Systems.PlaceholderSprites.Square;
            _fill.color = FillColor;
            _fill.type = Image.Type.Filled;
            _fill.fillMethod = Image.FillMethod.Horizontal;
            _fill.fillOrigin = 0;
            _fill.raycastTarget = false;

            var nameObject = new GameObject("Name", typeof(RectTransform));
            nameObject.transform.SetParent(_panel.transform, false);

            var nameRect = (RectTransform)nameObject.transform;
            nameRect.anchorMin = new Vector2(0f, 1f);
            nameRect.anchorMax = new Vector2(1f, 1f);
            nameRect.pivot = new Vector2(0.5f, 0f);
            nameRect.anchoredPosition = new Vector2(0f, 2f);
            nameRect.sizeDelta = new Vector2(0f, 32f);

            _nameText = nameObject.AddComponent<TextMeshProUGUI>();
            _nameText.alignment = TextAlignmentOptions.Center;
            _nameText.fontSize = 26f;
            _nameText.color = Color.white;
            _nameText.raycastTarget = false;
        }
    }
}
