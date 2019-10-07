
namespace Mochi.Tests

module Program = 
    
    open System
    open System.Runtime.CompilerServices
    open Serilog
    open Mochi.Core.Logging
    
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

    let commandReader _ =
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
                | "warn"  | "warning"   -> syslog.warning nstr
                | "error"               -> syslog.error nstr
                | "fatal" | "critical"  -> syslog.fatal nstr
                | "debug"               -> syslog.debug nstr
                | "console"             -> Console.WriteLine nstr
                | "info"                -> syslog.info nstr
                | _                     -> syslog.info str
            else if not (String.IsNullOrEmpty(str) || String.IsNullOrWhiteSpace(str)) then
                match (strs.[0]).ToLower () with
                | "gc"    -> GC.Collect(); GC.WaitForPendingFinalizers()
                | "q"     -> ()
                | "test1" -> ``Test function logging stack frame depth`` ()
                | _       -> syslog.info str

    let setupLogging _ =
        let mutable conf = LoggerConfiguration ()
        conf <- conf.MinimumLevel.Debug()
        conf <- conf.WriteTo.Console (outputTemplate = 
            "[{Timestamp:HH:mm:ss.ff} {Level:u4}] " + 
            "[{CallerNamespace}.{CallerName} ({CallerFile}:{CallerLineNumber})]: " + 
            "{Message:lj}. {NewLine}{Exception}"
        )
        Log.Logger <- conf.CreateLogger ()
        Mochi.Core.GCMonitor.start ()
        ()

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let ``prelude pathos`` _ =
        setupLogging ()
        syslog.info "MOCHI Test Environment"
        syslog.info <| sprintf "Running in %s mode" (releaseString ())
        ()

    [<EntryPoint>]
    let main _ =
        ``prelude pathos`` ()
        commandReader ()
        0


