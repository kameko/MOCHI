
namespace Mochi.Plugin.Discord

module DiscordPlugin =
    
    open System
    open Akka.Actor
    open Mochi.Core.Plugins

    let LoadPlugin () = 
        {
            info = {
                name            = "Discord"
                company         = "Caesura Software Solutions"
                version         = Version(1, 0, 0, 0)
                guid            = Guid("8f67899c-a141-4b11-b237-d9993cdbb3e5")
                description     = "Discord chat plugin"
                published       = DateTime(2019, 10, 7)
                author          = "Kameko"
                copyright       = "Kameko 2019"
                license         = "MS-PL"
            };
            loadDependencies    = []
            execDependencies    = []
            supervisor          = (fun mailbox -> ())
            onLoad              = (fun pe -> ())
            onReport            = (fun pe rr -> ())
            onUnload            = (fun ur -> ())
        }
    