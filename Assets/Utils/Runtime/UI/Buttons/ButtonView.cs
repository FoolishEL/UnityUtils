using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Utils.UI
{
    /// <summary>
    /// Class for storing different handlers on button clicks. 
    /// </summary>
    [RequireComponent(typeof(Button))]
    public sealed class ButtonView : MonoBehaviour
    {
        [SerializeField] private Button button;

        [SerializeReference] private List<AbstractButtonHandler> _buttonHandlers = new();

        private void OnValidate()
        {
            if (button is null)
                if (TryGetComponent<Button>(out var btn))

                    button = btn;
                else
                {
                    Debug.LogError("Missing Button!");
                    enabled = false;
                }
        }

        private void Reset() => button = GetComponent<Button>();

        private void Awake() => button.onClick.AddListener(OnButtonClicked);

        private void OnDestroy() => button.onClick.AddListener(OnButtonClicked);

        private void OnButtonClicked() => _buttonHandlers.ForEach(c => c.OnButtonClickedHandler());
    }
}