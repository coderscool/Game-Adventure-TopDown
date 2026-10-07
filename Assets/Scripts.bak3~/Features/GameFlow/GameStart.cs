using UnityEngine;

namespace Game.Features.GameFlow
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
