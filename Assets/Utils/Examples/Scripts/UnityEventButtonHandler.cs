using System;
using UnityEngine;
using UnityEngine.Events;

namespace Utils.UI
{
    [Serializable]
    public class UnityEventButtonHandler : AbstractButtonHandler
    {
        [SerializeField, DisableGUI] private string message = "Example class!";
        [SerializeField] private UnityEvent eventOnClick;
        public override void OnButtonClickedHandler() => eventOnClick.Invoke();
    }
}