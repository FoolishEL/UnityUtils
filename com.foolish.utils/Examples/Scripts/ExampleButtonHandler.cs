using System;
using UnityEngine;

namespace Foolish.Utils.UI
{
    /// <summary>
    /// Example class for handling Button clicks
    /// </summary>
    [Serializable]
    public class ExampleButtonHandler : AbstractButtonHandler
    {
        [SerializeField, UnInteractableGUI] private string message = "This class is only for test purposes only!";
        public override void OnButtonClickedHandler()
        {
            Debug.LogError($"{nameof(ExampleButtonHandler)} is handled!");
        }
    }
    
    /// <summary>
    /// Example class for handling Button clicks
    /// </summary>
    [Serializable]
    public class Example2ButtonHandler : AbstractButtonHandler
    {
        [SerializeField, UnInteractableGUI] private string message = "This class is only for test purposes only!";

        public override void OnButtonClickedHandler()
        {
            Debug.LogError($"{nameof(Example2ButtonHandler)} is handled!");
        }
    }
}
