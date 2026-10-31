using System;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;

namespace IsWiX.Designer
{
    /// <summary>How the Design and XML views share the editor.</summary>
    internal enum LayoutMode
    {
        /// <summary>Design above XML.</summary>
        Horizontal,

        /// <summary>Design and XML side by side.</summary>
        Vertical,

        /// <summary>Only the Design view shows.</summary>
        DesignOnly,

        /// <summary>Only the XML view shows.</summary>
        XmlOnly,
    }

    /// <summary>
    /// The layout the user chose, kept in Visual Studio's user settings so every .wxs editor opens the way the last one was left.
    /// What is floating is not kept: a view that was floating opens docked.
    /// </summary>
    internal sealed class DesignerLayoutSettings
    {
        const string Collection = "IsWiX\\DesignerLayout";

        // The Design view gets most of a split to begin with; the XML view is for checking. (Saved under a new name, so an earlier even split is not carried over.)
        const double DefaultShare = 0.75;

        /// <summary>The layout now.</summary>
        public LayoutMode Mode { get; set; } = LayoutMode.Horizontal;

        /// <summary>The split the layout returns to after a view was shown alone (Horizontal or Vertical).</summary>
        public LayoutMode Split { get; set; } = LayoutMode.Horizontal;

        /// <summary>The share of a split the Design view has, 0.1 to 0.9.</summary>
        public double Ratio { get; set; } = DefaultShare;

        public static DesignerLayoutSettings Load(System.IServiceProvider provider)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var settings = new DesignerLayoutSettings();
            try
            {
                var store = new ShellSettingsManager(provider).GetReadOnlySettingsStore(SettingsScope.UserSettings);
                if (store.CollectionExists(Collection))
                {
                    settings.Split = ParseSplit(Read(store, "Split"));
                    settings.Mode = Enum.TryParse<LayoutMode>(Read(store, "Mode"), out var mode) ? mode : settings.Split;
                    if (double.TryParse(Read(store, "DesignShare"), NumberStyles.Float, CultureInfo.InvariantCulture, out var ratio))
                    {
                        settings.Ratio = Math.Min(0.9, Math.Max(0.1, ratio));
                    }
                }
            }
            catch (Exception)
            {
                // The settings are a convenience; the defaults are fine.
            }

            return settings;
        }

        public void Save(System.IServiceProvider provider)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                var store = new ShellSettingsManager(provider).GetWritableSettingsStore(SettingsScope.UserSettings);
                if (!store.CollectionExists(Collection))
                {
                    store.CreateCollection(Collection);
                }

                store.SetString(Collection, "Mode", Mode.ToString());
                store.SetString(Collection, "Split", Split.ToString());
                store.SetString(Collection, "DesignShare", Ratio.ToString("0.####", CultureInfo.InvariantCulture));
            }
            catch (Exception)
            {
                // Not being able to remember the layout is not worth an error.
            }
        }

        static string? Read(SettingsStore store, string name) => store.PropertyExists(Collection, name) ? store.GetString(Collection, name) : null;

        static LayoutMode ParseSplit(string? text) => text == nameof(LayoutMode.Vertical) ? LayoutMode.Vertical : LayoutMode.Horizontal;
    }

    /// <summary>
    /// The tool window a view floats in. It holds the Design or the XML view while that view is undocked; closing it
    /// docks the view back into the editor. Commands the window gets go to the editor, which knows what has the focus.
    /// </summary>
    [Guid("0d1b7c32-6a4e-4f0a-9d53-3d0e7a2f6b11")]
    internal sealed class FloatingViewWindow : ToolWindowPane, IOleCommandTarget
    {
        public FloatingViewWindow()
            : base(null)
        {
        }

        /// <summary>The editor whose view this window holds.</summary>
        public IOleCommandTarget? Owner { get; set; }

        /// <summary>Raised as the window closes, before its content is taken down: the editor takes its view back then.</summary>
        public event EventHandler? Closing;

        protected override void OnClose()
        {
            Closing?.Invoke(this, EventArgs.Empty);
            base.OnClose();
        }

        int IOleCommandTarget.QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return Owner != null
                ? Owner.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText)
                : (int)Microsoft.VisualStudio.OLE.Interop.Constants.OLECMDERR_E_NOTSUPPORTED;
        }

        int IOleCommandTarget.Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return Owner != null
                ? Owner.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut)
                : (int)Microsoft.VisualStudio.OLE.Interop.Constants.OLECMDERR_E_NOTSUPPORTED;
        }
    }
}
