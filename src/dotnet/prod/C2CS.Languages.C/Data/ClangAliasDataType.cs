// Copyright (c) Lucas Girouard-Stranks (https://github.com/lithiumtoast). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

namespace C2CS.Languages.C
{
    public record ClangAliasDataType : ClangCommon
    {
        public readonly ClangType UnderlyingType;

        internal ClangAliasDataType(
            string name,
            ClangCodeLocation codeLocation,
            ClangType underlyingType)
            : base(ClangKind.AliasDataType, name, codeLocation)
        {
            UnderlyingType = underlyingType;
        }

        // Required for debugger string with records
        // ReSharper disable once RedundantOverriddenMember
        public override string ToString()
        {
            return base.ToString();
        }
    }
}
