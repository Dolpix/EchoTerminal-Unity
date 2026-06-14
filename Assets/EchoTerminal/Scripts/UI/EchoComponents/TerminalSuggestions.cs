using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EchoTerminal.Scripts.ScriptableObjects;
using EchoTerminal.Scripts.TerminalCore;
using EchoTerminal.Scripts.TerminalCore.Attributes;
using EchoTerminal.Scripts.TerminalCore.Command;
using EchoTerminal.Scripts.TerminalCore.Structs;
using EchoTerminal.Scripts.TerminalCore.Suggestions;
using EchoTerminal.Scripts.TerminalCore.Token;
using EchoTerminal.Scripts.TerminalCore.Token.TokenParser;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoTerminal.Scripts.UI.EchoComponents
{
    public class TerminalSuggestions : IEchoComponent
    {
        private const string _ghostColor = "#606060";
        private readonly Label _ghostLabel;
        private readonly TextField _inputField;
        private readonly VisualTreeAsset _itemTemplate;
        private readonly ScrollView _list;
        private readonly VisualElement _popup;

        private readonly Terminal _terminal;
        private int _selectedIndex = -1;
        private string _activeReplacePartial = string.Empty;
        private bool _isComplexSuggestion;
        private bool _accepting;
        private bool _accepted;
        private int _pendingFocusRestores;

        private List<string> _suggestions = new();

        public TerminalSuggestions(Terminal terminal, VisualElement root, TerminalConfig config)
        {
            _terminal = terminal;
            _inputField = root?.Q<TextField>("input-field");
            _ghostLabel = root?.Q<Label>("ghost-label");

            if (_inputField == null || config.SuggestionPopupTemplate == null || config.SuggestionItemTemplate == null)
            {
                return;
            }

            _itemTemplate = config.SuggestionItemTemplate;

            var inputRow = root.Q<VisualElement>("input-row");
            TemplateContainer popupClone = config.SuggestionPopupTemplate.CloneTree();
            _popup = popupClone.Q<VisualElement>("suggestion-popup");
            _list = popupClone.Q<ScrollView>("suggestion-list");
            inputRow.Insert(0, _popup);

            _inputField.RegisterValueChangedCallback(OnInputChanged);
            _inputField.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
            _inputField.RegisterCallback<FocusOutEvent>(OnFocusOut);
            _inputField.RegisterCallback<FocusInEvent>(OnFocusIn);
        }

        ~TerminalSuggestions()
        {
            _inputField?.UnregisterValueChangedCallback(OnInputChanged);
            _inputField?.UnregisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
            _inputField?.UnregisterCallback<FocusOutEvent>(OnFocusOut);
            _inputField?.UnregisterCallback<FocusInEvent>(OnFocusIn);
        }

        private void AcceptSuggestion(string suggestion)
        {
            string current = _inputField.value;
            string activePartial = _activeReplacePartial.Length > 0 && current.EndsWith(_activeReplacePartial)
                ? _activeReplacePartial
                : GetActivePartial(current);
            string prefix = current[..^activePartial.Length];

            _accepting = true;
            _pendingFocusRestores++;
            _inputField.value = prefix + suggestion + " ";
            _inputField.schedule.Execute(() =>
            {
                VisualElement inner = _inputField.Q("unity-text-input");
                (inner ?? _inputField).Focus();
                int len = _inputField.value.Length;
                _inputField.SelectRange(len, len);
                _accepting = false;
            });
        }

        private List<string> BuildSuggestions(string input)
        {
            CommandParseResult result = _terminal.CommandParser.Parse(input);
            Token? activeToken = GetActiveToken(result);
            ISuggester suggestor = ResolveSuggester(input, result, out Type expectedType);

            if (suggestor == null)
            {
                _activeReplacePartial = string.Empty;
                return new();
            }

            bool isComplex = IsComplexType(expectedType) && activeToken != null;
            string partial = isComplex ? activeToken.Value.Raw : GetActivePartial(input);
            _activeReplacePartial = partial;
            _isComplexSuggestion = isComplex;

            return suggestor.GetSuggestions(partial, expectedType).ToList();
        }

        private static bool IsComplexType(Type t)
        {
            return t != null && (t == typeof(BoundCommand) || typeof(IList).IsAssignableFrom(t));
        }

        private static string GetDisplayText(string suggestion, bool isComplex)
        {
            if (!isComplex)
            {
                return suggestion;
            }

            string inner = suggestion.Length > 0 && (suggestion[0] == '>' || suggestion[0] == '[')
                ? suggestion[1..]
                : suggestion;
            return GetActivePartial(inner);
        }

        private static int GetActiveParamIndex(CommandParseResult result)
        {
            if (result.ArgTokens == null)
            {
                return -1;
            }

            int last = result.ArgTokens.Count - 1;
            if (last < 0)
            {
                return -1;
            }

            return result.ArgTokens[last].State != TokenState.Completed ? last : -1;
        }

        private static string GetActivePartial(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            int spaceIdx = input.LastIndexOf(' ');
            return spaceIdx < 0 ? input : input[(spaceIdx + 1)..];
        }

        private static Token? GetActiveToken(CommandParseResult result)
        {
            if (result.ArgTokens == null || result.ArgTokens.Count == 0)
            {
                return null;
            }

            Token last = result.ArgTokens[^1];
            return last.State != TokenState.Completed ? last : null;
        }

        private void HidePopup()
        {
            if (_popup != null)
            {
                _popup.style.display = DisplayStyle.None;
                _popup.RemoveFromClassList("terminal-suggestion-popup--visible");
            }

            if (_ghostLabel != null)
            {
                _ghostLabel.text = string.Empty;
            }

            _suggestions.Clear();
            _selectedIndex = -1;
            _activeReplacePartial = string.Empty;
        }

        private static bool IsCommandTokenActive(string input, CommandParseResult result)
        {
            bool b = result.CommandToken.State == TokenState.Partial && !input.Contains(' ');
            return !input.Contains(' ') || b;
        }

        private void OnFocusIn(FocusInEvent evt)
        {
            if (_pendingFocusRestores > 0)
                _pendingFocusRestores--;
        }

        private void OnFocusOut(FocusOutEvent evt)
        {
            bool submitPending = _inputField.userData is int count && count > 0;
            if (_accepting || _pendingFocusRestores > 0 || submitPending)
                return;

            if (evt.relatedTarget is VisualElement next && (_inputField == next || _inputField.Contains(next)))
                return;

            HidePopup();
        }

        private void OnInputChanged(ChangeEvent<string> evt)
        {
            Rebuild(evt.newValue);
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.None && evt.character == '\n')
            {
                if (_accepted)
                {
                    _accepted = false;
                    evt.PreventDefault();
                    evt.StopImmediatePropagation();
                    evt.StopPropagation();
                }

                return;
            }

            if (_suggestions.Count == 0)
            {
                return;
            }

            switch (evt.keyCode)
            {
                case KeyCode.UpArrow:
                    _selectedIndex = _selectedIndex <= 0 ? _suggestions.Count - 1 : _selectedIndex - 1;
                    RefreshSelection();
                    evt.StopPropagation();
                    return;
                case KeyCode.DownArrow:
                    _selectedIndex = _selectedIndex >= _suggestions.Count - 1 ? 0 : _selectedIndex + 1;
                    RefreshSelection();
                    evt.StopPropagation();
                    return;
                case KeyCode.Return or KeyCode.KeypadEnter when _selectedIndex >= 0:
                    _accepted = true;
                    AcceptSuggestion(_suggestions[_selectedIndex]);
                    evt.PreventDefault();
                    evt.StopImmediatePropagation();
                    evt.StopPropagation();
                    return;
                case KeyCode.Tab when _selectedIndex >= 0:
                    AcceptSuggestion(_suggestions[_selectedIndex]);
                    evt.PreventDefault();
                    evt.StopImmediatePropagation();
                    evt.StopPropagation();
                    return;
            }
        }

        private void Rebuild(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                HidePopup();
                return;
            }

            _suggestions = BuildSuggestions(input);
            _selectedIndex = _suggestions.Count > 0 ? 0 : -1;

            _list.Clear();

            if (_suggestions.Count == 0)
            {
                HidePopup();
                return;
            }

            foreach (string suggestion in _suggestions)
            {
                TemplateContainer clone = _itemTemplate.CloneTree();
                var label = clone.Q<Label>("suggestion-label");
                label.text = GetDisplayText(suggestion, _isComplexSuggestion);
                label.RegisterCallback<MouseDownEvent>(evt =>
                    {
                        evt.StopPropagation();
                        AcceptSuggestion(suggestion);
                    },
                    TrickleDown.TrickleDown);
                _list.Add(clone);
            }

            _popup.style.display = DisplayStyle.Flex;
            _popup.schedule.Execute(() => _popup.AddToClassList("terminal-suggestion-popup--visible"));
            RefreshSelection();
        }

        private void RefreshSelection()
        {
            List<Label> items = _list.Query<Label>(className: "terminal-suggestion-item").ToList();

            for (var i = 0; i < items.Count; i++)
            {
                if (i == _selectedIndex)
                {
                    items[i].AddToClassList("terminal-suggestion-item--selected");
                    int idx = i;
                    _list.schedule.Execute(() =>
                    {
                        VisualElement target = items[idx].parent ?? items[idx];
                        if (target.panel != null && _list.contentContainer.Contains(target))
                        {
                            _list.ScrollTo(target);
                        }
                    });
                }
                else
                {
                    items[i].RemoveFromClassList("terminal-suggestion-item--selected");
                }
            }

            UpdateGhost();
        }

        private ISuggester ResolveSuggester(string input, CommandParseResult result, out Type expectedType)
        {
            expectedType = null;

            if (result.Entries == null || IsCommandTokenActive(input, result))
            {
                _terminal.Suggestors.TryGet(typeof(CommandName), out ISuggester s);
                return s;
            }

            Token? activeToken = GetActiveToken(result);
            bool cursorAdvanced = input.Length > 0 && input[^1] == ' ';
            bool allHaveTarget = result.Entries.All(e => e.HasTarget);

            int paramIndex = activeToken == null
                ? cursorAdvanced ? result.ArgTokens?.Count ?? 0 : (result.ArgTokens?.Count ?? 1) - 1
                : GetActiveParamIndex(result);

            if (paramIndex >= 0)
            {
                if (paramIndex == 0 && allHaveTarget)
                {
                    expectedType = typeof(Target);
                    _terminal.Suggestors.TryGet(expectedType, out ISuggester targetSuggester);
                    return targetSuggester;
                }

                bool inTargetContext = allHaveTarget &&
                                       result.ArgTokens?.Count > 0 &&
                                       result.ArgTokens[0].ExpectedType == typeof(Target);

                CommandEntry lookupEntry = inTargetContext
                    ? result.Entries.FirstOrDefault(e => e.HasTarget)
                    : result.Entries.FirstOrDefault();

                if (lookupEntry.Method == null)
                {
                    return null;
                }

                int actualParamIndex = inTargetContext ? paramIndex - 1 : paramIndex;
                ParameterInfo[] parameters = lookupEntry.Method.GetParameters();

                if (actualParamIndex >= 0 && actualParamIndex < parameters.Length)
                {
                    var attr = parameters[actualParamIndex].GetCustomAttribute<SuggestAttribute>();
                    if (attr != null)
                    {
                        expectedType = parameters[actualParamIndex].ParameterType;
                        return attr.Suggester;
                    }

                    if (activeToken == null && cursorAdvanced)
                    {
                        expectedType = parameters[actualParamIndex].ParameterType;
                        _terminal.Suggestors.TryGet(expectedType, out ISuggester nextSuggester);
                        return nextSuggester;
                    }
                }
            }

            if (activeToken == null)
            {
                if (cursorAdvanced || !(result.ArgTokens?.Count > 0))
                {
                    return null;
                }

                Token lastToken = result.ArgTokens[^1];
                expectedType = lastToken.ExpectedType;
                _terminal.Suggestors.TryGet(expectedType, out ISuggester lastSuggester);
                return lastSuggester;
            }

            expectedType = activeToken.Value.ExpectedType;
            _terminal.Suggestors.TryGet(expectedType, out ISuggester suggester);
            return suggester;
        }

        private void UpdateGhost()
        {
            if (_ghostLabel == null)
            {
                return;
            }

            if (_selectedIndex < 0 || _selectedIndex >= _suggestions.Count)
            {
                _ghostLabel.text = string.Empty;
                return;
            }

            string current = _inputField.value;
            string suggestion = _suggestions[_selectedIndex];
            string activePartial = _activeReplacePartial.Length > 0 && current.EndsWith(_activeReplacePartial)
                ? _activeReplacePartial
                : GetActivePartial(current);

            if (!suggestion.StartsWith(activePartial, StringComparison.OrdinalIgnoreCase))
            {
                _ghostLabel.text = string.Empty;
                return;
            }

            string suffix = suggestion[activePartial.Length..];
            _ghostLabel.text = $"<color=#00000000>{current}</color><color={_ghostColor}>{suffix}</color>";
        }
    }
}