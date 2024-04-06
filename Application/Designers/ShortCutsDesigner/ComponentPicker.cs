using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml.Linq;
using IsWiXAutomationInterface;

namespace ShortCutsDesigner
{
    public partial class ComponentPicker : Form
    {
        string _fileKey = string.Empty;
        string _fileName = string.Empty;
        XElement _fileElement = null;
        IsWiXShortCuts _shortCuts;
        List<ShortCutCandidate> _candidates;
        public string FileFilterPattern { get; set; }
        public XElement FileElement
        {
            get
            {
                return _fileElement;
            }
        }

        public string FileKey
        {
            get
            {
                return _fileKey;
            }
        }

        public string FileName
        {
            get
            {
                return _fileName;
            }
        }

        public ComponentPicker(IsWiXShortCuts shortCuts)
        {
            _shortCuts = shortCuts;
            _candidates = _shortCuts.GetShortCutCandidates();
            InitializeComponent();
            PopulateListBox();
            if (treeView1.Nodes.Count == 0 && string.IsNullOrEmpty(FileFilterPattern))
            {
                label1.Text = "No files were found to be suitable for creating a shortcut.";
                treeView1.Visible = false;
            }
        }


        private void PopulateListBox()
        {
            treeView1.Nodes.Clear();
            foreach (var candidate in _candidates)
            {
                if(string.IsNullOrEmpty(FileFilterPattern) || candidate.DestinationFilePath.ToLower().Contains(FileFilterPattern.ToLower()))
                {
                    TreeNode node = treeView1.Nodes.Add(candidate.DestinationFilePath);
                    node.Tag = candidate;
                }
            }

        }


        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            ShortCutCandidate scc = treeView1.SelectedNode.Tag as ShortCutCandidate;
            _fileKey = scc.FileKey;
            _fileName = scc.FileName;
            _fileElement = scc.FileElement;
            buttonSelect.Enabled = true;
        }

        private void label1_Click(object sender, System.EventArgs e)
        {

        }
        private void textBoxIncludeFilter_KeyDown(object sender, KeyEventArgs e)
        {
                FileFilterPattern = textBoxIncludeFilter.Text;
                PopulateListBox();
        }
    }
}
