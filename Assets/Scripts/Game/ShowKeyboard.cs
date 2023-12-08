using UnityEngine;
using TMPro;
using Microsoft.MixedReality.Toolkit.Experimental.UI;

// tutorial: https://www.youtube.com/watch?v=irJm8LkkDGw
namespace Game
{
    public class ShowKeyboard : MonoBehaviour
    {
        private const string EnterLobbyCode = "Enter lobby code";

        private TMP_InputField _inputField;

        /*
        public float distance = 0.5f;
        public float verticalOffset = -0.5f;

        public Transform positionSource;
        */

        private void Start()
        {
            _inputField = GetComponent<TMP_InputField>();
            _inputField.onSelect.AddListener(_ => OpenKeyboard());
        }

        private void OpenKeyboard()
        {
            if (EnterLobbyCode.Equals(_inputField.text))
                _inputField.text = "";

            var keyboard = NonNativeKeyboard.Instance;

            keyboard.InputField = _inputField;
            keyboard.PresentKeyboard(_inputField.text);

            /*
            var direction = positionSource.forward;
            direction.y = 0;
            direction.Normalize();

            var targetPosition = positionSource.position + direction * distance + Vector3.up * verticalOffset;
            keyboard.RepositionKeyboard(targetPosition);
            */

            SetCaretColorAlpha(1f);

            keyboard.OnClosed += CloseKeyboard;
        }

        private void CloseKeyboard(object sender, System.EventArgs e)
        {
            SetCaretColorAlpha(0f);
            NonNativeKeyboard.Instance.OnClosed -= CloseKeyboard;

            if (string.IsNullOrWhiteSpace(_inputField.text)) _inputField.text = EnterLobbyCode;
        }

        private void SetCaretColorAlpha(float value)
        {
            _inputField.customCaretColor = true;
            var caretColor = _inputField.caretColor;
            caretColor.a = value;
            _inputField.caretColor = caretColor;
        }
    }
}