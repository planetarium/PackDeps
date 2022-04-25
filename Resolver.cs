namespace PackDeps;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using PackDeps.Deps;

public static class Resolver
{
    public record struct Resolution(string SourcePath, string DestinationPath);

    public static IEnumerable<Resolution> CollectFilePaths(
        this Document depsDoc,
        string globalPackagesDir,
        IImmutableSet<string>? runtimes = null,
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

            string pkgDirBase = Path.GetFullPath(pkgDir);
            string runtimesDir = Path.Combine(pkgDir, "runtimes");
            if (!Directory.Exists(runtimesDir)) continue;

            foreach (string rDir in Directory.EnumerateDirectories(runtimesDir))
            {
                string rid = Path.GetFileName(rDir).ToLowerInvariant();
                if (runtimes is { } rids && !rids.Contains(rid)) continue;
                string nativeDir = Path.Combine(rDir, "native");
                if (!Directory.Exists(nativeDir)) continue;
                IEnumerable<string> runtimeLibs = Directory.EnumerateFiles(
                    nativeDir,
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
}
