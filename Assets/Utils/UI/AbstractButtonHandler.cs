using System;

namespace Utils.UI
{
    /// <summary>
    /// Base class for handling buttons clicks.
    /// </summary>
    [Serializable]
    public abstract class AbstractButtonHandler
    {
        public abstract void OnButtonClickedHandler();
    }
}