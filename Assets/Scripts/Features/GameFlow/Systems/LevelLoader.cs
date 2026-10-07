using UnityEngine;
using Game.Features.GameFlow.Managers;

namespace Game.Features.GameFlow.Systems
{
    public class LevelLoader : MonoBehaviour
    {
        private const KeyCode LoadNextLevelKey = KeyCode.Z;

        private void Update()
        {
            if (Input.GetKeyDown(LoadNextLevelKey))
                LoadLevelNext();
        }

        public void LoadLevelNext()
        {
            GameManager.Instance.LoadScene();
        }
    }
}
