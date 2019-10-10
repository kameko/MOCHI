
namespace Mochi.Core.Plugins

open System
open System.Collections.Generic
open Akka.Actor
open Akka.FSharp

type PluginInfo () =
    member val Name         = String.Empty  with get, set
    member val Company      = String.Empty  with get, set
    member val Version      = Version()     with get, set
    member val Guid         = Guid()        with get, set
    member val Descrption   = String.Empty  with get, set
    member val Published    = DateTime()    with get, set
    member val Author       = String.Empty  with get, set
    member val Copyright    = String.Empty  with get, set
    member val License      = String.Empty  with get, set

type PluginRequirement () =
    member val Name         = String.Empty  with get, set
    member val Company      = String.Empty  with get, set
    member val Version      = Version()     with get, set

type ActorInfo (prop : Props) =
    member val Props        = prop          with get, set
    member val ActorName    = String.Empty  with get, set
    public new () =
        ActorInfo (Props.Empty)

[<AbstractClass>]
type Plugin () =
    member val Info = PluginInfo() with get, set
    member val LoadDependencies = List<PluginRequirement>() with get, set
    member val ExecDependencies = List<PluginRequirement>() with get, set

    abstract PreLoad : unit -> unit
    abstract LoadSupervisor : unit -> ActorInfo

module FSharp =
    
    open Akka.FSharp

    let createProps (f : Actor<'Message> -> Cont<'Message, 'Returned>) =
        let e = Linq.Expression.ToExpression(fun () -> new FunActor<'Message, 'Returned>(f))
        Props.Create e

    let createPropsOpt (f : Actor<'Message> -> Cont<'Message, 'Returned>) (options : SpawnOption list) =
        let p = createProps f
        applySpawnOptions p options
