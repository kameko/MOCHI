
namespace Mochi.Core

module PluginLoader =

    open System
    open System.IO
    open System.Reflection
    open System.Runtime.Loader
    open System.Runtime.CompilerServices
    open Logging
    
    type PluginLoadContextError =
        | BadImageFormat of BadImageFormatException
        | AssemblyNull   of NullReferenceException
        | FileNotFound   of FileNotFoundException
        | Argument       of ArgumentException
        | FileLoad       of FileLoadException
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
                    raise <| NullReferenceException "Path to assembly is null"
                else
                    Ok asm
            with
                | :? BadImageFormatException as ex -> Error <| BadImageFormat ex
                | :? NullReferenceException  as ex -> Error <| AssemblyNull ex
                | :? FileNotFoundException   as ex -> Error <| FileNotFound ex
                | :? ArgumentException       as ex -> Error <| Argument ex
                | :? FileLoadException       as ex -> Error <| FileLoad ex
                |                               ex -> Error <| Other ex
    
    type AssemblyReference = {
        weakRef : WeakReference
        isAlive : unit -> bool
    }
    
    type PluginContext = {
        assembly   : Assembly
        asmref     : AssemblyReference
        unload     : unit -> unit
    }
    
    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let loadAssemblyName (assemblyPath : String) (assemblyName: AssemblyName) =
        let asmldr = PluginLoadContext (assemblyPath)
        let wref   = WeakReference(asmldr)
        let masm   = asmldr.LoadAssembly(assemblyName)
        match masm with
        | Ok asm -> Ok {
                assembly = asm
                asmref   = {
                    weakRef = wref
                    isAlive = fun () -> wref.IsAlive
                }
                unload   = fun () -> asmldr.Unload ()
            }
        | Error ex -> Error ex
    
    let loadAssemblyFullPath (fullPath : String) =
        let asmname = AssemblyName (Path.GetFileNameWithoutExtension fullPath)
        loadAssemblyName fullPath asmname
    
    let loadAssembly (relativePath : String) =
        let currentPath = (Assembly.GetEntryAssembly ()).Location |> Path.GetDirectoryName
        let fullPath    = Path.Combine [| currentPath; relativePath |]
        let asmname     = AssemblyName (Path.GetFileNameWithoutExtension relativePath)
        loadAssemblyName fullPath asmname
    
    let unloadAssembly (asmcontext : PluginContext) =
        asmcontext.unload ()
    
    let ensureUnload (asmref : AssemblyReference) =
        let rec kurikaesu (aref : AssemblyReference) count =
            GC.Collect ()
            GC.WaitForPendingFinalizers ()
            if (not <| aref.isAlive ()) then
                ()
            else if (count <= 0) then
                syslog.warning "Timeout reached while waiting for assembly to unload. Assembly may still be alive."
                ()
            else
                kurikaesu aref (count - 1)
        kurikaesu asmref 10
    
