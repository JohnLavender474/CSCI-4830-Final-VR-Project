using System.Collections.Generic;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Game
{
    public class Spawns : MonoBehaviour
    {
        private static readonly Dictionary<string, Spawns> SpawnsMap = new();

        [SerializeField] private string spawnsKey;

        private readonly List<Vector3> _spawnPositions = new();
        private System.Random _random;

        public static Spawns GetSpawnsFor(string key)
        {
            return SpawnsMap[key];
        }

        private void Awake()
        {
            Debug.Log("[Spawns] Awake called");

            foreach (Transform child in transform)
            {
                _spawnPositions.Add(child.position);
            }

            Debug.Log("[Spawns] Size of spawns for " + spawnsKey + ": " + _spawnPositions.Count);
            _random = new System.Random();
            SpawnsMap.Add(spawnsKey, this);
        }

        public Vector3 GetRandomSpawn()
        {
            return _spawnPositions[_random.Next(0, _spawnPositions.Count)];
        }

        private void OnDestroy()
        {
            Debug.Log("[Spawns] On destroy for key: " + spawnsKey);
            SpawnsMap.Remove(spawnsKey);
        }
    }
}