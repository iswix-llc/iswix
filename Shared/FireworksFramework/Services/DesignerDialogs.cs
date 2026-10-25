using System;
using System.Windows.Forms;

namespace FireworksFramework.Services
{
    /// <summary>
    /// Shows the dialogs a designer opens with the host's look (Visual Studio's theme colors) and parented to the host's
    /// window, so they stay on top of it. The host sets <see cref="Theme"/> and <see cref="OwnerHandle"/> once.
    /// </summary>
    public static class DesignerDialogs
    {
        /// <summary>Themes a dialog before it's shown.</summary>
        public static Action<Form>? Theme { get; set; }

        /// <summary>The handle of the window a dialog is owned by.</summary>
        public static Func<IntPtr>? OwnerHandle { get; set; }

        /// <summary>Themes and shows <paramref name="dialog"/> modally, owned by the host's window.</summary>
        public static DialogResult ShowDialog(Form dialog)
        {
            Theme?.Invoke(dialog);

            var handle = OwnerHandle?.Invoke() ?? IntPtr.Zero;
            return handle == IntPtr.Zero ? dialog.ShowDialog() : dialog.ShowDialog(new HandleWindow(handle));
        }

        sealed class HandleWindow : IWin32Window
        {
            public HandleWindow(IntPtr handle)
            {
                Handle = handle;
            }

            public IntPtr Handle { get; }
        }
    }
}
