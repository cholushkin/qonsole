using System;
using Gamelib.DataStructures;
using MoonSharp.Interpreter;
using UnityEngine;

namespace Qonsole
{
    public class WidgetQonsoleController : MonoBehaviour
    {
        public class LogEntry
        {
            public string Message { get; }

            public LogEntry(string message)
            {
                Message = message;
            }

        }


        private const int CircularBufferCapacity = 128;
        private CircularBuffer<LogEntry> _items = new CircularBuffer<LogEntry>(CircularBufferCapacity);
        public WidgetQonsoleView View;
        public Script Script { get; private set; }

        void Awake()
        {
            // Intercept debug entries
            Application.logMessageReceivedThreaded -= ReceivedLog;
            Application.logMessageReceivedThreaded += ReceivedLog;

            View.SetItems(_items);


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
            try
            {
                if (string.IsNullOrEmpty(luaCode))
                    return;
                var dVal = Script.DoString(luaCode);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        void ReceivedLog(string logString, string stackTrace, LogType logType)
        {
            _items.Enqueue(new LogEntry(logString));
        }
    }
}
