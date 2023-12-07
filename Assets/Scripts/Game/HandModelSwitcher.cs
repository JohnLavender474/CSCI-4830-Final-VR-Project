using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Game
{
    public class GripControllerModelSwitcher : MonoBehaviour
    {
        [SerializeField] private ActionBasedController actionBasedController;
        [SerializeField] private GameObject openControllerModel;
        [SerializeField] private GameObject closedControllerModel;

        private bool _isGripPressed;

        public bool IsGripPressed()
        {
            return _isGripPressed;
        }

        private void Start()
        {
            if (actionBasedController == null)
            {
                Debug.LogError("GripControllerModelSwitcher: ActionBasedController is not set!");
                return;
            }

            // Subscribe to grip button events
            actionBasedController.selectAction.action.performed += OnGripButtonPressed;
            actionBasedController.selectAction.action.canceled += OnGripButtonReleased;

            // Initially, show the open controller model
            if (openControllerModel != null)
            {
                openControllerModel.SetActive(true);
            }

            // Initially, hide the closed controller model
            if (closedControllerModel == null) return;
            closedControllerModel.SetActive(false);
        }

        private void OnDestroy()
        {
            // Unsubscribe from grip button events when the script is destroyed
            actionBasedController.selectAction.action.performed -= OnGripButtonPressed;
            actionBasedController.selectAction.action.canceled -= OnGripButtonReleased;
        }

        private void OnGripButtonPressed(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            _isGripPressed = true;
            UpdateControllerModel();
        }

        private void OnGripButtonReleased(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            _isGripPressed = false;
            UpdateControllerModel();
        }

        private void UpdateControllerModel()
        {
            if (openControllerModel != null)
            {
                openControllerModel.SetActive(!_isGripPressed);
            }

            if (closedControllerModel != null)
            {
                closedControllerModel.SetActive(_isGripPressed);
            }
        }
    }
}