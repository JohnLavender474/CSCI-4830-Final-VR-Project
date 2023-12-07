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
        public Transform head;
        public Transform leftHand;
        public Transform rightHand;

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