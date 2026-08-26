using Aml.Engine.Adapter;
using Aml.Engine.CAEX;
using MAPol.Models;
using MAPol.Views;
using System.Collections;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Web;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Xml.Linq;
using static MAPol.Models.ProcedureInfo;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace MAPol
{
    public partial class MdiContainer : Form
    {
        // Recent files state
        private readonly List<string> _recentFiles = new List<string>();
        private ToolStripMenuItem? _recentFilesMenu = new ToolStripMenuItem();
        private const int MaxRecentFiles = 20;

        public MdiContainer()
        {
            InitializeComponent();
            LoadRecentFiles();
            //tabControl1.SizeMode = TabSizeMode.Normal;
            //tabControl1.ItemSize = new Size(200, 25); // Width, Height

            //tabControl1.SizeMode = TabSizeMode.Fixed;
            //tabControl1.ItemSize = new Size(0, 25); // height = 30
        }

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {
        }

        private void labelPanel2Close_Click(object sender, EventArgs e)
        {
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            treeViewProject.ExpandAll();
            File.Delete("MAPOC.sqlite");

            // Initialize Recent Files menu item under the same parent as openAllProcessDisplaysToolStripMenuItem
            RefreshRecentFilesMenu(); // populate (empty at start)
        }

        /// <summary>
        /// Adds a file path to the recent files list and updates the menu.
        /// </summary>

        private void AddRecentFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            try
            {
                // Normalize
                string normalized = Path.GetFullPath(filePath);

                // Remove existing entry if present
                _recentFiles.RemoveAll(p => string.Equals(p, normalized, StringComparison.OrdinalIgnoreCase));

                // Insert at top
                _recentFiles.Insert(0, normalized);

                // Trim
                if (_recentFiles.Count > MaxRecentFiles)
                    _recentFiles.RemoveRange(MaxRecentFiles, _recentFiles.Count - MaxRecentFiles);

                SaveRecentFiles();
                RefreshRecentFilesMenu();
            }
            catch
            {
                // ignore normalization errors
            }
        }

        private readonly string _recentFilesPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MAPOL", "RecentFiles.txt");
        private void SaveRecentFiles()
        {
            try
            {
                var directory = Path.GetDirectoryName(_recentFilesPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                File.WriteAllLines(_recentFilesPath, _recentFiles);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save recent files: {ex.Message}");
            }
        }

        private void LoadRecentFiles()
        {
            try
            {
                if (!File.Exists(_recentFilesPath))
                    return;

                _recentFiles.Clear();

                foreach (var file in File.ReadAllLines(_recentFilesPath))
                {
                    if (!string.IsNullOrWhiteSpace(file))
                    {
                        _recentFiles.Add(file);
                    }
                }

                RefreshRecentFilesMenu();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load recent files: {ex.Message}");
            }
        }

        /// <summary>
        /// Rebuilds the Recent files submenu items.
        /// </summary>
        private void RefreshRecentFilesMenu()
        {
            if (_recentFilesMenu == null)
                return;

            recentMTPFilesToolStripMenuItem.DropDownItems.Clear();

            if (_recentFiles.Count == 0)
            {
                var none = new ToolStripMenuItem("(no recent files)")
                {
                    Enabled = false
                };
                _recentFilesMenu.DropDownItems.Add(none);
                return;
            }

            foreach (var file in _recentFiles)
            {
                string captured = file;

                var item = new ToolStripMenuItem(Path.GetFileName(file))
                {
                    ToolTipText = file
                };

                item.Click += (s, e) =>
                {
                    //ImportMtpFile(captured);
                    ImportMtpFile(new[] { captured });
                };

                recentMTPFilesToolStripMenuItem.DropDownItems.Add(item);
            }

            recentMTPFilesToolStripMenuItem.DropDownItems.Add(new ToolStripSeparator());
            var importAllItem = new ToolStripMenuItem("Import all recent files");
            importAllItem.Click += (s, e) =>
            {
                string[] fileList = new string[_recentFiles.Count];
                for (int i = 0; i < _recentFiles.Count; i++)
                {
                    fileList[i] = _recentFiles[i];
                }
                ImportMtpFile(fileList);
            };
            recentMTPFilesToolStripMenuItem.DropDownItems.Add(importAllItem);

            // Add a separator and Clear list action
            recentMTPFilesToolStripMenuItem.DropDownItems.Add(new ToolStripSeparator());
            var clearItem = new ToolStripMenuItem("Clear recent files");
            clearItem.Click += (s, e) =>
            {
                _recentFiles.Clear();
                RefreshRecentFilesMenu();
            };
            recentMTPFilesToolStripMenuItem.DropDownItems.Add(clearItem);
        }

        private void toolStripLabel1_Click(object sender, EventArgs e)
        {
            FormProcessDisplay1 form2 = new FormProcessDisplay1();
            form2.MdiParent = this;
            form2.Show();
        }

        //FormProcessDisplay1 formProcessDisplay = new FormProcessDisplay1();
        private void toolStripLabel5_Click(object sender, EventArgs e)
        {

            /*formProcessDisplay.MdiParent = this;
            formProcessDisplay.TopLevel = false;
            formProcessDisplay.Width = splitContainer1.Panel2.Width / 2;
            splitContainer1.Panel2.Controls.Add(formProcessDisplay);
            formProcessDisplay.Dock = DockStyle.Left;
            formProcessDisplay.Show();*/

        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            treeViewProject.Width = splitContainer1.Panel1.Width;
            treeViewProject.Height = splitContainer1.Panel1.Height - labelProjectView.Height - 20;
            labelProjectView.Width = treeViewProject.Width;
            labelProjectView.Top = tabControl1.Top + 3;
            treeViewProject.Top = labelProjectView.Top + labelProjectView.Height + 3;
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ImportMtpFile(Array.Empty<string>());
        }

        private void ImportMtpFile(string[] fileToImport)
        {
            TreeNode mtpNode = treeViewProject.Nodes[0].Nodes[0];
            if (fileToImport.Length == 0)
            {
                openFileDialog1 = new OpenFileDialog();
                openFileDialog1.Filter = "Mtp Files|*.mtp|All Files|*.*";
                openFileDialog1.Multiselect = true;
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {

                    string[] fileNames = openFileDialog1.SafeFileNames;
                    string[] fileNamesWithPath = openFileDialog1.FileNames;
                    int i = 0;
                    foreach (string fileName in fileNames)
                    {
                        TreeNode node = new TreeNode(Path.GetFileNameWithoutExtension(fileName));
                        node.ImageIndex = 0;
                        node.SelectedImageIndex = 0;
                        node.Tag = fileNamesWithPath[i];
                        mtpNode.Nodes.Add(node);
                        node.ContextMenuStrip = contextMenuStripMtpFile;


                        // Add imported file to recent files
                        try
                        {
                            AddRecentFile(fileNamesWithPath[i]);
                        }
                        catch { }

                        i++;
                    }

                    mtpNode.Expand();
                }
            }
            else
            {
                foreach (string s in fileToImport)
                {
                    TreeNode node = new TreeNode(Path.GetFileNameWithoutExtension(s));
                    node.ImageIndex = 0;
                    node.SelectedImageIndex = 0;
                    node.Tag = s;
                    mtpNode.Nodes.Add(node);
                    node.ContextMenuStrip = contextMenuStripMtpFile;
                }
            }

            mtpNode.Expand();
        }


        FormPlantTopology form2;

        private void treeViewProject_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            //Plant topology
            if (e.Node.Level == 1 && e.Node.Text == "Plant Topology")
            {
                form2 = new FormPlantTopology();
                form2.MdiParent = this;
                form2.TopLevel = false;
                TabPage tabPage = new TabPage();
                //tabPage.Height = 100;
                tabPage.Text = e.Node.Text;
                tabControl1.TabPages.Add(tabPage);
                tabControl1.TabPages[tabControl1.TabPages.Count - 1].Controls.Add(form2);
                tabControl1.SelectedIndex = tabControl1.TabCount - 1;
                form2.Dock = DockStyle.Fill;
                form2.FormBorderStyle = FormBorderStyle.None;
                form2.Show();
            }

            //Recipes
            else if (e.Node.Level == 1 && e.Node.Text == "Recipes")
            {
                FormRecipe form2 = new FormRecipe();
                form2.MdiParent = this;
                form2.TopLevel = false;
                TabPage tabPage = new TabPage();
                tabPage.Text = e.Node.Text;
                tabControl1.TabPages.Add(tabPage);
                tabControl1.TabPages[tabControl1.TabPages.Count - 1].Controls.Add(form2);
                tabControl1.SelectedIndex = tabControl1.TabCount - 1;
                form2.Dock = DockStyle.Fill;
                form2.FormBorderStyle = FormBorderStyle.None;
                form2.Show();
            }

            //HMI - Process display
            else if (e.Node.Level == 2)
            {
                openProcessDisplay(e.Node);
            }
        }

        /*private void openProcessDisplay(TreeNode treeNode)
        {
            FormProcessDisplay1 form2 = new FormProcessDisplay1();
            form2.MdiParent = this;
            form2.TopLevel = false;
            TabPage tabPage = new TabPage();
            tabPage.Text = treeNode.Text;
            tabControl1.TabPages.Add(tabPage);
            tabControl1.TabPages[tabControl1.TabPages.Count - 1].Controls.Add(form2);
            tabControl1.SelectedIndex = tabControl1.TabCount - 1;
            form2.mtpFileName = treeNode.Tag.ToString()!;
            form2.Dock = DockStyle.Fill;
            form2.FormBorderStyle = FormBorderStyle.None;
            form2.Show();
        }*/


        private void openProcessDisplay(TreeNode treeNode)
        {
            FormProcessDisplay1 formProcessDisplay = new FormProcessDisplay1();
            string key = treeNode.Tag.ToString()!;

            // Check whether the tab already exists
            foreach (TabPage page in tabControl1.TabPages)
            {
                if (page.Name == key)
                {
                    tabControl1.SelectedTab = page;
                    return;
                }
            }

            // Create new tab
            //FormProcessDisplay1 form2 = new FormProcessDisplay1();
            formProcessDisplay.MdiParent = this;
            formProcessDisplay.TopLevel = false;

            TabPage tabPage = new TabPage
            {
                Name = key,             // Unique identifier
                Text = treeNode.Text
            };

            tabControl1.TabPages.Add(tabPage);
            tabPage.Controls.Add(formProcessDisplay);

            tabControl1.SelectedTab = tabPage;

            formProcessDisplay.mtpFileName = key;
            formProcessDisplay.Dock = DockStyle.Fill;
            formProcessDisplay.FormBorderStyle = FormBorderStyle.None;
            formProcessDisplay.Show();

            // Add to recent files
            //AddRecentFile(key);
        }

        Dictionary<string, List<string>> MTPErrors = new Dictionary<string, List<string>>();

        public void AddMtpError(string mtpFile, List<string> errorList)
        {
            if (!MTPErrors.ContainsKey(mtpFile))
            {
                MTPErrors[mtpFile] = errorList;
            }
            else
            {
                MTPErrors.Add(mtpFile, errorList);
            }
        }

        private void tabControl1_MouseDown(object sender, MouseEventArgs e)
        {
            for (int i = 0; i < this.tabControl1.TabPages.Count; i++)
            {
                Rectangle r = this.tabControl1.GetTabRect(i);
                // Adjust the close button rectangle based on your drawing in DrawItem
                Rectangle closeButtonRect = new Rectangle(r.Right - 15, r.Top + (r.Height / 2) - 10, 20, 20);

                if (closeButtonRect.Contains(e.Location))
                {
                    this.tabControl1.TabPages.RemoveAt(i);
                    break;
                }
            }
        }

        private void tabControl1_DrawItem_1(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            using (Brush br = new SolidBrush(e.ForeColor))
            {
                StringFormat sf = new StringFormat();
                sf.Alignment = StringAlignment.Near;
                sf.LineAlignment = StringAlignment.Center;
                e.Graphics.DrawString(" " + this.tabControl1.TabPages[e.Index].Text, e.Font, br, e.Bounds, sf);
            }

            Rectangle closeButtonRect = new Rectangle(e.Bounds.Right - 22, e.Bounds.Top + (e.Bounds.Height / 2) - 10, 20, 20);
            e.Graphics.DrawString("\u00D7", new Font("Arial", 16, FontStyle.Bold), Brushes.Black, closeButtonRect);

            e.DrawFocusRectangle();
        }

        private void openAllProcessDisplaysToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode mtpNode = treeViewProject.Nodes[0].Nodes[0];
            if (mtpNode.Nodes.Count == 0)
            {
                MessageBox.Show("No MTP imported, please import MTP files before using this function");
                return;
            }

            foreach (TreeNode node in mtpNode.Nodes)
            {
                openProcessDisplay(node);
            }

        }

        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void toolStripLabel6_Click(object sender, EventArgs e)
        {
            splitContainer1.Panel1Collapsed = true;
        }

        private void toolStripLabel7_Click(object sender, EventArgs e)
        {
            splitContainer1.Panel1Collapsed = false;
        }

        private void toolStripMenuItemCloseAllTabs_Click(object sender, EventArgs e)
        {
            tabControl1.TabPages.Clear();
        }

        private void tabControl1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                System.Windows.Forms.TabControl tabControl = (System.Windows.Forms.TabControl)sender;

                for (int i = 0; i < tabControl.TabCount; i++)
                {
                    Rectangle tabRect = tabControl.GetTabRect(i);

                    if (tabRect.Contains(e.Location))
                    {
                        contextMenuStripTabPage.Show(tabControl, e.Location);
                        break;
                    }
                }
            }
        }

        private void toolStripMenuItemCloseAllButThis_Click(object sender, EventArgs e)
        {
            if (_rightClickedTabPage != null)
            {
                List<TabPage> tabPagesToRemove = new List<TabPage>();
                foreach (TabPage page in tabControl1.TabPages)
                {
                    if (page != _rightClickedTabPage)
                    {
                        tabPagesToRemove.Add(page);
                    }
                }

                foreach (TabPage pageToRemove in tabPagesToRemove)
                {
                    tabControl1.TabPages.Remove(pageToRemove);
                }
            }
        }

        private void contextMenuStripTabPage_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void splitContainer1_Panel1_Resize(object sender, EventArgs e)
        {
            treeViewProject.Width = splitContainer1.Panel1.Width;
        }

        private void tabControl1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                for (int i = 0; i < tabControl1.TabPages.Count; i++)
                {
                    Rectangle tabRect = tabControl1.GetTabRect(i);
                    if (tabRect.Contains(e.Location))
                    {
                        _rightClickedTabPage = tabControl1.TabPages[i];
                        contextMenuStripTabPage.Show(tabControl1, e.Location);
                        break;
                    }
                }
            }
        }

        private TabPage _rightClickedTabPage;

        private void treeViewProject_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void treeViewProject_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];
            List<string> fileList = GetAllDroppedMtpFiles(files);
            int i = 0;
            TreeNode mtpNode = treeViewProject.Nodes[0].Nodes[0];
            foreach (string file in fileList)
            {
                TreeNode node = new TreeNode(Path.GetFileNameWithoutExtension(file));
                node.ImageIndex = 0;
                node.SelectedImageIndex = 0;
                node.Tag = fileList[i];
                mtpNode.Nodes.Add(node);
                node.ContextMenuStrip = contextMenuStripMtpFile;

                importOpcUaItems(file);
                ParseServicesFromManifest(file, node);
                ParseOpcUaServersFromManifest(file, node);

                i++;
            }
            mtpNode.Expand();
        }

        private List<string> GetAllDroppedMtpFiles(string[] files)
        {
            List<string> mtpFiles = new List<string>();
            foreach (string path in files)
            {
                FileAttributes attributes = File.GetAttributes(path);

                if ((attributes & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    mtpFiles.AddRange(GetFilesInDirectory(path));
                }
                else
                {
                    if (path.EndsWith(".mtp", true, null))
                    {
                        mtpFiles.Add(path);
                    }
                }
            }
            return mtpFiles;
        }

        private List<string> GetFilesInDirectory(string directoryPath)
        {
            List<string> allFilesInDirectoryAndSubDirectory = new List<string>();
            string[] temp = Directory.GetFiles(directoryPath, "*.mtp", SearchOption.AllDirectories);
            foreach (string path in temp)
            {
                FileAttributes attributes = File.GetAttributes(path);
                if ((attributes & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    GetFilesInDirectory(path);
                }
                else
                {
                    if (path.EndsWith(".mtp", true, null))
                    {
                        allFilesInDirectoryAndSubDirectory.Add(path);
                    }
                }
            }
            return allFilesInDirectoryAndSubDirectory;
        }

        private void importOpcUaItems(string mtpFile)
        {
            string amlFile = UnpackMtpPackage(mtpFile) + "\\Manifest.aml";
            var doc1 = CAEXDocument.LoadFromFile(amlFile).CAEXFile;

            foreach (InstanceHierarchyType ih in doc1.InstanceHierarchy)
            {
                if (ih.Name == "ModuleTypePackage")
                {
                    var it = ih.InternalElement[0].InternalElement.FirstOrDefault(ie => ie.Name.Equals("CommunicationSet"));
                    if (it == null)
                    {
                        Trace.WriteLine("CommunicationSet not found in InstanceHierarchy.");
                        continue;
                    }

                    foreach (InternalElementType iele in it)
                    {
                        if (iele.Name == "SourceList")
                        {
                            var temp1 = iele.InternalElement[0]; //RefBaseSystemUnitPath="MTPCommunicationSUCLib/ServerAssembly/OPCUAServer"
                            string serverEndPoint = temp1.Attribute["Endpoint"]?.Value;

                            foreach (ExternalInterfaceType eit in temp1.ExternalInterface)
                            {
                                if (eit.RefBaseClassPath == "MTPCommunicationICLib/DataItem/OPCUAItem")
                                {
                                    OpcUaItem opcUaItem = new OpcUaItem();
                                    opcUaItem.ServerEndPoint = serverEndPoint;
                                    opcUaItem.Name = eit.Name;
                                    opcUaItem.Id = eit.ID;
                                    int access = -1;
                                    int.TryParse(eit.Attribute["Access"]?.Value, out access);
                                    opcUaItem.Access = access;

                                    opcUaItem.Identifier = eit.Attribute["Identifier"]?.Value;
                                    opcUaItem.OpcUaNamespace = eit.Attribute["Namespace"]?.Value;
                                    opcUaItems.Add(opcUaItem);
                                }
                            }

                            List<OpcUaItem> temp = opcUaItems;

                            //DBHandler dBHandler = new DBHandler();
                            //dBHandler.InsertOpcUaItems(opcUaItems);

                            break;
                        }

                        if (iele.RefBaseSystemUnitPath.Contains("OPCUAServer"))
                        {

                            break;
                        }
                    }

                    break;

                }
            }
        }

        private void ParseOpcUaServersFromManifest(string mtpFile, TreeNode treeNode)
        {
            string amlFile = UnpackMtpPackage(mtpFile) + "\\Manifest.aml";
            _opcUaServers.Clear();

            if (string.IsNullOrEmpty(amlFile) || !File.Exists(amlFile))
            {
                Trace.WriteLine("ParseOpcUaServersFromManifest: manifest file not found: " + amlFile);
                return;
            }

            XDocument doc;
            try
            {
                doc = XDocument.Load(amlFile);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("ParseOpcUaServersFromManifest: error loading manifest: " + ex.Message);
                return;
            }

            XNamespace ns = "http://www.dke.de/CAEX";

            // Find the SourceList(s) anywhere in the document
            var sourceLists = doc.Descendants(ns + "InternalElement")
                                 .Where(e => ((string)e.Attribute("RefBaseSystemUnitPath")) == "MTPSUCLib/CommunicationSet/SourceList");

            TreeNode opcServerNodes = treeNode.Nodes.Add("OPC UA Servers");
            
            foreach (var sourceList in sourceLists)
            {
                // OPC UA servers are InternalElement children with this RefBaseSystemUnitPath
                var serverElements = sourceList.Elements(ns + "InternalElement")
                                               .Where(s => ((string)s.Attribute("RefBaseSystemUnitPath")) == "MTPCommunicationSUCLib/ServerAssembly/OPCUAServer");

                foreach (var serverElem in serverElements)
                {
                    var server = new OpcUaServerInfo
                    {
                        Name = (string)serverElem.Attribute("Name") ?? "",
                        ID = (string)serverElem.Attribute("ID") ?? "",
                        Description = (string)serverElem.Element(ns + "Description") ?? ""
                    };

                    TreeNode serverNode = new TreeNode(server.Name) { Tag = server };
                    opcServerNodes.Nodes.Add(serverNode);

                    // Endpoint attribute (if present)
                    server.Endpoint = serverElem.Elements(ns + "Attribute")
                                                .FirstOrDefault(a => (string)a.Attribute("Name") == "Endpoint")
                                                ?.Element(ns + "Value")?.Value ?? "";

                    // ExternalInterface children that are OPCUA items
                    var items = serverElem.Elements(ns + "ExternalInterface")
                                      .Where(x => ((string)x.Attribute("RefBaseClassPath")) == "MTPCommunicationICLib/DataItem/OPCUAItem");

                    TreeNode opcUaItemNodes = serverNode.Nodes.Add("OPC UA Items");

                    foreach (var item in items)
                    {
                        var info = new OpcUaItemInfo
                        {
                            Name = (string)item.Attribute("Name") ?? "",
                            ID = (string)item.Attribute("ID") ?? "",
                            RefBaseClassPath = (string)item.Attribute("RefBaseClassPath") ?? ""
                        };

                        TreeNode opcUaItem = new TreeNode(info.Name) { Tag = info };
                        opcUaItemNodes.Nodes.Add(opcUaItem);

                        info.Access = item.Elements(ns + "Attribute")
                                          .FirstOrDefault(a => (string)a.Attribute("Name") == "Access")
                                          ?.Element(ns + "Value")?.Value ?? "";

                        info.Identifier = item.Elements(ns + "Attribute")
                                      .FirstOrDefault(a => (string)a.Attribute("Name") == "Identifier")
                                      ?.Element(ns + "Value")?.Value ?? "";

                        info.Namespace = item.Elements(ns + "Attribute")
                                             .FirstOrDefault(a => (string)a.Attribute("Name") == "Namespace")
                                             ?.Element(ns + "Value")?.Value ?? "";

                        server.Items.Add(info);
                    }

                    _opcUaServers.Add(server);
                }
            }

            Trace.WriteLine($"ParseOpcUaServersFromManifest: parsed {_opcUaServers.Count} OPC UA servers and {_opcUaServers.Sum(s => s.Items.Count)} items.");
        }

        private void removeAllMTPsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            treeViewProject.Nodes[0].Nodes[0].Nodes.Clear();
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeViewProject.SelectedNode != null)
            {
                treeViewProject.SelectedNode.Remove();
            }
        }

        private void treeViewProject_Click(object sender, EventArgs e)
        {

        }

        private void treeViewProject_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                TreeNode clickedNode = treeViewProject.GetNodeAt(e.X, e.Y);
                if (clickedNode != null)
                {
                    treeViewProject.SelectedNode = clickedNode;
                }
            }
        }

        private void mTPInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormMtpInfo formMtpInfo = new FormMtpInfo();
            formMtpInfo.fileName = treeViewProject.SelectedNode.Tag.ToString();
            formMtpInfo.ShowDialog();
        }

        private void openManifestXMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string extractPath = UnpackMtpPackage(treeViewProject.SelectedNode.Tag.ToString());

            string temp = "\"" + extractPath + "\\Manifest.aml" + "\"";
            try
            {
                //Process.Start("notepad++.exe", temp);
                var psi = new ProcessStartInfo(temp)
                {
                    UseShellExecute = true // important on .NET Core/.NET 8 to open via file association
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                Process.Start("notepad.exe", temp);
            }
        }

        private void treeViewProject_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void MdiContainer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                //Form form = ActiveMdiChild;
                //form = ActivateMdiChild(FormPlantTopology);
                if (form2 != null)
                {
                    form2.DeleteSelectedObject();
                }
            }
        }

        private string UnpackMtpPackage(string fileName)
        {
            string safeFileName = Path.GetFileNameWithoutExtension(fileName);
            string extractPath = "C:\\temp\\MA\\" + safeFileName;
            this.Text = "MTP Renderer - " + extractPath;

            extractPath = extractPath.Substring(0, extractPath.Length - 4);
            string manifestFileName = extractPath + "\\Manifest.aml";

            if (File.Exists(manifestFileName))
            {
                return extractPath;
            }

            try
            {
                var dir = new DirectoryInfo(extractPath);
                dir.Delete(true);
            }
            catch (IOException exception)
            {
                string error = exception.Message;
            }

            ZipFile.ExtractToDirectory(fileName, extractPath);
            return extractPath;
            //_amlFileName = extractPath + "\\Manifest.aml";
        }

        List<OpcUaItem> opcUaItems = new List<OpcUaItem>();

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            //formProcessDisplay.ShowMtpInfo();
            ShowMtpInfo();
        }

        public void ShowMtpInfo()
        {
            var firstItem = MTPErrors.FirstOrDefault();
            List<string> errors = firstItem.Value;
            FormMtpInfo formMtpInfo = new FormMtpInfo();
            string result = string.Join(Environment.NewLine, errors);
            string text = "Eclasses used:" + Environment.NewLine + result;
            formMtpInfo.InfoText = text;
            formMtpInfo.ShowDialog();
        }

        private void toolStripButtonErrorList_Click(object sender, EventArgs e)
        {
            //OpcUaClientLibrary.OpcUaClient opcUaClient = new OpcUaClientLibrary.OpcUaClient();
            //opcUaClient.PrintMethods();
        }

        private void ParseServicesFromManifest(string mtpFile, TreeNode mtpFileNode)
        {
            string amlFile = UnpackMtpPackage(mtpFile) + "\\Manifest.aml";
            if (string.IsNullOrEmpty(amlFile) || !File.Exists(amlFile))
            {
                Trace.WriteLine("ParseServicesFromManifest: manifest file not found: " + amlFile);
                return;
            }

            XDocument doc;
            try
            {
                doc = XDocument.Load(amlFile);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("ParseServicesFromManifest: error loading manifest: " + ex.Message);
                return;
            }

            XNamespace ns = "http://www.dke.de/CAEX";

            var servicesIH = doc.Descendants(ns + "InstanceHierarchy")
                                .FirstOrDefault(x => string.Equals((string)x.Attribute("Name"), "Services", StringComparison.OrdinalIgnoreCase));

            if (servicesIH == null)
            {
                Trace.WriteLine("ParseServicesFromManifest: no InstanceHierarchy with Name='Services' found.");
                return;
            }

            _services.Clear();

            // Service nodes have RefBaseSystemUnitPath == "MTPServiceSUCLib/Service"
            var serviceElements = servicesIH.Elements(ns + "InternalElement")
                                        .Where(e => (string)e.Attribute("RefBaseSystemUnitPath") == "MTPServiceSUCLib/Service");

            TreeNode servicesNode = new TreeNode("Services");

            if (serviceElements != null)
            {
                mtpFileNode.Nodes.Add(servicesNode);
            }

            foreach (var serviceElem in serviceElements)
            {
                var service = new ServiceInfo
                {
                    Name = (string)serviceElem.Attribute("Name") ?? "",
                    Description = (string)serviceElem.Element(ns + "Description") ?? ""
                };

                TreeNode serviceNode = new TreeNode(service.Name) { Tag = service };

                servicesNode.Nodes.Add(serviceNode);

                service.RefID = serviceElem.Elements(ns + "Attribute")
                                       .FirstOrDefault(a => (string)a.Attribute("Name") == "RefID")
                                       ?.Element(ns + "Value")?.Value ?? "";

                // Procedures are InternalElement nodes with RefBaseSystemUnitPath == "MTPServiceSUCLib/Service/Procedure"
                var procedureElements = serviceElem.Elements(ns + "InternalElement")
                                                   .Where(pe => (string)pe.Attribute("RefBaseSystemUnitPath") == "MTPServiceSUCLib/Service/Procedure");

                TreeNode proceduresNode = new TreeNode("Procedures");
                if (procedureElements != null)
                {

                    serviceNode.Nodes.Add(proceduresNode);
                }

                foreach (var procElem in procedureElements)
                {
                    var proc = new ProcedureInfo
                    {
                        Name = (string)procElem.Attribute("Name") ?? "",
                        Description = (string)procElem.Element(ns + "Description") ?? ""
                    };

                    

                    proc.RefID = procElem.Elements(ns + "Attribute")
                                         .FirstOrDefault(a => (string)a.Attribute("Name") == "RefID")
                                         ?.Element(ns + "Value")?.Value ?? "";

                    var procIdStr = procElem.Elements(ns + "Attribute")
                                             .FirstOrDefault(a => (string)a.Attribute("Name") == "ProcedureID")
                                             ?.Element(ns + "Value")?.Value;
                    if (int.TryParse(procIdStr, out int parsedProcId))
                    {
                        proc.ProcedureID = parsedProcId;
                    }

                    var isSelfStr = procElem.Elements(ns + "Attribute")
                                             .FirstOrDefault(a => (string)a.Attribute("Name") == "IsSelfCompleting")
                                             ?.Element(ns + "Value")?.Value;
                    if (bool.TryParse(isSelfStr, out bool parsedIsSelf))
                    {
                        proc.IsSelfCompleting = parsedIsSelf;
                        
                    }

                    TreeNode procedureNode = new TreeNode(proc.Name) { Tag = proc };
                    
                    if((bool)proc.IsSelfCompleting)
                    {
                        procedureNode.ForeColor = Color.Green; // or any color you prefer
                    }
                    else
                    {
                        procedureNode.ForeColor = Color.Red; // or any color you prefer
                        procedureNode.ImageIndex = 6;
                    }

                    proceduresNode.Nodes.Add(procedureNode);
                    TreeNode parameterNode = procedureNode.Nodes.Add("Parameters");
                    TreeNode reportValueNode = procedureNode.Nodes.Add("ReportValue");
                    TreeNode processValueIn = procedureNode.Nodes.Add("ProcessValueIn");
                    TreeNode processValueOut = procedureNode.Nodes.Add("ProcessValueOut");

                    // collect direct child InternalElement nodes (process values, report values, parameters, etc.)
                    foreach (var child in procElem.Elements(ns + "InternalElement"))
                    {
                        string rb = (string)child.Attribute("RefBaseSystemUnitPath") ?? "";
                        var elementRef = new ElementRef
                        {
                            Name = (string)child.Attribute("Name") ?? "",
                            RefBaseSystemUnitPath = rb,
                            RefID = child.Elements(ns + "Attribute")
                                         .FirstOrDefault(a => (string)a.Attribute("Name") == "RefID")
                                         ?.Element(ns + "Value")?.Value ?? ""
                        };

                        if (rb.Contains("ProcessValue/ProcessValueOut", StringComparison.OrdinalIgnoreCase))
                        {
                            proc.ProcessValuesOut.Add(elementRef);
                            processValueOut.Nodes.Add(elementRef.Name);
                        }
                        else if (rb.Contains("ProcessValue/ProcessValueIn", StringComparison.OrdinalIgnoreCase))
                        {
                            proc.ProcessValuesIn.Add(elementRef);
                            processValueIn.Nodes.Add(elementRef.Name);
                        }
                        else if (rb.IndexOf("ReportValue", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            proc.ReportValues.Add(elementRef);
                            reportValueNode.Nodes.Add(elementRef.Name);
                        }
                        else if (rb.IndexOf("ServiceParameter", StringComparison.OrdinalIgnoreCase) >= 0
                                 || rb.IndexOf("ProcedureParameter", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            proc.Parameters.Add(elementRef);
                            parameterNode.Nodes.Add(elementRef.Name);
                        }
                        else
                        {
                            // other child types can be inspected if needed
                        }
                    }

                    service.Procedures.Add(proc);
                }

                _services.Add(service);
            }

            Trace.WriteLine($"ParseServicesFromManifest: parsed {_services.Count} services and {_services.Sum(s => s.Procedures.Count)} procedures.");
        }

        private List<ServiceInfo> _services = new List<ServiceInfo>();

        private void contextMenuStripTopology_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private List<OpcUaServerInfo> _opcUaServers = new List<OpcUaServerInfo>();

        private class OpcUaServerInfo
        {
            public string Name { get; set; } = "";
            public string ID { get; set; } = "";
            public string Description { get; set; } = "";
            public string Endpoint { get; set; } = "";
            public List<OpcUaItemInfo> Items { get; } = new List<OpcUaItemInfo>();
        }

        private class OpcUaItemInfo
        {
            public string Name { get; set; } = "";
            public string ID { get; set; } = "";
            public string RefBaseClassPath { get; set; } = "";
            public string Access { get; set; } = "";
            public string Identifier { get; set; } = "";
            public string Namespace { get; set; } = "";
        }
    }
}
