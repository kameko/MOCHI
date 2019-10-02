
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
                    //NullReferenceException ("Path to assembly is null") |> AssemblyNull |> Error
                    raise <| NullReferenceException "Path to assembly is null"
                else
                    Ok asm
            with
                | :? NullReferenceException  as ex -> Error <| AssemblyNull ex
                | :? ArgumentException       as ex -> Error <| Argument ex
                | :? BadImageFormatException as ex -> Error <| BadImageFormat ex
                | :? FileLoadException       as ex -> Error <| FileLoad ex
                | :? FileNotFoundException   as ex -> Error <| FileNotFound ex
                |                               ex -> Error <| Other ex
    
    type PluginContext = {
        asmWeakRef : WeakReference
        assembly   : Assembly
        unload     : unit -> unit
    }
    
    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let loadAssemblyName (assemblyPath : String) (assemblyName: AssemblyName) =
        let asmldr = PluginLoadContext (assemblyPath)
        let wref = WeakReference(asmldr)
        let masm = asmldr.LoadAssembly(assemblyName)
        match masm with
        | Ok asm -> Ok {
                asmWeakRef = wref
                assembly   = asm
                unload     = asmldr.Unload
            }
        | Error ex -> Error ex
    
    let loadAssembly (assemblyPath : String) =
        raise <| NotImplementedException ()
        ()
    
    let unloadAssembly (asmcontext : PluginContext) =
        asmcontext.unload ()
    
    let ensureUnload (weakref : WeakReference) =
        let rec kurikaesu (wref : WeakReference) count =
            GC.Collect ()
            GC.WaitForPendingFinalizers ()
            if (not wref.IsAlive) || (count <= 0) then
                ()
            else
                kurikaesu wref (count - 1)
        kurikaesu weakref 10
