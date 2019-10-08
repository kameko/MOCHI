
namespace Mochi.Plugin.Discord

module ChannelActor =
    
    open System
    open Akka.Actor
    open Akka.FSharp
    


    let spawnChannelActor (mailbox : Actor<'Message>) =
        let rec channelActor () = actor {
            let! message = mailbox.Receive()
            match message with
            | _ -> return! channelActor ()
        }
        channelActor ()
            
