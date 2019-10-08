

namespace Mochi.Core

module Plugins =

    open System
    open System.IO
    open System.Threading.Tasks
    open Akka.Actor
    open Akka.FSharp
    
    type UnloadReason =
        | UserUnload
        | PluginFault
        | Reloading
    
    type ReportReason =
        | PluginUnloaded of UnloadReason
        | PluginLoaded
    
    type PluginInfo = {
        name        : string
        version     : Version
        guid        : Guid
        description : string
        published   : DateTime
        author      : string
        copyright   : string
        license     : string
    }
    
    type PluginRequirement = {
        name    : string
        version : Version
    }

    type PluginEnvironment = {
        plugins : list<Plugin>
    }
    and Plugin = {
        info             : PluginInfo
        loadDependencies : list<PluginRequirement> // plugins that must be loaded before this one can run.
        execDependencies : list<PluginRequirement> // plugins that must be running when this one is.
        supervisor       : Actor<Object> -> unit
        onLoad           : PluginEnvironment -> unit // runs once after all dependencies are resolved
        onReport         : PluginEnvironment -> ReportReason -> unit // runs after every time a plugin is loaded or unloaded
        onUnload         : UnloadReason -> unit
    }
    
    let isValidPlugin (plugin : Plugin) =
        let r1 = List.exists (fun i -> List.contains i plugin.execDependencies) plugin.loadDependencies
        not r1
