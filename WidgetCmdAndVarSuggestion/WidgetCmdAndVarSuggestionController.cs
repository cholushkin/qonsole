using System;
using System.Collections.Generic;
using Qonsole;
using UnityEngine;

public class WidgetCmdAndVarSuggestionController : MonoBehaviour
{
    public WidgetQonsoleView View;
    private List<ConsoleSystem.ConsoleMethodInfo> _consoleCmdSuggestions;
    private List<ConsoleSystem.ConsoleVariableInfo> _consoleVarSuggestions;


    void Awake()
    {
        _consoleCmdSuggestions = new List<ConsoleSystem.ConsoleMethodInfo>(128);
        _consoleVarSuggestions = new List<ConsoleSystem.ConsoleVariableInfo>(128);
        View.SetCmdSuggestionList(_consoleCmdSuggestions);
        View.SetVarSuggestionList(_consoleVarSuggestions);
    }


    public void UpdateSuggestionLists(string userInput)
    {
        _consoleCmdSuggestions = GetCmdSuggestions(userInput);
        View.SetCmdSuggestionList(_consoleCmdSuggestions);

        _consoleVarSuggestions = GetVarSuggestions(userInput);
        View.SetVarSuggestionList(_consoleVarSuggestions);
    }


    private List<ConsoleSystem.ConsoleMethodInfo> GetCmdSuggestions(string userInput)
    {
        // Find commands that contain the input text
        var cmds = ConsoleSystem.Methods.FindAll(command => command.FullName.Contains(userInput) || command.AliasName.Contains(userInput));

        // Sort the suggestions based on relevance (exact match first)
        cmds.Sort((a, b) =>
        {
            if (a.FullName == userInput || a.AliasName == userInput)
                return -1;
            if (b.FullName == userInput || b.AliasName == userInput)
                return 1;
            return string.Compare(a.FullName, b.FullName, StringComparison.Ordinal);
        });

        return cmds;
    }


    private List<ConsoleSystem.ConsoleVariableInfo> GetVarSuggestions(string userInput)
    {
        // Find vars that contain the input text
        var vars = ConsoleSystem.Variables.FindAll(varInfo => varInfo.FullName.Contains(userInput) || varInfo.AliasName.Contains(userInput));

        // Sort the suggestions based on relevance (exact match first)
        vars.Sort((a, b) =>
        {
            if (a.FullName == userInput || a.AliasName == userInput)
                return -1;
            if (b.FullName == userInput || b.AliasName == userInput)
                return 1;
            return string.Compare(a.FullName, b.FullName, StringComparison.Ordinal);
        });

        return vars;
    }

}
