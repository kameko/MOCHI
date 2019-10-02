
namespace Mochi.Core

module PluginLoader =

    open System
    open System.IO
    open System.Reflection
    open System.Runtime.Loader
    open System.Runtime.CompilerServices
    
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
    
    type PluginContext = {
        asmWeakRef : WeakReference
        assembly   : Assembly
        unload     : unit -> unit
    }
    
    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let loadAssemblyName (assemblyPath : String) (assemblyName: AssemblyName) =
        let asmldr = PluginLoadContext (assemblyPath)
        let wref   = WeakReference(asmldr)
        let masm   = asmldr.LoadAssembly(assemblyName)
        match masm with
        | Ok asm -> Ok {
                asmWeakRef = wref
                assembly   = asm
                unload     = asmldr.Unload
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
    
    let ensureUnload (weakref : WeakReference) =
        let rec kurikaesu (wref : WeakReference) count =
            GC.Collect ()
            GC.WaitForPendingFinalizers ()
            if (not wref.IsAlive) || (count <= 0) then
                ()
            else
                kurikaesu wref (count - 1)
        kurikaesu weakref 10
    
