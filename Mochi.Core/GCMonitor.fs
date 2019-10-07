
namespace Mochi.Core

module GCMonitor = 
    
    open System
    open System.Runtime.CompilerServices
    open System.Reflection
    open System.Diagnostics
    open Logging

    type GCMonitor () =
        
        static let _onCollect = Event<_>()
        static let mutable _logCollect = false
        static let mutable _logMessage = "Garbage Collector has run"

        static let mutable _canRun : bool = false
        static let mutable _isRelease : bool = true

        do
            _isRelease <- GCMonitor.IsReleaseMode ()
        
        static member public IsRelease
            with get ()    = _isRelease

        static member public LogCollect
            with get ()    = _logCollect
            and  set value = _logCollect <- value

        static member public LogMessage
            with get ()    = _logMessage
            and  set value = _logMessage <- value

        static member public IsReleaseMode _ =
            let asm = Assembly.GetEntryAssembly ()
            let atrbs = asm.GetCustomAttributes(typeof<DebuggableAttribute>, true)
            if (isNull atrbs) || atrbs.Length = 0 then
                true
            else if atrbs.[0] :? DebuggableAttribute then
                let dbgatr = atrbs.[0] :?> DebuggableAttribute
                if dbgatr.IsJITOptimizerDisabled then
                    false
                else
                    true
            else
                true

        static member public Subscribe (callback : unit -> unit) =
            Event.add (callback) _onCollect.Publish

        static member public Stop () =
            _canRun <- false

        static member public Start () =
            _canRun <- true
            GCMonitor () |> ignore

        [<MethodImpl(MethodImplOptions.NoInlining)>]
        member private this.monitor () =
            // stack frames are different between debug and release,
            // so we need to figure out which one we are for correct
            // logging from the finalizer.
            if _isRelease then
                logInfo1 -1 GCMonitor.LogMessage
            else
                logInfo1  0 GCMonitor.LogMessage

        override this.Finalize () =
            this.monitor ()
            _onCollect.Trigger ()
            if (not Environment.HasShutdownStarted) && (not (AppDomain.CurrentDomain.IsFinalizingForUnload())) && _canRun then
                GCMonitor () |> ignore
            else
                _canRun <- false
    
    // Strange place to put this, I know, but this is
    // the earliest part of the program that needs it.
    let isRelease _ =
        GCMonitor.IsRelease

    let releaseString _ =
        if GCMonitor.IsRelease then "RELEASE" else "DEBUG"

    let subscribe (callback : unit -> unit) =
        GCMonitor.Subscribe callback

    let start () =
        GCMonitor.LogCollect <- true
        GCMonitor.Start ()
