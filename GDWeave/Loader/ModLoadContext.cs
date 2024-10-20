using System.Reflection;
using System.Runtime.Loader;

namespace GDWeave;

public class ModLoadContext(string path) : AssemblyLoadContext(isCollectible: true) {
    private AssemblyDependencyResolver resolver = new(path);

    protected override Assembly Load(AssemblyName assemblyName) {
        if (assemblyName.Name == "GDWeave") return GDWeave.Assembly;
        var assemblyPath = this.resolver.ResolveAssemblyToPath(assemblyName);
        if (assemblyPath is null) return null!;

        // FASM moment
        return assemblyPath.Contains("Reloaded")
                   ? this.LoadFromAssemblyPath(assemblyPath)
                   : this.LoadFromFile(assemblyPath);
    }

    public Assembly LoadFromFile(string path) {
        var symbols = path.Replace(".dll", ".pdb");
        if (File.Exists(symbols)) {
            return this.LoadFromStream(
                new MemoryStream(File.ReadAllBytes(path)),
                new MemoryStream(File.ReadAllBytes(symbols))
            );
        } else {
            return this.LoadFromStream(new MemoryStream(File.ReadAllBytes(path)));
        }
    }

    protected override nint LoadUnmanagedDll(string unmanagedDllName) {
        var libraryPath = this.resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        return libraryPath is not null
                   ? this.LoadUnmanagedDllFromPath(libraryPath)
                   : nint.Zero;
    }
}
