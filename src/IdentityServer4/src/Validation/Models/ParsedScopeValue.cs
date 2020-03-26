// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityServer4.Validation
{
    /// <summary>
    /// Models a parsed scope value.
    /// </summary>
    public class ParsedScopeValue
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="name"></param>
        public ParsedScopeValue(string name)
            : this(name, name, null)
        {
        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="paramaterValue"></param>
        public ParsedScopeValue(string name, string value, string paramaterValue)
        {
            Name = name;
            Value = value;
            ParameterValue = paramaterValue;
        }

        /// <summary>
        /// The name of the scope.
        /// </summary>
        public string Name { get; set; }

        // future: maybe this should be something w/ more structure? dictionary?

        /// <summary>
        /// The value of the parsed scope. If the scope has no structure, then the value will be the same as the name.
        /// </summary>
        public string Value { get; set; }
        
        /// <summary>
        /// The parameter value of the parsed scope. If the scope has no structure, then the value will be null.
        /// </summary>
        public string ParameterValue { get; set; }
    }
}