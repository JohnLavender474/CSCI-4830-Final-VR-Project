using System;
using System.Collections;
using Game;
using Network.Health___Damage;
using Siccity.GLTFUtility;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Networking;

namespace Network
{
    [RequireComponent(typeof(NetworkObject))]
    public class NetworkPlayer : NetworkBehaviour
    {
        private const string AvatarSpine = "Hips/Spine";
        private const string AvatarNeck = AvatarSpine + "/Neck";
        private const string AvatarHead = AvatarNeck + "/Head";

        private const int MaxLives = 3;
        private const int MaxHealth = 100;

        // TODO: rpm netcode sample
        /*
        private const string FULL_BODY_LEFT_EYE_BONE_NAME = "Armature/Hips/Spine/Spine1/Spine2/Neck/Head/LeftEye";
        private const string FULL_BODY_RIGHT_EYE_BONE_NAME = "Armature/Hips/Spine/Spine1/Spine2/Neck/Head/RightEye";

        public static string InputUrl = string.Empty;
        public NetworkVariable<FixedString64Bytes> avatarUrl = new(writePerm: NetworkVariableWritePermission.Owner);
        public event Action OnPlayerLoadComplete;
        */

        [SerializeField] private string playerSpawnsKey;
        [SerializeField] private Transform root;
        [SerializeField] private Transform body;
        [SerializeField] private Transform head;
        [SerializeField] private Transform leftHand;
        [SerializeField] private Transform rightHand;

        // for now, avatar url is hardcoded; should allow player to change this value
        // in which case this will need to be a network variable
        public string avatarUrl = "https://models.readyplayer.me/6570ff98869b42cd90a10bb6.glb";

        // the game object for the avatar
        private GameObject _avatar;

        // TODO: rpm netcode sample
        /*
        [SerializeField] private Transform leftEye;
        [SerializeField] private Transform rightEye;

        private Animator _animator;
        private SkinnedMeshRenderer[] _skinnedMeshRenderers;
        */

        // the meshes that should be disabled when the player is not the owner
        [SerializeField] private Renderer[] meshToDisable;

        // the meshes whose materials should change when the player takes damage
        [SerializeField] private Renderer[] meshesChangedOnDamage;

        // the material to use when the player takes damage
        [SerializeField] private Material damagedMaterial;

        // the duration to show the damaged material
        [SerializeField] private float damageDuration = 1;

        // when the player takes damage, the materials are changed to damagedMaterial, and the old
        // materials are stored in this variable so that they can be restored when the damage time
        // is over
        private Material[] _materialsBeforeDamage;

        // current health of the player
        private readonly NetworkVariable<int> _currentHealth = new(MaxHealth);

        //  when lives = 0, the player should no longer be able to respawn
        //  when all players on a team have reached 0 lives, the game should end
        //  with the other team winning 
        private readonly NetworkVariable<int> _lives = new(MaxLives, NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);

        // damage time is used to determine when to stop showing the red mesh
        private readonly NetworkVariable<float> _damageTime = new();

        // team number is assigned on spawn
        private readonly NetworkVariable<int> _team = new();

        private void Awake()
        {
            StartCoroutine(LoadAvatar());

            // TODO: rpm netcode sample
            /*
            _animator = GetComponent<Animator>();

            leftEye = transform.Find(FULL_BODY_LEFT_EYE_BONE_NAME);
            rightEye = transform.Find(FULL_BODY_RIGHT_EYE_BONE_NAME);

            _skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
            */
        }

        private void Update()
        {
            // avatar transforms are mapped to in-script transforms, NOT to rig reference transforms 
            if (_avatar != null)
            {
                var avatarHead = _avatar.transform.Find(AvatarHead);
                var avatarNeck = _avatar.transform.Find(AvatarNeck);
                var avatarLeftHand = _avatar.transform.Find(AvatarSpine + "/LeftHand");
                var avatarRightHand = _avatar.transform.Find(AvatarSpine + "/RightHand");
                var avatarLeftEye = _avatar.transform.Find(AvatarHead + "/LeftEye");
                var avatarRightEye = _avatar.transform.Find(AvatarHead + "/RightEye");

                // set head and neck transforms

                SetTransform(avatarHead, head);

                var neckOffset = new Vector3(0f, -0.015f, 0f);
                SetTransform(avatarNeck, head, neckOffset);

                var neckXZ = avatarNeck.forward;
                neckXZ.y = 0;
                neckXZ.Normalize();

                var headXZ = avatarHead.forward;
                headXZ.y = 0;
                headXZ.Normalize();

                var headNeckOffset = Vector3.SignedAngle(neckXZ, headXZ, Vector3.up);

                if (Mathf.Abs(headNeckOffset) > 40)
                {
                    _avatar.transform.Rotate(Vector3.up, headNeckOffset * Time.deltaTime);
                    avatarHead.rotation = head.rotation;
                }

                // set left and right eye transforms, and then move avatar to compensate for offset

                var pos = (avatarLeftEye.position + avatarRightEye.position) / 2;
                var offset = avatarHead.position - pos;
                _avatar.transform.position = offset;

                // set left hand and right hand positions

                SetTransform(avatarLeftHand, leftHand);
                SetTransform(avatarRightHand, rightHand);
            }

            // if not the owner, then return early
            if (!IsOwner) return;

            if (_damageTime.Value > 0f)
                DecrementDamageTimeServerRpc(Time.deltaTime);

            var reference = VRRigReferences.singleton;
            SetTransform(root, reference.root);
            SetTransform(body, reference.body);
            SetTransform(head, reference.head);
            SetTransform(leftHand, reference.leftHand);
            SetTransform(rightHand, reference.rightHand);
        }

        public void SetAvatar(string url)
        {
            avatarUrl = url;
            StartCoroutine(LoadAvatar());
        }

        private IEnumerator LoadAvatar()
        {
            /*
            var temp = new GameObject
            {
                name = "avatar",
                transform =
                {
                    parent = transform,
                    localPosition = Vector3.zero,
                    localRotation = Quaternion.identity
                }
            };
            */

            if (_avatar != null)
            {
                Destroy(_avatar);
                _avatar = null;
            }

            // TODO: if we change from glt-utility to glt-fast, we need to replace the code below with this
            /*
             GltfAsset gltf = gameObject.AddComponent<GltfAsset>();
             ImportSetting importSettings = new ImportSetting();
             importSettings.AnimationMethod = AnimationMethodType.Mecanim;
             gltf.ImportSettings = importSettings;

             var t = gltf.Load(avatarUrl);
             while (!t.IsCompleted)
             {
                 yield return null;
             }
             if (t.IsCompletedSuccessfully) {
                avatar = this.transform.Find("AvatarRoot").gameObject;
                this.GateComponent<Animator>().runtimeAnimatorController = controller;
             }

             yield return null;
             */

            using var webRequest = UnityWebRequest.Get(avatarUrl);
            yield return webRequest.SendWebRequest();

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError("Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError("HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log("nReceived: " + webRequest.downloadHandler.data.Length);
                    _avatar = Importer.LoadFromBytes(webRequest.downloadHandler.data);
                    break;
                case UnityWebRequest.Result.InProgress:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void OnNetworkSpawn()
        {
            Debug.Log("[NetworkPlayer] On network spawn called");
            base.OnNetworkSpawn();

            if (!IsOwner) return;
            // TODO: rpm netcode sample
            /*
            avatarUrl.Value = InputUrl;
            LoadAvatar(InputUrl);
            */

            Respawn();

            Debug.Log("[NetworkPlayer] Owner: OnNetworkSpawn called");
            foreach (var mesh in meshToDisable)
            {
                mesh.enabled = false;
            }

            _currentHealth.OnValueChanged += OnHealthChange;
            Game.singleton.timeRemaining.OnValueChanged += OnTimeRemainingChange;

            // TODO: rpm netcode sample
            /*
            else if (Uri.IsWellFormedUriString(avatarUrl.Value.ToString(), UriKind.Absolute))
            {
                LoadAvatar(avatarUrl.Value.ToString());
            }

            avatarUrl.OnValueChanged += (_, newValue) => { LoadAvatar(newValue.ToString()); };
            */
        }

        public override void OnNetworkDespawn()
        {
            Debug.Log("[NetworkPlayer] On network despawn called");
            base.OnNetworkDespawn();

            // if this is the owner, then enable the meshes and unsubscribe from health changes
            if (!IsOwner) return;

            Debug.Log("[NetworkPlayer] Owner so disabling meshes");
            foreach (var mesh in meshToDisable)
            {
                mesh.enabled = true;
            }

            _currentHealth.OnValueChanged -= OnHealthChange;
            Game.singleton.timeRemaining.OnValueChanged -= OnTimeRemainingChange;
        }

        private bool IsDamaged()
        {
            return _damageTime.Value > 0f;
        }

        [ServerRpc(RequireOwnership = false)]
        private void UnsetDamagedServerRpc()
        {
            if (!IsServer) throw new Exception("[NetworkPlayer] UnsetDamagedServerRpc must be called on server");

            Debug.Log("[NetworkPlayer] UnsetDamagedServerRpc called");
            _damageTime.Value = 0f;

            UnsetDamagedClientRpc();
        }

        [ClientRpc]
        private void UnsetDamagedClientRpc()
        {
            Debug.Log("[NetworkPlayer] UnSetDamagedClientRpc called");
            for (var i = 0; i < meshesChangedOnDamage.Length; i++)
            {
                meshesChangedOnDamage[i].material = _materialsBeforeDamage[i];
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void DecrementDamageTimeServerRpc(float time)
        {
            _damageTime.Value -= time;
            if (_damageTime.Value <= 0f)
            {
                UnsetDamagedServerRpc();
            }
        }

        public void TakeDamage(int damage)
        {
            Debug.Log("[NetworkPlayer] TakeDamage: Taking damage");

            if (IsServer)
            {
                Debug.Log("[NetworkPlayer] TakeDamage: Server: taking damage");
                OnTakeDamageClientRpc(damage);
            }
            else
            {
                Debug.Log("[NetworkPlayer] TakeDamage: Client: taking damage");
                OnTakeDamageServerRpc(damage);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void SetHealthServerRpc(int health)
        {
            _currentHealth.Value = health;
        }

        [ServerRpc(RequireOwnership = false)]
        private void OnTakeDamageServerRpc(int damage)
        {
            OnTakeDamageClientRpc(damage);
        }

        [ClientRpc]
        private void OnTakeDamageClientRpc(int damage)
        {
            Debug.Log("[NetworkPlayer] OnTakeDamage: Called");

            // if the player is already damaged, then return early
            if (IsDamaged())
            {
                Debug.Log("[NetworkPlayer] TakeDamage: Already damaged so returning early");
                return;
            }

            Debug.Log("[NetworkPlayer] TakeDamage: Not damaged so taking damage");
            // apply damage and set damage time only on the server
            if (IsOwner)
            {
                DeductHealthAndRestartDamageTimerServerRpc(damage);
            }

            // change the materials to damaged on the client
            _materialsBeforeDamage = new Material[meshesChangedOnDamage.Length];
            for (var i = 0; i < meshesChangedOnDamage.Length; i++)
            {
                _materialsBeforeDamage[i] = meshesChangedOnDamage[i].material;
                meshesChangedOnDamage[i].material = damagedMaterial;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void DeductHealthAndRestartDamageTimerServerRpc(int damage)
        {
            _currentHealth.Value -= damage;
            _damageTime.Value = damageDuration;
        }

        private void OnHealthChange(int previousValue, int newValue)
        {
            Debug.Log("[NetworkPlayer] Health changed from " + previousValue + " to " + newValue);

            VRRigReferences.singleton.healthbarImage.fillAmount =
                (float)newValue / NetworkPlayerHealthScript.MaxHealth;
            VRRigReferences.singleton.timeRemainingText.text = "Health: " + newValue;

            if (newValue > 0) return;

            Debug.Log("[NetworkPlayer] Player died");

            _lives.Value--;
            Debug.Log("[NetworkPlayer] Player lives: " + _lives.Value);
            if (_lives.Value <= 0)
                Debug.Log("[NetworkPlayer] Player has no lives left");
            else
                Respawn();
        }

        private static void OnTimeRemainingChange(float previousValue, float newValue)
        {
            VRRigReferences.singleton.timeRemainingText.text = "Time: " + (int)newValue;
        }

        // TODO: perform on owner instead of server?
        private void Respawn()
        {
            var spawn = Spawns.GetSpawnsFor(playerSpawnsKey).GetRandomSpawn();
            VRRigReferences.singleton.SetPosition(spawn);
            SetHealthServerRpc(MaxHealth);
            /*
            Debug.Log("[NetworkPlayer] Respawning player");
            RespawnServerRpc();
            */
        }

        // TODO: perform on owner instead of server?
        /*
        [ServerRpc]
        private void RespawnServerRpc()
        {
            currentHealth.Value = NetworkPlayerHealthScript.MaxHealth;
            RespawnClientRpc();
        }

        [ClientRpc]
        private void RespawnClientRpc()
        {
            if (!IsOwner) return;

            var spawn = Spawns.GetSpawnsFor(playerSpawnsKey).GetRandomSpawn();
            VRRigReferences.singleton.SetPosition(spawn);
        }
        */

        // TODO: perform on owner instead of server?
        /*
        [ServerRpc]
        private void DieServerRpc()
        {
            lives.Value--;
            Debug.Log("[NetworkPlayer] Player lives: " + lives.Value);
            if (lives.Value <= 0)
                Debug.Log("[NetworkPlayer] Player has no lives left");
            else
                Respawn();
        }
        */

        private static void SetTransform(Transform toSet, Transform setFrom, Vector3 offset = new())
        {
            toSet.position = setFrom.position;
            toSet.position += offset;
            toSet.rotation = setFrom.rotation;
            toSet.localScale = setFrom.localScale;
        }
    }
}