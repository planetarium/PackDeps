namespace PackDeps;

using System;
using System.IO;
using System.Text.Json;
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
            depsDoc = await JsonSerializer.DeserializeAsync<Document>(file) ??
                throw new CommandExitedException(
                    $"Failed to parse {depsJson} file.",
                    1
                );
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
            Console.WriteLine("");
        }
    }
}
