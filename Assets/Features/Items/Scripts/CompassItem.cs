using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Features.Items
{
    [CreateAssetMenu(menuName = "Item/Compass")]
    public class CompassItem : ItemData
    {
        [FormerlySerializedAs("nextIsland")]
        [SerializeField] private string _nextIsland;

        public string NextIsland => _nextIsland;
    }
}
