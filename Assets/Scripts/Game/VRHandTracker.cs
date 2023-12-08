using Lets_Make_a_VR_Game.Oculus_Hands.Scripts;
using UnityEngine;

namespace Game
{
    public class VRHandTracker : MonoBehaviour
    {
        public AnimateHandOnInput leftHandAnimator;
        public AnimateHandOnInput rightHandAnimator;
        public bool isLeftHandTracked;
        public bool isRightHandTracked;

        public bool IsLeftHandGripping()
        {
            return leftHandAnimator != null && leftHandAnimator.IsGripping();
        }

        public bool IsRightHandGripping()
        {
            return rightHandAnimator != null && rightHandAnimator.IsGripping();
        }
    }
}