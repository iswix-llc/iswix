using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using FilesAndFoldersDesigner;
using FireworksFramework.Services;

namespace Designers.FilesAndFolders
{
    // Which of the file's component groups is shown. It starts as the group named after the file, the one the component
    // rules default to; the others the file defines can be chosen from the list, and a new one can be added.
    public partial class FilesAndFolders
    {
        ComboBox comboBoxComponentGroups = null!;
        Button buttonNewComponentGroup = null!;
        Button buttonDeleteComponentGroup = null!;
        Label labelNoComponentGroups = null!;
        bool _loadingGroupList;

        // The group shown, kept across reloads (the designer is loaded again after any change to the text).
        string? _selectedGroupId;

        static readonly Regex GroupIdPattern = new Regex(@"^[A-Za-z_][A-Za-z0-9_.]*$");

        private void AddComponentGroupSelector()
        {
            var label = new Label { Text = "Component Group:", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(6, 0, 6, 0) };
            comboBoxComponentGroups = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 320, Margin = new Padding(0, 2, 6, 2) };
            buttonNewComponentGroup = new Button { Text = "New...", AutoSize = true, Margin = new Padding(0, 2, 6, 2) };
            buttonDeleteComponentGroup = new Button { Text = "Delete", AutoSize = true, Margin = new Padding(0, 2, 6, 2) };

            var bar = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = false };
            bar.Controls.Add(label);
            bar.Controls.Add(comboBoxComponentGroups);
            bar.Controls.Add(buttonNewComponentGroup);
            bar.Controls.Add(buttonDeleteComponentGroup);

            // Shown in place of the folders and files while the file has no component group.
            labelNoComponentGroups = new Label
            {
                Text = "This file has no component groups. Click New... to create one.",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = false,
            };
            splitContainer1.Panel2.Controls.Add(labelNoComponentGroups);

            // At the top of the designer, above the filters and component rules (which are for the group chosen here) and
            // the folders and files; the split container filling the rest.
            Controls.Add(bar);

            comboBoxComponentGroups.SelectedIndexChanged += (s, e) =>
            {
                if (_loadingGroupList || comboBoxComponentGroups.SelectedItem is not GroupItem item)
                {
                    return;
                }

                if (item.FilePath == null)
                {
                    ShowGroup(item.Id);
                }
                else
                {
                    GoToGroupInOtherFile(item);
                }
            };
            buttonNewComponentGroup.Click += (s, e) => NewComponentGroup();
            buttonDeleteComponentGroup.Click += (s, e) => DeleteComponentGroup();
        }

        // The groups to choose from, with the one shown selected.
        private void RefreshGroupList()
        {
            _loadingGroupList = true;
            try
            {
                comboBoxComponentGroups.Items.Clear();
                GroupItem? shown = null;
                var own = _isWiXComponentGroup.GetGroupIds();
                foreach (var id in own)
                {
                    var item = new GroupItem(id, null);
                    comboBoxComponentGroups.Items.Add(item);
                    if (_isWiXComponentGroup.HasGroup && string.Equals(id, _isWiXComponentGroup.GroupId, StringComparison.OrdinalIgnoreCase))
                    {
                        shown = item;
                    }
                }

                // The project's other files' groups, after this file's, with the file named. A group the file has too is its own.
                foreach (var group in _otherGroups
                    .Where(g => g.FilePath != null && !own.Contains(g.Id, StringComparer.OrdinalIgnoreCase))
                    .OrderBy(g => Path.GetFileName(g.FilePath), StringComparer.OrdinalIgnoreCase)
                    .ThenBy(g => g.Id, StringComparer.OrdinalIgnoreCase))
                {
                    comboBoxComponentGroups.Items.Add(new GroupItem(group.Id, group.FilePath));
                }

                comboBoxComponentGroups.SelectedItem = shown;
                comboBoxComponentGroups.Enabled = comboBoxComponentGroups.Items.Count > 0;
                buttonDeleteComponentGroup.Enabled = _isWiXComponentGroup.HasGroup;
                labelNoComponentGroups.Visible = !_isWiXComponentGroup.HasGroup;
                // The include and exclude filters and the component rules are for a group's files.
                splitContainer1.Panel1Collapsed = !_isWiXComponentGroup.HasGroup;
                scMain.Visible = _isWiXComponentGroup.HasGroup;
            }
            finally
            {
                _loadingGroupList = false;
            }
        }

        // An entry of the list: a group of this file (FilePath null), or of another file of the project, shown with the
        // file's name after it.
        sealed class GroupItem
        {
            public GroupItem(string id, string? filePath)
            {
                Id = id;
                FilePath = filePath;
            }

            public string Id { get; }
            public string? FilePath { get; }

            public override string ToString() => FilePath == null ? Id : $"{Id} ({Path.GetFileName(FilePath)})";
        }

        // The other files' groups, read from disk away from the user interface and put in the list when they are in.
        List<ComponentGroupInfo> _otherGroups = new List<ComponentGroupInfo>();
        int _scanVersion;

        private void ScanOtherGroups()
        {
            var path = _documentManager.DocumentPath;
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var version = ++_scanVersion;
            Task.Run(() => ProjectComponentGroups.FindInOtherFiles(path)).ContinueWith(task =>
            {
                if (task.IsFaulted || task.IsCanceled || version != _scanVersion || IsDisposed || !IsHandleCreated)
                {
                    return;
                }

                try
                {
                    BeginInvoke(new Action(() =>
                    {
                        if (version == _scanVersion && !IsDisposed)
                        {
                            _otherGroups = task.Result;
                            RefreshGroupList();
                        }
                    }));
                }
                catch (InvalidOperationException)
                {
                    // The designer was closed in the meantime.
                }
            });
        }

        // Another file's group: that file is opened with the group selected, and the list goes back to this file's own.
        private void GoToGroupInOtherFile(GroupItem item)
        {
            var opened = item.FilePath != null && DesignerNavigation.GoToDefinition is { } goTo && goTo(item.FilePath, "ComponentGroup", item.Id);
            if (!opened)
            {
                CallMessageBox($"Couldn't open {item.FilePath}.", "Component Group");
            }

            RefreshGroupList();
        }

        // Switches to another of the file's groups: its folders and files replace the current ones.
        private void ShowGroup(string? id)
        {
            _isWiXComponentGroup = new ComponentGroupDocument(_documentManager, id);
            _selectedGroupId = _isWiXComponentGroup.GroupId;
            RefreshGroupList();
            LoadDocument();
        }

        // After the user confirms: the group goes with its components, and the file's first other group, if any, is shown.
        private void DeleteComponentGroup()
        {
            var id = _isWiXComponentGroup.GroupId;
            var count = _isWiXComponentGroup.ComponentCount;
            var message = count == 0
                ? $"Delete the component group '{id}'?"
                : $"Delete the component group '{id}' and its {count} component{(count == 1 ? "" : "s")}?";
            if (ThemedMessageBox.Show(message, "Delete Component Group", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            _isWiXComponentGroup.DeleteGroup();
            ShowGroup(null);
        }

        private void NewComponentGroup()
        {
            var prompt = PromptForGroupId();
            if (prompt is not var (id, inNewFile))
            {
                return;
            }

            if (inNewFile)
            {
                CreateGroupInNewFile(id);
                return;
            }

            if (!_isWiXComponentGroup.CreateGroup(id))
            {
                CallMessageBox($"This file already has a component group '{id}'.", "New Component Group");
                return;
            }

            ShowGroup(id);
        }

        // The new group goes in a new file named after it, next to this one, which is opened. This file is left as it is.
        private void CreateGroupInNewFile(string id)
        {
            var path = NewGroupFilePath(id);
            var ns = _documentManager.Document.Root?.Name.Namespace ?? XNamespace.None;
            var document = new XDocument(new XElement(ns + "Wix", new XProcessingInstruction("define", $"SourceDir=\"{ComponentGroupDocument.DefaultSourceDir}\""), new XElement(ns + "Fragment", new XElement(ns + "ComponentGroup", new XAttribute("Id", id)))));
            try
            {
                using var writer = XmlWriter.Create(path, new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true, Encoding = new UTF8Encoding(true) });
                document.Save(writer);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                CallMessageBox($"Couldn't create {path}: {ex.Message}", "New Component Group");
                return;
            }

            if (DesignerNavigation.GoToDefinition is not { } goTo || !goTo(path, "ComponentGroup", id))
            {
                CallMessageBox($"Created {path}, but couldn't open it.", "New Component Group");
            }
        }

        string NewGroupFilePath(string id) => Path.Combine(Path.GetDirectoryName(_documentManager.DocumentPath) ?? string.Empty, id + ".wxs");

        // Asks for the new group's Id, and whether it goes in a new .wxs file, until the Id is a valid identifier (and its
        // file doesn't exist yet, if it is to have one); null if the user cancels.
        private (string Id, bool InNewFile)? PromptForGroupId()
        {
            using var form = new Form
            {
                Text = "New Component Group",
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false,
                ShowInTaskbar = false,
                AutoScaleMode = AutoScaleMode.Dpi,
                ClientSize = new Size(420, 140),
            };
            var label = new Label { Text = "Id of the new component group:", AutoSize = true, Location = new Point(12, 12) };
            var box = new TextBox { Location = new Point(12, 36), Width = 396 };
            var newFile = new CheckBox { Text = "Create a new .wxs file for the group (recommended)", AutoSize = true, Checked = true, Location = new Point(12, 68) };
            var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, Location = new Point(252, 102) };
            var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Location = new Point(333, 102) };
            form.Controls.AddRange(new Control[] { label, box, newFile, ok, cancel });
            form.AcceptButton = ok;
            form.CancelButton = cancel;

            // Through the host, which themes it (light or dark) and keeps it on top of the host's window.
            while (DesignerDialogs.ShowDialog(form) == DialogResult.OK)
            {
                var id = box.Text.Trim();
                if (!GroupIdPattern.IsMatch(id))
                {
                    CallMessageBox("An Id starts with a letter or underscore and has only letters, digits, underscores and periods.", "New Component Group");
                }
                else if (newFile.Checked && File.Exists(NewGroupFilePath(id)))
                {
                    CallMessageBox($"{id}.wxs already exists in this folder.", "New Component Group");
                }
                else
                {
                    return (id, newFile.Checked);
                }
            }

            return null;
        }
    }
}
