
namespace Mochi.Core

module AkkaLogging =
    
    open System
    open System.Runtime.CompilerServices
    open System.Diagnostics
    open Akka.Actor
    open Akka.Event
    open Akka.FSharp
    open Akka.Logger.Serilog
    open Serilog
    open Logging

    let makeContext (sl : StructuredLog) (la : ILoggingAdapter) =
        let mutable logger = la
        logger <- logger.ForContext ("CallerName"       , sl.CallerName)
        logger <- logger.ForContext ("CallerNamespace"  , sl.CallerNamespace)
        logger <- logger.ForContext ("CallerFullName"   , sprintf "%s.%s" sl.CallerNamespace sl.CallerName)
        logger <- logger.ForContext ("CallerFile"       , sl.CallerFile)
        logger <- logger.ForContext ("CallerDirectory"  , sl.CallerDirectory)
        logger <- logger.ForContext ("CallerLineNumber" , sl.CallerLineNumber)
        logger <- logger.ForContext ("CallerFileNumber" , sprintf "%s:%i" sl.CallerFile sl.CallerLineNumber)
        logger

    let makeMailboxContext (sl : StructuredLog) (mailbox : Actor<'Message>) =
        let mutable logger = StructuredLog.FormContext sl
        logger <- logger.ForContext ("ActorPath" , sprintf "[%O]" mailbox.Self.Path)
        logger <- logger.ForContext ("ActorName" , mailbox.Self.Path.Name)
        logger

    let akkalogger (mailbox : Actor<'Message>) =
        let sl = StructuredLog.Create (1, String.Empty)
        makeContext sl <| mailbox.Context.GetLogger ()

    let akkaloggerm (mailbox : Actor<'Message>) (msg : string) =
        let sl = StructuredLog.Create (1, msg)
        makeContext sl <| mailbox.Context.GetLogger ()

    let akkaloggerem (mailbox : Actor<'Message>) (excp : Exception) (msg : string) =
        let sl = StructuredLog.Create (1, msg, excp)
        makeContext sl <| mailbox.Context.GetLogger ()
        
    type akkalog () =
        static member private context (mailbox : Actor<'Message>) (msg : string) =
            makeMailboxContext (StructuredLog.Create ((if isRelease () then 1 else 2), msg)) mailbox

        [<MethodImpl(MethodImplOptions.NoInlining)>]
        static member info (mailbox : Actor<'Message>) (msg : string) =
            let logger = akkalog.context mailbox msg
            logger.Information msg

        [<MethodImpl(MethodImplOptions.NoInlining)>]
        static member infoe (mailbox : Actor<'Message>) (excp : Exception) (msg : string) =
            let logger = akkalog.context mailbox msg
            logger.Information (excp, msg)

        [<MethodImpl(MethodImplOptions.NoInlining)>]
        static member warning (mailbox : Actor<'Message>) (msg : string) =
            let logger = akkalog.context mailbox msg
            logger.Warning msg

        [<MethodImpl(MethodImplOptions.NoInlining)>]
        static member warninge (mailbox : Actor<'Message>) (excp : Exception) (msg : string) =
            let logger = akkalog.context mailbox msg
            logger.Warning (excp, msg)

        [<MethodImpl(MethodImplOptions.NoInlining)>]
        static member error (mailbox : Actor<'Message>) (msg : string) =
            let logger = akkalog.context mailbox msg
            logger.Error msg

        [<MethodImpl(MethodImplOptions.NoInlining)>]
        static member errore (mailbox : Actor<'Message>) (excp : Exception) (msg : string) =
            let logger = akkalog.context mailbox msg
            logger.Error (excp, msg)

        [<MethodImpl(MethodImplOptions.NoInlining)>]
        static member fatal (mailbox : Actor<'Message>) (msg : string) =
            let logger = akkalog.context mailbox msg
            logger.Fatal msg

        [<MethodImpl(MethodImplOptions.NoInlining)>]
        static member fatale (mailbox : Actor<'Message>) (excp : Exception) (msg : string) =
            let logger = akkalog.context mailbox msg
            logger.Fatal (excp, msg)

        [<Conditional("DEBUG")>]
        [<MethodImpl(MethodImplOptions.NoInlining)>]
        static member debug (mailbox : Actor<'Message>) (msg : string) =
            let logger = akkalog.context mailbox msg
            logger.Information msg

        [<Conditional("DEBUG")>]
        [<MethodImpl(MethodImplOptions.NoInlining)>]
        static member debuge (mailbox : Actor<'Message>) (excp : Exception) (msg : string) =
            let logger = akkalog.context mailbox msg
            logger.Information (excp, msg)
