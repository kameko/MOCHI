
namespace Mochi.Plugin.Discord

module DiscordEventActors =
    
    open System
    open Akka.Actor
    open Akka.FSharp
    open Discord
    open Mochi.Core.AkkaLogging

    type EventMessage<'a> =
    | Subscribe    of IActorRef
    | Unsubscribe  of IActorRef
    | Notification of 'a

    let spawnMessageRecievedActor (mailbox : Actor<_>) =
        let rec messageRecievedActor (subscriptions : List<IActorRef>) = actor {
            let! (message : obj) = mailbox.Receive()
            match message with
            | :? EventMessage<obj> as evmsg ->
                match evmsg with
                | Subscribe actref ->
                    let nsubs = actref :: subscriptions
                    return! messageRecievedActor nsubs
                | Unsubscribe actref ->
                    let nsubs = List.except [actref] subscriptions
                    return! messageRecievedActor nsubs
                | Notification msg ->
                    List.map (fun i -> i <! msg) subscriptions |> ignore
                    return! messageRecievedActor subscriptions
            | _ -> 
                mailbox.Unhandled ()
                return! messageRecievedActor subscriptions
        }
        messageRecievedActor []


