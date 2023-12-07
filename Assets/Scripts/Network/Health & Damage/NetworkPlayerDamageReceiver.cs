using Game;
using Unity.Netcode;
using UnityEngine;

namespace Network.Health___Damage
{
    public class NetworkPlayerDamageReceiver : NetworkBehaviour
    {
        [SerializeField] public NetworkPlayer player;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            Debug.Log("[NetworkPlayerDamageReceiverScript] On network spawn called");
        }

        public void OnTriggerEnter(Collider other)
        {
            var damager = other.gameObject.GetComponent<Damager>();

            if (!damager)
            {
                Debug.Log("[NetworkPlayerDamageReceiverScript] No damager found");
                return;
            }

            Debug.Log("[NetworkPlayerDamageReceiverScript] Taking damage");
            player.TakeDamage(damager.damage);
        }
    }
}