
namespace Fakka.Core

module Plugins =

    open System
    open System.IO
    open System.Reflection
    open System.Runtime.Loader
    open System.Runtime.CompilerServices
    
    type PluginLoadContextError =
        | None
        | AssemblyNull   of NullReferenceException
        | Argument       of ArgumentException
        | BadImageFormat of BadImageFormatException
        | FileLoad       of FileLoadException
        | FileNotFound   of FileNotFoundException
        | Other          of Exception
    
    type private PluginLoadContext (loadPath: String) =
        inherit AssemblyLoadContext (true)
        
        let resolver = AssemblyDependencyResolver(loadPath)
        
        override this.Load (assemblyName: AssemblyName) =
            let assemblyPath = resolver.ResolveAssemblyToPath(assemblyName)
            if isNull assemblyPath then
                null
            else
                this.LoadFromAssemblyPath(assemblyPath)

        member this.LoadAssembly (assemblyName: AssemblyName) =
            try
                let asm = this.Load assemblyName
                if isNull asm then
                    NullReferenceException ("Path to assembly is null") |> AssemblyNull |> Error
                else
                    Ok asm
            with
                | :? ArgumentException       as ex1 -> Error <| Argument ex1
                | :? BadImageFormatException as ex2 -> Error <| BadImageFormat ex2
                | :? FileLoadException       as ex3 -> Error <| FileLoad ex3
                | :? FileNotFoundException   as ex4 -> Error <| FileNotFound ex4
                |                               ex0 -> Error <| Other ex0
    
    type PluginContext = {
        assembly : Assembly
        unload   : unit -> unit
    }
    
    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let loadAssemblyName (assemblyPath : String) (assemblyName: AssemblyName) =
        let asmldr = PluginLoadContext (assemblyPath)
        let masm = asmldr.LoadAssembly(assemblyName)
        match masm with
        | Ok asm -> Ok {
                assembly = asm
                unload   = asmldr.Unload
            }
        | Error ex -> Error ex
    
    let loadAssembly (assemblyPath : String) =
        raise (NotImplementedException ())
        ()
    
    let unloadAssembly (asmcontext : PluginContext) =
        asmcontext.unload ()
    
    
