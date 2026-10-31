using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FilesAndFoldersDesigner;
using FireworksFramework.Services;

namespace RegistryDesigner
{
    /// <summary>The Permissions dialog of a registry key or value: its list of permissions, and one to edit at a time.</summary>
    internal static class PermissionsDialog
    {
        // The plain Permission (the LockPermissions table) is not offered: it has behavior that surprises, so permissions are
        // the Util extension's PermissionEx (a user and rights) or a security descriptor. A file's Permission opens as the Util one.
        static readonly string[] KindNames = { "PermissionEx (Util extension)", "PermissionEx (security descriptor, SDDL)" };

        static int IndexOf(PermissionKind kind) => kind == PermissionKind.Sddl ? 1 : 0;

        static PermissionKind KindAt(int index) => index == 1 ? PermissionKind.Sddl : PermissionKind.UtilPermissionEx;

        // A list entry: the permission and how the list shows it.
        sealed class Entry
        {
            public Entry(PermissionSpec spec)
            {
                Spec = spec;
            }

            public PermissionSpec Spec { get; }

            public override string ToString()
            {
                if (Spec.Kind == PermissionKind.Sddl)
                {
                    var text = string.IsNullOrWhiteSpace(Spec.Sddl) ? "(no descriptor yet)" : Spec.Sddl!;
                    return "SDDL: " + (text.Length > 40 ? text.Substring(0, 37) + "..." : text);
                }

                var who = string.IsNullOrWhiteSpace(Spec.User) ? "(no user yet)" : string.IsNullOrWhiteSpace(Spec.Domain) ? Spec.User! : Spec.Domain + "\\" + Spec.User;
                return who;
            }
        }

        static PermissionSpec Clone(PermissionSpec spec)
        {
            var copy = new PermissionSpec { Kind = spec.Kind == PermissionKind.Permission ? PermissionKind.UtilPermissionEx : spec.Kind, User = spec.User, Domain = spec.Domain, Inheritable = spec.Inheritable, Sddl = spec.Sddl, Condition = spec.Condition };
            foreach (var right in spec.Rights)
            {
                copy.Rights.Add(right);
            }

            return copy;
        }

        /// <summary>Edits a list of permissions. Null if the user cancels; the new list, which may be empty, if they accept.</summary>
        public static List<PermissionSpec>? Edit(string title, IReadOnlyList<PermissionSpec> current)
        {
            using var form = new Form
            {
                Text = title,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false,
                ShowInTaskbar = false,
                AutoScaleDimensions = new SizeF(96F, 96F),
                AutoScaleMode = AutoScaleMode.Dpi,
                ClientSize = new Size(740, 470),
                Padding = new Padding(12),
            };

            var entries = new ListBox { Dock = DockStyle.Fill, IntegralHeight = false };
            foreach (var spec in current)
            {
                entries.Items.Add(new Entry(Clone(spec)));
            }

            var kind = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
            kind.Items.AddRange(KindNames.Cast<object>().ToArray());
            var user = new TextBox { Dock = DockStyle.Fill };
            var domain = new TextBox { Dock = DockStyle.Fill };
            var inheritable = new CheckBox { Text = "Inherited by subkeys", AutoSize = true, ThreeState = true };
            var sddl = new TextBox { Dock = DockStyle.Fill, Multiline = true, Height = 70, ScrollBars = ScrollBars.Vertical };
            var condition = new TextBox { Dock = DockStyle.Fill };
            var rights = new CheckedListBox { Dock = DockStyle.Fill, CheckOnClick = true, IntegralHeight = false, Height = 200 };
            rights.Items.AddRange(RegistryDocument.Rights.Cast<object>().ToArray());

            var editor = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 0, Padding = new Padding(12, 0, 0, 0) };
            editor.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            var labels = new Dictionary<Control, Label>();
            void Row(string text, Control control, bool grow = false)
            {
                var label = new Label { Text = text, AutoSize = true, Anchor = AnchorStyles.Left | AnchorStyles.Top, Margin = new Padding(0, 6, 8, 3) };
                labels[control] = label;
                editor.RowCount++;
                editor.RowStyles.Add(grow ? new RowStyle(SizeType.Percent, 100F) : new RowStyle(SizeType.AutoSize));
                control.Margin = new Padding(0, 3, 0, 3);
                editor.Controls.Add(label, 0, editor.RowCount - 1);
                editor.Controls.Add(control, 1, editor.RowCount - 1);
            }

            Row("Kind:", kind);
            Row("User:", user);
            Row("Domain:", domain);
            Row("Inheritance:", inheritable);
            Row("SDDL:", sddl);
            Row("Condition:", condition);
            Row("Rights:", rights, grow: true);

            var loading = false;
            Entry? Selected() => entries.SelectedItem as Entry;

            // Shows what the selected kind has.
            void ShowKind(PermissionKind k)
            {
                var sddlKind = k == PermissionKind.Sddl;
                foreach (var control in new Control[] { user, domain, rights })
                {
                    control.Visible = !sddlKind;
                    labels[control].Visible = !sddlKind;
                }

                inheritable.Visible = k == PermissionKind.UtilPermissionEx;
                labels[inheritable].Visible = inheritable.Visible;
                foreach (var control in new Control[] { sddl, condition })
                {
                    control.Visible = sddlKind;
                    labels[control].Visible = sddlKind;
                }
            }

            void Load()
            {
                loading = true;
                try
                {
                    var entry = Selected();
                    // The editor is there only while there is a permission to edit.
                    editor.Visible = entry != null;
                    if (entry == null)
                    {
                        return;
                    }

                    var spec = entry.Spec;
                    kind.SelectedIndex = IndexOf(spec.Kind);
                    user.Text = spec.User ?? string.Empty;
                    domain.Text = spec.Domain ?? string.Empty;
                    inheritable.CheckState = spec.Inheritable == null ? CheckState.Indeterminate : spec.Inheritable.Value ? CheckState.Checked : CheckState.Unchecked;
                    sddl.Text = spec.Sddl ?? string.Empty;
                    condition.Text = spec.Condition ?? string.Empty;
                    for (var i = 0; i < rights.Items.Count; i++)
                    {
                        rights.SetItemChecked(i, spec.Rights.Contains((string)rights.Items[i]));
                    }

                    ShowKind(spec.Kind);
                }
                finally
                {
                    loading = false;
                }
            }

            // The list shows the entry again after a change to it.
            void Refresh(Entry entry)
            {
                var index = entries.Items.IndexOf(entry);
                if (index >= 0)
                {
                    loading = true;
                    entries.Items[index] = entry;
                    loading = false;
                }
            }

            entries.SelectedIndexChanged += (s, e) =>
            {
                if (!loading)
                {
                    Load();
                }
            };
            kind.SelectedIndexChanged += (s, e) =>
            {
                if (!loading && Selected() is { } entry)
                {
                    entry.Spec.Kind = KindAt(kind.SelectedIndex);
                    ShowKind(entry.Spec.Kind);
                    Refresh(entry);
                }
            };
            user.TextChanged += (s, e) => { if (!loading && Selected() is { } entry) { entry.Spec.User = user.Text; Refresh(entry); } };
            domain.TextChanged += (s, e) => { if (!loading && Selected() is { } entry) { entry.Spec.Domain = domain.Text; Refresh(entry); } };
            sddl.TextChanged += (s, e) => { if (!loading && Selected() is { } entry) { entry.Spec.Sddl = sddl.Text; Refresh(entry); } };
            condition.TextChanged += (s, e) => { if (!loading && Selected() is { } entry) { entry.Spec.Condition = condition.Text; } };
            inheritable.CheckStateChanged += (s, e) =>
            {
                if (!loading && Selected() is { } entry)
                {
                    entry.Spec.Inheritable = inheritable.CheckState == CheckState.Indeterminate ? (bool?)null : inheritable.CheckState == CheckState.Checked;
                }
            };
            rights.ItemCheck += (s, e) =>
            {
                if (!loading && Selected() is { } entry)
                {
                    var right = (string)rights.Items[e.Index];
                    if (e.NewValue == CheckState.Checked)
                    {
                        entry.Spec.Rights.Add(right);
                    }
                    else
                    {
                        entry.Spec.Rights.Remove(right);
                    }
                }
            };

            var add = new Button { Text = "Add", AutoSize = true };
            add.Click += (s, e) =>
            {
                var entry = new Entry(new PermissionSpec { Kind = PermissionKind.UtilPermissionEx });
                entries.Items.Add(entry);
                entries.SelectedItem = entry;
                user.Focus();
            };
            var remove = new Button { Text = "Remove", AutoSize = true };
            remove.Click += (s, e) =>
            {
                var index = entries.SelectedIndex;
                if (index < 0)
                {
                    return;
                }

                entries.Items.RemoveAt(index);
                if (entries.Items.Count > 0)
                {
                    entries.SelectedIndex = Math.Min(index, entries.Items.Count - 1);
                }
                else
                {
                    Load();
                }
            };
            var listButtons = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };
            listButtons.Controls.Add(add);
            listButtons.Controls.Add(remove);
            var left = new Panel { Dock = DockStyle.Fill };
            left.Controls.Add(entries);
            left.Controls.Add(listButtons);

            var ok = new Button { Text = "OK", AutoSize = true, MinimumSize = new Size(88, 0) };
            var cancel = new Button { Text = "Cancel", AutoSize = true, DialogResult = DialogResult.Cancel, MinimumSize = new Size(88, 0), Margin = new Padding(8, 0, 0, 0) };
            ok.Click += (s, e) =>
            {
                // Each permission needs what makes it one: a user, or a descriptor.
                foreach (Entry entry in entries.Items)
                {
                    var missing = entry.Spec.Kind == PermissionKind.Sddl ? string.IsNullOrWhiteSpace(entry.Spec.Sddl) : string.IsNullOrWhiteSpace(entry.Spec.User);
                    if (missing)
                    {
                        entries.SelectedItem = entry;
                        ThemedMessageBox.Show(entry.Spec.Kind == PermissionKind.Sddl ? "A security descriptor is required." : "A user is required.", "Permissions", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                form.DialogResult = DialogResult.OK;
            };
            var bar = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };
            bar.Controls.Add(cancel);
            bar.Controls.Add(ok);
            form.AcceptButton = ok;
            form.CancelButton = cancel;

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 240F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.Controls.Add(left, 0, 0);
            layout.Controls.Add(editor, 1, 0);
            layout.Controls.Add(bar, 0, 1);
            layout.SetColumnSpan(bar, 2);
            form.Controls.Add(layout);

            if (entries.Items.Count > 0)
            {
                entries.SelectedIndex = 0;
            }
            else
            {
                Load();
            }

            if (DesignerDialogs.ShowDialog(form) != DialogResult.OK)
            {
                return null;
            }

            return entries.Items.Cast<Entry>().Select(entry => entry.Spec).ToList();
        }
    }
}
