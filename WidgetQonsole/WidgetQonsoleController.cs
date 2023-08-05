using System;
using Alg;
using Gamelib.DataStructures;
using GameLib.Log;
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

        public bool RunAutoexec;

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
            AddFunctionsToRegistryTable();
            AddVariablesToRegistryTable();
            RegisterCommandsAndVariable();

            if (RunAutoexec)
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
                        if (Script.Globals.Get(parType.Name).Type != DataType.Nil)
                        {
                            Debug.LogError($"Can't register lua type '{parType.Name}'. The name already exists globally");
                            break;
                        }
                        Script.Globals[parType.Name] = parType;
                    }
                }
            }
        }

        private void AddFunctionsToRegistryTable()
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

            LogChecker.Print(LogChecker.Level.Normal, $"Registered {ConsoleSystem.Methods.Count} console commands");
            if (LogChecker.Verbose())
                Script.DoString("PrintTable(__CSCommandsRegister, 1, 4)");
        }

        private void AddVariablesToRegistryTable()
        {
            foreach (var consoleVarInfo in ConsoleSystem.Variables)
            {
                var regTable = new Table(Script);
                regTable["alias"] = consoleVarInfo.AliasName;
                regTable["fullName"] = consoleVarInfo.FullName;
                regTable["getter"] = consoleVarInfo.Property.GetGetMethod();
                regTable["setter"] = consoleVarInfo.Property.GetSetMethod();

                Script.Globals["__tmpRegItem"] = regTable; // Put method's parameters (alias,fullName,getter, setter) needed for AddToCommandRegister function on Lua side
                Script.DoString("AddToVariableRegister(__tmpRegItem.alias, __tmpRegItem.fullName, __tmpRegItem.getter,__tmpRegItem.setter)");
            }

            Script.DoString("__tmpRegItem = nil"); // Keep global namespace clean

            LogChecker.Print(LogChecker.Level.Normal, $"Registered {ConsoleSystem.Variables.Count} console variables");
            if (LogChecker.Verbose())
                Script.DoString("PrintTable(__CSVariablesRegister, 1, 4)");
        }

        private void RegisterCommandsAndVariable()
        {
            Script.DoString("Register()");
        }

        public void ExecuteString(string luaCode)
        {
            try
            {
                if (string.IsNullOrEmpty(luaCode))
                    return;
                if( CountLines(luaCode) > 1)
                    Debug.Log($">>\n{luaCode}");
                else
                    Debug.Log($">>{luaCode}");

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

        private static int CountLines(string inputString)
        {
            if (string.IsNullOrEmpty(inputString))
                return 0;

            int lineCount = 1; // If the inputString is not empty, there is at least one line.

            for (int i = 0; i < inputString.Length; i++)
            {
                if (inputString[i] == '\n')
                    lineCount++;
            }

            return lineCount;
        }
    }
}
