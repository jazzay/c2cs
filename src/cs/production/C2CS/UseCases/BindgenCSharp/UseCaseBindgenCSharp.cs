// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System;
using System.Collections.Immutable;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using C2CS.UseCases.ExtractAbstractSyntaxTreeC;

namespace C2CS.UseCases.BindgenCSharp;

public class UseCaseBindgenCSharp : UseCase<RequestBindgenCSharp, ResponseBindgenCSharp>
{
    protected override void Execute(RequestBindgenCSharp request, ResponseBindgenCSharp response)
    {
        Validate(request);
        TotalSteps(4);

        var abstractSyntaxTreeC = Step(
            "Load C abstract syntax tree from disk",
            request.InputFilePath,
            LoadAbstractSyntaxTree);

        var abstractSyntaxTreeCSharp = Step(
            "Map C abstract syntax tree to C#",
            request.ClassName,
            abstractSyntaxTreeC,
            request.TypeAliases,
            request.IgnoredTypeNames,
            abstractSyntaxTreeC.Bitness,
            Diagnostics,
            MapCToCSharp);

        var codeCSharp = Step(
            "Generate C# code",
            abstractSyntaxTreeCSharp,
            request,
            GenerateCSharpCode);

        Step(
            "Write C# code to disk",
            request.OutputFilePath,
            codeCSharp,
            WriteCSharpCode);
    }

    private static void Validate(RequestBindgenCSharp request)
    {
        if (!File.Exists(request.InputFilePath))
        {
            throw new UseCaseException($"File does not exist: `{request.InputFilePath}`.");
        }
    }

    private static CAbstractSyntaxTree LoadAbstractSyntaxTree(string inputFilePath)
    {
        var fileContents = File.ReadAllText(inputFilePath);
        var serializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            }
        };
        var serializerContext = new CJsonSerializerContext(serializerOptions);
        var abstractSyntaxTree = JsonSerializer.Deserialize(fileContents, serializerContext.CAbstractSyntaxTree)!;
        return abstractSyntaxTree;
    }

    private static CSharpAbstractSyntaxTree MapCToCSharp(
        string className,
        CAbstractSyntaxTree abstractSyntaxTree,
        ImmutableArray<CSharpTypeAlias> typeAliases,
        ImmutableArray<string> ignoredTypeNames,
        int bitness,
        DiagnosticsSink diagnostics)
    {
        var mapper = new CSharpMapper(className, typeAliases, ignoredTypeNames, bitness, diagnostics);
        return mapper.AbstractSyntaxTree(abstractSyntaxTree);
    }

    private static string GenerateCSharpCode(
        CSharpAbstractSyntaxTree abstractSyntaxTree,
        RequestBindgenCSharp request)
    {
        var codeGenerator = new CSharpCodeGenerator(request);
        return codeGenerator.EmitCode(abstractSyntaxTree);
    }

    private static void WriteCSharpCode(
        string outputFilePath, string codeCSharp)
    {
        var outputDirectory = Path.GetDirectoryName(outputFilePath)!;
        if (string.IsNullOrEmpty(outputDirectory))
        {
            outputDirectory = AppContext.BaseDirectory;
            outputFilePath = Path.Combine(Environment.CurrentDirectory, outputFilePath);
        }

        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        File.WriteAllText(outputFilePath, codeCSharp);
        Console.WriteLine(outputFilePath);
    }
}
