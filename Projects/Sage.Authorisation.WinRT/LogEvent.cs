using System;

namespace Sage.Authorisation.WinRT
{
    /// <summary>
    ///     Contains information about the log event, specifically a
    ///     <see cref="Sage.Authorisation.WinRT.LogEventType">event type</see>,
    ///     a <see cref="Sage.Authorisation.WinRT.LogLevel">log level</see> and a
    ///     <see cref="Sage.Authorisation.WinRT.LogEvent.Message">message.</see>
    /// </summary>
    public sealed class LogEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="LogEvent" /> class.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="event">The event.</param>
        /// <param name="message">The message.</param>
        /// <exception cref="System.ArgumentException">Message cannot be null or empty.;message</exception>
        internal LogEvent(LogLevel level, LogEventType @event, string message)
        {
            #region Input validation

            if (String.IsNullOrEmpty(message))
            {
                throw new ArgumentException("Message cannot be null or empty.", "message");
            }

            #endregion

            Message = message;
            Event = @event;
            Level = level;
        }

        /// <summary>
        ///     The situation, operation or state that occured to raise the log event
        /// </summary>
        public LogEventType Event { get; private set; }

        /// <summary>
        ///     This is the log level of the event that occured, e.g. Diagnostic
        /// </summary>
        public LogLevel Level { get; private set; }

        /// <summary>
        ///     This is a text description or message relating to the event that occured
        /// </summary>
        public string Message { get; private set; }
    }
}