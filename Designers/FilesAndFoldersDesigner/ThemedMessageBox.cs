using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FireworksFramework.Services;

namespace FilesAndFoldersDesigner
{
    // A message box that has the host's theme (light or dark): the system's MessageBox can't be themed. It is shown through
    // DesignerDialogs, which themes it and keeps it on top of the host's window. Its parts are laid out to fit their text
    // and the screen's scaling, rather than placed at fixed sizes.
    internal static class ThemedMessageBox
    {
        public static DialogResult Show(string message, string caption, MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None)
        {
            using var form = new MessageForm
            {
                Text = caption,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false,
                ShowInTaskbar = false,
                AutoScaleDimensions = new SizeF(96F, 96F),
                AutoScaleMode = AutoScaleMode.Dpi,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(12),
            };

            var layout = new TableLayoutPanel
            {
                ColumnCount = 1,
                RowCount = 2,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Fill,
            };

            // The icon, if there is one, beside the text, which wraps at a width that scales with the screen.
            var content = new FlowLayoutPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = false, Margin = new Padding(0, 0, 0, 12) };
            var picture = SystemIcon(icon);
            if (picture != null)
            {
                content.Controls.Add(new PictureBox { Image = picture.ToBitmap(), SizeMode = PictureBoxSizeMode.AutoSize, Margin = new Padding(0, 0, 12, 0) });
            }

            var dpi = form.DeviceDpi / 96F;
            content.Controls.Add(new Label { Text = message, AutoSize = true, MaximumSize = new Size((int)(420 * dpi), 0), Margin = new Padding(0, 4, 0, 0) });
            layout.Controls.Add(content);

            // The buttons, right-aligned under the text; the first is the default and the last cancels.
            var (labels, results) = Buttons(buttons);
            var bar = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Right, Margin = new Padding(0) };
            for (var i = labels.Length - 1; i >= 0; i--)
            {
                // Added last-first so the first sits leftmost, but tabbed first-to-last: Tab goes OK, then Cancel, and the
                // dialog starts with OK focused, so Enter (which presses the focused button) confirms.
                var button = new Button { Text = labels[i], DialogResult = results[i], AutoSize = true, MinimumSize = new Size((int)(88 * dpi), 0), Margin = new Padding(8, 0, 0, 0), TabIndex = i, TabStop = true };
                bar.Controls.Add(button);
                form.Choices.Insert(0, button);
                ShowFocusWithBorder(button);
                if (i == 0)
                {
                    form.AcceptButton = button;
                }
                if (i == labels.Length - 1)
                {
                    form.CancelButton = button;
                }
            }

            layout.Controls.Add(bar);
            form.Controls.Add(layout);
            // Focus starts on the default button (a button the Enter key presses is the focused one, not the form's default).
            form.ActiveControl = form.AcceptButton as Control;
            return DesignerDialogs.ShowDialog(form);
        }

        // A flat (themed) button has no focus indicator of its own but the dotted rectangle Windows draws once the keyboard
        // has been used, so the focused button would not show it was: the arrow keys seem dead until Tab is pressed. The
        // focused button gets a thick border in the selection color, and gives it back when it loses the focus.
        static void ShowFocusWithBorder(Button button)
        {
            Color original = Color.Empty;
            button.GotFocus += (s, e) =>
            {
                original = button.FlatAppearance.BorderColor;
                button.FlatAppearance.BorderSize = 2;
                button.FlatAppearance.BorderColor = SystemColors.Highlight;
            };
            button.LostFocus += (s, e) =>
            {
                button.FlatAppearance.BorderSize = 1;
                button.FlatAppearance.BorderColor = original;
            };
        }

        static (string[] Labels, DialogResult[] Results) Buttons(MessageBoxButtons buttons) => buttons switch
        {
            MessageBoxButtons.OKCancel => (new[] { "OK", "Cancel" }, new[] { DialogResult.OK, DialogResult.Cancel }),
            MessageBoxButtons.YesNo => (new[] { "Yes", "No" }, new[] { DialogResult.Yes, DialogResult.No }),
            _ => (new[] { "OK" }, new[] { DialogResult.OK }),
        };

        static Icon? SystemIcon(MessageBoxIcon icon) => icon switch
        {
            MessageBoxIcon.Warning => SystemIcons.Warning,
            MessageBoxIcon.Error => SystemIcons.Error,
            MessageBoxIcon.Question => SystemIcons.Question,
            MessageBoxIcon.Information => SystemIcons.Information,
            _ => null,
        };

        // The dialog itself takes the arrow keys, and the focus when it opens. Opened over Visual Studio's window it can
        // come up without a button actually holding the keyboard focus, and then the arrow keys (which move focus between
        // controls only from a focused one) do nothing until Tab is pressed.
        sealed class MessageForm : Form, IMessageFilter
        {
            const int WM_KEYDOWN = 0x0100;

            // The buttons, in tab order.
            public List<Button> Choices { get; } = new List<Button>();

            protected override void OnActivated(System.EventArgs e)
            {
                base.OnActivated(e);
                // Whenever the dialog is activated with no button focused, the default is.
                if (!Choices.Any(b => b.Focused))
                {
                    (AcceptButton as Control)?.Focus();
                }
            }

            [System.Runtime.InteropServices.DllImport("user32.dll")]
            static extern System.IntPtr SendMessage(System.IntPtr window, int message, System.IntPtr wParam, System.IntPtr lParam);

            // Windows hides the focus and accelerator cues until the keyboard is used: asked to show them now, so the
            // button that has the focus shows it from the start, not after a first Tab.
            protected override void OnHandleCreated(System.EventArgs e)
            {
                base.OnHandleCreated(e);
                const int WM_CHANGEUISTATE = 0x0127, UIS_CLEAR = 2, UISF_HIDEFOCUS = 1, UISF_HIDEACCEL = 2;
                SendMessage(Handle, WM_CHANGEUISTATE, (System.IntPtr)(UIS_CLEAR | ((UISF_HIDEFOCUS | UISF_HIDEACCEL) << 16)), System.IntPtr.Zero);
            }

            protected override void OnShown(System.EventArgs e)
            {
                base.OnShown(e);
                // For the keys to reach the dialog whichever window holds the keyboard focus (see PreFilterMessage).
                Application.AddMessageFilter(this);
                Activate();
                (AcceptButton as Control)?.Focus();
            }

            protected override void OnFormClosed(FormClosedEventArgs e)
            {
                Application.RemoveMessageFilter(this);
                base.OnFormClosed(e);
            }

            // Keys the dialog takes while it is the active window, wherever the keyboard focus is: the arrow keys move
            // between the buttons, and Enter and Escape act as the default and cancel buttons if no button has the focus to
            // do it (with one focused, the keys are left to it).
            public bool PreFilterMessage(ref Message m)
            {
                if (m.Msg != WM_KEYDOWN || !Visible || ActiveForm != this)
                {
                    return false;
                }

                return HandleKey((Keys)(int)m.WParam);
            }

            protected override bool ProcessCmdKey(ref Message msg, Keys keyData) => HandleKey(keyData) || base.ProcessCmdKey(ref msg, keyData);

            bool HandleKey(Keys key)
            {
                var focused = Choices.FirstOrDefault(b => b.Focused);
                if ((key == Keys.Right || key == Keys.Down || key == Keys.Left || key == Keys.Up) && Choices.Count > 1)
                {
                    // From the focused button, or from the default one while none is.
                    var current = focused ?? AcceptButton as Button ?? Choices[0];
                    var index = Choices.IndexOf(current) + (key == Keys.Right || key == Keys.Down ? 1 : -1);
                    Choices[(index + Choices.Count) % Choices.Count].Focus();
                    return true;
                }

                if (focused == null && key == Keys.Enter && AcceptButton is { } accept)
                {
                    accept.PerformClick();
                    return true;
                }

                if (focused == null && key == Keys.Escape && CancelButton is { } cancel)
                {
                    cancel.PerformClick();
                    return true;
                }

                return false;
            }
        }
    }
}
