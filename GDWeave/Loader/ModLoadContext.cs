using System.Reflection;
using System.Runtime.Loader;

namespace GDWeave;

public class ModLoadContext(string path) : AssemblyLoadContext(isCollectible: true) {
    private AssemblyDependencyResolver resolver = new(path);

    protected override Assembly Load(AssemblyName assemblyName) {
        if (assemblyName.Name == "GDWeave") return GDWeave.Assembly;
        var existing = GDWeave.Assembly.GetReferencedAssemblies().FirstOrDefault(a => a.Name == assemblyName.Name);
        if (existing is not null) return Assembly.Load(existing);

        var assemblyPath = this.resolver.ResolveAssemblyToPath(assemblyName);
        if (assemblyPath is null) return null!;

        return this.LoadFromAssemblyPath(assemblyPath);
    }

    protected override nint LoadUnmanagedDll(string unmanagedDllName) {
        var libraryPath = this.resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        return libraryPath is not null
                   ? this.LoadUnmanagedDllFromPath(libraryPath)
                   : nint.Zero;
    }
}
