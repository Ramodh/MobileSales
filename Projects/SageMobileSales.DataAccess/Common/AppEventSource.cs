using System;
using System.Diagnostics.Tracing;

namespace SageMobileSales.DataAccess.Common
{
    public sealed class AppEventSource : EventSource
    {
        public static AppEventSource Log = new AppEventSource();

        [Event(1, Level = EventLevel.Verbose)]
        public void Debug(string message)
        {
            WriteEvent(1, message);
        }

        [Event(2, Level = EventLevel.Informational)]
        public void Info(string message)
        {
            WriteEvent(2, message);
        }

        [Event(3, Level = EventLevel.Warning)]
        public void Warn(string message)
        {
            WriteEvent(3, message);
        }

        [Event(4, Level = EventLevel.Error)]
        public void Error(string message)
        {
            WriteEvent(4, message);
        }

        [Event(5, Level = EventLevel.Critical)]
        public void Critical(string message)
        {
            WriteEvent(5, message);
        }

        public string WriteLine(Exception e)
        {
            var s =
                WriteLineExcep("EXCEPTION :{0} {1} STACK TRACE: {2}", e.Message,
                    e.InnerException != null ? " HAS INNER EXCEPTION" : "", e.StackTrace);


            if (e.InnerException != null)
            {
                WriteLine(e.InnerException);
            }
            return s;
        }

        private string WriteLineExcep(string format, params object[] args)
        {
            var s = string.Format(format, args);

            return s;
        }
    }
}