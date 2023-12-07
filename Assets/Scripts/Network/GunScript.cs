using System;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Network
{
    public class GunScript : NetworkBehaviour
    {
        [SerializeField] public NetworkVariable<int> bulletCount = new(10, NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);

        [SerializeField] public NetworkVariable<float> timeToDie = new(2f, NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);

        [Header("Prefab References")] public GameObject bulletPrefab;
        public GameObject casingPrefab;
        public GameObject muzzleFlashPrefab;

        [Header("Location References")] [SerializeField]
        private Animator gunAnimator;

        [SerializeField] private Transform barrelLocation;
        [SerializeField] private Transform casingExitLocation;

        [Header("Settings")] [Tooltip("Specify time to destroy the casing object")] [SerializeField]
        private float destroyTimer = 2f;

        [Tooltip("Bullet Speed")] [SerializeField]
        private float shotPower = 500f;

        [Tooltip("Casing Ejection Speed")] [SerializeField]
        private float ejectPower = 150f;

        public AudioSource audioSource;
        public AudioClip fireSound;
        public AudioClip emptySound;

        private static readonly int Fire = Animator.StringToHash("Fire");

        private void Start()
        {
            if (barrelLocation == null)
                barrelLocation = transform;

            if (gunAnimator == null)
                gunAnimator = GetComponentInChildren<Animator>();
        }

        public override void OnNetworkSpawn()
        {
            Debug.Log("[GunScript] On network spawn called");
            base.OnNetworkSpawn();

            // if this is the server, then add the gun to the game manager

            if (!IsOwner) return;
            Debug.Log("[GunScript] Server: adding player to game manager");
            // TODO: add to game
        }

        public override void OnNetworkDespawn()
        {
            Debug.Log("[GunScript] On network despawn called");
            base.OnNetworkDespawn();

            // if this is the server, then remove the gun from the game manager

            if (!IsOwner) return;
            Debug.Log("[GunScript] Removing gun from game manager");
            // TODO: remove from game
        }

        public void PullTrigger()
        {
            if (!IsOwner) return;

            gunAnimator.SetTrigger(Fire);
            Debug.Log("[GunScript] Shooting");

            if (bulletCount.Value <= 0)
            {
                audioSource.PlayOneShot(emptySound);
                return;
            }

            bulletCount.Value--;
            audioSource.PlayOneShot(fireSound);
            if (muzzleFlashPrefab)
            {
                var tempFlash = Instantiate(muzzleFlashPrefab, barrelLocation.position, barrelLocation.rotation);

                Destroy(tempFlash, destroyTimer);
            }

            if (!bulletPrefab) return;

            Instantiate(bulletPrefab, barrelLocation.position, barrelLocation.rotation)
                .GetComponent<Rigidbody>().AddForce(barrelLocation.forward * shotPower);
        }

        private void Update()
        {
            if (!IsOwner || bulletCount.Value > 0) return;

            UpdateTimeToDie();
        }

        private void UpdateTimeToDie()
        {
            if (!IsServer)
                throw new Exception("[GunScript] " +
                                    "UpdateTimeToDie should only be called on the server.");

            timeToDie.Value -= Time.deltaTime;
            Debug.Log("[GunScript] Gun time to die: " + timeToDie.Value);

            if (!(timeToDie.Value <= 0.0f)) return;

            Debug.Log("[GunScript] Destroy gun");
            GetComponent<NetworkObject>().Despawn();

            // TODO: Game.singleton.SpawnNewGun();
        }

        private void CasingRelease()
        {
            if (!IsServer) throw new Exception("[GunScript] OnCasingRelease should only be called on the server.");

            if (!casingExitLocation || !casingPrefab) return;

            var tempCasing =
                Instantiate(casingPrefab, casingExitLocation.position, casingExitLocation.rotation);

            tempCasing.GetComponent<Rigidbody>().AddExplosionForce(Random.Range(ejectPower * 0.7f, ejectPower),
                casingExitLocation.position - casingExitLocation.right * 0.3f - casingExitLocation.up * 0.6f, 1f);

            tempCasing.GetComponent<Rigidbody>()
                .AddTorque(new Vector3(0, Random.Range(100f, 500f), Random.Range(100f, 1000f)), ForceMode.Impulse);

            Destroy(tempCasing, destroyTimer);
        }
    }
}