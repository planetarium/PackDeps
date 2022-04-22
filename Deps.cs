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
