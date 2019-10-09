
using System;

namespace Mochi.Plugins
{
    using System.Collections.Generic;
    using System.Linq;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Loader;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public class Context : AssemblyLoadContext
    {
        private AssemblyDependencyResolver Resolver { get; set; }

        public Context(string loadPath) : base(true)
        {
            this.Resolver = new AssemblyDependencyResolver(loadPath);
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            var assemblyPath = this.Resolver.ResolveAssemblyToPath(assemblyName);
            if (assemblyPath is null)
            {
                return null;
            }
            else
            {
                return this.LoadFromAssemblyPath(assemblyPath);
            }
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            string libraryPath = this.Resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            if (libraryPath != null)
            {
                return this.LoadUnmanagedDllFromPath(libraryPath);
            }

            return IntPtr.Zero;
        }
    }

    public struct LoadedContext
    {
        public Context Context { get; set; }
        public Assembly Assembly { get; set; }

        public LoadedContext(Context context, Assembly assembly)
        {
            this.Context = context;
            this.Assembly = assembly;
        }
    }

    public class Loader
    {
        public static LoadedContext Load(string relativePath)
        {
            var currentPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var path = Path.Combine(currentPath, relativePath);
            var asmname = new AssemblyName(Path.GetFileNameWithoutExtension(relativePath));
            var context = new Context(path);
            var assembly = context.LoadFromAssemblyName(asmname);
            return new LoadedContext(context, assembly);
        }

        public static T LoadType<T>(string relativePath)
        {
            var context = Load(relativePath);
            return LoadType<T>(context);
        }

        public static T LoadType<T>(LoadedContext context)
        {
            var types = context.Assembly.GetExportedTypes(); //.Where(x => x.IsSubclassOf(typeof(T)) || x.IsAssignableFrom(typeof(T))).FirstOrDefault();
            Type type = default;
            foreach (var t in types)
            {
                if (t.IsSubclassOf(typeof(T)) || t.IsAssignableFrom(typeof(T)))
                {
                    type = t;
                    break;
                }
            }
            if (type is null)
            {
                return default;
            }
            return (T)Activator.CreateInstance(type);
        }
    }
}
