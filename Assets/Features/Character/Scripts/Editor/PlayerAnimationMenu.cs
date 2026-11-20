using UnityEditor;
using UnityEngine;

namespace Game.Features.Character.EditorTools
{
    internal static class PlayerAnimationMenu
    {
        private const string DialogTitle = "Build Player Animations";

        private const string DialogMessage =
            "This will:\n" +
            "1. Slice the 12 Idle/Walk strips (48x64, 8 frames, Center pivot, 16 PPU).\n" +
            "2. Create the missing Idle_*/Walk_* clips (existing clips are kept).\n" +
            "3. REBUILD Player.controller (existing states and parameters are replaced).\n\n" +
            "Continue?";

        [MenuItem("Tools/Character/Build Player Animations")]
        private static void BuildPlayerAnimations()
        {
            if (!EditorUtility.DisplayDialog(DialogTitle, DialogMessage, "Build", "Cancel"))
                return;

            if (!PlayerSpriteSlicer.SliceAll())
            {
                Debug.LogError("Build stopped: slicing failed. See the errors above.");
                return;
            }

            if (!PlayerClipBuilder.BuildAll())
            {
                Debug.LogError("Build stopped: creating clips failed. See the errors above.");
                return;
            }

            if (!PlayerAnimatorBuilder.Rebuild())
            {
                Debug.LogError("Build stopped: rebuilding the animator failed. See the errors above.");
                return;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Player animations built. Assign Player.controller to the Player's Animator.");
        }
    }
}
