using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

namespace Network
{
    public class NetworkConnect : MonoBehaviour
    {
        public int maxConnection = 20;
        public UnityTransport transport;

        private Lobby _currentLobby;
        private float _heartBeatTimer;

        private async void Awake()
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            // TODO: switch off if dev wants player to choose to create or join
            JoinOrCreate();
        }

        private async void JoinOrCreate()
        {
            Debug.Log(" [NetworkConnect] Calling join or create lobby method. Will attempt to join a " +
                      "lobby if one exists. If one does not exist, then a new lobby will be created");
            var joined = await Join();
            if (joined) return;

            Debug.Log(" [NetworkConnect] Attempted to join lobby on awake but no lobby found. " +
                      "Falling back to creating lobby instead.");
            var created = await Create();
            if (!created)
            {
                Debug.LogError("[NetworkConnect] Failed to create lobby");
            }
        }

        public async Task<bool> Create()
        {
            try
            {
                var allocation = await RelayService.Instance.CreateAllocationAsync(maxConnection);
                var newJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

                transport.SetHostRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port,
                    allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData);

                var data = new Dictionary<string, DataObject>();

                var joinCodeData = new DataObject(DataObject.VisibilityOptions.Public, newJoinCode);
                data.Add("JOIN_CODE", joinCodeData);

                var lobbyOptions = new CreateLobbyOptions
                {
                    IsPrivate = false,
                    Data = data
                };


                // TODO: make lobby name more meaningful
                _currentLobby = await Lobbies.Instance.CreateLobbyAsync("Lobby Name", maxConnection, lobbyOptions);

                NetworkManager.Singleton.StartHost();

                Debug.Log("[NetworkConnect] Successfully started lobby with code: " + newJoinCode);
                return true;
            }
            catch (Exception ex)
            {
                Debug.Log(
                    " [ NetworkConnect ] Attempted to create lobby but caught exception. Exception is now being " +
                    "rethrown after being caught. Exception message: " + ex.Message);
                return false;
            }
        }

        public async Task<bool> Join()
        {
            try
            {
                // TODO: use other method to join lobby
                _currentLobby = await Lobbies.Instance.QuickJoinLobbyAsync();
                var joinCode = _currentLobby.Data["JOIN_CODE"].Value;

                Debug.Log("[ NetworkConnect ] Join lobby with code: " + joinCode);

                var allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

                transport.SetClientRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port,
                    allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData,
                    allocation.HostConnectionData);

                NetworkManager.Singleton.StartClient();
                return true;
            }
            catch (Exception ex)
            {
                Debug.Log(
                    " [NetworkConnect] Attempted to join lobby but caught exception. Exception is now being " +
                    "rethrown after being caught. Exception message: " + ex.Message);
                return false;
            }
        }

        private void Update()
        {
            if (_heartBeatTimer > 15)
            {
                _heartBeatTimer -= 15;
                if (_currentLobby != null && _currentLobby.HostId == AuthenticationService.Instance.PlayerId)
                {
                    LobbyService.Instance.SendHeartbeatPingAsync(_currentLobby.Id);
                }
            }

            _heartBeatTimer += Time.deltaTime;
        }
    }
}