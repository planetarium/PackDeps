namespace PackDeps;

using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Cocona;
using PackDeps.Deps;

public class Program
{
    public static Task Main(string[] args) =>
        CoconaApp.RunAsync<Program>(args);

    public async Task RunAsync(
        [Argument(Description = "The path to the deps.json file.")]
        [FileExists]
        string depsJson,
        [Argument(Description = "The directory to place the assemblies.")]
        string outDir,
        [Option(Description = "The path to NuGet global packages directory.")]
        string? globalPackages = null,
        [Option(
            "runtime",
            new char[] { 'r' },
            Description = "RIDs (Runtime Identifiers) of the native " +
                "libraries to include.  Include everything by default."
        )]
        string[]? runtimes = null,
        [Option('X', Description = "Exclude XML docs.")]
        bool excludeXmlDocs = false
    )
    {
        if (!Directory.Exists(outDir))
        {
            Directory.CreateDirectory(outDir);
        }

        globalPackages ??= GetDefaultGlobalPackages();
        Document depsDoc;
        using (FileStream file = File.OpenRead(depsJson))
        {
            var options = new JsonSerializerOptions
            {
                Converters =
                {
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, false),
                }
            };
            depsDoc =
                await JsonSerializer.DeserializeAsync<Document>(file, options)
                ?? throw new CommandExitedException(
                    $"Failed to parse {depsJson} file.",
                    1);
        }

        var collectedFilePaths = depsDoc.CollectFilePaths(
            globalPackages,
            runtimes
                ?.Select(rid => rid.ToLowerInvariant())
                ?.ToImmutableHashSet(),
            excludeXmlDocs
        );
        foreach ((string src, string dst) in collectedFilePaths)
        {
            string targetPath = Path.Combine(outDir, dst);
            if (!(Path.GetDirectoryName(targetPath) is { } targetDir)) continue;
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            Console.Error.WriteLine("{0} -> {1}", src, targetPath);
            File.Copy(src, targetPath, overwrite: true);
        }
    }

    private static string GetDefaultGlobalPackages()
    {
        if (Environment.GetEnvironmentVariable("NUGET_PACKAGES") is { } path)
        {
            return path;
        }

        string home =
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(home, ".nuget", "packages");
    }
}
