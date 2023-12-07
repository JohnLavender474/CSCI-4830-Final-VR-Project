using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Network.Health___Damage
{
    public class NetworkPlayerDamageLogicScript : NetworkBehaviour
    {
        [SerializeField] public NetworkPlayerHealthScript playerHealthScript;

        [SerializeField] public List<MeshRenderer> playerMeshRenderers;
        [SerializeField] public Material materialOnHit;
        [SerializeField] public float damageTime;

        private bool _justHit;
        private int _damageToTake;

        private bool _hit;
        private float _timer;
        private readonly Dictionary<MeshRenderer, Material> _materialsBeforeHit = new();

        public override void OnNetworkSpawn()
        {
            Debug.Log("[NetworkPlayerDamageLogicScript] On network spawn called");
            base.OnNetworkSpawn();
        }
        
        public void SetHit(int damage)
        {
            _justHit = true;
            _damageToTake = damage;
            
            _hit = true;
            _timer = damageTime;
            
            if (playerHealthScript.IsDead())
            {
                Debug.Log("[NetworkPlayerDamageLogicScript] Player is dead");
                // TODO: do something on death
                return;
            }
            
            foreach (var meshRenderer in playerMeshRenderers)
            {
                _materialsBeforeHit.Add(meshRenderer, meshRenderer.material);
                meshRenderer.material = materialOnHit;
            }
        }
        
        public bool IsHit()
        {
            return _hit;
        }

        private void Update()
        {
            if (!IsServer || !IsHit()) return;

            if (_justHit)
            {
                playerHealthScript.TranslateHealth(_damageToTake);
                _justHit = false;
            }

            _timer -= Time.deltaTime;
            if (_timer > 0) return;

            _hit = false;
            foreach (var (meshRenderer, material) in _materialsBeforeHit)
            {
                meshRenderer.material = material;
            }
            _materialsBeforeHit.Clear();
        }
    }
}