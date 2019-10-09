
namespace Mochi.Core

module PluginLoader =

    open System
    open System.Linq
    open System.IO
    open System.Reflection
    open System.Runtime.Loader
    open System.Runtime.CompilerServices
    open System.Runtime.InteropServices
    open Plugins
    open Logging
    
    type PluginLoadContextError =
        | BadImageFormat of BadImageFormatException
        | AssemblyNull   of NullReferenceException
        | FileNotFound   of FileNotFoundException
        | Argument       of ArgumentException
        | FileLoad       of FileLoadException
        | Other          of Exception
    
    type PluginLoadContext (loadPath: String) =
        inherit AssemblyLoadContext (true)
        
        let resolver = AssemblyDependencyResolver(loadPath)
        
        override this.Load (assemblyName: AssemblyName) =
            let assemblyPath = resolver.ResolveAssemblyToPath(assemblyName)
            if isNull assemblyPath then
                null
            else
                this.LoadFromAssemblyPath(assemblyPath)

        override this.LoadUnmanagedDll (unmanagedName : string) =
            let assemblyPath = resolver.ResolveUnmanagedDllToPath(unmanagedName)
            if isNull assemblyPath then
                IntPtr.Zero
            else
                this.LoadUnmanagedDllFromPath(assemblyPath)

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
        weakRef     : WeakReference<PluginLoadContext>
        getRef      : unit -> Option<PluginLoadContext>
        isAlive     : unit -> bool
    }
    
    type AssemblyContext = {
        assembly    : Assembly
        asmref      : AssemblyReference
        unload      : unit -> unit
    }
    
    [<MethodImpl(MethodImplOptions.NoInlining)>] // make sure we don't keep the plugin reference around elsewhere
    let loadAssemblyName (assemblyPath : String) (assemblyName: AssemblyName) =
        let asmldr = PluginLoadContext (assemblyPath)
        let wref   = WeakReference<PluginLoadContext>(asmldr)
        let masm   = asmldr.LoadAssembly(assemblyName)
        match masm with
        | Ok asm -> Ok {
                assembly = asm
                asmref   = {
                    weakRef = wref
                    getRef  = fun () ->
                        let mutable target : PluginLoadContext = Unchecked.defaultof<PluginLoadContext>
                        if wref.TryGetTarget(&target) then Some target else None
                    isAlive = fun () -> 
                        let mutable target : PluginLoadContext = Unchecked.defaultof<PluginLoadContext>
                        wref.TryGetTarget(&target)
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
    
    let unloadAssembly (asmcontext : AssemblyContext) =
        asmcontext.unload ()
    
    let ensureUnload (asmref : AssemblyReference) =
        let rec ensureUnloadKurikaesu (aref : AssemblyReference) count =
            GC.Collect ()
            GC.WaitForPendingFinalizers ()
            if (not <| aref.isAlive ()) then
                ()
            else if (count <= 0) then
                syslog.warning "Timeout reached while waiting for assembly to unload. Assembly may still be alive."
                ()
            else
                ensureUnloadKurikaesu aref (count - 1)
        ensureUnloadKurikaesu asmref 10

    let isValidPlugin (plugin : Plugin) =
        let r1 = not (List.exists (fun i -> List.contains i plugin.execDependencies) plugin.loadDependencies)
        r1

    let getPluginFromAssembly (context : AssemblyContext) =
        try
            let asm = context.assembly
            let (exportedtypes : Type[]) = asm.GetExportedTypes ()
            let baseplugintype = Array.find (fun (t : Type) -> 
                    t.IsSubclassOf typeof<BasePlugin>) (exportedtypes) 
            let baseplugin = Activator.CreateInstance(baseplugintype) :?> BasePlugin
            let plugin = baseplugin.Load ()
            if isValidPlugin plugin then
                Some plugin
            else
                None
        with
        | _ -> None
    
    let getPlugin (path : string) =
        let asm = loadAssembly path
        match asm with
        | Ok context ->
            let plugin = getPluginFromAssembly context
            plugin
        | _ -> None