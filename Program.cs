namespace PackDeps;

using System;
using System.IO;
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
        string? globalPackages = null
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

        foreach (string asm in depsDoc.CollectFilePaths(globalPackages))
        {
            string basename = Path.GetFileName(asm);
            string targetPath = Path.Combine(outDir, basename);
            Console.Error.WriteLine("{0} -> {1}", asm, targetPath);
            File.Copy(asm, targetPath, overwrite: true);
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
