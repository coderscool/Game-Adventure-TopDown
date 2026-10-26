using UnityEngine;
using Game.Features.GameFlow.Managers;

namespace Game.Features.GameFlow.Views
{
    public class GameStart : MonoBehaviour
    {
        // Bound to a UnityEvent in the GameStart scene - keep the name.
        public void ContinueGame()
        {
            GameManager.Instance.LoadScene();
        }
    }
}
