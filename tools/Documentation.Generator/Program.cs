using System.Reflection;
using XmlDocMarkdown.Core;

AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
{
    var assemblyName = new AssemblyName(args.Name);

    // Try already-loaded assemblies first
    var existing = AppDomain.CurrentDomain.GetAssemblies()
        .FirstOrDefault(a => a.GetName().Name == assemblyName.Name);
    if (existing != null)
        return existing;

    // Fallback: search XmlDocGen's own output directory (deps are copied there via project refs)
    var probingPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{assemblyName.Name}.dll");
    if (File.Exists(probingPath))
        return Assembly.Load(File.ReadAllBytes(probingPath));

    return null;
};

return XmlDocMarkdownApp.Run(args);