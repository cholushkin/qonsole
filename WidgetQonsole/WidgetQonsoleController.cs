using System;
using System.Text;
using Alg;
using Gamelib.DataStructures;
using MoonSharp.Interpreter;
using MoonSharp.UnityWrapper;
using UnityEngine;

namespace Qonsole
{
    public class WidgetQonsoleController : Singleton<WidgetQonsoleController>
    {
        public class LogEntry
        {
            public string Message { get; }
            public string StackTrace { get; }
            public LogType LogType { get; }

            public LogEntry(string message, string stackTrace, LogType logType)
            {
                Message = message;
                StackTrace = stackTrace;
                LogType = logType;
            }
        }


        private const int CircularBufferCapacity = 4096;
        private CircularBuffer<LogEntry> _items = new CircularBuffer<LogEntry>(CircularBufferCapacity);
        public WidgetQonsoleView View;
        public Script Script { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            // Intercept debug entries
            Application.logMessageReceivedThreaded -= ReceivedLog;
            Application.logMessageReceivedThreaded += ReceivedLog;

            View.SetItems(_items);


            // Redefine print to print using Unity Debug.Log
            Script.DefaultOptions.DebugPrint = s => Debug.Log(s);
            
            Script = new Script();
            Script.DoFile("QonsoleLuaScripts/LuaImplHelpers");

            ConsoleSystem.SortMethodsTable();
            ConsoleSystem.PrepareSearchTable();

            RegisterLuaWrapperTypes();
            RegisterParameterTypes();
            RegisterLuaFunctions();
            RegisterLuaVariables();

            Script.DoFile("Autoexec");
        }

        private void RegisterLuaWrapperTypes()
        {
            LuaVector3.RegisterWrapperType(Script);
        }

        private void RegisterParameterTypes()
        {
            foreach (var consoleMethodInfo in ConsoleSystem.Methods)
            {
                foreach (var parType in consoleMethodInfo.ParameterTypes)
                {
                    if (!UserData.IsTypeRegistered(parType) && parType.IsEnum)
                    {
                        UserData.RegisterType(parType);
                        // todo: check availability of the name
                        Script.Globals[parType.Name] = parType;
                    }
                }
            }
        }

        private void RegisterLuaFunctions()
        {
            foreach (var consoleMethodInfo in ConsoleSystem.Methods)
            {
                var regTable = new Table(Script);
                regTable["alias"] = consoleMethodInfo.AliasName;
                regTable["fullName"] = consoleMethodInfo.FullName;
                regTable["func"] = consoleMethodInfo.Method;
                Script.Globals["__tmpRegItem"] = regTable; // Put method's parameters (alias,fullName,CS static method) needed for AddToCommandRegister function on Lua side
                Script.DoString("AddToCommandRegister(__tmpRegItem.alias, __tmpRegItem.fullName, __tmpRegItem.func)"); 
            }

            Script.DoString("__tmpRegItem = nil"); // Keep global namespace clean
            Script.DoString("RegisterCommands()"); // Distribute names to namespaces in Lua, check for path conflicts 


            Debug.Log($"Registered {ConsoleSystem.Methods.Count} console commands");
        }

        private void RegisterLuaVariables()
        {
            StringBuilder sb = new StringBuilder(1024);
            int registeredCounter = 0;

            
            foreach (var variableInfo in ConsoleSystem.Variables)
            {
                try
                {
                    Script.Globals[variableInfo.AliasName] = variableInfo.Property;
                    sb.Append(variableInfo.AliasName);
                    sb.Append(" ");
                    registeredCounter++;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Exception registering {variableInfo.AliasName}: " + e.Message);
                }
            }
            Debug.Log($"Registered {registeredCounter} console variables");
            if(registeredCounter > 0)
                Debug.Log(sb.ToString());
        }

        public void ExecuteString(string luaCode)
        {
            try
            {
                if (string.IsNullOrEmpty(luaCode))
                    return;
                Debug.Log($">> {luaCode}");
                var dVal = Script.DoString(luaCode);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void Clear()
        {
            _items.Clear();
        }

        void ReceivedLog(string logString, string stackTrace, LogType logType)
        {
            _items.Enqueue(new LogEntry(logString, stackTrace, logType));
        }
    }
}
