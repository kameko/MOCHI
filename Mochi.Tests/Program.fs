
namespace Mochi.Tests

module Program = 
    
    open System
    open Serilog
    open Mochi.Core.Logging
    
    let mytest _ =
        let depth = 0
        logInfo1 depth "logInfo top level"
        syslog.info "syslog.info top level"
        let innerFunc _ =
            logInfo1 depth "logInfo inner 1"
            syslog.info "syslog.info inner 1"
            let innerFunc2 _ =
                logInfo1 depth "logInfo inner 2"
                syslog.warning "syslog.warning inner 2"
                ()
            innerFunc2 ()
            ()
        innerFunc ()
    
    let setupLogging _ =
        let mutable conf = LoggerConfiguration ()
        conf <- conf.WriteTo.Console (outputTemplate = 
            "[{Timestamp:HH:mm:ss.ff} {Level:u3}] " + 
            "[{CallerNamespace}.{CallerName} ({CallerFile}:{CallerLineNumber})] " + 
            "{NewLine} --> " +
            "{Message:lj}{NewLine}{Exception}"
        )
        Log.Logger <- conf.CreateLogger ()
        ()

    [<EntryPoint>]
    let main _ =
        setupLogging ()
        mytest ()
        // Console.ReadLine () |> ignore
        0


