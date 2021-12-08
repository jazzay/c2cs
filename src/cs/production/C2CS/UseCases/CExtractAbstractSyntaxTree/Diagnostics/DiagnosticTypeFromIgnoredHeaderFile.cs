// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

namespace C2CS.UseCases.CExtractAbstractSyntaxTree;

public class DiagnosticTypeFromIgnoredHeaderFile : Diagnostic
{
    public DiagnosticTypeFromIgnoredHeaderFile(string typeName, string filePath)
        : base(DiagnosticSeverity.Warning)
    {
        Summary =
            $"The type '{typeName}' belongs to the ignored file '{filePath}', but is used in the abstract syntax tree.";
    }
}
