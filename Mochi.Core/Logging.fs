
namespace Mochi.Core

module Logging =

    open System
    open System.IO
    open System.Diagnostics
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
            let mutable stack = StackFrame (scope + 1, true)
            if (stack.GetMethod ()).Name = "Invoke" then
                // If the current call stack is "Invoke" (a lambda) that means we're
                // probably inside the callback in the "syslog" functions below. we
                // need to go up a frame to get the actual caller.
                stack <- StackFrame (scope + 2, true)
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
        
        static member LogInfo (scope, msg) =
            let sl = StructuredLog.Create (scope + 1, msg, null)
            Log.Information ("{info}", sl)
            Console.WriteLine(sl)
            ()
        
        static member LogWarning (scope, msg) =
            raise <| NotImplementedException ()
            ()
            
        static member LogError(scope, msg) =
            raise <| NotImplementedException ()
            ()
            
        static member LogCritical (scope, msg) =
            raise <| NotImplementedException ()
            ()
        
        [<Conditional("DEBUG")>]
        static member LogDebug (scope, msg) =
            let sl = StructuredLog.Create (scope + 1, msg, null)
            Log.Debug ("{info}", sl)
            ()
        
        member this.ToJson () =
            ()
        
        override this.ToString () =
            if isNull this.Exception then
                String.Format("[{0}.{1} ({2}:{3})] {4}", 
                    this.CallerNamespace, 
                    this.CallerName, 
                    this.CallerFile, 
                    this.CallerLineNumber, 
                    this.LogMessage
                )
            else
                String.Format("[{0}.{1} ({2}:{3})] {4}: {5}", 
                    this.CallerNamespace, 
                    this.CallerName, 
                    this.CallerFile, 
                    this.CallerLineNumber, 
                    this.LogMessage,
                    this.Exception
                )
    
    let logInfo1 (msg : string) =
        StructuredLog.LogInfo (1, msg)
    
    let logWarning1 (msg : string) =
        StructuredLog.LogWarning (1, msg)
    
    let logError1 (msg : string) =
        StructuredLog.LogError (1, msg)
        
    let logCritical1 (msg : string) =
        StructuredLog.LogCritical (1, msg)
    
    let logDebug1 (msg : string) =
        StructuredLog.LogDebug (1, msg)
    
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
    
