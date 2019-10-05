
namespace Mochi.Tests

module Program = 
    
    open System
    open Serilog
    open Mochi.Core.Logging
    
    let mytest _ =
        Mochi.Core.GCMonitor.monitor ()
        Mochi.Core.GCMonitor.subscribe (fun _ -> syslog.info "wooow GC")
        Mochi.Core.GCMonitor.start ()
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
        GC.Collect ()
        GC.WaitForPendingFinalizers ()
    
    let mytest2 _ =
        Mochi.Core.GCMonitor.monitor ()
        Mochi.Core.GCMonitor.start ()
        let mutable run = true
        let mutable str = String.Empty
        Console.WriteLine "Lets do it"
        while run do
            Console.Write "> "
            str <- Console.ReadLine ()
            match str with
            | "q" -> run <- false
            | _ -> ()
    
    let mytest3 _ =
        Mochi.Core.GCMonitor.monitor ()
        Mochi.Core.GCMonitor.start ()
        Console.WriteLine "Lets do it"
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
                

    let setupLogging _ =
        let mutable conf = LoggerConfiguration ()
        conf <- conf.WriteTo.Console (outputTemplate = 
            "[{Timestamp:HH:mm:ss.ff} {Level:u4}] " + 
            "[{CallerNamespace}.{CallerName} ({CallerFile}:{CallerLineNumber})]: " + 
            //"{NewLine} --> " +
            "{Message:lj}. {NewLine}{Exception}"
        )
        Log.Logger <- conf.CreateLogger ()
        ()

    [<EntryPoint>]
    let main _ =
        setupLogging ()
        mytest3 ()
        // Console.ReadLine () |> ignore
        0


