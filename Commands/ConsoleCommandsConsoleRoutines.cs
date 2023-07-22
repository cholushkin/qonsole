using System.Linq;
using System.Text;
using MoonSharp.Interpreter;
using UnityEngine;

namespace Qonsole
{
    public class ConsoleCommandsConsoleRoutines
    {
        [ConsoleMethod("console.cmd.list", "help", "Print the list of all available commands"), UnityEngine.Scripting.Preserve]
		public static void PrintAllCommands()
        {
            StringBuilder stringBuilder = new StringBuilder(256);
            stringBuilder.Append($"Available commands: {ConsoleSystem.Methods.Count}");

            for (int i = 0; i < ConsoleSystem.Methods.Count; i++)
            {
                stringBuilder.Append("\n  - ").Append(ConsoleSystem.Methods[i].Signature);
                if(!string.IsNullOrEmpty(ConsoleSystem.Methods[i].CmdDescription))
                    stringBuilder.Append(" : ").Append(ConsoleSystem.Methods[i].CmdDescription);
                if (!ConsoleSystem.Methods[i].IsValid())
                    stringBuilder.Append("[Invalid]");
            }

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

        [ConsoleMethod("console.var.list", "lsv", "Print the list of all available variables"), UnityEngine.Scripting.Preserve]
        public static void PrintAllVariables()
        {
            int length = 25;
            for (int i = 0; i < ConsoleSystem.Variables.Count; i++)
            {
                //if (ConsoleSystem.Variables[i].IsValid())
                    length += ConsoleSystem.Variables[i].Signature.Length + 7;
            }

            StringBuilder stringBuilder = new StringBuilder(length);
            stringBuilder.Append($"Available variables: {ConsoleSystem.Variables.Count}");

            for (int i = 0; i < ConsoleSystem.Variables.Count; i++)
            {
                //if (ConsoleSystem.Variables[i].IsValid())
                {
                    stringBuilder.Append("\n  - ").Append(ConsoleSystem.Variables[i].Signature);
                    //stringBuilder.Append($" = {ConsoleSystem.Variables[i].Field.GetValue(null)}"); // Print default value
                }
            }

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



        // todo:
        // console.cmd.help - Print help for a specific command
        // console.dumphelp - Print all cmds and vars help to the text file

    }
}