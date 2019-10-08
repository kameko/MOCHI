

namespace Mochi.Core

module Plugins =

    open System
    open System.IO
    open System.Threading.Tasks
    open Akka.Actor
    open Akka.FSharp
    
    type PluginInfo = {
        name        : string
        company     : string
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
        company : string
        version : Version
    }

    type UnloadReason =
        | UserUnload
        | SystemUnload
        | PluginFault
        | Reloading
    
    type ReportReason =
        | PluginUnloaded of PluginInfo * UnloadReason
        | PluginLoaded of PluginInfo

    type PluginEnvironment = {
        plugins : list<PluginInfo>
    }

    type Plugin = {
        info             : PluginInfo
        loadDependencies : list<PluginRequirement> // plugins that must be loaded before this one can run.
        execDependencies : list<PluginRequirement> // plugins that must be running when this one is.
        supervisor       : Actor<Object> -> unit
        onLoad           : PluginEnvironment -> unit // runs once after all dependencies are resolved
        onReport         : PluginEnvironment -> ReportReason -> unit // runs after every time a plugin is loaded or unloaded
        onUnload         : UnloadReason -> unit
    }
