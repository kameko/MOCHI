
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
    
    // TODO: contain some solid information about the current build of
    // the system, since the log will contain file and line number info.
    // probably want both software version and some GUID unique to the build.
    type StructuredLog () =
        let mutable _logMessage       : string = String.Empty
        let mutable _callerName       : string = String.Empty
        let mutable _callerNamespace  : string = String.Empty
        let mutable _callerFile       : string = String.Empty
        let mutable _callerDirectory  : string = String.Empty
        let mutable _callerLineNumber : int = 0
        let mutable _exception        : Exception = null
        
        member this.LogMessage
            with get ()    = _logMessage
            and  set value = _logMessage <- value
        member this.CallerName
            with get ()    = _callerName
            and  set value = _callerName <- value
        member this.CallerNamespace
            with get ()    = _callerNamespace
            and  set value = _callerNamespace <- value
        member this.CallerFile
            with get ()    = _callerFile
            and  set value = _callerFile <- value
        member this.CallerDirectory
            with get ()    = _callerDirectory
            and  set value = _callerDirectory <- value
        member this.CallerLineNumber
            with get ()    = _callerLineNumber
            and  set value = _callerLineNumber <- value
        member this.Exception
            with get ()    = _exception
            and  set value = _exception <- value
        
        static member Create (scope, msg, excp) =
            let stack = StackFrame (scope + 1, true)
            let mutable sl = StructuredLog ()
            sl.LogMessage       <- msg
            sl.CallerName       <- (stack.GetMethod ()).Name
            sl.CallerNamespace  <- ((stack.GetMethod ()).ReflectedType).FullName
            sl.CallerFile       <- Path.GetFileName (stack.GetFileName ())
            sl.CallerDirectory  <- Path.GetDirectoryName (stack.GetFileName ())
            sl.CallerLineNumber <- (stack.GetFileLineNumber ())
            sl.Exception        <- excp
            sl
        
        static member Create (scope, msg) =
            StructuredLog.Create (scope + 1, msg, null)
        
        static member Create (scope, excp) =
            StructuredLog.Create (scope + 1, null, excp)
        
        member this.ToJson () =
            ()
        
        override this.ToString () =
            // TODO: handle the presence of the Exception property not being null
            String.Format("[{0}.{1} ({2}:{3})] {4}", 
                this.CallerNamespace, 
                this.CallerName, 
                this.CallerFile, 
                this.CallerLineNumber, 
                this.LogMessage
            )
    
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
        //scope of 2 instead of 1 because this becomes a lambda and we need to skip Invoke
        let ls = StructuredLog.Create (2, msg)
        Log.Information ("{info}", ls)
        ()
    
    let logWarning1 (msg : string) =
        let ls = StructuredLog.Create (2, msg)
        raise <| NotImplementedException ()
        ()
    
    let logError1 (msg : string) =
        let ls = StructuredLog.Create (2, msg)
        raise <| NotImplementedException ()
        ()
        
    let logCritical1 (msg : string) =
        let ls = StructuredLog.Create (2, msg)
        raise <| NotImplementedException ()
        ()
    
    let logDebug1 (msg : string) =
        if Debugger.IsAttached then
            let ls = StructuredLog.Create (2, msg)
            raise <| NotImplementedException ()
        ()
    
    type Logger = {
        info     : string -> unit
        warning  : string -> unit
        error    : string -> unit
        critical : string -> unit
        debug    : string -> unit
    }
    
    let syslog = {
        info     = logInfo1
        warning  = logWarning1
        error    = logError1
        critical = logCritical1
        debug    = logDebug1
    }
