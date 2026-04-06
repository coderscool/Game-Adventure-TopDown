using UnityEngine;

public class GameStart : MonoBehaviour
{
    public void ContinueGame()
    {
        Debug.Log("Continue");
        GameManager.Instance.LoadScene();
    }
}
