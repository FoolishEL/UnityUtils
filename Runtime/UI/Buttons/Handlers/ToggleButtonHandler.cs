using System;
using UnityEngine;
using UnityEngine.Events;

namespace Foolish.Utils.UI
{
    [Serializable]
    public class ToggleButtonHandler : AbstractToggleButtonHandler
    {
        [SerializeField] private UnityEvent<bool> unityEvent; 
        protected override void OnValueChanged(bool status)
        {
            unityEvent.Invoke(status);
        }
    }
}
