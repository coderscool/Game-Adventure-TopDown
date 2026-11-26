using UnityEngine;
using Game.Features.Combat.Model;
using Game.Features.Save;

namespace Game.Features.Combat.Systems
{
    /// <summary>
    /// Remembers that this boss was defeated, so it does not come back when the scene is loaded again.
    /// Needs a BossController on the same object.
    /// </summary>
    [RequireComponent(typeof(BossController))]
    public class BossPersistence : SaveableBehaviour
    {
        private BossController _boss;
        private bool _defeated;

        private void Awake()
        {
            _boss = GetComponent<BossController>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (_boss != null)
                _boss.Defeated += OnDefeated;
        }

        private void OnDisable()
        {
            if (_boss != null)
                _boss.Defeated -= OnDefeated;
        }

        public override string CaptureState()
        {
            return JsonUtility.ToJson(new BossState { defeated = _defeated });
        }

        public override void RestoreState(string json)
        {
            if (string.IsNullOrEmpty(json))
                return;

            BossState state = JsonUtility.FromJson<BossState>(json);
            if (state == null || !state.defeated)
                return;

            _defeated = true;
            gameObject.SetActive(false);
        }

        private void OnDefeated(EnemyController enemy)
        {
            _defeated = true;
            RequestSceneSave();
        }
    }
}
