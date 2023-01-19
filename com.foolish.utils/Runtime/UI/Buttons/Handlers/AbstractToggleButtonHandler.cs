using System;
using UnityEngine;
using Utils.Common;

namespace Utils.UI
{
    [Serializable]
    public abstract class AbstractToggleButtonHandler : AbstractButtonHandler,IDisposable,IInitable
    {
        [SerializeField]
        private ToggleButtonHandlerGroup toggleButtonHandlerGroup;

        protected abstract void OnValueChanged(bool status);
        
        [Tooltip("Is the toggle currently on or off?")]
        [SerializeField]
        private bool isOn;

        private void SetToggleGroup(ToggleButtonHandlerGroup newGroup, bool setMemberValue)
        {
            if (toggleButtonHandlerGroup != null)
                toggleButtonHandlerGroup.UnregisterToggle(this);
            
            if (setMemberValue)
                toggleButtonHandlerGroup = newGroup;
            
            if (newGroup != null)
                newGroup.RegisterToggle(this);
            if (newGroup != null && IsOn)
                newGroup.NotifyToggleOn(this);
        }

        public bool IsOn
        {
            get => isOn;

            set => Set(value);
        }

        public void SetIsOnWithoutNotify(bool value) => Set(value, false);

        private void Set(bool value, bool sendCallback = true)
        {
            if (isOn == value)
                return;
            
            isOn = value;
            if (toggleButtonHandlerGroup != null && toggleButtonHandlerGroup.isActiveAndEnabled)
            {
                if (isOn || (!toggleButtonHandlerGroup.AnyTogglesOn() && !toggleButtonHandlerGroup.AllowSwitchOff))
                {
                    isOn = true;
                    toggleButtonHandlerGroup.NotifyToggleOn(this, sendCallback);
                }
            }
            if (sendCallback)
            {
                OnValueChanged(isOn);
            }
        }

        private void InternalToggle() => IsOn = !IsOn;

        public virtual void Dispose()
        {
            SetToggleGroup(null, false);
            if (toggleButtonHandlerGroup != null)
                toggleButtonHandlerGroup.EnsureValidState();
        }

        public virtual void Initialize()
        {
            SetToggleGroup(toggleButtonHandlerGroup, false);
            if (toggleButtonHandlerGroup is not null)
                toggleButtonHandlerGroup.EnsureValidState();
            OnValueChanged(IsOn);
        }

        public override void OnButtonClickedHandler() => InternalToggle();
    }
}
