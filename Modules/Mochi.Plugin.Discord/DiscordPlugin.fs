
namespace Mochi.Plugin.Discord

open System
open Akka.Actor
open Akka.FSharp
open Mochi.Core.Plugins
open Mochi.Core.Plugins.FSharp

type DiscordPlugin () =
    inherit Plugin ()

    override this.PreLoad () =
        this.Info.Name       <- "Discord"
        this.Info.Company    <- "Caesura Software Solutions"
        this.Info.Version    <- Version(1, 0, 0, 0)
        this.Info.Guid       <- Guid("8f67899c-a141-4b11-b237-d9993cdbb3e5")
        this.Info.Descrption <- "Discord chat plugin"
        this.Info.Published  <- DateTime(2019, 10, 10)
        this.Info.Author     <- "Kameko"
        this.Info.Copyright  <- "Kameko 2019"
        this.Info.License    <- "MS-PL"

    override this.LoadSupervisor () =
        let prop = createProps Supervisor.spawnSupervisor
        //let prop = Props.Create ()
        ActorInfo(prop)

