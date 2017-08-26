using LiveSplit.Model;
using LiveSplit.Web;
using System;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace LiveSplit.UI.Components
{
    class LiveSplitWebSocketBehavior : WebSocketBehavior
    {
        private LiveSplitState state;
        private ITimerModel model;
        private Settings settings;

        public LiveSplitWebSocketBehavior(LiveSplitState state, ITimerModel model, Settings settings)
        {
            this.state = state;
            this.model = model;
            this.settings = settings;
        }

        protected override void OnOpen()
        {
            dynamic jsonData = new DynamicJsonObject();
            jsonData.open = new DynamicJsonObject();
            jsonData.open.response = "success";
            jsonData.state = JsonState.Create(state);
            Send(jsonData.ToString());
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            dynamic messageData;
            try
            {
                messageData = JSON.FromString(e.Data);
                if (!(messageData is DynamicJsonObject))
                {
                    throw new ArgumentException();
                }
            }
            catch(ArgumentException)
            {
                messageData = new DynamicJsonObject();
                messageData.action = e.Data;
                messageData.data = null;
            }
            dynamic jsonData = new DynamicJsonObject();
            jsonData.response = new DynamicJsonObject();
            jsonData.response.response = messageData.action;
            switch (messageData.action)
            {
                case "hi":
                    Send(jsonData.ToString());
                    break;
                case "state":
                    jsonData.state = JsonState.Create(state);
                    Send(jsonData.ToString());
                    break;
                case "startorsplit":
                    if (settings.ReadOnly)
                    {
                        break;
                    }
                    if (state.CurrentPhase == TimerPhase.Running)
                    {
                        model.Split();
                    }
                    else
                    {
                        model.Start();
                    }
                    break;
                case "split":
                    if (settings.ReadOnly)
                    {
                        break;
                    }
                    model.Split();
                    break;
                case "unsplit":
                    if (settings.ReadOnly)
                    {
                        break;
                    }
                    model.UndoSplit();
                    break;
                case "skipsplit":
                    if (settings.ReadOnly)
                    {
                        break;
                    }
                    model.SkipSplit();
                    break;
                case "pause":
                    if (settings.ReadOnly)
                    {
                        break;
                    }
                    if (state.CurrentPhase != TimerPhase.Paused)
                    {
                        model.Pause();
                    }
                    break;
                case "resume":
                    if (settings.ReadOnly)
                    {
                        break;
                    }
                    if (state.CurrentPhase == TimerPhase.Paused)
                    {
                        model.Pause();
                    }
                    break;
                case "reset":
                    if (settings.ReadOnly)
                    {
                        break;
                    }
                    model.Reset();
                    break;
                case "starttimer":
                    if (settings.ReadOnly)
                    {
                        break;
                    }
                    model.Start();
                    break;
                case "pausegametime":
                    if (settings.ReadOnly)
                    {
                        break;
                    }
                    state.IsGameTimePaused = true;
                    break;
                case "unpausegametime":
                    if (settings.ReadOnly)
                    {
                        break;
                    }
                    state.IsGameTimePaused = false;
                    break;
            }
        }
    }
}
