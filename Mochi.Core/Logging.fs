
namespace Mochi.Core

module Logging =

    open System
    open System.IO
    open System.Diagnostics
    open System.Reflection
    open System.Runtime.CompilerServices
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

        static let mutable _isRelease : bool = true
        
        static do
            _isRelease <- StructuredLog.IsReleaseMode ()

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
        
        [<JsonIgnore>]
        static member public IsRelease
            with get ()    = _isRelease

        static member public IsReleaseMode _ =
            let asm = Assembly.GetEntryAssembly ()
            let atrbs = asm.GetCustomAttributes(typeof<DebuggableAttribute>, true)
            if (isNull atrbs) || atrbs.Length = 0 then
                true
            else if atrbs.[0] :? DebuggableAttribute then
                let dbgatr = atrbs.[0] :?> DebuggableAttribute
                if dbgatr.IsJITOptimizerDisabled then
                    false
                else
                    true
            else
                true

        static member Create (scope, msg, excp) =
            let stack = StackFrame (scope + 1, true)
            let sl = StructuredLog ()
            if isNull (stack.GetMethod ()) then
                sl.CallerName       <- "NOCALL"
                sl.CallerNamespace  <- "NOCALL"
            else
                if (stack.GetMethod ()).Name = "Invoke" then
                    // Caller is a lambda, we have to treat this special
                    let ns = ((stack.GetMethod ()).ReflectedType).FullName.Split '+'
                    let fn = (ns.[1]).Split '@'
                    // if we ever want to get the line number the lambda is defined on, since it contains that in it's name:
                    // sl.CallerName       <- String.Format("{0} (Line {1})", fn.[0], ((fn.[1]).Substring(0, (fn.[1].Length - 1))))
                    sl.CallerName       <- fn.[0]
                    sl.CallerNamespace  <- ns.[0]
                else if ((stack.GetMethod ()).ReflectedType).FullName.Contains("+") then
                    let ns = ((stack.GetMethod ()).ReflectedType).FullName.Split '+'
                    sl.CallerName       <- (stack.GetMethod ()).Name
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
        
        static member FormContext (sl : StructuredLog) =
            let mutable logger : ILogger = Log.Logger
            logger <- logger.ForContext ("CallerName"       , sl.CallerName)
            logger <- logger.ForContext ("CallerNamespace"  , sl.CallerNamespace)
            logger <- logger.ForContext ("CallerFullName"   , sprintf "%s.%s" sl.CallerNamespace sl.CallerName)
            logger <- logger.ForContext ("CallerFile"       , sl.CallerFile)
            logger <- logger.ForContext ("CallerDirectory"  , sl.CallerDirectory)
            logger <- logger.ForContext ("CallerLineNumber" , sl.CallerLineNumber)
            logger <- logger.ForContext ("CallerFileNumber" , sprintf "%s:%i" sl.CallerFile sl.CallerLineNumber)
            logger

        static member LogInfo (scope, msg) =
            let sl = StructuredLog.Create (scope + 1, msg, null)
            let logger = StructuredLog.FormContext (sl)
            logger.Information (msg)
            ()

        static member LogInfo (scope, excp, msg) =
            let sl = StructuredLog.Create (scope + 1, msg, excp)
            let logger = StructuredLog.FormContext (sl)
            logger.Information (excp, msg)
            ()
        
        static member LogWarning (scope, msg) =
            let sl = StructuredLog.Create (scope + 1, msg, null)
            let logger = StructuredLog.FormContext (sl)
            logger.Warning (msg)
            ()

        static member LogWarning (scope, excp, msg) =
            let sl = StructuredLog.Create (scope + 1, msg, excp)
            let logger = StructuredLog.FormContext (sl)
            logger.Warning (excp, msg)
            ()
            
        static member LogError (scope, msg) =
            let sl = StructuredLog.Create (scope + 1, msg, null)
            let logger = StructuredLog.FormContext (sl)
            logger.Error (msg)
            ()

        static member LogError (scope, excp, msg) =
            let sl = StructuredLog.Create (scope + 1, msg, excp)
            let logger = StructuredLog.FormContext (sl)
            logger.Error (excp, msg)
            ()
            
        static member LogFatal (scope, msg) =
            let sl = StructuredLog.Create (scope + 1, msg, null)
            let logger = StructuredLog.FormContext (sl)
            logger.Fatal (msg)
            ()

        static member LogFatal (scope, excp, msg) =
            let sl = StructuredLog.Create (scope + 1, msg, excp)
            let logger = StructuredLog.FormContext (sl)
            logger.Fatal (excp, msg)
            ()
        
        [<Conditional("DEBUG")>]
        static member LogDebug (scope, msg) =
            let sl = StructuredLog.Create (scope + 1, msg, null)
            let logger = StructuredLog.FormContext (sl)
            logger.Debug (msg)
            ()

        [<Conditional("DEBUG")>]
        static member LogDebug (scope, excp, msg) =
            let sl = StructuredLog.Create (scope + 1, msg, excp)
            let logger = StructuredLog.FormContext (sl)
            logger.Debug (excp, msg)
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
    
    let isRelease _ =
        StructuredLog.IsRelease

    let releaseString _ =
        if StructuredLog.IsRelease then "RELEASE" else "DEBUG"
    
    type syslog () =
        [<MethodImpl(MethodImplOptions.NoInlining)>]
        static member info msg =
            StructuredLog.LogInfo (1, msg)
            
        [<MethodImpl(MethodImplOptions.NoInlining)>]
        static member infoe excp msg =
            StructuredLog.LogInfo (1, excp, msg)

        [<MethodImpl(MethodImplOptions.NoInlining)>]
        static member warning msg =
            StructuredLog.LogWarning (1, msg)

        [<MethodImpl(MethodImplOptions.NoInlining)>]
        static member warninge excp msg =
            StructuredLog.LogWarning (1, excp, msg)

        [<MethodImpl(MethodImplOptions.NoInlining)>]
        static member error msg =
            StructuredLog.LogError (1, msg)

        [<MethodImpl(MethodImplOptions.NoInlining)>]
        static member errore excp msg =
            StructuredLog.LogError (1, excp, msg)

        [<MethodImpl(MethodImplOptions.NoInlining)>]
        static member fatal msg =
            StructuredLog.LogFatal (1, msg)

        [<MethodImpl(MethodImplOptions.NoInlining)>]
        static member fatale excp msg =
            StructuredLog.LogFatal (1, excp, msg)

        [<Conditional("DEBUG")>]
        [<MethodImpl(MethodImplOptions.NoInlining)>]
        static member debug msg =
            StructuredLog.LogDebug (1, msg)

        [<Conditional("DEBUG")>]
        [<MethodImpl(MethodImplOptions.NoInlining)>]
        static member debuge excp msg =
            StructuredLog.LogDebug (1, excp, msg)
    
