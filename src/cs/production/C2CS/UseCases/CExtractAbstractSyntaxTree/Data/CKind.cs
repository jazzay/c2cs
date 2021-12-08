// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Diagnostics.CodeAnalysis;

namespace C2CS.UseCases.CExtractAbstractSyntaxTree;

public enum CKind
{
    Unknown = 0,
    TranslationUnit,
    Primitive,
    Pointer,
    Array,
    Function,
    FunctionPointer,
    Record,
    Enum,
    EnumValue,
    OpaqueType,
    Typedef,
    Variable,
    MacroDefinition
}
