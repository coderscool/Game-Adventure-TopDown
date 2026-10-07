using UnityEngine;

public static class SaveGameService
{
    public static bool TrySaveCurrentPlayer()
    {
        if (Player.Instance == null)
        {
            Debug.LogWarning("SaveGame: Player.Instance is null.");
            return false;
        }

        Player.Instance.SavePlayer();
        return true;
    }
}
