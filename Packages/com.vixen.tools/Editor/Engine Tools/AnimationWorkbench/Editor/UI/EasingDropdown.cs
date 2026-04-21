#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Vixenlicious.AnimationWorkbench
{
    /// <summary>
    /// Lightweight easing selector for UI Toolkit, backed by GenericMenu.
    /// No EditorWindow, no HostView issues.
    /// </summary>
    public class EasingDropdown : VisualElement
    {
        public event Action<EasingFunctions.EaseType> OnValueChanged;

        private readonly Label _label;
        private readonly Button _button;
        private readonly List<EasingFunctions.EaseType> _options;

        private EasingFunctions.EaseType _value;

        public EasingFunctions.EaseType value
        {
            get => _value;
            set
            {
                if (_value == value)
                    return;

                _value = value;
                _label.text = _value.ToString();
                OnValueChanged?.Invoke(_value);
            }
        }

        public EasingDropdown(EasingFunctions.EaseType defaultValue)
        {
            AddToClassList("easing-dropdown");

            style.flexDirection = FlexDirection.Row;
            style.alignItems = Align.Center;

            _options = new List<EasingFunctions.EaseType>(
                (EasingFunctions.EaseType[])Enum.GetValues(typeof(EasingFunctions.EaseType))
            );

            // Label shows the currently selected easing
            _label = new Label(defaultValue.ToString());
            _label.AddToClassList("easing-dropdown-label");
            _label.style.flexGrow = 1;
            Add(_label);

            // Button opens a GenericMenu near the mouse
            _button = new Button(OpenPopup)
            {
                text = "▼"
            };
            _button.AddToClassList("easing-dropdown-button");
            Add(_button);

            _value = defaultValue;
        }

        private void OpenPopup()
        {
            var menu = new GenericMenu();

            foreach (var opt in _options)
            {
                bool isCurrent = opt.Equals(_value);
                string label = RenderPreview(opt);

                // Capture local variable
                var captured = opt;
                menu.AddItem(new GUIContent(label), isCurrent, () =>
                {
                    value = captured;
                });
            }

            // Show near mouse; safe in UI Toolkit / editor context
            menu.ShowAsContext();
        }

        // Text preview for each easing type
        private string RenderPreview(EasingFunctions.EaseType t)
        {
            return t switch
            {
                EasingFunctions.EaseType.Linear => "Linear         | ----",
                EasingFunctions.EaseType.SmoothStep => "SmoothStep     | ~~--",
                EasingFunctions.EaseType.EaseInQuad => "EaseInQuad     | (  \\__",
                EasingFunctions.EaseType.EaseOutQuad => "EaseOutQuad    | __/  )",
                EasingFunctions.EaseType.EaseInOutCubic => "EaseInOutCubic | ~~~==~~",
                EasingFunctions.EaseType.EaseOutCubic => "EaseOutCubic   | __/~~~",
                _ => t.ToString()
            };
        }
    }
}
#endif
