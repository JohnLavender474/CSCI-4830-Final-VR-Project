using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Network
{
    public class NetworkXRGrabInteractableOwnership : NetworkBehaviour
    {
        [SerializeField] private NetworkObject networkObject;
        [SerializeField] private XRGrabInteractable interactable;
        private ulong _clientId;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _clientId = NetworkManager.Singleton.LocalClientId;
            Debug.Log("[NetworkXRGrabInteractableOwnership] On network spawn called with client id = " + _clientId);
            interactable.selectEntered.AddListener(_ => OnGrabbed());
        }

        private void OnGrabbed()
        {
            RequestOwnershipServerRpc(_clientId);
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestOwnershipServerRpc(ulong clientId)
        {
            Debug.Log("[NetworkXRGrabInteractableOwnership] Requesting ownership from " + clientId);
            networkObject.ChangeOwnership(clientId);
        }
    }
}