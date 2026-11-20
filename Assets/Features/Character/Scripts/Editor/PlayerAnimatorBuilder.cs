using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Game.Features.Character.EditorTools
{
    /// <summary>Rebuilds Player.controller: MoveX/MoveY/IsMoving, an Idle and a Walk blend tree, and the transitions.</summary>
    internal static class PlayerAnimatorBuilder
    {
        private const string MoveX = "MoveX";
        private const string MoveY = "MoveY";
        private const string IsMoving = "IsMoving";
        private const string IdleKind = "Idle";
        private const string WalkKind = "Walk";

        private const float Diagonal = 0.7071f;
        private const float StateRowSpacing = 120f;

        // Input direction -> clip. East/West reuse the "down" diagonal art because there are no side sprites.
        private static readonly (Vector2 position, string clipSuffix)[] Layout =
        {
            (new Vector2(0f, 1f), "Up"),
            (new Vector2(Diagonal, Diagonal), "RightUp"),
            (new Vector2(1f, 0f), "RightDown"),
            (new Vector2(Diagonal, -Diagonal), "RightDown"),
            (new Vector2(0f, -1f), "Down"),
            (new Vector2(-Diagonal, -Diagonal), "LeftDown"),
            (new Vector2(-1f, 0f), "LeftDown"),
            (new Vector2(-Diagonal, Diagonal), "LeftUp"),
        };

        public static bool Rebuild()
        {
            // Check every clip exists before touching the controller.
            if (!TryLoadClips(IdleKind, out AnimationClip[] idleClips)
                || !TryLoadClips(WalkKind, out AnimationClip[] walkClips))
            {
                return false;
            }

            AnimatorController controller = LoadOrCreateController();
            Clear(controller);
            AddParameters(controller);

            AnimatorState idle = AddBlendState(controller, IdleKind, idleClips);
            AnimatorState walk = AddBlendState(controller, WalkKind, walkClips);
            AddTransition(idle, walk, AnimatorConditionMode.If);
            AddTransition(walk, idle, AnimatorConditionMode.IfNot);

            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
            stateMachine.defaultState = idle;
            LayoutStates(stateMachine);

            EditorUtility.SetDirty(controller);
            return true;
        }

        private static bool TryLoadClips(string kind, out AnimationClip[] clips)
        {
            clips = new AnimationClip[Layout.Length];

            for (int i = 0; i < Layout.Length; i++)
            {
                string path = PlayerAnimationAssets.ClipPath(kind, Layout[i].clipSuffix);
                clips[i] = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);

                if (clips[i] == null)
                {
                    Debug.LogError($"Missing clip: {path}");
                    return false;
                }
            }

            return true;
        }

        private static AnimatorController LoadOrCreateController()
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(PlayerAnimationAssets.ControllerPath);

            return controller != null
                ? controller
                : AnimatorController.CreateAnimatorControllerAtPath(PlayerAnimationAssets.ControllerPath);
        }

        private static void Clear(AnimatorController controller)
        {
            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;

            foreach (ChildAnimatorState child in stateMachine.states)
                stateMachine.RemoveState(child.state);

            // Blend trees are sub-assets of the controller; removing a state can leave them behind.
            string path = AssetDatabase.GetAssetPath(controller);
            foreach (Object subAsset in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                if (subAsset is BlendTree)
                    Object.DestroyImmediate(subAsset, true);
            }

            foreach (AnimatorControllerParameter parameter in controller.parameters)
                controller.RemoveParameter(parameter);
        }

        private static void AddParameters(AnimatorController controller)
        {
            controller.AddParameter(MoveX, AnimatorControllerParameterType.Float);

            // Face down by default so the first frame matches the Down clips.
            controller.AddParameter(new AnimatorControllerParameter
            {
                name = MoveY,
                type = AnimatorControllerParameterType.Float,
                defaultFloat = -1f,
            });

            controller.AddParameter(IsMoving, AnimatorControllerParameterType.Bool);
        }

        private static AnimatorState AddBlendState(AnimatorController controller, string stateName, AnimationClip[] clips)
        {
            AnimatorState state = controller.CreateBlendTreeInController(stateName, out BlendTree tree, 0);

            tree.blendType = BlendTreeType.FreeformDirectional2D;
            tree.blendParameter = MoveX;
            tree.blendParameterY = MoveY;
            tree.useAutomaticThresholds = false;

            for (int i = 0; i < Layout.Length; i++)
                tree.AddChild(clips[i], Layout[i].position);

            return state;
        }

        private static void AddTransition(AnimatorState from, AnimatorState to, AnimatorConditionMode mode)
        {
            AnimatorStateTransition transition = from.AddTransition(to);
            transition.hasExitTime = false;
            transition.hasFixedDuration = true;
            transition.duration = 0f;
            transition.AddCondition(mode, 0f, IsMoving);
        }

        private static void LayoutStates(AnimatorStateMachine stateMachine)
        {
            ChildAnimatorState[] children = stateMachine.states;

            for (int i = 0; i < children.Length; i++)
                children[i].position = new Vector3(250f, 50f + i * StateRowSpacing, 0f);

            stateMachine.states = children;
        }
    }
}
