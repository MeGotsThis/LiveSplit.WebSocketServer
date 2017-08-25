using LiveSplit.Model;
using LiveSplit.Web;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.UI.Components
{
    class JsonState
    {
        public static dynamic Create(LiveSplitState state)
        {
            dynamic json = new DynamicJsonObject();
            json.run = new DynamicJsonObject();
            json.run.gameIcon = Base64EncodePngImage(state.Run.GameIcon);
            json.run.gameName = state.Run.GameName;
            json.run.categoryName = state.Run.CategoryName;
            json.run.startingOffset = ConvertTimeSpanToJson(state.Run.Offset);
            json.run.attemptCount = state.Run.AttemptCount;
            json.run.finishedCount = state.Run.AttemptHistory.Where(x => x.Time.RealTime != null).Count();
            json.run.comparisons = state.Run.Comparisons;
            json.run.advancedSumOfBest = ConvertTimeSpanToJson(SumOfBest.CalculateSumOfBest(state.Run, false, true, state.CurrentTimingMethod));
            json.run.totalPlaytime = ConvertTimeSpanToJson(CalculateTotalPlaytime(state));
            json.run.segments = new DynamicJsonObject[state.Run.Count];
            json.timerState = state.CurrentPhase.ToString();
            json.currentComparison = state.CurrentComparison;
            json.currentTimingMethod = state.CurrentTimingMethod.ToString();
            json.currentTime = ConvertTimeToJson(state.CurrentTime);
            json.loadingTimes = ConvertTimeSpanToJson(state.LoadingTimes);
            json.isGameTimeInitialized = state.IsGameTimeInitialized;
            json.isGameTimePaused = state.IsGameTimePaused;
            json.currentTime = ConvertTimeToJson(state.CurrentTime);
            json.attemptStarted = ConvertAtomicDateTimeToJson(state.AttemptStarted);
            json.attemptEnded = ConvertAtomicDateTimeToJson(state.AttemptEnded);
            json.pauseTime = ConvertTimeSpanToJson(state.PauseTime);
            json.currentAttemptDuration = ConvertTimeSpanToJson(state.CurrentAttemptDuration);
            json.currentSplitIndex = state.CurrentSplitIndex;
            for (var i = 0; i < state.Run.Count; i++)
            {
                var segment = state.Run[i];
                dynamic segmentJson = new DynamicJsonObject();
                json.run.segments[i] = segmentJson;
                segmentJson.icon = Base64EncodePngImage(segment.Icon);
                segmentJson.name = segment.Name;
                segmentJson.splitTime = ConvertTimeToJson(segment.SplitTime);
                segmentJson.personalBest = ConvertTimeToJson(segment.PersonalBestSplitTime);
                segmentJson.bestSegment = ConvertTimeToJson(segment.BestSegmentTime);
                var comparisons = new Dictionary<string, dynamic>();
                foreach (var item in segment.Comparisons)
                {
                    comparisons[item.Key] = ConvertTimeToJson(item.Value);
                }
                segmentJson.comparisons = comparisons;
            }
            return json;
        }

        public static string Base64EncodePngImage(Image image)
        {
            if (image == null)
            {
                return null;
            }
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                return "data:image/png;base64," + Convert.ToBase64String(ms.ToArray());
            }
        }

        public static long? ConvertTimeSpanToJson(TimeSpan? time)
        {
            if (!time.HasValue)
            {
                return null;
            }
            return ConvertTimeSpanToJson(time.Value);
        }

        public static long ConvertTimeSpanToJson(TimeSpan time)
        {
            return (long)time.TotalMilliseconds;
        }

        public static long? ConvertTimeToJson(Time? time)
        {
            if (!time.HasValue)
            {
                return null;
            }
            return ConvertTimeToJson(time.Value);
        }

        public static string ConvertAtomicDateTimeToJson(AtomicDateTime datetime)
        {
            return datetime.Time.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        }

        public static dynamic ConvertTimeToJson(Time time)
        {
            dynamic json = new DynamicJsonObject();
            json.realTime = (long?) time.RealTime?.TotalMilliseconds;
            json.gameTime = (long?) time.GameTime?.TotalMilliseconds;
            return json;
        }

        public static TimeSpan CalculateTotalPlaytime(LiveSplitState state)
        {
            var totalPlaytime = TimeSpan.Zero;

            foreach (var attempt in state.Run.AttemptHistory)
            {
                var duration = attempt.Duration;

                if (duration.HasValue)
                {
                    //Either >= 1.6.0 or a finished run
                    totalPlaytime += duration.Value - (attempt.PauseTime ?? TimeSpan.Zero);
                }
                else
                {
                    //Must be < 1.6.0 and a reset
                    //Calculate the sum of the segments for that run

                    foreach (var segment in state.Run)
                    {
                        Time segmentHistoryElement;
                        if (segment.SegmentHistory.TryGetValue(attempt.Index, out segmentHistoryElement) && segmentHistoryElement.RealTime.HasValue)
                            totalPlaytime += segmentHistoryElement.RealTime.Value;
                    }
                }
            }

            return totalPlaytime;
        }
    }
}
