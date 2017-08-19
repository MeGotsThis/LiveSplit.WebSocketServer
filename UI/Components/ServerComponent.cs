using LiveSplit.Model;
using LiveSplit.Options;
using LiveSplit.Web;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using WebSocketSharp.Server;

namespace LiveSplit.UI.Components
{
    public class ServerComponent : IComponent
    {
        public Settings Settings { get; set; }
        public WebSocketServer Server { get; set; }

        protected System.Timers.Timer Timer { get; set; }

        protected LiveSplitState State { get; set; }
        protected Form Form { get; set; }
        protected TimerModel Model { get; set; }

        public float PaddingTop => 0;
        public float PaddingBottom => 0;
        public float PaddingLeft => 0;
        public float PaddingRight => 0;

        public string ComponentName => $"LiveSplit WebSocket Server ({ Settings.Port })";

        public IDictionary<string, Action> ContextMenuControls { get; protected set; }

        public ServerComponent(LiveSplitState state)
        {
            Settings = new Settings();
            Model = new TimerModel();

            ContextMenuControls = new Dictionary<string, Action>();
            ContextMenuControls.Add("Start WebSocket Server", Start);

            State = state;
            Form = state.Form;

            Model.CurrentState = State;

            State.OnSplit += State_OnSplit;
            State.OnUndoSplit += State_OnUndoSplit;
            State.OnSkipSplit += State_OnSkipSplit;
            State.OnStart += State_OnStart;
            State.OnReset += State_OnReset;
            State.OnPause += State_OnPause;
            State.OnUndoAllPauses += State_OnUndoAllPauses;
            State.OnResume += State_OnResume;
            State.OnScrollUp += State_OnScrollUp;
            State.OnScrollDown += State_OnScrollDown;
            State.OnSwitchComparisonPrevious += State_OnSwitchComparisonPrevious;
            State.OnSwitchComparisonNext += State_OnSwitchComparisonNext;
            State.RunManuallyModified += State_RunManuallyModified;
            State.ComparisonRenamed += State_ComparisonRenamed;
        }

        public void Start()
        {
            CloseAllConnections();

            Server = new WebSocketServer(Settings.Port);
            Server.AddWebSocketService<LiveSplitWebSocketBehavior>("/", () => new LiveSplitWebSocketBehavior(State, Model));
            Server.Start();

            Timer = new System.Timers.Timer(15000);
            Timer.AutoReset = true;
            Timer.Elapsed += Timer_Elapsed;
            Timer.Start();

            ContextMenuControls.Clear();
            ContextMenuControls.Add("Stop WebSocket Server", Stop);
        }

        public void Stop()
        {
            CloseAllConnections();
            ContextMenuControls.Clear();
            ContextMenuControls.Add("Start WebSocket Server", Start);
        }

        protected void CloseAllConnections()
        {
            if (Server != null)
            {
                Server.Stop();
                Server = null;
            }
            if (Timer != null)
            {
                Timer.Stop();
                Timer = null;
            }
        }

        public void DrawVertical(Graphics g, LiveSplitState state, float width, Region clipRegion)
        {
        }

        public void DrawHorizontal(Graphics g, LiveSplitState state, float height, Region clipRegion)
        {
        }

        public float VerticalHeight => 0;

        public float MinimumWidth => 0;

        public float HorizontalWidth => 0;

        public float MinimumHeight => 0;

        public XmlNode GetSettings(XmlDocument document)
        {
            return Settings.GetSettings(document);
        }

        public Control GetSettingsControl(LayoutMode mode)
        {
            return Settings;
        }

        public void SetSettings(XmlNode settings)
        {
            Settings.SetSettings(settings);
            if (Server == null && Settings.AutoStart)
            {
                var worker = new BackgroundWorker();
                worker.DoWork += delegate
                {
                    Thread.Sleep(500);
                    Start();
                };
                worker.RunWorkerAsync();
            }
        }

        public void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
        }

        private void SendState(string action, object data)
        {
            if (Server != null)
            {
                dynamic jsonData = new DynamicJsonObject();
                jsonData.action = new DynamicJsonObject();
                jsonData.action.action = action;
                jsonData.action.data = data;
                jsonData.state = JsonState.Create(State);
                Server.WebSocketServices["/"].Sessions.Broadcast(jsonData.ToString());
            }
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            SendState("refresh", null);
        }

        private void State_OnSplit(object sender, EventArgs e)
        {
            SendState("split", null);
        }

        private void State_OnUndoSplit(object sender, EventArgs e)
        {
            SendState("undo-split", null);
        }

        private void State_OnSkipSplit(object sender, EventArgs e)
        {
            SendState("skip-split", null);
        }

        private void State_OnStart(object sender, EventArgs e)
        {
            SendState("start", null);
        }

        private void State_OnReset(object sender, TimerPhase value)
        {
            SendState("reset", null);
        }

        private void State_OnPause(object sender, EventArgs e)
        {
            SendState("pause", null);
        }

        private void State_OnUndoAllPauses(object sender, EventArgs e)
        {
            SendState("undo-all-pauses", null);
        }

        private void State_OnResume(object sender, EventArgs e)
        {
            SendState("resume", null);
        }

        private void State_OnScrollUp(object sender, EventArgs e)
        {
            SendState("scroll", "up");
        }

        private void State_OnScrollDown(object sender, EventArgs e)
        {
            SendState("scroll", "down");
        }

        private void State_OnSwitchComparisonPrevious(object sender, EventArgs e)
        {
            SendState("switch-comparison", "previous");
        }

        private void State_OnSwitchComparisonNext(object sender, EventArgs e)
        {
            SendState("switch-comparison", "next");
        }

        private void State_RunManuallyModified(object sender, EventArgs e)
        {
            SendState("run-manually-modified", null);
        }

        private void State_ComparisonRenamed(object sender, EventArgs e)
        {
            SendState("comparison-renamed", null);
        }

        public void Dispose()
        {
            State.OnSplit -= State_OnSplit;
            State.OnUndoSplit -= State_OnUndoSplit;
            State.OnSkipSplit -= State_OnSkipSplit;
            State.OnStart -= State_OnStart;
            State.OnReset -= State_OnReset;
            State.OnPause -= State_OnPause;
            State.OnUndoAllPauses -= State_OnUndoAllPauses;
            State.OnResume -= State_OnResume;
            State.OnScrollUp -= State_OnScrollUp;
            State.OnScrollDown -= State_OnScrollDown;
            State.OnSwitchComparisonPrevious -= State_OnSwitchComparisonPrevious;
            State.OnSwitchComparisonNext -= State_OnSwitchComparisonNext;
            State.RunManuallyModified -= State_RunManuallyModified;
            State.ComparisonRenamed -= State_ComparisonRenamed;

            CloseAllConnections();
        }

        public int GetSettingsHashCode()
        {
            return Settings.GetSettingsHashCode();
        }
    }
}
