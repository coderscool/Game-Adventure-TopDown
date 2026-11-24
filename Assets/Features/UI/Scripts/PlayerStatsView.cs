using Game.Features.Character.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Features.UI
{
    /// <summary>
    /// HUD: shows Hp / Energy / Spirit bars and gold. Bars are Images with Image Type = Filled.
    /// Updates only when Player.StatsChanged fires (no per-frame polling).
    /// </summary>
    public class PlayerStatsView : MonoBehaviour
    {
        [Header("Bars (Image Type = Filled)")]
        [SerializeField] private Image _hpFill;
        [SerializeField] private Image _energyFill;
        [SerializeField] private Image _spiritFill;

        [Header("Optional texts")]
        [SerializeField] private TMP_Text _hpText;
        [SerializeField] private TMP_Text _energyText;
        [SerializeField] private TMP_Text _spiritText;
        [SerializeField] private TMP_Text _goldText;

        private Player _player;

        private void OnEnable()
        {
            Bind();
        }

        private void Start()
        {
            // Player may not exist yet at OnEnable (bootstrap order); retry once.
            if (_player == null) Bind();
        }

        private void OnDisable()
        {
            if (_player != null) _player.StatsChanged -= Refresh;
            _player = null;
        }

        private void Bind()
        {
            if (_player != null || Player.Instance == null) return;
            _player = Player.Instance;
            _player.StatsChanged += Refresh;
            Refresh();
        }

        private void Refresh()
        {
            SetBar(_hpFill, _hpText, _player.Hp, _player.MaxHp);
            SetBar(_energyFill, _energyText, _player.Energy, _player.MaxEnergy);
            SetBar(_spiritFill, _spiritText, _player.Spirit, _player.MaxSpirit);
            if (_goldText != null) _goldText.text = _player.Gold.ToString();
        }

        private static void SetBar(Image fill, TMP_Text text, float value, float max)
        {
            if (fill != null) fill.fillAmount = max > 0f ? value / max : 0f;
            if (text != null) text.text = $"{Mathf.CeilToInt(value)}/{Mathf.CeilToInt(max)}";
        }
    }
}
