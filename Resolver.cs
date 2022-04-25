namespace PackDeps;

using System.Collections.Generic;
using System.IO;
using PackDeps.Deps;

public static class Resolver
{
    public record struct Resolution(string SourcePath, string DestinationPath);

    public static IEnumerable<Resolution> CollectFilePaths(
        this Document depsDoc,
        string globalPackagesDir,
        bool excludeNativeLibraries = false,
        bool excludeXmlDocs = false
    )
    {
        var references = depsDoc.Targets[depsDoc.RuntimeTarget.Name];
        foreach ((string pkg, Reference reference) in references)
        {
            if (depsDoc.Libraries is not { } libs) continue;
            Library lib;
            try
            {
                lib = libs[pkg];
            }
            catch (KeyNotFoundException)
            {
                continue;
            }

            if (reference.Runtime is not { } runtime) continue;
            string pkgDir = Path.Combine(globalPackagesDir, lib.Path);
            foreach (string file in reference.Runtime.Keys)
            {
                string filePath = Path.Combine(pkgDir, file);
                if (File.Exists(filePath))
                {
                    yield return new Resolution
                    {
                        SourcePath = filePath,
                        DestinationPath = Path.GetFileName(filePath),
                    };
                }

                bool isDll = Path.GetExtension(filePath).Equals(
                    ".dll",
                    System.StringComparison.InvariantCultureIgnoreCase
                );
                if (!isDll) continue;

                if (excludeXmlDocs) continue;
                string xmlPath = Path.ChangeExtension(filePath, ".xml");
                if (File.Exists(xmlPath))
                {
                    yield return new Resolution
                    {
                        SourcePath = xmlPath,
                        DestinationPath = Path.GetFileName(xmlPath),
                    };
                }
            }

            if (excludeNativeLibraries) continue;
            string pkgDirBase = Path.GetFullPath(pkgDir);
            string runtimesDir = Path.Combine(pkgDir, "runtimes");
            if (!Directory.Exists(runtimesDir)) continue;

            IEnumerable<string> runtimeLibs = Directory.EnumerateFiles(
                runtimesDir,
                "*",
                SearchOption.AllDirectories
            );
            foreach (string runtimeLib in runtimeLibs)
            {
                if (!runtimeLib.StartsWith(runtimesDir)) continue;
                yield return new Resolution
                {
                    SourcePath = runtimeLib,
                    DestinationPath =
                        runtimeLib.Substring(pkgDirBase.Length + 1),
                };
            }
        }
    }
}
