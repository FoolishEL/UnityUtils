using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Foolish.Utils.Common;

namespace Foolish.Utils.UI
{
    /// <summary>
    /// Class for storing different handlers on button clicks. 
    /// </summary>
    [RequireComponent(typeof(Button))]
    public sealed class ButtonView : MonoBehaviour
    {
        [SerializeField] private Button button;

        [SerializeReference] private List<AbstractButtonHandler> _buttonHandlers = new List<AbstractButtonHandler>();

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

        private void Start()
        {
            button.onClick.AddListener(OnButtonClicked);
            foreach (var buttonHandler  in _buttonHandlers)
            {
                if(buttonHandler is IInitable initable)
                    initable.Initialize();
            }
        }

        private void OnDestroy()
        {
            button.onClick.AddListener(OnButtonClicked);
            foreach (var buttonHandler  in _buttonHandlers)
            {
                if(buttonHandler is IDisposable disposable)
                    disposable.Dispose();
            }
        }

        private void OnButtonClicked() => _buttonHandlers.ForEach(c => c.OnButtonClickedHandler());
    }
}