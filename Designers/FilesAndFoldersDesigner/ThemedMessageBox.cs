using System.Drawing;
using System.Windows.Forms;
using FireworksFramework.Services;

namespace FilesAndFoldersDesigner
{
    // A message box that has the host's theme (light or dark): the system's MessageBox can't be themed. It is shown through
    // DesignerDialogs, which themes it and keeps it on top of the host's window.
    internal static class ThemedMessageBox
    {
        public static DialogResult Show(string message, string caption, MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None)
        {
            using var form = new Form
            {
                Text = caption,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false,
                ShowInTaskbar = false,
                AutoScaleMode = AutoScaleMode.Dpi,
            };

            var picture = SystemIcon(icon);
            var left = 12;
            if (picture != null)
            {
                form.Controls.Add(new PictureBox { Image = picture.ToBitmap(), SizeMode = PictureBoxSizeMode.AutoSize, Location = new Point(12, 12) });
                left = 12 + picture.Width + 12;
            }

            var text = new Label { Text = message, AutoSize = true, MaximumSize = new Size(420, 0), Location = new Point(left, 16) };
            form.Controls.Add(text);

            // The buttons, right-aligned under the text; the first is the default and the last cancels.
            var (labels, results) = Buttons(buttons);
            var width = left + 432;
            var top = System.Math.Max(text.PreferredHeight, picture?.Height ?? 0) + 36;
            var x = width - 12;
            for (var i = labels.Length - 1; i >= 0; i--)
            {
                var button = new Button { Text = labels[i], DialogResult = results[i], Size = new Size(88, 28), Location = new Point(x - 88, top) };
                form.Controls.Add(button);
                x -= 88 + 8;
                if (i == 0)
                {
                    form.AcceptButton = button;
                }
                if (i == labels.Length - 1)
                {
                    form.CancelButton = button;
                }
            }

            form.ClientSize = new Size(width, top + 28 + 12);
            return DesignerDialogs.ShowDialog(form);
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
    }
}
