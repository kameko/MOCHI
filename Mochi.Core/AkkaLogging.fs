
namespace Mochi.Core

module AkkaLogging =
    
    open System
    open System.Runtime.CompilerServices
    open Akka.Actor
    open Akka.Event
    open Akka.FSharp
    open Akka.Logger.Serilog
    open Logging

    let makeContext (sl : StructuredLog) (la : ILoggingAdapter) =
        let mutable logger = la
        logger <- logger.ForContext ("CallerName"       , sl.CallerName)
        logger <- logger.ForContext ("CallerNamespace"  , sl.CallerNamespace)
        logger <- logger.ForContext ("CallerFile"       , sl.CallerFile)
        logger <- logger.ForContext ("CallerDirectory"  , sl.CallerDirectory)
        logger <- logger.ForContext ("CallerLineNumber" , sl.CallerLineNumber)
        logger

    let akkalogger (mailbox : Actor<'Message>) =
        let sl = StructuredLog.Create (1, String.Empty)
        makeContext sl <| mailbox.Context.GetLogger ()

    let akkaloggerm (mailbox : Actor<'Message>) (msg : string) =
        let sl = StructuredLog.Create (1, msg)
        makeContext sl <| mailbox.Context.GetLogger ()

    // [<MethodImpl(MethodImplOptions.NoInlining)>]
    let akkaloggerem (mailbox : Actor<'Message>) (excp : Exception) (msg : string) =
        let sl = StructuredLog.Create (1, msg, excp)
        makeContext sl <| mailbox.Context.GetLogger ()
        
