using System;
using UnityEngine;
using UnityEngine.Events;

namespace Foolish.Utils.UI
{
    [Serializable]
    public class UnityEventButtonHandler : AbstractButtonHandler
    {
        [SerializeField, UnInteractableGUI] private string message = "Example class!";
        [SerializeField] private UnityEvent eventOnClick;
        public override void OnButtonClickedHandler() => eventOnClick.Invoke();
    }
}