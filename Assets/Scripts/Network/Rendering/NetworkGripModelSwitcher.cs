using System;
using Game;
using Unity.Netcode;
using UnityEngine;

namespace Network
{
    public class NetworkGripModelSwitcher : NetworkBehaviour
    {
        [SerializeField] private string controllerGameObjectName;
        [SerializeField] private GameObject openControllerModel;
        [SerializeField] private GameObject closedControllerModel;

        private GripControllerModelSwitcher _controllerSwitcher;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (!IsOwner)
            {
                Debug.Log("[NetworkGripModelSwitcher] Return from OnNetworkSpawn() since IsOwner is false");
                return;
            }
            Debug.Log("[NetworkGripModelSwitcher] Proceed with OnNetworkSpawn() since IsOwner is true");

            if (openControllerModel == null)
            {
                throw new Exception("[NetworkGripModelSwitcher] Open model controller cannot be null!");
            }

            if (closedControllerModel == null)
            {
                throw new Exception("[NetworkGripModelSwitcher] Closed controller model cannot be null!");
            }

            var controller = GameObject.Find(controllerGameObjectName);
            if (controller == null)
            {
                throw new Exception("[NetworkGripModelSwitcher] No game object found for controller " +
                                    "with name: " + controllerGameObjectName);
            }

            _controllerSwitcher = controller.GetComponent<GripControllerModelSwitcher>();
            if (!_controllerSwitcher)
            {
                throw new Exception("[NetworkGripModelSwitcher] No controller switcher found!");
            }
        }

        private void Update()
        {
            if (!IsOwner) return;

            UpdateControllerModel();
        }

        private void UpdateControllerModel()
        {
            openControllerModel.SetActive(!_controllerSwitcher.IsGripPressed());
            closedControllerModel.SetActive(_controllerSwitcher.IsGripPressed());
        }
    }
}