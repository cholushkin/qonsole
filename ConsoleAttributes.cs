using System;

namespace Qonsole
{
	[AttributeUsage( AttributeTargets.Method, Inherited = false, AllowMultiple = true )]
	public class ConsoleMethodAttribute : Attribute
	{
        public string FullName { get; } // Full name including path stored in nested tables in Lua
        public string AliasName { get; } // Short global alias name 
        public string Description { get; }
        public string[] ParameterNames { get; }
        
        public ConsoleMethodAttribute( string fullName, string aliasName, string description, params string[] parameterNames )
		{
            FullName = fullName;
            AliasName = aliasName;
            Description = description;
            ParameterNames = parameterNames;
		}
	}


    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class ConsoleVariableAttribute : Attribute
    {
        public string FullName { get; } // Full name including path stored in nested tables in Lua
        public string AliasName { get; } // Short global alias name 
        public string Description { get; }

        public ConsoleVariableAttribute(string fullName, string aliasName, string description)
        {
            FullName = fullName;
            AliasName = aliasName;
            Description = description;
        }
    }
}