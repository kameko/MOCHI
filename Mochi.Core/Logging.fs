
namespace Mochi.Core

module Logging =

    open System
    open System.IO
    open System.Diagnostics
    open System.Text.Json
    open System.Text.Json.Serialization
    open Serilog
    
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
            if (stack.GetMethod ()).Name = "Invoke" then
                // Caller is a lambda, we have to treat this special
                let ns = ((stack.GetMethod ()).ReflectedType).FullName.Split '+'
                let fn = (ns.[1]).Split '@'
                // if we ever want to get the line number the lambda is defined on, since it contains that in it's name:
                // sl.CallerName       <- String.Format("{0} (Line {1})", fn.[0], ((fn.[1]).Substring(0, (fn.[1].Length - 1))))
                sl.CallerName       <- fn.[0]
                sl.CallerNamespace  <- ns.[0]
            else
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
            let mutable logger : ILogger = Log.Logger
            logger <- logger.ForContext ("CallerName"       , sl.CallerName)
            logger <- logger.ForContext ("CallerNamespace"  , sl.CallerNamespace)
            logger <- logger.ForContext ("CallerFile"       , sl.CallerFile)
            logger <- logger.ForContext ("CallerDirectory"  , sl.CallerDirectory)
            logger <- logger.ForContext ("CallerLineNumber" , sl.CallerLineNumber)
            logger

        static member LogInfo (scope, msg) =
            let sl = StructuredLog.Create (scope + 1, msg, null)
            let logger = StructuredLog.FormContext (sl)
            logger.Information (msg)
            ()

        static member LogInfo (scope, msg, excp) =
            let sl = StructuredLog.Create (scope + 1, msg, excp)
            let logger = StructuredLog.FormContext (sl)
            logger.Information (msg)
            ()
        
        static member LogWarning (scope, msg) =
            let sl = StructuredLog.Create (scope + 1, msg, null)
            let logger = StructuredLog.FormContext (sl)
            logger.Warning (msg)
            ()

        static member LogWarning (scope, msg, excp) =
            let sl = StructuredLog.Create (scope + 1, msg, excp)
            let logger = StructuredLog.FormContext (sl)
            logger.Warning (msg)
            ()
            
        static member LogError (scope, msg) =
            let sl = StructuredLog.Create (scope + 1, msg, null)
            let logger = StructuredLog.FormContext (sl)
            logger.Error (msg)
            ()

        static member LogError (scope, msg, excp) =
            let sl = StructuredLog.Create (scope + 1, msg, excp)
            let logger = StructuredLog.FormContext (sl)
            logger.Error (msg)
            ()
            
        static member LogFatal (scope, msg) =
            let sl = StructuredLog.Create (scope + 1, msg, null)
            let logger = StructuredLog.FormContext (sl)
            logger.Fatal (msg)
            ()

        static member LogFatal (scope, msg, excp) =
            let sl = StructuredLog.Create (scope + 1, msg, excp)
            let logger = StructuredLog.FormContext (sl)
            logger.Fatal (msg)
            ()
        
        [<Conditional("DEBUG")>]
        static member LogDebug (scope, msg) =
            let sl = StructuredLog.Create (scope + 1, msg, null)
            let logger = StructuredLog.FormContext (sl)
            logger.Debug (msg)
            ()

        [<Conditional("DEBUG")>]
        static member LogDebug (scope, msg, excp) =
            let sl = StructuredLog.Create (scope + 1, msg, excp)
            let logger = StructuredLog.FormContext (sl)
            logger.Debug (msg)
            ()

        member this.ToJson (options : JsonSerializerOptions) =
            JsonSerializer.Serialize<StructuredLog>(this, options)

        member this.ToJson (pretty : bool) =
            let options = JsonSerializerOptions (
                            WriteIndented = pretty
                          )
            this.ToJson options

        member this.ToJson () =
            this.ToJson false
            
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

    let logInfo2 (scope : int) (msg : string) (excp : Exception) =
        StructuredLog.LogInfo (scope + 1, msg, excp)
    
    let logWarning2 (scope : int) (msg : string) (excp : Exception) =
        StructuredLog.LogWarning (scope + 1, msg, excp)
    
    let logError2 (scope : int) (msg : string) (excp : Exception) =
        StructuredLog.LogError (scope + 1, msg, excp)
        
    let logFatal2 (scope : int) (msg : string) (excp : Exception) =
        StructuredLog.LogFatal (scope + 1, msg, excp)
    
    let logDebug2 (scope : int) (msg : string) (excp : Exception) =
        StructuredLog.LogDebug (scope + 1, msg, excp)
    
    type Logger = {
        info     : string -> unit
        warning  : string -> unit
        error    : string -> unit
        fatal    : string -> unit
        debug    : string -> unit
        info2    : string -> Exception -> unit
        warning2 : string -> Exception -> unit
        error2   : string -> Exception -> unit
        fatal2   : string -> Exception -> unit
        debug2   : string -> Exception -> unit
    }
    
    let syslog = {
        info     = logInfo1    1
        warning  = logWarning1 1
        error    = logError1   1
        fatal    = logFatal1   1
        debug    = logDebug1   1
        info2    = logInfo2    1
        warning2 = logWarning2 1
        error2   = logError2   1
        fatal2   = logFatal2   1
        debug2   = logDebug2   1
    }
    
