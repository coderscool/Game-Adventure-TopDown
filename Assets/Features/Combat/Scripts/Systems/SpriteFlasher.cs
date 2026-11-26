using System.Collections;
using UnityEngine;

namespace Game.Features.Combat.Systems
{
    /// <summary>
    /// Colours every sprite under this object for hit feedback (flash) and attack warnings (hold).
    /// This is the stand-in for animations until real ones exist.
    /// </summary>
    public class SpriteFlasher : MonoBehaviour
    {
        private SpriteRenderer[] _renderers;
        private Color[] _baseColors;
        private Coroutine _flashRoutine;
        private bool _isFlashing;
        private bool _hasHold;
        private Color _holdColor;

        private void Awake()
        {
            _renderers = GetComponentsInChildren<SpriteRenderer>(true);
            _baseColors = new Color[_renderers.Length];

            for (int i = 0; i < _renderers.Length; i++)
                _baseColors[i] = _renderers[i].color;
        }

        /// <summary>Tints briefly, then goes back to the held colour (or the normal one).</summary>
        public void Flash(Color tint, float seconds)
        {
            if (_flashRoutine != null)
                StopCoroutine(_flashRoutine);

            _flashRoutine = StartCoroutine(FlashRoutine(tint, seconds));
        }

        /// <summary>Keeps a tint until ClearHold is called, e.g. the warning before an attack.</summary>
        public void SetHold(Color tint)
        {
            _hasHold = true;
            _holdColor = tint;

            if (!_isFlashing)
                Apply(tint);
        }

        public void ClearHold()
        {
            _hasHold = false;

            if (!_isFlashing)
                Restore();
        }

        private IEnumerator FlashRoutine(Color tint, float seconds)
        {
            _isFlashing = true;
            Apply(tint);

            yield return new WaitForSeconds(seconds);

            _isFlashing = false;
            _flashRoutine = null;

            if (_hasHold)
                Apply(_holdColor);
            else
                Restore();
        }

        private void Apply(Color tint)
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] != null)
                    _renderers[i].color = _baseColors[i] * tint;
            }
        }

        private void Restore()
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] != null)
                    _renderers[i].color = _baseColors[i];
            }
        }
    }
}
