using UnityEngine;

namespace Slash
{
    // Shared procedural sprites for runtime-spawned entities. Uses a custom 1x1
    // white texture with Point filtering so UI Images stay sharp at any scale.
    public static class SpriteUtil
    {
        static Sprite _square;
        static Sprite _squareBottom;

        public static Sprite Square
        {
            get
            {
                if (_square != null) return _square;
                _square = Build(new Vector2(0.5f, 0.5f));
                return _square;
            }
        }

        public static Sprite SquareBottom
        {
            get
            {
                if (_squareBottom != null) return _squareBottom;
                _squareBottom = Build(new Vector2(0.5f, 0f));
                return _squareBottom;
            }
        }

        static Sprite Build(Vector2 pivot)
        {
            var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0f, 0f, 1f, 1f), pivot, 1f);
        }
    }
}
