using System;
using System.Diagnostics;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace IsWiX.Designer
{
    /// <summary>
    /// A trace of what the designer window and the designers are doing, in Visual Studio's Output window (the "IsWiX"
    /// pane) and to the debugger, for finding out why something isn't working. It is off unless the environment variable
    /// ISWIX_TRACE is 1 when Visual Studio starts; off, a call costs a check and the message is never built.
    /// </summary>
    internal static class Tracing
    {
        // The Output window's pane for these lines.
        static readonly Guid PaneGuid = new Guid("0b0b8f3b-6c1d-4a6b-9c0e-5d3f1f6a7e21");

        static IVsOutputWindowPane? _pane;

        /// <summary>Gets whether the trace is on.</summary>
        public static bool Enabled { get; } = Environment.GetEnvironmentVariable("ISWIX_TRACE") == "1";

        /// <summary>Writes a line to the trace if it is on. The message is only built then. Any thread.</summary>
        public static void Write(string category, Func<string> message)
        {
            if (!Enabled)
            {
                return;
            }

            var line = $"{DateTime.Now:HH:mm:ss.fff} [{category}] {message()}";
            Debug.WriteLine("IsWiX " + line);
            if (ThreadHelper.CheckAccess())
            {
                Output(line);
            }
            else
            {
                // From another thread (the scan of the project's files): written on the main thread when it is idle.
                ThreadHelper.JoinableTaskFactory.StartOnIdle(() => Output(line)).FileAndForget("iswix/trace");
            }
        }

        /// <summary>Writes a line to the trace if it is on.</summary>
        public static void Write(string category, string message) => Write(category, () => message);

        // The Output window's pane is made the first time a line is written. Only called on the main thread, which getting
        // the window needs; Write is what makes sure of that, so a caller isn't asked to be on it.
#pragma warning disable VSTHRD010
        static void Output(string line)
        {
            try
            {
                if (_pane is null && Package.GetGlobalService(typeof(SVsOutputWindow)) is IVsOutputWindow window)
                {
                    var guid = PaneGuid;
                    window.CreatePane(ref guid, "IsWiX", 1, 0);
                    window.GetPane(ref guid, out _pane);
                    _pane?.OutputStringThreadSafe("IsWiX trace is on (ISWIX_TRACE=1)." + Environment.NewLine);
                }

                _pane?.OutputStringThreadSafe(line + Environment.NewLine);
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                // The Output window is gone (Visual Studio is closing): there is nowhere to write to.
            }
        }
#pragma warning restore VSTHRD010
    }
}
