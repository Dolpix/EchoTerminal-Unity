using EchoTerminal.Scripts.Commands.Bind;
using EchoTerminal.Scripts.ScriptableObjects;
using EchoTerminal.Scripts.TerminalCore;
using EchoTerminal.Scripts.UI.EchoComponents;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace EchoTerminal.Scripts.UI
{
[RequireComponent(typeof(UIDocument))]
public class GameTerminalUI : MonoBehaviour
{
	[SerializeField] private TerminalConfig _config;
	[SerializeField] private InputActionAsset _inputActions;
	private TextField _inputField;
	private TerminalView _view;

	public Terminal Terminal => _view?.Terminal;
	public static bool IsFocused { get; private set; }

	private void Awake()
	{
		VisualElement root = GetComponent<UIDocument>().rootVisualElement;

		if (root.childCount == 0)
		{
			Debug.LogError(
				"[EchoTerminal] UIDocument has no visual tree. " +
				"Assign EchoTerminalGame.uxml as Source Asset on the UIDocument component."
			);
			return;
		}

		if (_config == null)
		{
			Debug.LogError(
				"[EchoTerminal] TerminalConfig is not assigned. " +
				"Create one via Create > Echo Terminal > UI Config and assign it."
			);
			return;
		}

		_view = new(root, _config);

		var gameWindow = root.Q<VisualElement>("game-window");
		if (gameWindow != null)
		{
			gameWindow.style.display = DisplayStyle.Flex;
		}

		var executor = gameObject.AddComponent<BindExecutor>();
		executor.Init(this);

		if (_inputActions != null && root.Q<VisualElement>("game-window") != null)
		{
			_view.AddComponent(new TerminalToggle(root, _inputActions));
		}

		_inputField = root.Q<TextField>("input-field");

		if (_inputField == null)
		{
			return;
		}

		_inputField.RegisterCallback<FocusInEvent>(_ => IsFocused = true);
		_inputField.RegisterCallback<FocusOutEvent>(_ => IsFocused = false);
		_inputField.schedule.Execute(() => _inputField.Focus());
	}

	private void OnDestroy()
	{
		IsFocused = false;
		_view = null;
	}
}
}