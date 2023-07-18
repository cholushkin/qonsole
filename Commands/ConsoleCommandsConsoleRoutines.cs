using System.Text;
using UnityEngine;

namespace Qonsole
{
    public class ConsoleCommandsConsoleRoutines
    {
        [ConsoleMethod("console.list", "help", "Print the list of all available commands"), UnityEngine.Scripting.Preserve]
		public static void PrintAllCommands()
        {
            int length = 25;
            for (int i = 0; i < ConsoleSystem.Methods.Count; i++)
            {
                if (ConsoleSystem.Methods[i].IsValid())
                    length += ConsoleSystem.Methods[i].Signature.Length + 7;
            }

            StringBuilder stringBuilder = new StringBuilder(length);
            stringBuilder.Append($"Available commands: {ConsoleSystem.Methods.Count}");

            for (int i = 0; i < ConsoleSystem.Methods.Count; i++)
            {
                if (ConsoleSystem.Methods[i].IsValid())
                    stringBuilder.Append("\n  - ").Append(ConsoleSystem.Methods[i].Signature);
            }

            Debug.Log(stringBuilder.ToString());
        }

        [ConsoleMethod("console.listvar", "lsv", "Print the list of all available variables"), UnityEngine.Scripting.Preserve]
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

        [ConsoleMethod("console.clear", "cls", "Clear the console"), UnityEngine.Scripting.Preserve]
        public static void ClearConsole()
        {
            WidgetQonsoleController.Instance.Clear();
        }



        // todo:
        // help(command name)
        // list variables
        // reset variables to it's default values

    }
}