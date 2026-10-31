using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using FilesAndFoldersDesigner;
using FireworksFramework.Services;

namespace RegistryDesigner
{
    internal enum EntryMode { NewValue, EditValue, NewRemoval }

    /// <summary>What the user entered in the entry dialog.</summary>
    internal sealed class EntryResult
    {
        public RegistryValueSpec Spec { get; set; } = new RegistryValueSpec();

        /// <summary>For a removal: a key (true) or a value (false).</summary>
        public bool RemoveKey { get; set; }

        /// <summary>For a key to remove: RemoveOnInstall or RemoveOnUninstall, or null while not set.</summary>
        public string? RemoveAction { get; set; }

        /// <summary>The component to add to; null for the group's own registry component.</summary>
        public string? ComponentId { get; set; }

        /// <summary>The registry view of the group's own component: default, always32 or always64.</summary>
        public string Bitness { get; set; } = "default";
    }

    /// <summary>The dialogs of the Registry designer. They are shown through <see cref="DesignerDialogs"/>, which themes them.</summary>
    internal static class Dialogs
    {
        static readonly Regex Identifier = new Regex(@"^[A-Za-z_][A-Za-z0-9_.]*$");

        static Form NewForm(string title) => new Form
        {
            Text = title,
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

        static TableLayoutPanel NewLayout() => new TableLayoutPanel
        {
            ColumnCount = 2,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Fill,
        };

        static Label AddRow(TableLayoutPanel layout, string text, Control control, float dpi)
        {
            var label = new Label { Text = text, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 6, 12, 6) };
            control.Margin = new Padding(0, 3, 0, 3);
            control.Width = (int)(320 * dpi);
            layout.RowCount++;
            layout.Controls.Add(label, 0, layout.RowCount - 1);
            layout.Controls.Add(control, 1, layout.RowCount - 1);
            return label;
        }

        static void AddButtons(Form form, TableLayoutPanel layout, Button ok, float dpi)
        {
            var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, AutoSize = true, MinimumSize = new Size((int)(88 * dpi), 0), Margin = new Padding(8, 0, 0, 0) };
            ok.AutoSize = true;
            ok.MinimumSize = new Size((int)(88 * dpi), 0);
            // The same margins as Cancel, so the two line up.
            ok.Margin = cancel.Margin;
            var bar = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Right, Margin = new Padding(0, 12, 0, 0) };
            bar.Controls.Add(cancel);
            bar.Controls.Add(ok);
            layout.RowCount++;
            layout.Controls.Add(bar, 0, layout.RowCount - 1);
            layout.SetColumnSpan(bar, 2);
            form.AcceptButton = ok;
            form.CancelButton = cancel;
        }

        public static bool IsIdentifier(string text) => Identifier.IsMatch(text);

        // ---------------------------------------------------------------- directory

        /// <summary>Asks which Directory a new component uses. Null if the user cancels.</summary>
        public static string? PickDirectory(IEnumerable<string> known, string suggested)
        {
            using var form = NewForm("Directory for the Registry Component");
            var dpi = form.DeviceDpi / 96F;
            var layout = NewLayout();
            var intro = new Label
            {
                Text = "A component needs a Directory. Which one should the registry entries' component use? Choose one, or type the Id of a directory defined elsewhere in the project.",
                AutoSize = true,
                MaximumSize = new Size((int)(440 * dpi), 0),
                Margin = new Padding(0, 0, 0, 8),
            };
            layout.RowCount++;
            layout.Controls.Add(intro, 0, 0);
            layout.SetColumnSpan(intro, 2);

            var box = new ComboBox { DropDownStyle = ComboBoxStyle.DropDown };
            box.Items.AddRange(known.Cast<object>().ToArray());
            box.Text = suggested;
            AddRow(layout, "Directory:", box, dpi);

            var ok = new Button { Text = "OK" };
            ok.Click += (s, e) =>
            {
                var text = box.Text.Trim();
                if (!Identifier.IsMatch(text))
                {
                    ThemedMessageBox.Show("An Id starts with a letter or underscore and has only letters, digits, underscores and periods.", "Directory", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                form.DialogResult = DialogResult.OK;
            };
            AddButtons(form, layout, ok, dpi);
            form.Controls.Add(layout);
            return DesignerDialogs.ShowDialog(form) == DialogResult.OK ? box.Text.Trim() : null;
        }

        // ---------------------------------------------------------------- entries

        /// <summary>
        /// Asks for a registry value (new or to change) or a registry key or value to remove. Null if the user cancels.
        /// </summary>
        /// <param name="initial">The values to start from.</param>
        /// <param name="groupId">The component group, for naming its own components.</param>
        /// <param name="components">The components an entry can be added to (not asked for when editing).</param>
        /// <param name="references">What a value can refer to, for the Insert Property button; null for none.</param>
        /// <param name="defaultOnly">The value is the key's default value: it has no name to enter.</param>
        public static EntryResult? EditEntry(EntryMode mode, RegistryValueSpec initial, string groupId, IReadOnlyList<RegistryComponentInfo> components,
            IReadOnlyList<ReferenceChoice>? references = null, bool defaultOnly = false)
        {
            var title = mode == EntryMode.NewValue ? "New Registry Value" : mode == EntryMode.EditValue ? "Registry Value" : "Remove Registry Entry";
            using var form = NewForm(title);
            var dpi = form.DeviceDpi / 96F;
            var layout = NewLayout();

            var removalKind = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            removalKind.Items.AddRange(new object[] { "Remove a key and what is in it", "Remove a value" });
            removalKind.SelectedIndex = 0;
            if (mode == EntryMode.NewRemoval)
            {
                AddRow(layout, "Remove:", removalKind, dpi);
            }

            var root = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            root.Items.AddRange(Labels.Roots.Cast<object>().ToArray());
            root.SelectedItem = Labels.Roots.Contains(initial.Root.ToUpperInvariant()) ? initial.Root.ToUpperInvariant() : "HKLM";
            AddRow(layout, "Root:", root, dpi);

            var key = new TextBox { Text = initial.Key };
            AddRow(layout, "Key:", key, dpi);

            // A default value has no name to enter: a label says so in place of the box (the box stays unused and disabled).
            var name = new TextBox { Text = initial.Name ?? string.Empty };
            Control nameControl = name;
            if (defaultOnly)
            {
                name.Text = string.Empty;
                name.Enabled = false;
                nameControl = new Label { Text = "(Default)", AutoSize = true, Anchor = AnchorStyles.Left };
            }

            var nameLabel = AddRow(layout, "Name:", nameControl, dpi);
            if (defaultOnly)
            {
                nameControl.Margin = new Padding(0, 6, 0, 6);
            }


            var type = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            type.Items.AddRange(Labels.Kinds.Cast<object>().ToArray());
            type.SelectedIndex = (int)initial.Type;
            var typeLabel = AddRow(layout, "Type:", type, dpi);

            var value = new TextBox { Multiline = true, ScrollBars = ScrollBars.Vertical, Height = (int)(72 * dpi) };
            value.Text = initial.Type == RegKind.MultiString ? string.Join("\r\n", initial.MultiStrings) : initial.Value ?? string.Empty;
            var valueLabel = AddRow(layout, "Value:", value, dpi);

            // Puts a property, directory or file path reference into the value, at the caret.
            var insert = new Button { Text = "Insert Property..." };
            var insertLabel = AddRow(layout, string.Empty, insert, dpi);
            insert.AutoSize = true;
            insert.Width = (int)(150 * dpi);
            insert.Click += (s, e) =>
            {
                if (references != null && PickReference(references) is { } text)
                {
                    value.SelectedText = text;
                    value.Focus();
                }
            };

            var action = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            var actionLabel = AddRow(layout, "Action:", action, dpi);

            var component = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            var view = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            view.Items.AddRange(Labels.Views.Cast<object>().ToArray());
            view.SelectedIndex = 0;
            if (mode != EntryMode.EditValue)
            {
                component.Items.Add(new ComponentChoice(null, "A registry component of the group (made if needed)"));
                foreach (var info in components)
                {
                    component.Items.Add(new ComponentChoice(info.Id, Labels.ComponentLabel(groupId, info)));
                }

                component.SelectedIndex = 0;
                AddRow(layout, "Component:", component, dpi);
                AddRow(layout, "Registry view:", view, dpi);
                component.SelectedIndexChanged += (s, e) => view.Enabled = component.SelectedIndex == 0;
            }

            // What the choices above allow.
            void Refresh()
            {
                var removal = mode == EntryMode.NewRemoval;
                var removingKey = removal && removalKind.SelectedIndex == 0;
                var kind = (RegKind)Math.Max(0, type.SelectedIndex);
                name.Enabled = !removingKey && !defaultOnly;
                nameLabel.Enabled = !removingKey;
                insert.Visible = !removal && references != null;
                insertLabel.Visible = insert.Visible;
                type.Visible = !removal;
                typeLabel.Visible = !removal;
                value.Visible = !removal;
                valueLabel.Visible = !removal;
                valueLabel.Text = kind == RegKind.MultiString ? "Strings (one per line):" : "Value:";
                value.AcceptsReturn = kind == RegKind.MultiString;
                action.Items.Clear();
                if (removal)
                {
                    action.Visible = removingKey;
                    actionLabel.Visible = removingKey;
                    action.Items.AddRange(Labels.RemoveActions.Cast<object>().ToArray());
                    action.SelectedIndex = 0;
                }
                else
                {
                    action.Visible = true;
                    actionLabel.Visible = true;
                    action.Items.AddRange(Labels.ValueActions.Cast<object>().ToArray());
                    action.SelectedItem = Labels.ValueActionLabel(initial.Action);
                }
            }

            Refresh();
            type.SelectedIndexChanged += (s, e) => Refresh();
            removalKind.SelectedIndexChanged += (s, e) => Refresh();

            var ok = new Button { Text = "OK" };
            ok.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(key.Text))
                {
                    ThemedMessageBox.Show("The key is required.", title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                form.DialogResult = DialogResult.OK;
            };
            AddButtons(form, layout, ok, dpi);
            // An entry needs a key: OK waits for one.
            void UpdateOk() => ok.Enabled = key.Text.Trim().Trim('\\').Length > 0;
            key.TextChanged += (s, e) => UpdateOk();
            UpdateOk();
            form.Controls.Add(layout);
            form.ActiveControl = key;
            if (DesignerDialogs.ShowDialog(form) != DialogResult.OK)
            {
                return null;
            }

            var result = new EntryResult();
            result.Spec.Root = (string)root.SelectedItem!;
            result.Spec.Key = key.Text.Trim().Trim('\\');
            result.Spec.Name = string.IsNullOrEmpty(name.Text) || !name.Enabled ? null : name.Text;
            if (mode == EntryMode.NewRemoval)
            {
                result.RemoveKey = removalKind.SelectedIndex == 0;
                result.RemoveAction = result.RemoveKey ? Labels.RemoveActionOf(action.SelectedItem as string) : null;
            }
            else
            {
                result.Spec.Type = (RegKind)type.SelectedIndex;
                result.Spec.Action = Labels.ValueActionOf(action.SelectedItem as string);
                if (result.Spec.Type == RegKind.MultiString)
                {
                    result.Spec.MultiStrings.AddRange(value.Text.Replace("\r\n", "\n").Split('\n').Where(line => line.Length > 0));
                }
                else
                {
                    result.Spec.Value = value.Text;
                }
            }

            if (mode != EntryMode.EditValue && component.SelectedItem is ComponentChoice chosen)
            {
                result.ComponentId = chosen.Id;
                result.Bitness = chosen.Id == null ? Labels.BitnessOf(view.SelectedItem as string) : "default";
            }

            return result;
        }

        // ---------------------------------------------------------------- references

        /// <summary>
        /// Asks which reference (a property, directory or file path) to put in a value. Null if the user cancels; else its text,
        /// such as [INSTALLFOLDER].
        /// </summary>
        public static string? PickReference(IReadOnlyList<ReferenceChoice> references)
        {
            using var form = NewForm("Insert Property");
            var dpi = form.DeviceDpi / 96F;
            var layout = NewLayout();

            var filter = new TextBox();
            AddRow(layout, "Find:", filter, dpi);

            var list = new ListView { View = View.Details, FullRowSelect = true, MultiSelect = false, HideSelection = false, Height = (int)(260 * dpi), Width = (int)(520 * dpi) };
            list.Columns.Add("Reference", (int)(190 * dpi));
            list.Columns.Add("Kind", (int)(80 * dpi));
            list.Columns.Add("Detail", (int)(230 * dpi));
            layout.RowCount++;
            list.Margin = new Padding(0, 6, 0, 0);
            layout.Controls.Add(list, 0, layout.RowCount - 1);
            layout.SetColumnSpan(list, 2);

            void Fill()
            {
                var text = filter.Text.Trim();
                list.BeginUpdate();
                list.Items.Clear();
                foreach (var reference in references.Where(r => text.Length == 0 || r.Text.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    r.Kind.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0 || (r.Detail ?? string.Empty).IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    var item = new ListViewItem(reference.Text) { Tag = reference };
                    item.SubItems.Add(reference.Kind);
                    item.SubItems.Add(reference.Detail ?? string.Empty);
                    list.Items.Add(item);
                }

                if (list.Items.Count > 0)
                {
                    list.Items[0].Selected = true;
                }

                list.EndUpdate();
            }

            Fill();
            filter.TextChanged += (s, e) => Fill();
            var ok = new Button { Text = "Insert" };
            ok.Click += (s, e) =>
            {
                if (list.SelectedItems.Count > 0)
                {
                    form.DialogResult = DialogResult.OK;
                }
            };
            list.DoubleClick += (s, e) => ok.PerformClick();
            AddButtons(form, layout, ok, dpi);
            form.Controls.Add(layout);
            form.ActiveControl = filter;
            return DesignerDialogs.ShowDialog(form) == DialogResult.OK && list.SelectedItems.Count > 0 ? ((ReferenceChoice)list.SelectedItems[0].Tag).Text : null;
        }

        // ---------------------------------------------------------------- find

        /// <summary>Asks what to find and where. Null if the user cancels.</summary>
        public static FindOptions? AskFind(FindOptions initial)
        {
            using var form = NewForm("Find");
            var dpi = form.DeviceDpi / 96F;
            var layout = NewLayout();

            var text = new TextBox { Text = initial.Text };
            AddRow(layout, "Find what:", text, dpi);
            var keys = new CheckBox { Text = "Key names", AutoSize = true, Checked = initial.Keys };
            var names = new CheckBox { Text = "Value names", AutoSize = true, Checked = initial.Names };
            var data = new CheckBox { Text = "Value data", AutoSize = true, Checked = initial.Data };
            var matchCase = new CheckBox { Text = "Match case", AutoSize = true, Checked = initial.MatchCase };
            var options = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = false };
            options.Controls.Add(keys);
            options.Controls.Add(names);
            options.Controls.Add(data);
            options.Controls.Add(matchCase);
            AddRow(layout, "Look at:", options, dpi);

            var ok = new Button { Text = "Find Next" };
            ok.Click += (s, e) =>
            {
                if (text.Text.Length == 0 || !(keys.Checked || names.Checked || data.Checked))
                {
                    ThemedMessageBox.Show(text.Text.Length == 0 ? "Enter the text to find." : "Choose at least one place to look.", "Find", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                form.DialogResult = DialogResult.OK;
            };
            AddButtons(form, layout, ok, dpi);
            form.Controls.Add(layout);
            form.ActiveControl = text;
            text.SelectAll();
            return DesignerDialogs.ShowDialog(form) == DialogResult.OK
                ? new FindOptions { Text = text.Text, MatchCase = matchCase.Checked, Keys = keys.Checked, Names = names.Checked, Data = data.Checked }
                : null;
        }
    }
}
