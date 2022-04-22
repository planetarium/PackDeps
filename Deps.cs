using System;
using System.Linq;

namespace PackDeps.Deps;

using System.Collections.Generic;
using System.Text.Json.Serialization;

public class Document
{
    [JsonPropertyName("runtimeTarget")]
    public RuntimeTarget RuntimeTarget { get; set; } = new RuntimeTarget();

    [JsonPropertyName("targets")]
    public Dictionary<string, Dictionary<string, Reference>> Targets
    {
        get;
        set;
    } = new Dictionary<string, Dictionary<string, Reference>>();

    [JsonPropertyName("libraries")]
    public Dictionary<string, Library> Libraries
    {
        get;
        set;
    } = new Dictionary<string, Library>();
}

public class RuntimeTarget
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
}

public class Reference
{
    [JsonPropertyName("dependencies")]
    public Dictionary<string, string>? Dependencies { get; set; }

    [JsonPropertyName("runtime")]
    public Dictionary<string, LinkedAssembly>? Runtime { get; set; }
}

public class LinkedAssembly
{
    [JsonPropertyName("assemblyVersion")]
    public string AssemblyVersion { get; set; } = "";

    [JsonPropertyName("fileVersion")]
    public string FileVersion { get; set; } = "";
}

public class Library
{
    [JsonPropertyName("type")]
    public LibraryType Type { get; set; }

    [JsonPropertyName("serviceable")]
    public bool Serviceable { get; set; }

    [JsonPropertyName("sha512")] public string Sha512String { get; set; } = "";

    [JsonIgnore]
    public byte[] Sha512
    {
        get
        {
            if (Sha512String is null || !Sha512String.Any())
            {
                return Array.Empty<byte>();
            }
            else if (!Sha512String.StartsWith("sha512-"))
            {
                throw new InvalidOperationException(
                    $"Invalid SHA512 string: {Sha512String}");
            }

            return Convert.FromBase64String(Sha512String.Substring(7));
        }
    }

    [JsonPropertyName("path")]
    public string Path { get; set; } = "";
}

public enum LibraryType
{
    Package,
    Project,
}
