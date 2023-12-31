using UnityEngine;
using UnityEngine.InputSystem;

namespace Lets_Make_a_VR_Game.Oculus_Hands.Scripts
{
    public class AnimateHandOnInput : MonoBehaviour
    {
        public InputActionProperty pinchAnimationAction;
        public InputActionProperty gripAnimationAction;
        public Animator handAnimator;

        private static readonly int Trigger = Animator.StringToHash("Trigger");
        private static readonly int Grip = Animator.StringToHash("Grip");

        public bool IsPinching()
        {
            return pinchAnimationAction.action.ReadValue<float>() > 0.1f;
        }

        public bool IsGripping()
        {
            return gripAnimationAction.action.ReadValue<float>() > 0.1f;
        }

        // Update is called once per frame
        private void Update()
        {
            var triggerValue = pinchAnimationAction.action.ReadValue<float>();
            handAnimator.SetFloat(Trigger, triggerValue);

            var gripValue = gripAnimationAction.action.ReadValue<float>();
            handAnimator.SetFloat(Grip, gripValue);
        }
    }
}