using Network;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Game
{
    public class GunActivatedHapticInteractable : MonoBehaviour
    {
        [SerializeField] private GunScript gun;
        [SerializeField] private XRGrabInteractable interactable;

        [SerializeField] [Range(0, 1)] private float intensityOnFire = 0.5f;
        [SerializeField] private float durationOnFire = 0.5f;

        [SerializeField] [Range(0, 1)] private float intensityOnEmpty = 0.5f;
        [SerializeField] private float durationOnEmpty = 0.5f;

        private void Start()
        {
            interactable.activated.AddListener(TriggerHaptic);
        }

        private void TriggerHaptic(BaseInteractionEventArgs args)
        {
            if (args.interactorObject is XRBaseControllerInteractor controllerInteractor)
            {
                TriggerHaptic(controllerInteractor.xrController);
            }
        }

        private void TriggerHaptic(XRBaseController controller)
        {
            if (gun.bulletCount.Value > 0)
            {
                if (intensityOnFire > 0)
                {
                    controller.SendHapticImpulse(intensityOnFire, durationOnFire);
                }
            }
            else
            {
                if (intensityOnEmpty > 0)
                {
                    controller.SendHapticImpulse(intensityOnEmpty, durationOnEmpty);
                }
            }
        }
    }
}