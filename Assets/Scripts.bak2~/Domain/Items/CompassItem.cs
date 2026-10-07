using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Item/Compass")]
public class CompassItem : ItemData
{
    [FormerlySerializedAs("nextIsland")]
    [SerializeField] private string _nextIsland;

    public string NextIsland => _nextIsland;
}
