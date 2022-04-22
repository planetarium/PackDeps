namespace PackDeps;

using System;
using System.Collections.Generic;
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

    public async Task RunAsync([Argument, FileExists]string depsJson)
    {
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

        var references = depsDoc.Targets[depsDoc.RuntimeTarget.Name];
        foreach ((string pkg, Reference reference) in references)
        {
            Console.WriteLine("{0}", pkg);
            if (reference.Runtime is {} runtime)
            {
                foreach (string asm in reference.Runtime.Keys)
                {
                    Console.WriteLine("- {0}", asm);
                }
            }

            if (depsDoc.Libraries is { } libs)
            {
                if (libs.GetValueOrDefault(pkg, null) is {} lib)
                {
                    Console.WriteLine("{0}", lib.Path);
                }
            }
            Console.WriteLine("");
        }
    }
}
