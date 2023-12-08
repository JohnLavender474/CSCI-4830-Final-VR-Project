using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;

namespace Network
{
    public class NetworkConnect : MonoBehaviour
    {
        private const string JoinCode = "JOIN_CODE";

        public int maxConnection = 20;
        public UnityTransport transport;
        public TMP_Text myLobbyCodeText;
        public TMP_InputField lobbyCodeInputField;

        private Lobby _currentLobby;
        private float _heartBeatTimer;

        private async void Awake()
        {
            await UnityServices.InitializeAsync();
            AuthenticationService.Instance.SignedIn += () =>
            {
                Debug.Log("[NetworkConnect] Signed in with player id = " + AuthenticationService.Instance.PlayerId);
            };
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            QuickJoinOrCreateLobby();
        }

        public async void JoinByLobbyCodeInInputField()
        {
            var lobbyCode = lobbyCodeInputField.text.Trim();
            if (string.IsNullOrWhiteSpace(lobbyCode))
            {
                Debug.Log("[NetworkConnect] Join by code called but join code is empty");
                return;
            }

            var result = await JoinLobbyByCode(lobbyCode);
            if (result)
                Debug.Log("[NetworkConnect] Successfully joined lobby with code: " + lobbyCode);
            else
                Debug.LogError("[NetworkConnect] Failed to join lobby with code: " + lobbyCode);
        }

        private async Task<bool> JoinLobbyByCode(string lobbyCode)
        {
            try
            {
                var lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode);
                var joinCode = lobby.Data[JoinCode].Value;

                Debug.Log("[NetworkConnect] Joining lobby with join code = " + joinCode + " and lobby code = " +
                          lobbyCode);

                var allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
                transport.SetClientRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port,
                    allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData,
                    allocation.HostConnectionData);

                _currentLobby = lobby;

                NetworkManager.Singleton.StartClient();

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("[NetworkConnect] Failed to join lobby by code: " + lobbyCode + ". Exception: " +
                               e.Message);
                return false;
            }
        }

        private async void QuickJoinOrCreateLobby()
        {
            Debug.Log(" [NetworkConnect] Calling join or create lobby method. Will attempt to join a " +
                      "lobby if one exists. If one does not exist, then a new lobby will be created");
            var joined = await QuickJoinLobby();
            if (joined) return;

            Debug.Log(" [NetworkConnect] Attempted to join lobby on awake but no lobby found. " +
                      "Falling back to creating lobby instead.");
            var created = await CreateLobby();
            if (!created)
            {
                Debug.LogError("[NetworkConnect] Failed to create lobby");
            }
        }

        private async Task<bool> CreateLobby()
        {
            try
            {
                var allocation = await RelayService.Instance.CreateAllocationAsync(maxConnection);
                var newJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

                transport.SetHostRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port,
                    allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData);

                var data = new Dictionary<string, DataObject>();
                var joinCodeData = new DataObject(DataObject.VisibilityOptions.Public, newJoinCode);
                data.Add(JoinCode, joinCodeData);

                var lobbyOptions = new CreateLobbyOptions
                {
                    IsPrivate = false,
                    Data = data
                };

                _currentLobby = await Lobbies.Instance.CreateLobbyAsync("Lobby Name", maxConnection, lobbyOptions);
                NetworkManager.Singleton.StartHost();
                Debug.Log("[NetworkConnect] Successfully started lobby with lobby id = " + _currentLobby.Id +
                          ", join code = " + newJoinCode + ", and lobby code = " + _currentLobby.LobbyCode);
                return true;
            }
            catch (Exception ex)
            {
                Debug.Log(
                    " [NetworkConnect] Attempted to create lobby but caught exception. Exception is now being " +
                    "rethrown after being caught. Exception message: " + ex.Message);
                return false;
            }
        }

        private async Task<bool> QuickJoinLobby()
        {
            try
            {
                await UnityServices.InitializeAsync();
                _currentLobby = await Lobbies.Instance.QuickJoinLobbyAsync();
                var joinCode = _currentLobby.Data[JoinCode].Value;

                Debug.Log("[NetworkConnect] Quick joining lobby with join code: " + joinCode);

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
            if (_currentLobby == null) return;
            myLobbyCodeText.text = "My lobby code: " + _currentLobby?.LobbyCode;
            HandleLobbyHeartbeat();
        }

        private async void HandleLobbyHeartbeat()
        {
            _heartBeatTimer += Time.deltaTime;

            if (_heartBeatTimer < 15) return;

            _heartBeatTimer = 0;
            if (_currentLobby != null)
            {
                await LobbyService.Instance.SendHeartbeatPingAsync(_currentLobby.Id);
            }
        }
    }
}
