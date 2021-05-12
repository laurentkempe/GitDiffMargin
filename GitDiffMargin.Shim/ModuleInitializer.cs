namespace GitDiffMargin.Shim
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    internal static class ModuleInitializer
    {
        [ModuleInitializer]
        internal static void Initialize()
        {
            var shellInternal = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(
                assembly => assembly.GetName().Name == "Microsoft.VisualStudio.Shell.UI.Internal");
            var shellVersion = shellInternal.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
            if (!Version.TryParse(shellVersion, out var version))
            {
                return;
            }

            var subFolder = version switch
            {
                // Prior to 15.6 Preview 7, use legacy command handling
                { Major: 15, Build: < 27428 } => "dev15",

                // 15.6 Preview 7 includes modern command handling
                { Major: 15, Build: >= 27428 } => "dev16",

                // All of 16.x uses dev16
                { Major: 16 } => "dev16",

                // All of 17.x uses dev17
                { Major: 17 } => "dev17",

                _ => null,
            };

            if (subFolder is null)
            {
                return;
            }

            var baseDirectory = Path.GetDirectoryName(typeof(ModuleInitializer).Assembly.Location);
            var implementationAssembly = Path.Combine(baseDirectory, subFolder, "GitDiffMargin.Impl.dll");
            Assembly.LoadFrom(implementationAssembly);
        }
    }
}
