using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            LoadLevelNext();
        }
    }

    public void LoadLevelNext()
    {
        GameManager.Instance.LoadScene();
    }
}
