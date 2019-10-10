
using System;

namespace Mochi.TestPlugin1
{
    using System.Collections.Generic;
    using Mochi.Plugins.Types;
    using Akka.Actor;

    public class Plugin1 : Plugin
    {
        public override void PreLoad()
        {
            throw new NotImplementedException();
        }

        public override ActorInfo LoadSupervisor()
        {
            throw new NotImplementedException();
        }
    }

    // FIXME: creating a plugin from C# is absolutely impossible.
    /*
    using System.Collections.Generic;
    using Mochi.Core;

    public class Plugin1 : Plugins.BasePlugin
    {
        public override Plugins.Plugin Load()
        {
            var plugin = new Plugins.Plugin(
                info: new Plugins.PluginInfo(
                    name: "Plugin1",
                    company: "Caesura Software Solutions",
                    version: new Version(1, 0, 0, 0),
                    guid: new Guid("01d5dd9f-9c5a-4aee-ac59-51a7cf6f0cf4"),
                    description: "Test plugin in C#",
                    published: new DateTime(2019, 10, 9),
                    author: "Kameko",
                    copyright: "Kameko 2019",
                    license: "MS-PL"
                ),
                loadDependencies: new Microsoft.FSharp.Collections.FSharpList<Plugins.PluginRequirement>(),
                execDependencies: new List<Plugins.PluginRequirement>(),
                supervisor: delegate (Akka.FSharp.Actors.Actor<object> mailbox) { return null; },
                onLoad: pe => null,
                onReport: (pe, rr) => null,
                onUnload: ur => null
            );
            return plugin;
        }
    }
    */
}
