// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections.Immutable;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace C2CS.UseCases.BindgenCSharp;

// NOTE: Properties are required for System.Text.Json serialization
[PublicAPI]
public class ConfigurationBindgenCSharp
{
    [JsonPropertyName("aliases")]
    public ImmutableArray<CSharpTypeAlias> Aliases { get; set; } = ImmutableArray<CSharpTypeAlias>.Empty;
}
