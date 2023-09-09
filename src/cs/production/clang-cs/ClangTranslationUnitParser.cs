// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System;
using System.Collections.Immutable;

public static class ClangTranslationUnitParser
{
    public static Clang.CXTranslationUnit Parse(
        string headerFilePath,
        ImmutableArray<string> clangArgs)
    {
        var clangArgsConcat = string.Join(" ", clangArgs);
        Console.WriteLine($"libclang: Parsing '{headerFilePath}' with the following arguments...");
        Console.WriteLine($"\t{clangArgsConcat}");

        if (!TryParseTranslationUnit(headerFilePath, clangArgs, out var translationUnit))
        {
            throw new ClangException("libclang failed.");
        }

        var diagnostics = GetCompilationDiagnostics(translationUnit);
        if (diagnostics.IsDefaultOrEmpty)
        {
            return translationUnit;
        }

        var defaultDisplayOptions = Clang.clang_defaultDiagnosticDisplayOptions();
        Console.Error.WriteLine("Clang diagnostics:");
        var hasErrors = false;
        foreach (var diagnostic in diagnostics)
        {
            Console.Error.Write("\t");
            var clangString = Clang.clang_formatDiagnostic(diagnostic, defaultDisplayOptions);
            var diagnosticStringC = Clang.clang_getCString(clangString);
            var diagnosticString = Clang.CStrings.String(diagnosticStringC);
            Console.Error.WriteLine(diagnosticString);

            var severity = Clang.clang_getDiagnosticSeverity(diagnostic);
            if (severity == Clang.CXDiagnosticSeverity.CXDiagnostic_Error ||
                severity == Clang.CXDiagnosticSeverity.CXDiagnostic_Fatal)
            {
                hasErrors = true;
            }
        }

        if (hasErrors)
        {
            throw new ClangException("Clang parsing errors.");
        }

        return translationUnit;
    }

    private static unsafe bool TryParseTranslationUnit(
        string filePath,
        ImmutableArray<string> commandLineArgs,
        out Clang.CXTranslationUnit translationUnit)
    {
        // ReSharper disable BitwiseOperatorOnEnumWithoutFlags
        const uint options = 0x00001000 | // CXTranslationUnit_IncludeAttributedTypes
                             0x00004000 | // CXTranslationUnit_IgnoreNonErrorsFromIncludedFiles
                             0x00000040 | // CXTranslationUnit_SkipFunctionBodies
                             0x1 | // CXTranslationUnit_DetailedPreprocessingRecord
                             0x0;

        var index = Clang.clang_createIndex(0, 0);
        var cSourceFilePath = Clang.CStrings.CString(filePath);
        var cCommandLineArgs = Clang.CStrings.CStringArray(commandLineArgs.AsSpan());

        Clang.CXErrorCode errorCode;
        fixed (Clang.CXTranslationUnit* translationUnitPointer = &translationUnit)
        {
            errorCode = Clang.clang_parseTranslationUnit2(
                index,
                cSourceFilePath,
                cCommandLineArgs,
                commandLineArgs.Length,
                (Clang.CXUnsavedFile*)IntPtr.Zero,
                0,
                options,
                translationUnitPointer);
        }

        var result = errorCode == Clang.CXErrorCode.CXError_Success;
        return result;
    }

    private static ImmutableArray<Clang.CXDiagnostic> GetCompilationDiagnostics(Clang.CXTranslationUnit translationUnit)
    {
        var diagnosticsCount = (int)Clang.clang_getNumDiagnostics(translationUnit);
        var builder = ImmutableArray.CreateBuilder<Clang.CXDiagnostic>(diagnosticsCount);

        for (uint i = 0; i < diagnosticsCount; ++i)
        {
            var diagnostic = Clang.clang_getDiagnostic(translationUnit, i);
            builder.Add(diagnostic);
        }

        return builder.ToImmutable();
    }
}
