// Copyright (c) Lucas Girouard-Stranks (https://github.com/lithiumtoast). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

namespace C2CS.UseCases.BindgenCSharp
{
    public record CSharpVariable : CSharpNode
    {
        public string Type { get; set; }

        public CSharpVariable(string name, string locationComment, string type)
            : base(name, locationComment)
        {
            Type = type;
        }
    }
}
