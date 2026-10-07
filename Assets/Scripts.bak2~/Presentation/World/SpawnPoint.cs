using UnityEngine;
using UnityEngine.Serialization;

public class SpawnPoint : MonoBehaviour
{
    [FormerlySerializedAs("spawnID")]
    [SerializeField] private string _spawnId;

    public string SpawnId => _spawnId;
}
