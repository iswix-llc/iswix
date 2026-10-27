using System;

namespace FireworksFramework.Services
{
    /// <summary>
    /// Lets a designer say what it is doing, for finding out why something isn't working. The host sets
    /// <see cref="Sink"/> when tracing is on; with none set a call costs a null check, and the message is never built.
    /// </summary>
    public static class DesignerTrace
    {
        /// <summary>Receives a category (the designer, say) and a message; null while tracing is off.</summary>
        public static Action<string, string>? Sink { get; set; }

        /// <summary>Gets whether anything is listening.</summary>
        public static bool Enabled => Sink != null;

        /// <summary>Writes a line, if tracing is on. The message is only built then.</summary>
        public static void Write(string category, Func<string> message)
        {
            var sink = Sink;
            if (sink != null)
            {
                sink(category, message());
            }
        }
    }
}
