
namespace Mochi.Plugin.Discord

module Supervisor =
    
    open System
    open Akka.Actor
    open Akka.FSharp
    open Mochi.Core.AkkaLogging

    let spawnSupervisor (mailbox : Actor<_>) =
        let rec supervisor () = actor {
            let! (message : obj) = mailbox.Receive()
            match message with
            | :? string as s -> 
                akkalog.info mailbox <| sprintf "Discord got message: %s" s
                //Console.WriteLine("Discord got message: {0}", s)
                return! supervisor ()
            | _ -> return! supervisor ()
        }
        supervisor()

