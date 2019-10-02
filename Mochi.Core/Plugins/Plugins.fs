

namespace Mochi.Core

module Plugins =

    open System
    open System.IO
    open System.Threading.Tasks
    
    type UnloadReason =
        | UserUnload
        | PluginFault
        | Reloading
    
    type ReportReason =
        | PluginUnloaded of UnloadReason
        | PluginLoaded
    
    type PluginEnvironment = {
        plugins : list<Plugin>
    }
    and Plugin = {
        name             : string
        loadDependencies : list<string> // plugins that must be loaded before this one can run.
        execDependencies : list<string> // plugins that must be running when this one is.
        onLoad           : PluginEnvironment -> Task // runs once after all dependencies are resolved
        onReport         : PluginEnvironment -> ReportReason -> Task // runs after every time a plugin is loaded or unloaded
        onUnload         : UnloadReason -> Task
    }
    
    let validPlugin (plugin : Plugin) =
        let r1 = List.exists (fun i -> List.contains i plugin.execDependencies) plugin.loadDependencies
        not r1
