using Game;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.UI;

namespace Network
{
    public class VRRigReferences : MonoBehaviour
    {
        public static VRRigReferences singleton;

        public XROrigin rig;
        public Image healthbarImage;
        public TextMeshProUGUI timeRemainingText;
        public Transform root;

        public Transform body;

        // TODO:
        //   maybe use character controller instead of custom body object because the body object's
        //   relative position for some reason is not always where it should be?
        public CharacterController characterController;
        
        public Transform head;
        public Transform leftHand;
        public Transform rightHand;

        // TODO:
        //   if rendering avatar hands instead of blue hands,
        //   then this script can be used for knowing when to
        //   open and close the hands  
        public VRHandTracker handTracker;

        private void Awake()
        {
            Debug.Log("[VRRigReferences] Awake called");
            singleton = this;
        }

        public void SetPosition(Vector3 position)
        {
            rig.transform.position = position;
        }
    }
}