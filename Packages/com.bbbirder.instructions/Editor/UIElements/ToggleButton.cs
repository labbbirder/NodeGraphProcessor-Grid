using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace BBBirder.Instructions
{
    public class ToggleButton : BaseBoolField// BindableElement, INotifyValueChanged<bool>
    {
        public static readonly new string ussClassName = "toggle-button";


        public Texture2D _normalIcon, _activeIcon;

        VisualElement veIcon;

        public ToggleButton(string label, Texture2D normalIcon) : this(label, normalIcon, normalIcon)
        {
        }

        public ToggleButton(string label, Texture2D normalIcon, Texture2D activeIcon) : base(label)
        {
            veIcon = new VisualElement
            {
                name = "icon",
            };
            this.Add(veIcon);
            this._normalIcon = normalIcon;
            this._activeIcon = activeIcon;
            UpdateIcons(value);
        }

        private void UpdateIcons(bool state)
        {
            var normalIcon = this._normalIcon;
            var activeIcon = this._activeIcon ? this._activeIcon : this._normalIcon;
            veIcon.style.backgroundImage = new StyleBackground()
            {
                value = new()
                {
                    texture = state ? activeIcon : normalIcon,
                },
            };
        }

        public override void SetValueWithoutNotify(bool newValue)
        {
            base.SetValueWithoutNotify(newValue);
            EnableInClassList("toggle-button__active", newValue);
            UpdateIcons(newValue);
        }

    }
}
