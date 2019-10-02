
namespace Mochi.Core

module Logging =

    open System
    open System.IO
    open System.Reflection
    open System.Runtime.Loader
    open System.Runtime.CompilerServices
    open Serilog
    
    let logInfo (msg : string) =
        ()
    
    let logWarning (msg : string) =
        ()
    
    type Logger = {
        info    : string -> unit
        warning : string -> unit
    }
    
    let log = {
        info    = logInfo
        warning = logWarning
    }
