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

        public LiveSplitWebSocketBehavior(LiveSplitState state, ITimerModel model)
        {
            this.state = state;
            this.model = model;
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
                    model.Split();
                    break;
                case "unsplit":
                    model.UndoSplit();
                    break;
                case "skipsplit":
                    model.SkipSplit();
                    break;
                case "pause":
                    if (state.CurrentPhase != TimerPhase.Paused)
                    {
                        model.Pause();
                    }
                    break;
                case "resume":
                    if (state.CurrentPhase == TimerPhase.Paused)
                    {
                        model.Pause();
                    }
                    break;
                case "reset":
                    model.Reset();
                    break;
                case "starttimer":
                    model.Start();
                    break;
                case "pausegametime":
                    state.IsGameTimePaused = true;
                    break;
                case "unpausegametime":
                    state.IsGameTimePaused = false;
                    break;
            }
        }
    }
}
