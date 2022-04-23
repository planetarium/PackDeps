namespace PackDeps;

using System.Collections.Generic;
using System.IO;
using PackDeps.Deps;

public static class Resolver
{
    public static IEnumerable<string> CollectFilePaths(
        this Document depsDoc,
        string globalPackagesDir
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
            foreach (string asm in reference.Runtime.Keys)
            {
                string asmPath = Path.Combine(pkgDir, asm);
                if (File.Exists(asmPath))
                {
                    yield return asmPath;
                }
            }
        }
    }
}
