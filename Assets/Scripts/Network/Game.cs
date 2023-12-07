using Game;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;

namespace Network
{
    public class Game : NetworkBehaviour
    {
        public static Game singleton { get; private set; }

        private const float StandardGameTime = 500f;

        public GameObject gunPrefab;
        [SerializeField] private string gunSpawnsKey;

        public NetworkVariable<float> timeRemaining { get; } = new();
        private bool _started;

        private void Awake()
        {
            Assert.IsNull(singleton, $"Multiple instances of {nameof(Game)} detected. " +
                                     "This should not be allowed.");
            singleton = this;
        }

        private void Update()
        {
            if (!IsOwner) return;
            
            var time = timeRemaining.Value - Time.deltaTime;
            if (IsServer)
                SetTimeRemaining(time);
            else
                SetTimeRemainingServerRpc(time);
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            Debug.Log("[Game] OnNetworkSpawn called");
            
            if (IsOwner && !_started) Start();
        }

        private void Start()
        {
            Debug.Log("[Game] Start called");

            if (!IsOwner)
            {
                Debug.Log("[Game] Start called but not owner");
                return;
            }

            if (_started)
            {
                Debug.Log("[Game] Start called but game already started");
                return;
            }

            Debug.Log("[Game] Starting game");
            _started = true;

            Debug.Log("[Game] Client: requesting time remaining");
            if (IsServer)
                SetTimeRemaining(StandardGameTime);
            else
                SetTimeRemainingServerRpc(StandardGameTime);

            for (var i = 0; i < 10; i++)
            {
                if (IsServer)
                    SpawnNewGun();
                else 
                    SpawnNewGunServerRpc();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void SetTimeRemainingServerRpc(float time)
        {
            // SetTimeRemainingClientRpc(time);
            SetTimeRemaining(time);
        }

        /*
        [ClientRpc]
        private void SetTimeRemainingClientRpc(float time)
        {
            if (IsServer) return;

            SetTimeRemaining(time);
        }
        */

        private void SetTimeRemaining(float time)
        {
            timeRemaining.Value = time;
        }

        [ServerRpc(RequireOwnership = false)]
        public void SpawnNewGunServerRpc()
        {
            Debug.Log("[Game] SpawnNewGunServerRpc: requesting new gun");
            // SpawnNewGunClientRpc();
            SpawnNewGun();
        }

        /*
        [ClientRpc]
        public void SpawnNewGunClientRpc()
        {
            if (IsServer) return;

            SpawnNewGun();
        }
        */

        public void SpawnNewGun()
        {
            Debug.Log("[Game] Spawning new gun");
            var spawnPosition = Spawns.GetSpawnsFor(gunSpawnsKey).GetRandomSpawn();
            Instantiate(gunPrefab, spawnPosition, Quaternion.identity).GetComponent<NetworkObject>().Spawn();
        }
    }
}