using Unity.Netcode;
using UnityEngine;

namespace Network.Health___Damage
{
    public class NetworkPlayerHealthScript : NetworkBehaviour
    {
        public const int MaxHealth = 100;

        public readonly NetworkVariable<int> currentHealth = new(MaxHealth);

        private void Update()
        {
            if (!IsServer) return;
            BoundHealth();
        }

        private void BoundHealth()
        {
            if (!IsServer)
            {
                Debug.LogError("[NetworkPlayerHealthScript] BoundHealth can only be called on server!");
                return;
            }

            if (currentHealth.Value > MaxHealth)
            {
                currentHealth.Value = MaxHealth;
            }

            if (currentHealth.Value < 0)
            {
                currentHealth.Value = 0;
            }
        }

        public float GetHealth()
        {
            return currentHealth.Value;
        }

        public void SetHealth(int health)
        {
            if (!IsServer)
            {
                Debug.LogError("[NetworkPlayerHealthScript] SetHealth can only be called on server!");
                return;
            }

            currentHealth.Value = health;
            BoundHealth();
        }

        public void TranslateHealth(int amount)
        {
            if (!IsServer)
            {
                Debug.LogError("[NetworkPlayerHealthScript] TranslateHealth can only be called on server!");
                return;
            }

            currentHealth.Value += amount;
            BoundHealth();
        }

        public bool IsDead()
        {
            return currentHealth.Value <= 0;
        }
    }
}