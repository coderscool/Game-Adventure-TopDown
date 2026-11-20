namespace Game.Features.Character.EditorTools
{
    /// <summary>One facing direction of the player art. East/West have no sprites (6-direction art).</summary>
    internal readonly struct PlayerDirection
    {
        public readonly string ClipSuffix;   // Idle_{ClipSuffix}
        public readonly string FileSuffix;   // idle_{FileSuffix}.png

        public PlayerDirection(string clipSuffix, string fileSuffix)
        {
            ClipSuffix = clipSuffix;
            FileSuffix = fileSuffix;
        }
    }

    /// <summary>Single place for the paths and sprite-sheet layout used by the player animation tools.</summary>
    internal static class PlayerAnimationAssets
    {
        public const string CharacterRoot = "Assets/Features/Character";
        public const string ControllerPath = CharacterRoot + "/Animations/Player.controller";

        public const int FrameCount = 8;
        public const int FrameWidth = 48;
        public const int FrameHeight = 64;
        public const float FramesPerSecond = 12f;
        public const float PixelsPerUnit = 16f;

        public static readonly string[] Kinds = { "Idle", "Walk" };

        public static readonly PlayerDirection[] Directions =
        {
            new PlayerDirection("Down", "down"),
            new PlayerDirection("Up", "up"),
            new PlayerDirection("LeftDown", "left_down"),
            new PlayerDirection("LeftUp", "left_up"),
            new PlayerDirection("RightDown", "right_down"),
            new PlayerDirection("RightUp", "right_up"),
        };

        public static string TexturePath(string kind, PlayerDirection direction)
        {
            return $"{CharacterRoot}/Sprites/{kind}/{kind.ToLowerInvariant()}_{direction.FileSuffix}.png";
        }

        public static string ClipPath(string kind, string clipSuffix)
        {
            return $"{CharacterRoot}/Animations/{kind}_{clipSuffix}.anim";
        }
    }
}
