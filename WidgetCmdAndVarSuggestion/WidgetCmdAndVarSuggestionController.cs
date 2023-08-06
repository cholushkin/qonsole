using System;
using System.Collections.Generic;
using Qonsole;
using UnityEngine;

public class WidgetCmdAndVarSuggestionController : MonoBehaviour
{
    public WidgetQonsoleView View;
    private List<ConsoleSystem.ConsoleMethodInfo> _consoleSuggestions;


    public void Awake()
    {
        _consoleSuggestions = new List<ConsoleSystem.ConsoleMethodInfo>(128);
        View.SetCmdSuggestionList(_consoleSuggestions);
    }


    public void UpdateSuggestionList(string userInput)
    {
        // Find commands that contain the input text
        _consoleSuggestions = ConsoleSystem.Methods.FindAll(command => command.FullName.Contains(userInput) || command.AliasName.Contains(userInput));

        // Sort the suggestions based on relevance (exact match first)
        _consoleSuggestions.Sort((a, b) =>
        {
            if (a.FullName == userInput || a.AliasName == userInput)
                return -1;
            if (b.FullName == userInput || b.AliasName == userInput)
                return 1;
            return string.Compare(a.FullName, b.FullName, StringComparison.Ordinal);
        });
        View.SetCmdSuggestionList(_consoleSuggestions);
    }
}
