using LiveSplit.Model;
using System;
using LiveSplit.UI.Components;

[assembly: ComponentFactory(typeof(ServerFactory))]

namespace LiveSplit.UI.Components
{
    public class ServerFactory : IComponentFactory
    {
        public string ComponentName => "LiveSplit WebSocket Server";

        public string Description => "Allows a remote connection and control of LiveSplit by starting a web socket server within LiveSplit.";

        public ComponentCategory Category => ComponentCategory.Control;

        public IComponent Create(LiveSplitState state) => new ServerComponent(state);

        public string UpdateName => ComponentName;

        public string UpdateURL => null;

        public Version Version => Version.Parse("1.1.0");

        public string XMLURL => null;
    }
}
