
namespace Mochi.Core

module Logging =

    open System
    open System.IO
    open System.Reflection
    open System.Runtime.Loader
    open System.Runtime.CompilerServices
    open System.Runtime.Serialization
    open System.Diagnostics
    open Microsoft.FSharp.Reflection
    open FSharp.Compiler.SourceCodeServices
    open Serilog
    
    let getCallStack scope =
        let stack = StackFrame (scope, true)
        stack
        (*
        // can use this if we want to ignore subfunctions
        if (stack.GetMethod ()).Name = "Invoke" then
            StackFrame (scope + 1, true)
        else
            stack
        *)
    
    let genJsonLog (stack : StackFrame) =
        ()
    
    let logInfo1 (msg : string) =
        ()
    
    let logWarning1 (msg : string) =
        let stack = getCallStack 3
        let name = (stack.GetMethod ()).Name
        let ns   = ((stack.GetMethod ()).ReflectedType).FullName
        let file = (stack.GetFileName ())
        let line = (stack.GetFileLineNumber ())
        Console.WriteLine("[{0}.{1} ({2}:{3})] {4}", ns, name, file, line, msg)
        ()
    
    type Logger = {
        info    : string -> unit
        warning : string -> unit
    }
    
    let syslog = {
        info    = logInfo1
        warning = logWarning1
    }
