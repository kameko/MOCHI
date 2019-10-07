
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
        
        static member public LogCollect
            with get ()    = _logCollect
            and  set value = _logCollect <- value

        static member public LogMessage
            with get ()    = _logMessage
            and  set value = _logMessage <- value
        
        static member public Subscribe (callback : unit -> unit) =
            Event.add (callback) _onCollect.Publish

        static member public Stop () =
            _canRun <- false

        static member public Start () =
            _canRun <- true
            GCMonitor () |> ignore

        [<MethodImpl(MethodImplOptions.NoInlining)>]
        member private this.monitor () =
            syslog.info GCMonitor.LogMessage

        override this.Finalize () =
            this.monitor ()
            _onCollect.Trigger ()
            if (not Environment.HasShutdownStarted) && (not (AppDomain.CurrentDomain.IsFinalizingForUnload())) && _canRun then
                GCMonitor () |> ignore
            else
                _canRun <- false
    
    let subscribe (callback : unit -> unit) =
        GCMonitor.Subscribe callback

    let start () =
        GCMonitor.LogCollect <- true
        GCMonitor.Start ()
