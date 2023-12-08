using UnityEngine;

namespace Game
{
    public class HideCodeInputs : MonoBehaviour
    {
        public GameObject[] inputsToToggle;

        private bool _showing;

        private void Start()
        {
            _showing = true;
        }

        public void Toggle()
        {
            _showing = !_showing;
            foreach (var o in inputsToToggle)
                o.SetActive(_showing);
        }
    }
}