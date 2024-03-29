
namespace Mochi.Tests

module Program = 
    
    open System
    open System.Runtime.CompilerServices
    open Akka.Actor
    open Akka.FSharp
    open Serilog
    open Mochi.Core.Logging
    open Mochi.Core.AkkaLogging
    open Mochi.Core.PluginLoader
    
    let ``Test function logging stack frame depth`` _ =
        syslog.info "syslog.info top level"
        let innerFunc _ =
            syslog.info "syslog.info inner 1"
            let innerFunc2 _ =
                syslog.warning "syslog.warning inner 2"
                ()
            innerFunc2 ()
            ()
        innerFunc ()
    
    let ``Test GCMonitor runs multiple times`` _ =
        let mutable interrupt = false
        let mutable run = true
        let mutable count = 0
        Mochi.Core.GCMonitor.subscribe (fun _ -> interrupt <- true)
        while run do
            if interrupt then
                Console.WriteLine("INTERRUPTED")
                interrupt <- false
                while not (Console.ReadKey(true).Key = ConsoleKey.C) do
                    ()
            if Console.KeyAvailable then
                if (Console.ReadKey(true).Key = ConsoleKey.Q) then
                    Console.WriteLine("Execution ended.")
                    run <- false
                if (Console.ReadKey(true).Key = ConsoleKey.P) then
                    while not (Console.ReadKey(true).Key = ConsoleKey.C) do
                        ()
            else
                System.Threading.Thread.Sleep(10)
                let guid1 = ((Guid.NewGuid ()).ToString()).ToUpper()
                let guid2 = ((Guid.NewGuid ()).ToString()).ToUpper()
                Console.WriteLine("{0} {1} ({2})", guid1, guid2, count)
                count <- count + 1
    
    let ``Test actor message passing`` _ =
        ()

    let commandReader (system : ActorSystem) (master : IActorRef) =
        let mutable run = true
        let mutable str = String.Empty
        while run do
            Console.Write "CMD> "
            str <- Console.ReadLine ()
            if str = "q" then
                run <- false
            let strs = str.Split()
            if strs.Length > 1 then
                let strindex = str.IndexOf(" ") + 1
                let nstr = str.Substring strindex
                match (strs.[0]).ToLower () with
                | "info"                -> syslog.info nstr
                | "warn"  | "warning"   -> syslog.warning nstr
                | "error"               -> syslog.error nstr
                | "fatal" | "critical"  -> syslog.fatal nstr
                | "debug"               -> syslog.debug nstr
                | "console"             -> Console.WriteLine nstr
                | "master"              -> master <! nstr
                | "rawlog"              -> Log.Information nstr
                | _                     -> syslog.info str
            else if not (String.IsNullOrEmpty(str) || String.IsNullOrWhiteSpace(str)) then
                match (strs.[0]).ToLower () with
                | "gc"    -> GC.Collect(); GC.WaitForPendingFinalizers()
                | "q"     -> ()
                | "test1" -> ``Test function logging stack frame depth`` ()
                | _       -> syslog.info str
    
    let spawnMasterActor system =
        let aref = spawn system "master" (fun mailbox ->
            let rec masterActor () = actor {
                let! (message : obj) = mailbox.Receive()
                match message with
                | _ -> 
                    akkalog.info mailbox <| sprintf "Got message: %O" message
                    // mailbox.Unhandled ()
                    return! masterActor ()
            }
            masterActor ())
        aref

    let setupLogging _ =
        let mutable conf = LoggerConfiguration ()
        conf <- conf.MinimumLevel.Debug() //.MinimumLevel.ControlledBy(levelSwitch)
        conf <- conf.WriteTo.Console (outputTemplate = 
            "[{Timestamp:HH:mm:ss.ff} {Level:u3}] " + 
            "{LIB}[{FullFileInfo}{SourceContext}]{CallerThreadNumber}{ActorPath}: " + 
            "{Message:lj}. {NewLine}{Exception}"
        )
        Log.Logger <- conf.CreateLogger ()
        Log.Logger <- Log.Logger.ForContext ("LIB", "[LIB] ")
        Mochi.Core.GCMonitor.GCMonitor.LogMessage <- "Garbage collection occurred"
        ()

    let setupActors _ =
        //let system = System.create "mochi" (Configuration.load())
        let config = Configuration.parse "\
            akka { \
                loglevel=DEBUG, \
                loggers=[\"Akka.Logger.Serilog.SerilogLogger, Akka.Logger.Serilog\"] \
                log-config-on-start = on \
                actor { \
                    debug { \
                        receive = on \
                        autoreceive = on \
                        lifecycle = on \
                        event-stream = on \
                        unhandled = on \
                    } \
                } \
            } \
            "
        let system = System.create "mochi" config
        let master = spawnMasterActor system
        master <! "yo wassap"
        master <! "hey"
        master <! "hey2"
        (master, system)
    
    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let ``prelude pathos`` _ =
        setupLogging ()
        syslog.info "MOCHI Test Environment"
        syslog.info <| sprintf "Running in %s mode" (releaseString ())
        syslog.info <| sprintf "%i cores present" Environment.ProcessorCount
        Mochi.Core.GCMonitor.start ()
        let (master, system) = setupActors ()

        //(* // Non-plugin
        let discord = Mochi.Plugin.Discord.DiscordPlugin()
        discord.PreLoad()
        let dai = (discord.LoadSupervisor()).Props
        let pa = system.ActorOf(dai, (sprintf "%sActor" discord.Info.Name))
        pa <! "Hey!"
        //*)

        (* // Plugin (broken)
        let plugin = getPlugin ".\\..\\..\\..\\..\\Modules\\Mochi.Plugin.Discord\\bin\\Debug\\netcoreapp3.0\\Mochi.Plugin.Discord.dll"
        match plugin with
        | Ok p -> 
            syslog.info <| sprintf "Plugin loaded: %s" p.Info.Name
            let pa = system.ActorOf((p.LoadSupervisor ()).Props)
            pa <! "Hey!"
            ()
        | Error e -> 
            syslog.info <| sprintf "Plugin not loaded: %A" e
            ()
        //*)
        commandReader system master
        system.Dispose ()
        ()

    [<EntryPoint>]
    let main _ =
        ``prelude pathos`` ()
        0


