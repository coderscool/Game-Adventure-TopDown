using UnityEngine;

namespace Game.Features.Combat.Systems
{
    /// <summary>Plain white sprite made in code, for placeholder visuals such as the slash.</summary>
    public static class PlaceholderSprites
    {
        private static Sprite _square;

        /// <summary>A white square that is exactly 1 x 1 world units.</summary>
        public static Sprite Square
        {
            get
            {
                if (_square == null)
                {
                    var texture = new Texture2D(4, 4, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
                    var pixels = new Color[16];

                    for (int i = 0; i < pixels.Length; i++)
                        pixels[i] = Color.white;

                    texture.SetPixels(pixels);
                    texture.Apply();

                    _square = Sprite.Create(texture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
                    _square.name = "PlaceholderSquare";
                }

                return _square;
            }
        }
    }
}
