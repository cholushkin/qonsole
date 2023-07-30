using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MoonSharp.Interpreter;
using UnityEngine;

namespace Qonsole
{
    public class ConsoleCommandsConsoleRoutines
    {
        [ConsoleMethod("console.cmd.list", "help", "Print the list of all available commands", "Specify commands to print by a wildcard. Works with full names of commands"), UnityEngine.Scripting.Preserve]
		public static void PrintAllCommands(string wildcard = null)
        {
            StringBuilder stringBuilder = new StringBuilder(4096);
            int counter = 0;

            stringBuilder.Append($"Format: FullName<AliasName>(Parameters) : Description\n");

            for (int i = 0; i < ConsoleSystem.Methods.Count; i++)
            {
                if (!string.IsNullOrEmpty(wildcard))
                {
                    var regExpression = _wildCardToRegular(wildcard);
                    var pass = Regex.IsMatch(ConsoleSystem.Methods[i].FullName, regExpression);
                    if (!pass)
                        continue;
                }

                stringBuilder.Append("\n  - ").Append(ConsoleSystem.Methods[i].Signature);
                if(!string.IsNullOrEmpty(ConsoleSystem.Methods[i].CmdDescription))
                    stringBuilder.Append(" : ").Append(ConsoleSystem.Methods[i].CmdDescription);
                if (!ConsoleSystem.Methods[i].IsValid())
                    stringBuilder.Append("[Invalid]");
                ++counter;
            }

            stringBuilder.Append($"\n\nCommands amount: {counter}");

            Debug.Log(stringBuilder.ToString());
        }

        [ConsoleMethod("console.cmd.help", "hlp", "Print help for specific command", "Full name or alias of command. Parameter could be a string or a direct address of the function. For example: hlp(hlp) or hlp('hlp') or hlp('console.cmd.hlp')"), UnityEngine.Scripting.Preserve]
        public static void PrintCommandHelp(DynValue command)
        {
            void PrintCommandHelpImpl(ConsoleSystem.ConsoleMethodInfo consoleMethodInfo)
            {
                StringBuilder stringBuilder = new StringBuilder(256);
                stringBuilder.Append(consoleMethodInfo.Signature);
                if (!string.IsNullOrEmpty(consoleMethodInfo.CmdDescription))
                    stringBuilder.Append(" : ").Append(consoleMethodInfo.CmdDescription);
                if (!consoleMethodInfo.IsValid())
                    stringBuilder.Append("[Invalid]");
                Debug.Log(stringBuilder.ToString());
                foreach (var desc in consoleMethodInfo.ParameterDescriptions)
                {
                    Debug.Log($"  - {desc}");
                }
            }

            if (command.Callback != null)
            {
                var reg = ConsoleSystem.Methods.FirstOrDefault(x => command.Callback.Name == x.Method.Name);
                if (reg == null)
                {
                    Debug.LogError($"Can't find Clr method {command.Callback.Name}");
                    return;
                }

                PrintCommandHelpImpl(reg);
                return;
            }

            if (command.String != null)
            {
                var commandName = command.String;
                commandName = commandName.ToLower().Trim();

                var methodInfo = ConsoleSystem.Methods.FirstOrDefault(x => x.AliasName == commandName);
                if (methodInfo == null)
                    methodInfo = ConsoleSystem.Methods.FirstOrDefault(x => x.FullName == commandName);
                if (methodInfo == null)
                {
                    Debug.LogError($"Can't find command with full name or alias '{commandName}'");
                    return;
                }

                PrintCommandHelpImpl(methodInfo);
            }
        }

        [ConsoleMethod("console.var.list", "lsv", "Print the list of all available variables", "Specify variables to print by a wildcard. Works with full names of variables"), UnityEngine.Scripting.Preserve]
        public static void PrintAllVariables(string wildcard = null)
        {
            StringBuilder stringBuilder = new StringBuilder(4096);
            int counter = 0;

            for (int i = 0; i < ConsoleSystem.Variables.Count; i++)
            {
                if (!string.IsNullOrEmpty(wildcard))
                {
                    var regExpression = _wildCardToRegular(wildcard);
                    var pass = Regex.IsMatch(ConsoleSystem.Variables[i].FullName, regExpression);
                    if (!pass)
                        continue;
                }
                stringBuilder.Append("  - ").Append(ConsoleSystem.Variables[i].Signature).AppendLine();
                ++counter;
            }

            stringBuilder.AppendLine();
            stringBuilder.AppendLine($"Variables amount: {counter}");

            Debug.Log(stringBuilder.ToString());
        }

        [ConsoleMethod("console.printregisteredtypes", "printregisteredtypes", "Print the list of registered types"), UnityEngine.Scripting.Preserve]
        public static void PrintRegisteredTypes()
        {
            Debug.Log("Registered types:");
            var registeredTypes = UserData.GetRegisteredTypes();
            foreach (var registeredType in registeredTypes)
                Debug.Log(registeredType);
        }

        [ConsoleMethod("console.clear", "cls", "Clear the console"), UnityEngine.Scripting.Preserve]
        public static void ClearConsole()
        {
            WidgetQonsoleController.Instance.Clear();
        }

        private static string _wildCardToRegular(string wildcard)
        {
            return "^" + Regex.Escape(wildcard).Replace("\\*", ".*") + "$";
        }


        // todo:
        // console.dumphelp - Print all cmds and vars help to the text file

    }
}