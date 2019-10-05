
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
        let mutable _callerName       : string = String.Empty
        let mutable _callerNamespace  : string = String.Empty
        let mutable _callerFile       : string = String.Empty
        let mutable _callerDirectory  : string = String.Empty
        let mutable _callerLineNumber : int = 0
        let mutable _logMessage       : string = String.Empty
        let mutable _exception        : Exception = null
        
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
        member this.LogMessage
            with get ()    = _logMessage
            and  set value = _logMessage <- value

        static member Create (scope, msg, excp) =
            let stack = StackFrame (scope + 1, true)
            let sl = StructuredLog ()
            sl.CallerName       <- (stack.GetMethod ()).Name
            sl.CallerNamespace  <- ((stack.GetMethod ()).ReflectedType).FullName
            sl.CallerFile       <- Path.GetFileName (stack.GetFileName ())
            sl.CallerDirectory  <- Path.GetDirectoryName (stack.GetFileName ())
            sl.CallerLineNumber <- (stack.GetFileLineNumber ())
            sl.LogMessage       <- msg
            sl.Exception        <- excp
            sl
        
        static member Create (scope, msg) =
            StructuredLog.Create (scope + 1, msg, null)
        
        static member Create (scope, excp) =
            StructuredLog.Create (scope + 1, null, excp)
        
        static member private FormContext (sl : StructuredLog) =
            let mutable logger : ILogger = Log.ForContext ("CallerName", sl.CallerName)
            logger <- logger.ForContext ("CallerNamespace", sl.CallerNamespace)
            logger <- logger.ForContext ("CallerFile", sl.CallerFile)
            logger <- logger.ForContext ("CallerDirectory", sl.CallerDirectory)
            logger <- logger.ForContext ("CallerLineNumber", sl.CallerLineNumber)
            logger

        static member LogInfo (scope, msg) =
            let sl = StructuredLog.Create (scope + 1, msg, null)
            let logger = StructuredLog.FormContext (sl)
            logger.Information (msg)
            ()
        
        static member LogWarning (scope, msg) =
            let sl = StructuredLog.Create (scope + 1, msg, null)
            let logger = StructuredLog.FormContext (sl)
            logger.Warning (msg)
            ()
            
        static member LogError (scope, msg) =
            let sl = StructuredLog.Create (scope + 1, msg, null)
            let logger = StructuredLog.FormContext (sl)
            logger.Error (msg)
            ()
            
        static member LogFatal (scope, msg) =
            let sl = StructuredLog.Create (scope + 1, msg, null)
            let logger = StructuredLog.FormContext (sl)
            logger.Fatal (msg)
            ()
        
        [<Conditional("DEBUG")>]
        static member LogDebug (scope, msg) =
            let sl = StructuredLog.Create (scope + 1, msg, null)
            let logger = StructuredLog.FormContext (sl)
            logger.Debug (msg)
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
    
    let logInfo1 (scope : int) (msg : string) =
        StructuredLog.LogInfo (scope + 1, msg)
    
    let logWarning1 (scope : int) (msg : string) =
        StructuredLog.LogWarning (scope + 1, msg)
    
    let logError1 (scope : int) (msg : string) =
        StructuredLog.LogError (scope + 1, msg)
        
    let logFatal1 (scope : int) (msg : string) =
        StructuredLog.LogFatal (scope + 1, msg)
    
    let logDebug1 (scope : int) (msg : string) =
        StructuredLog.LogDebug (scope + 1, msg)
    
    type Logger = {
        info    : string -> unit
        warning : string -> unit
        error   : string -> unit
        fatal   : string -> unit
        debug   : string -> unit
    }
    
    let syslog = {
        info    = logInfo1 1
        warning = logWarning1 1
        error   = logError1 1
        fatal   = logFatal1 1
        debug   = logDebug1 1
    }
    
