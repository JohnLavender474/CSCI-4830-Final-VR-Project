using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Network
{
    public class NetworkAnimateHandOnInput : NetworkBehaviour
    {
        public InputActionProperty pinchAnimationAction;
        public InputActionProperty gripAnimationAction;
        public Animator handAnimator;

        private static readonly int Trigger = Animator.StringToHash("Trigger");
        private static readonly int Grip = Animator.StringToHash("Grip");

        // Update is called once per frame
        private void Update()
        {
            if (!IsOwner) return;

            var triggerValue = pinchAnimationAction.action.ReadValue<float>();
            handAnimator.SetFloat(Trigger, triggerValue);

            var gripValue = gripAnimationAction.action.ReadValue<float>();
            handAnimator.SetFloat(Grip, gripValue);
        }
    }
}