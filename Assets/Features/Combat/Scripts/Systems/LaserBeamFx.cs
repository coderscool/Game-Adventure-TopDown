using UnityEngine;

namespace Game.Features.Combat.Systems
{
    /// <summary>
    /// The visuals of a boss laser: a thin warning line, the charge-up sparks and the beam itself.
    /// Lives in the world (not under the boss) so the boss scale does not stretch it.
    /// </summary>
    public class LaserBeamFx : MonoBehaviour
    {
        private const string SortingLayerName = "Characters";
        private const int SortingOrder = 29000;

        private static readonly Color WarningColor = new Color(1f, 0.2f, 0.2f, 0.3f);
        private static readonly Color BarColor = new Color(0.3f, 0.9f, 1f, 0.85f);

        private SpriteRenderer _bar;
        private SpriteRenderer _sprite;

        public static LaserBeamFx Create()
        {
            var fx = new GameObject("BossLaserFx").AddComponent<LaserBeamFx>();
            fx.Build();
            return fx;
        }

        private void Build()
        {
            _bar = CreateRenderer("Bar");
            _bar.sprite = PlaceholderSprites.Square;

            _sprite = CreateRenderer("Sprite");
            HideAll();
        }

        private SpriteRenderer CreateRenderer(string childName)
        {
            var child = new GameObject(childName);
            child.transform.SetParent(transform, false);

            var renderer = child.AddComponent<SpriteRenderer>();
            renderer.sortingLayerName = SortingLayerName;
            renderer.sortingOrder = SortingOrder;
            return renderer;
        }

        /// <summary>A flat coloured bar from origin along angle. Used for the warning and when there is no art.</summary>
        public void ShowBar(Vector2 origin, float angle, float length, float width, bool isWarning)
        {
            _bar.color = isWarning ? WarningColor : BarColor;
            Place(_bar.transform, origin + Direction(angle) * (length * 0.5f), angle, new Vector3(length, width, 1f));
            _bar.enabled = true;
        }

        /// <summary>One frame of art. scale stretches it; the sprite's pivot should sit on the beam start.</summary>
        public void ShowSprite(Sprite sprite, Vector2 origin, float angle, Vector2 scale)
        {
            _sprite.sprite = sprite;
            Place(_sprite.transform, origin, angle, new Vector3(scale.x, scale.y, 1f));
            _sprite.enabled = true;
        }

        public void HideSprite()
        {
            _sprite.enabled = false;
        }

        public void HideBar()
        {
            _bar.enabled = false;
        }

        public void HideAll()
        {
            _bar.enabled = false;
            _sprite.enabled = false;
        }

        private static void Place(Transform target, Vector2 position, float angle, Vector3 scale)
        {
            target.position = position;
            target.rotation = Quaternion.Euler(0f, 0f, angle);
            target.localScale = scale;
        }

        private static Vector2 Direction(float angle)
        {
            float radians = angle * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        }
    }
}
