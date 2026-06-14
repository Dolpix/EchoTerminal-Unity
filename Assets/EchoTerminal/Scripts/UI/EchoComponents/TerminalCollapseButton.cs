using UnityEngine.UIElements;

namespace EchoTerminal.Scripts.UI.EchoComponents
{
    public class TerminalCollapseButton : IEchoComponent
    {
        private readonly Button _button;
        private bool _collapseEnabled;
        private readonly TerminalLogDisplay _display;
        private const string ActiveClass = "terminal-toolbar-toggle--active";

        public TerminalCollapseButton(TerminalLogDisplay display, VisualElement root)
        {
            _display = display;
            _button = root?.Q<Button>("collapse-toggle");

            _button?.RegisterCallback<ClickEvent>(OnClicked);
        }

        ~TerminalCollapseButton()
        {
            _button?.UnregisterCallback<ClickEvent>(OnClicked);
        }

        private void OnClicked(ClickEvent evt)
        {
            _collapseEnabled = !_collapseEnabled;
            _button?.EnableInClassList(ActiveClass, _collapseEnabled);
            _display.SetCollapse(_collapseEnabled);
        }
    }
}