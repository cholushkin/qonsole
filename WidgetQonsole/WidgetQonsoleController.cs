using System.Collections;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using NaughtyAttributes;
using UnityEngine;

namespace Qonsole
{
    public class WidgetQonsoleController : MonoBehaviour
    {
        [ResizableTextArea] public string LuaScript;
        [Button]
        void RunScript()
        {
            ExecuteString(LuaScript);
        }


        public WidgetQonsoleView View;
        public Script Script { get; private set; }


        
        void Awake()
        {
            // Redefine print to print using Unity Debug.Log
            Script.DefaultOptions.DebugPrint = s => Debug.Log(s);
            Script = new Script();

            RegisterLuaFunctions();

        }

        private void RegisterLuaFunctions()
        {
            foreach (var consoleMethodInfo in ConsoleSystem.Methods)
            {
                Script.Globals[consoleMethodInfo.AliasName] = consoleMethodInfo.Method;
            }
            Debug.Log($"Registered {ConsoleSystem.Methods.Count} console commands");
        }

        public void ExecuteString(string luaCode)
        {
            if(string.IsNullOrEmpty(luaCode))
                return;
            var dVal = Script.DoString(luaCode);
        }
    }


}
