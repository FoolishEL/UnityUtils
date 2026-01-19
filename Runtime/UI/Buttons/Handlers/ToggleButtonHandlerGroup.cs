using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Foolish.Utils.UI
{
    [DisallowMultipleComponent]
    public class ToggleButtonHandlerGroup : UIBehaviour
    {
        [SerializeField] private bool allowSwitchOff = false;
        
        public bool AllowSwitchOff 
        { 
            get => allowSwitchOff;
            set => allowSwitchOff = value;
        }
        private List<AbstractToggleButtonHandler> toggles = new List<AbstractToggleButtonHandler>();
        protected override void Awake()
        {
            EnsureValidState();
            base.Awake();
        }
        protected override void OnEnable()
        {
            EnsureValidState();
            base.OnEnable();
        }
        private void ValidateToggleIsInGroup(AbstractToggleButtonHandler toggle)
        {
            if (toggle == null || !toggles.Contains(toggle))
                throw new ArgumentException(string.Format("Toggle {0} is not part of ToggleGroup {1}", new object[] {toggle, this}));
        }
        public void NotifyToggleOn(AbstractToggleButtonHandler toggle, bool sendCallback = true)
        {
            ValidateToggleIsInGroup(toggle);
            for (var i = 0; i < toggles.Count; i++)
            {
                if (toggles[i] == toggle)
                    continue;

                if (sendCallback)
                    toggles[i].IsOn = false;
                else
                    toggles[i].SetIsOnWithoutNotify(false);
            }
        }
        public void UnregisterToggle(AbstractToggleButtonHandler toggle)
        {
            if (toggles.Contains(toggle))
                toggles.Remove(toggle);
        }
        public void RegisterToggle(AbstractToggleButtonHandler toggle)
        {
            if (!toggles.Contains(toggle))
                toggles.Add(toggle);
        }
        public void EnsureValidState()
        {
            if (!AllowSwitchOff && !AnyTogglesOn() && toggles.Count != 0)
            {
                toggles[0].IsOn = true;
                NotifyToggleOn(toggles[0]);
            }

            IEnumerable<AbstractToggleButtonHandler> activeToggles = ActiveToggles();

            if (activeToggles.Count() > 1)
            {
                AbstractToggleButtonHandler firstActive = GetFirstActiveToggle();

                foreach (AbstractToggleButtonHandler toggle in activeToggles)
                {
                    if (toggle == firstActive)
                    {
                        continue;
                    }
                    toggle.IsOn = false;
                }
            }
        }
        public bool AnyTogglesOn() => toggles.Find(x => x.IsOn) != null;
        private IEnumerable<AbstractToggleButtonHandler> ActiveToggles() => toggles.Where(x => x.IsOn);
        private AbstractToggleButtonHandler GetFirstActiveToggle()
        {
            IEnumerable<AbstractToggleButtonHandler> activeToggles = ActiveToggles();
            return activeToggles.Count() > 0 ? activeToggles.First() : null;
        }

    }
}
