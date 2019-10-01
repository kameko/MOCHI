
namespace Fakka.Core

module Plugins =

    open System
    open System.Reflection
    open System.Runtime.Loader

    type private PluginLoadContext (loadPath: String) =
        inherit AssemblyLoadContext ()
        
        let resolver = AssemblyDependencyResolver(loadPath)
        
        override this.Load (assemblyName: AssemblyName) =
            let assemblyPath = resolver.ResolveAssemblyToPath(assemblyName)
            if isNull assemblyPath then
                null
            else
                this.LoadFromAssemblyPath(assemblyPath)

        member this.LoadAssembly (assemblyName: AssemblyName) =
            let asm = this.Load assemblyName // TODO: try/catch
            if isNull asm then
                None
            else
                Some asm

    let loadAssembly (assemblyPath : String) (assemblyName: AssemblyName) =
        let asmldr = PluginLoadContext (assemblyPath)
        let masm = asmldr.LoadAssembly(assemblyName)
        match masm with
        | Some asm -> Some asm
        | None -> None
