
namespace Mochi.Plugin.Discord

module InitDiscord =
    
    open System
    open System.Threading.Tasks
    open Discord
    open Discord.WebSocket
    open Mochi.Core.Logging
    
    let discordLog (msg : LogMessage) =
        match msg.Severity with
        | LogSeverity.Info     -> syslog.info    <| sprintf "%O" msg
        | LogSeverity.Warning  -> syslog.warning <| sprintf "%O" msg
        | LogSeverity.Error    -> syslog.error   <| sprintf "%O" msg
        | LogSeverity.Critical -> syslog.fatal   <| sprintf "%O" msg
        | LogSeverity.Debug    -> syslog.debug   <| sprintf "%O" msg
        | LogSeverity.Verbose  -> syslog.debug   <| sprintf "VERBOSE: %O" msg
        | _                    -> syslog.debug   <| sprintf "UNHANDLED DISCORD LOG LEVEL: %O" msg
        Task.CompletedTask

    let createClient () =
        let conf = DiscordSocketConfig()

        if StructuredLog.IsRelease then
            conf.LogLevel <- LogSeverity.Info
        else
            conf.LogLevel <- LogSeverity.Debug

        let client = new DiscordSocketClient(conf)
        client.add_Log(fun msg -> discordLog msg)
        client

    let setup () =
        let client = createClient ()
        ()