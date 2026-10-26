using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Features.World
{
    public class SpawnPoint : MonoBehaviour
    {
        [FormerlySerializedAs("spawnID")]
        [SerializeField] private string _spawnId;

        public string SpawnId => _spawnId;
    }
}
