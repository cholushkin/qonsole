using System.Text;
using UnityEngine;

namespace Qonsole
{
    public class ConsoleCommandsConsoleRoutines
    {
        [ConsoleMethod("commands.list", "ls", "Print list of all available commands"), UnityEngine.Scripting.Preserve]
		public static void PrintAllCommands()
        {
            int length = 25;
            for (int i = 0; i < ConsoleSystem.Methods.Count; i++)
            {
                if (ConsoleSystem.Methods[i].IsValid())
                    length += ConsoleSystem.Methods[i].Signature.Length + 7;
            }

            StringBuilder stringBuilder = new StringBuilder(length);
            stringBuilder.Append("Available commands:");

            for (int i = 0; i < ConsoleSystem.Methods.Count; i++)
            {
                if (ConsoleSystem.Methods[i].IsValid())
                    stringBuilder.Append("\n    - ").Append(ConsoleSystem.Methods[i].Signature);
            }

            Debug.Log(stringBuilder.ToString());
        }

        // todo:
        // help(command name)
        // list variables
        // reset variables to it's default values

	}
}