using EchoTerminal.Scripts.TerminalCore;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoTerminal.Scripts.UI.EchoComponents
{
    public class TerminalInput : IEchoComponent
    {
        private readonly TextField _inputField;
        private readonly Terminal _terminal;
        private bool _submitted;

        public TerminalInput(Terminal terminal, VisualElement root)
        {
            _terminal = terminal;
            _inputField = root?.Q<TextField>("input-field");
            _inputField?.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
        }

        ~TerminalInput()
        {
            _inputField?.UnregisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.None && evt.character == '\n')
            {
                if (!_submitted)
                {
                    return;
                }

                _submitted = false;
                evt.PreventDefault();
                evt.StopImmediatePropagation();
                evt.StopPropagation();

                return;
            }

            if (evt.keyCode != KeyCode.Return && evt.keyCode != KeyCode.KeypadEnter)
            {
                return;
            }

            string text = _inputField.value;
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            _submitted = true;
            _terminal.Submit(text);
            _inputField.value = "";

            evt.PreventDefault();
            evt.StopImmediatePropagation();
            evt.StopPropagation();
        }
    }
}