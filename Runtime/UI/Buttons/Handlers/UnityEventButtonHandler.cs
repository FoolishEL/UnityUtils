using System;
using UnityEngine;
using UnityEngine.Events;

namespace Foolish.Utils.UI
{
    [Serializable]
    public class UnityEventButtonHandler : AbstractButtonHandler
    {
        [SerializeField] private UnityEvent eventOnClick;
        public override void OnButtonClickedHandler() => eventOnClick.Invoke();
    }
}