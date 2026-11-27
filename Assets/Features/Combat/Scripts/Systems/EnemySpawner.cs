using System.Collections.Generic;
using UnityEngine;
using Game.Features.Character.Systems;
using Game.Features.Combat.Data;

namespace Game.Features.Combat.Systems
{
    /// <summary>
    /// Keeps a few roaming enemies along a path. Put it on an empty object and add child objects
    /// as waypoints (or fill Path Points): enemies appear at random places along the line through them.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private EnemyController _enemyPrefab;
        [Tooltip("Optional. Overrides the data on the prefab, so one prefab can be many kinds of enemy.")]
        [SerializeField] private EnemyData _dataOverride;

        [Header("Area (use this OR the path below)")]
        [Tooltip("A trigger collider (Box or Polygon) that marks the area. Enemies appear inside it and never leave it.")]
        [SerializeField] private Collider2D _zone;

        [Header("Path")]
        [Tooltip("Leave empty to use the child objects of this spawner, in order.")]
        [SerializeField] private Transform[] _pathPoints;
        [Tooltip("Enemies appear up to this far from the exact line.")]
        [SerializeField] private float _sideSpread = 0.6f;

        [Header("Rules")]
        [Min(1)]
        [SerializeField] private int _maxAlive = 3;
        [SerializeField] private float _respawnSeconds = 10f;
        [Tooltip("Never spawns an enemy closer to the player than this.")]
        [SerializeField] private float _minPlayerDistance = 6f;
        [Tooltip("Does nothing while the player is farther than this.")]
        [SerializeField] private float _activeDistance = 30f;

        private readonly List<EnemyController> _alive = new List<EnemyController>();
        private float _nextSpawnTime;

        private void Start()
        {
            _nextSpawnTime = Time.time;
        }

        private void Update()
        {
            if (_alive.Count >= _maxAlive || Time.time < _nextSpawnTime || _enemyPrefab == null)
                return;

            Player player = Player.Instance;
            if (player == null)
                return;

            float playerDistance = Vector2.Distance(player.transform.position, transform.position);
            if (playerDistance > _activeDistance)
                return;

            Vector2 point = _zone != null ? RandomZonePoint() : RandomPathPoint();

            if (Vector2.Distance(point, player.transform.position) < _minPlayerDistance)
            {
                // Try again soon rather than waiting a full respawn time.
                _nextSpawnTime = Time.time + 1f;
                return;
            }

            Spawn(point);
            _nextSpawnTime = Time.time + (_alive.Count >= _maxAlive ? _respawnSeconds : 0.5f);
        }

        private void Spawn(Vector2 position)
        {
            EnemyController enemy = Instantiate(_enemyPrefab, position, Quaternion.identity, transform.parent);

            if (_dataOverride != null)
                enemy.Initialize(_dataOverride);

            if (_zone != null)
                enemy.SetZone(_zone);

            _alive.Add(enemy);
            enemy.Defeated += OnEnemyDefeated;
        }

        private void OnEnemyDefeated(EnemyController enemy)
        {
            enemy.Defeated -= OnEnemyDefeated;
            _alive.Remove(enemy);
            _nextSpawnTime = Mathf.Max(_nextSpawnTime, Time.time + _respawnSeconds);
        }

        private List<Vector2> GetPath()
        {
            var points = new List<Vector2>();

            if (_pathPoints != null && _pathPoints.Length > 0)
            {
                foreach (Transform point in _pathPoints)
                {
                    if (point != null)
                        points.Add(point.position);
                }
            }
            else
            {
                foreach (Transform child in transform)
                    points.Add(child.position);
            }

            if (points.Count == 0)
                points.Add(transform.position);

            return points;
        }

        private Vector2 RandomZonePoint()
        {
            Bounds bounds = _zone.bounds;

            for (int i = 0; i < 30; i++)
            {
                var point = new Vector2(
                    Random.Range(bounds.min.x, bounds.max.x),
                    Random.Range(bounds.min.y, bounds.max.y));

                if (_zone.OverlapPoint(point))
                    return point;
            }

            return bounds.center;
        }

        private Vector2 RandomPathPoint()
        {
            List<Vector2> path = GetPath();
            Vector2 point = path[0];

            if (path.Count > 1)
            {
                int segment = Random.Range(0, path.Count - 1);
                point = Vector2.Lerp(path[segment], path[segment + 1], Random.value);
            }

            return point + Random.insideUnitCircle * _sideSpread;
        }

        private void OnDrawGizmos()
        {
            if (_zone != null)
            {
                Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.9f);
                Gizmos.DrawWireCube(_zone.bounds.center, _zone.bounds.size);
                return;
            }

            List<Vector2> path = GetPath();
            Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.9f);

            for (int i = 0; i < path.Count; i++)
            {
                Gizmos.DrawWireSphere(path[i], 0.25f);

                if (i + 1 < path.Count)
                    Gizmos.DrawLine(path[i], path[i + 1]);
            }
        }
    }
}
