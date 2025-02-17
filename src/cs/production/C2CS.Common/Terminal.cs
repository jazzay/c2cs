// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using JetBrains.Annotations;

// ReSharper disable once EmptyNamespace
namespace C2CS;

[PublicAPI]
public static class Terminal
{
    public static string RunCommandWithCapturingStandardOutput(
        this string command,
        string? workingDirectory = null,
        string? fileName = null)
    {
        using var process = CreateProcess(command, workingDirectory, fileName);

        process.Start();
        process.WaitForExit();

        var result = process.StandardOutput.ReadToEnd().Trim('\n', '\r');
        return result;
    }

    public static bool RunCommandWithoutCapturingOutput(
        this string command,
        string? workingDirectory,
        string? fileName = null)
    {
        using var process = CreateProcess(command, workingDirectory, fileName);

        process.OutputDataReceived += OnProcessOnOutputDataReceived;
        process.ErrorDataReceived += OnProcessOnErrorDataReceived;

        process.Start();

        while (!process.HasExited)
        {
            Thread.Sleep(100);

            try
            {
                process.BeginOutputReadLine();
            }
            catch (InvalidOperationException)
            {
                process.CancelOutputRead();
            }

            try
            {
                process.BeginErrorReadLine();
            }
            catch (InvalidOperationException)
            {
                process.CancelErrorRead();
            }
        }

        return process.ExitCode == 0;
    }

    private static void OnProcessOnErrorDataReceived(object sender, DataReceivedEventArgs args)
    {
        Console.WriteLine(args.Data);
    }

    private static void OnProcessOnOutputDataReceived(object sender, DataReceivedEventArgs args)
    {
        Console.WriteLine(args.Data);
    }

    private static Process CreateProcess(string command, string? workingDirectory, string? fileName)
    {
        if (workingDirectory != null && !Directory.Exists(workingDirectory))
        {
            throw new DirectoryNotFoundException(workingDirectory);
        }

        var processStartInfo = new ProcessStartInfo
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = workingDirectory ?? Environment.CurrentDirectory,
            CreateNoWindow = true
        };

        if (!string.IsNullOrEmpty(fileName))
        {
            processStartInfo.FileName = fileName;
            processStartInfo.Arguments = command;
        }
        else
        {
            var platform = Platform.OperatingSystem;
            if (platform == RuntimeOperatingSystem.Windows)
            {
                processStartInfo.FileName = "wsl";
                processStartInfo.Arguments = command;
            }
            else
            {
                processStartInfo.FileName = "bash";
                var escapedArgs = command.Replace("\"", "\\\"", StringComparison.InvariantCulture);
                processStartInfo.Arguments = $"-c \"{escapedArgs}\"";
            }
        }

        var process = new Process
        {
            StartInfo = processStartInfo
        };
        return process;
    }

    public static bool CMake(string rootDirectory, string cMakeDirectoryPath, string targetLibraryDirectoryPath)
    {
        if (!Directory.Exists(rootDirectory))
        {
            throw new DirectoryNotFoundException(cMakeDirectoryPath);
        }

        if (!Directory.Exists(cMakeDirectoryPath))
        {
            throw new DirectoryNotFoundException(cMakeDirectoryPath);
        }

        var cMakeCommand = "cmake -S . -B cmake-build-release -G 'Unix Makefiles' -DCMAKE_BUILD_TYPE=Release";

        var platform = Platform.OperatingSystem;
        if (platform == RuntimeOperatingSystem.Windows)
        {
            var toolchainFilePath = WindowsToLinuxPath($"{rootDirectory}/mingw-w64-x86_64.cmake");
            cMakeCommand += $" -DCMAKE_TOOLCHAIN_FILE=\"{toolchainFilePath}\"";
        }

        var isSuccess = cMakeCommand.RunCommandWithoutCapturingOutput(cMakeDirectoryPath);
        if (!isSuccess)
        {
            return false;
        }

        isSuccess = "make -C ./cmake-build-release".RunCommandWithoutCapturingOutput(cMakeDirectoryPath);
        if (!isSuccess)
        {
            return false;
        }

        var outputDirectoryPath = Path.Combine(cMakeDirectoryPath, "lib");
        if (!Directory.Exists(outputDirectoryPath))
        {
            return false;
        }

        var runtimePlatform = Platform.OperatingSystem;
        var libraryFileNameExtension = Platform.LibraryFileNameExtension(runtimePlatform);
        var outputFilePaths = Directory.EnumerateFiles(
            outputDirectoryPath, $"*{libraryFileNameExtension}", SearchOption.AllDirectories);
        foreach (var outputFilePath in outputFilePaths)
        {
            var targetFilePath = outputFilePath.Replace(
                    outputDirectoryPath, targetLibraryDirectoryPath, StringComparison.InvariantCulture);
            var targetFileName = Path.GetFileName(targetFilePath);

            if (runtimePlatform == RuntimeOperatingSystem.Windows)
            {
                if (targetFileName.StartsWith("lib", StringComparison.InvariantCulture))
                {
                    targetFileName = targetFileName[3..];
                }
            }

            var targetFileDirectoryPath = Path.GetDirectoryName(targetFilePath)!;
            targetFilePath = Path.Combine(targetFileDirectoryPath, targetFileName);
            if (!Directory.Exists(targetFileDirectoryPath))
            {
                Directory.CreateDirectory(targetFileDirectoryPath);
            }

            if (File.Exists(targetFilePath))
            {
                File.Delete(targetFilePath);
            }

            File.Copy(outputFilePath, targetFilePath);
        }

        Directory.Delete(outputDirectoryPath, true);
        Directory.Delete($"{cMakeDirectoryPath}/cmake-build-release", true);

        return true;
    }

    public static string DotNetPath()
    {
        Version dotNetRuntimeVersion = new(0, 0, 0, 0);
        var dotNetPath = string.Empty;
        var dotnetRuntimesString = "dotnet --list-runtimes".RunCommandWithCapturingStandardOutput();
        var dotnetRuntimesStrings =
            dotnetRuntimesString.Split(new[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var x in dotnetRuntimesStrings)
        {
            var parse = x.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            if (!parse[0].Contains("Microsoft.NETCore.App", StringComparison.InvariantCulture))
            {
                continue;
            }

            var versionCandidate = parse[1];
            var versionCharIndexHyphen = versionCandidate.IndexOf('-', StringComparison.InvariantCulture);
            if (versionCharIndexHyphen != -1)
            {
                // can possibly happen for release candidates of .NET
                versionCandidate = versionCandidate[..versionCharIndexHyphen];
            }

            var version = Version.Parse(versionCandidate);
            if (version <= dotNetRuntimeVersion)
            {
                continue;
            }

            dotNetRuntimeVersion = version;
            dotNetPath = Path.Combine(parse[2].Trim('[', ']'), parse[1]);
        }

        return dotNetPath;
    }

    private static string WindowsToLinuxPath(string path)
    {
        var pathWindows = Path.GetFullPath(path);
        var pathRootWindows = Path.GetPathRoot(pathWindows)!;
        var pathRootLinux = $"/mnt/{pathRootWindows.ToUpperInvariant()[0]}/";
        var pathLinux = pathWindows
            .Replace(pathRootWindows, pathRootLinux, StringComparison.InvariantCulture)
            .Replace('\\', '/');
        return pathLinux;
    }
}
