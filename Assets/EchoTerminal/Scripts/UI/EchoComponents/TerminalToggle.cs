using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace EchoTerminal.Scripts.UI.EchoComponents
{
public class TerminalToggle : IEchoComponent
{
	private readonly InputAction _toggleAction;
	private readonly VisualElement _window;
	private readonly TextField _inputField;

	public TerminalToggle(VisualElement root, InputActionAsset inputActions)
	{
		_window = root.Q<VisualElement>("game-window");
		_inputField = root.Q<TextField>("input-field");

		if (_window == null || inputActions == null)
		{
			return;
		}

		_window.style.display = DisplayStyle.None;

		_toggleAction = inputActions.FindActionMap("Terminal")?.FindAction("Toggle");

		if (_toggleAction == null)
		{
			Debug.LogWarning("[EchoTerminal] Terminal/Toggle action not found in InputActionAsset.");
			return;
		}

		_toggleAction.performed += OnToggle;
		_toggleAction.Enable();
	}

	~TerminalToggle()
	{
		if (_toggleAction == null)
		{
			return;
		}

		_toggleAction.performed -= OnToggle;
		_toggleAction.Disable();
	}

	private void OnToggle(InputAction.CallbackContext ctx)
	{
		if (_window == null)
		{
			return;
		}

		bool isHidden = _window.resolvedStyle.display == DisplayStyle.None;
		_window.style.display = isHidden ? DisplayStyle.Flex : DisplayStyle.None;

		if (isHidden)
		{
			_inputField?.schedule.Execute(() => _inputField.Focus()).StartingIn(100);
		}
	}
}
}