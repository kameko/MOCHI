
namespace Mochi.Core

module GCMonitor = 
    
    open System
    open Logging

    type GCMonitor () =
        
        static let _onCollect = Event<_>()

        static let mutable _canRun : bool = false

        static member public Subscribe (callback : unit -> unit) =
            Event.add (callback) _onCollect.Publish

        static member public Stop () =
            _canRun <- false

        static member public Start () =
            _canRun <- true
            GCMonitor () |> ignore

        override this.Finalize () =
            _onCollect.Trigger ()
            if (not Environment.HasShutdownStarted) && (not (AppDomain.CurrentDomain.IsFinalizingForUnload())) && _canRun then
                GCMonitor () |> ignore
            else
                _canRun <- false
    
    let monitor () =
        GCMonitor.Subscribe (fun _ -> syslog.info "Garbage Collector has run")

    let subscribe (callback : unit -> unit) =
        GCMonitor.Subscribe callback

    let start () =
        GCMonitor.Start ()
