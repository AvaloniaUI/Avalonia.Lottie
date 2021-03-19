﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Avalonia.Lottie
{
    public static class LottieLog
    {
        internal const string Tag = "LOTTIE";

        private const int MaxDepth = 100;

        /// <summary>
        ///     Set to ensure that we only log each message one time max.
        /// </summary>
        private static readonly List<string> _loggedMessages = new();

        private static bool _traceEnabled;
        private static bool _shouldResetTrace;
        private static string[] _sections;
        private static long[] _startTimeNs;
        private static int _traceDepth;
        private static int _depthPastMaxDepth;

        private static readonly Queue<string> Msgs = new();

        private static readonly DateTime Epoc = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static readonly StringBuilder Sb = new();

        public static bool TraceEnabled
        {
            set
            {
                if (_traceEnabled == value) return;
                if (value)
                {
                    _shouldResetTrace = true;
                    TryResetTrace();
                }
                else
                {
                    _traceEnabled = false;
                    _shouldResetTrace = false;
                }
            }
            get => _traceEnabled;
        }

        /// <summary>
        ///     Warn to Debug. Keeps track of messages so they are only logged once ever.
        /// </summary>
        /// <param name="msg"></param>
        [Conditional("DEBUG")]
        public static void Warn(string msg)
        {
            if (_loggedMessages.Contains(msg)) return;
            Debug.WriteLine(msg, Tag);
            _loggedMessages.Add(msg);
        }

        [Conditional("DEBUG")]
        internal static void BeginSection(string section)
        {
            TryResetTrace();

            if (_traceDepth == MaxDepth)
            {
                _depthPastMaxDepth++;
                return;
            }

            _traceDepth++;

            if (!_traceEnabled) return;

            _sections[_traceDepth - 1] = section;
            _startTimeNs[_traceDepth - 1] = CurrentUnixTime();
            BatchedDebugWriteLine($"Begin Section: {section}");
        }

        internal static float EndSection(string section)
        {
#if DEBUG
            if (_depthPastMaxDepth > 0)
            {
                _depthPastMaxDepth--;
                return 0;
            }

            _traceDepth--;

            if (!_traceEnabled) return 0;

            if (_traceDepth == -1) throw new InvalidOperationException("Can't end trace section. There are none.");
            if (!section.Equals(_sections[_traceDepth]))
                throw new InvalidOperationException("Unbalanced trace call " + section + ". Expected " +
                                                    _sections[_traceDepth] + ".");
            var duration = (CurrentUnixTime() - _startTimeNs[_traceDepth]) / 1000000f;
            BatchedDebugWriteLine($"End Section - {duration}ms");
            return duration;
#endif
            return 0f;
        }

        [Conditional("DEBUG")]
        private static void TryResetTrace()
        {
            if (_shouldResetTrace && _traceDepth == 0)
            {
                _traceEnabled = true;
                _shouldResetTrace = false;

                _sections = new string[MaxDepth];
                _startTimeNs = new long[MaxDepth];

                _depthPastMaxDepth = 0;
            }
        }

        [Conditional("DEBUG")]
        private static void BatchedDebugWriteLine(string message)
        {
            Msgs.Enqueue($"{new string(' ', _traceDepth)}{message}");
            if (_traceDepth == 0 && Msgs.Count >= MaxDepth)
            {
                Sb.Clear();
                while (Msgs.Count > 0) Sb.AppendLine(Msgs.Dequeue());
                Debug.WriteLine(Sb.ToString(), Tag);
            }
        }

        private static long CurrentUnixTime()
        {
            return (long) (DateTime.UtcNow - Epoc).TotalMilliseconds;
        }
    }
}