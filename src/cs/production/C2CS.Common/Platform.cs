﻿// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System;
using System.Runtime.InteropServices;

namespace C2CS;

/// <summary>
///     The collection of utilities for platform interoperability with native libraries in C#.
/// </summary>
public static class Platform
{
    /// <summary>
    ///     Gets the current <see cref="RuntimeOperatingSystem" />.
    /// </summary>
    public static RuntimeOperatingSystem OperatingSystem => GetRuntimeOperatingSystem();

    /// <summary>
    ///     Gets the current <see cref="RuntimeArchitecture" />.
    /// </summary>
    public static RuntimeArchitecture Architecture => GetRuntimeArchitecture();

    /// <summary>
    ///     Gets the library file name extension given a <see cref="RuntimeOperatingSystem" />.
    /// </summary>
    /// <param name="operatingSystem">The runtime platform.</param>
    /// <returns>
    ///     A <see cref="string" /> containing the library file name extension for the <paramref name="operatingSystem" />
    ///     .
    /// </returns>
    /// <exception cref="NotImplementedException"><paramref name="operatingSystem" /> is not available yet with .NET 5.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="operatingSystem" /> is not a known valid value.</exception>
    public static string LibraryFileNameExtension(RuntimeOperatingSystem operatingSystem)
    {
        switch (operatingSystem)
        {
            case RuntimeOperatingSystem.Windows:
            case RuntimeOperatingSystem.Xbox:
                return ".dll";
            case RuntimeOperatingSystem.macOS:
            case RuntimeOperatingSystem.tvOS:
            case RuntimeOperatingSystem.iOS:
                return ".dylib";
            case RuntimeOperatingSystem.Linux:
            case RuntimeOperatingSystem.FreeBSD:
            case RuntimeOperatingSystem.Android:
            case RuntimeOperatingSystem.PlayStation:
                return ".so";
            case RuntimeOperatingSystem.Browser:
            case RuntimeOperatingSystem.Switch:
            case RuntimeOperatingSystem.Unknown:
                throw new NotImplementedException();
            default:
                throw new ArgumentOutOfRangeException(nameof(operatingSystem), operatingSystem, null);
        }
    }

    /// <summary>
    ///     Gets the library file name prefix for a <see cref="RuntimeOperatingSystem" />.
    /// </summary>
    /// <param name="targetOperatingSystem">The runtime platform.</param>
    /// <returns>
    ///     A <see cref="string" /> containing the library file name prefix for the
    ///     <paramref name="targetOperatingSystem" />.
    /// </returns>
    /// <exception cref="NotImplementedException"><paramref name="targetOperatingSystem" /> is not available yet with .NET 5.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="targetOperatingSystem" /> is not a known valid value.</exception>
    public static string LibraryFileNamePrefix(RuntimeOperatingSystem targetOperatingSystem)
    {
        switch (targetOperatingSystem)
        {
            case RuntimeOperatingSystem.Windows:
            case RuntimeOperatingSystem.Xbox:
                return string.Empty;
            case RuntimeOperatingSystem.macOS:
            case RuntimeOperatingSystem.tvOS:
            case RuntimeOperatingSystem.iOS:
            case RuntimeOperatingSystem.Linux:
            case RuntimeOperatingSystem.FreeBSD:
            case RuntimeOperatingSystem.Android:
            case RuntimeOperatingSystem.PlayStation:
                return "lib";
            case RuntimeOperatingSystem.Browser:
            case RuntimeOperatingSystem.Switch:
            case RuntimeOperatingSystem.Unknown:
                throw new NotImplementedException();
            default:
                throw new ArgumentOutOfRangeException(nameof(targetOperatingSystem), targetOperatingSystem, null);
        }
    }

    private static RuntimeArchitecture GetRuntimeArchitecture()
    {
        return RuntimeInformation.OSArchitecture switch
        {
            System.Runtime.InteropServices.Architecture.Arm64 => RuntimeArchitecture.ARM64,
            System.Runtime.InteropServices.Architecture.Arm => RuntimeArchitecture.ARM32,
            System.Runtime.InteropServices.Architecture.X86 => RuntimeArchitecture.X86,
            System.Runtime.InteropServices.Architecture.X64 => RuntimeArchitecture.X64,
            System.Runtime.InteropServices.Architecture.Wasm => RuntimeArchitecture.Unknown,
            System.Runtime.InteropServices.Architecture.S390x => RuntimeArchitecture.Unknown,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static RuntimeOperatingSystem GetRuntimeOperatingSystem()
    {
        if (System.OperatingSystem.IsWindows())
        {
            return RuntimeOperatingSystem.Windows;
        }

        if (System.OperatingSystem.IsMacOS())
        {
            return RuntimeOperatingSystem.macOS;
        }

        if (System.OperatingSystem.IsLinux())
        {
            return RuntimeOperatingSystem.Linux;
        }

        if (System.OperatingSystem.IsAndroid())
        {
            return RuntimeOperatingSystem.Android;
        }

        if (System.OperatingSystem.IsIOS())
        {
            return RuntimeOperatingSystem.iOS;
        }

        if (System.OperatingSystem.IsTvOS())
        {
            return RuntimeOperatingSystem.tvOS;
        }

        if (System.OperatingSystem.IsBrowser())
        {
            return RuntimeOperatingSystem.Browser;
        }

        return RuntimeOperatingSystem.Unknown;
    }
}
