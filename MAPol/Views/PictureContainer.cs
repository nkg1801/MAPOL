using Aml.Engine.CAEX;
using MAPol.Models;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.IO.Compression;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
using static System.ComponentModel.Design.ObjectSelectorEditor;

namespace MAPol.Views
{
    public partial class PictureContainer : Form
    {
        public PictureContainer()
        {
            InitializeComponent();
        }

        public string mtpFileName;

        private void PictureContainer_Load(object sender, EventArgs e)
        {
            OpenFile(mtpFileName);

            /*FormProcessDisplay1 formProcessDisplay = new FormProcessDisplay1(null, this);
            formProcessDisplay.TopLevel = false;
            TabPage tabPage = tabControl1.TabPages[0];
            tabPage.Controls.Add(formProcessDisplay);
            formProcessDisplay.mtpFileName = mtpFileName;
            formProcessDisplay.Dock = DockStyle.Fill;
            formProcessDisplay.FormBorderStyle = FormBorderStyle.None;
            formProcessDisplay.Show();*/

            /*PictureObject pictureObject = mtpPictureObjects.First();
            RenderGraphic2(pictureObject);
            _parentForm.SetTabPageName(0, pictureObject.Name);*/

            foreach (PictureObject pictureObject in mtpPictureObjects)
            {
                //_parentForm.AddForm2Tab(pictureObject1);

                TabPage tabPage = new TabPage(pictureObject.Name);

                FormProcessDisplay1 form = new FormProcessDisplay1(pictureObject, this);
                form._indicatorElementList = _indicatorElementList;
                form._inputElementList = _inputElementList;
                form._operationElementList = _operationElementList;
                form._activeElementList = _activeElementList;
                form._diagnosticElementList = _diagnosticElementList;
                form._parameterElementList = _parameterElementList;
                form._serviceElementList= _serviceElementList;
                form.TopLevel = false;
                form.FormBorderStyle = FormBorderStyle.None;
                form.Dock = DockStyle.Fill;

                tabPage.Controls.Add(form);
                tabControl1.TabPages.Add(tabPage);

                form.Show();
            }

        }

        public void SetTabPageName(int tabPageIndex, String tabText)
        {
            if (tabControl1.TabPages.Count >= tabPageIndex)
            {
                tabControl1.TabPages[tabPageIndex].Text = tabText;
            }
        }

        public void AddForm2Tab(PictureObject pictureObject)
        {
            TabPage tabPage = new TabPage(pictureObject.Name);

            FormProcessDisplay1 form = new FormProcessDisplay1(pictureObject, this);
            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;

            tabPage.Controls.Add(form);
            tabControl1.TabPages.Add(tabPage);

            form.Show();
        }

        private void OpenFile(string filename)
        {
            int index = filename.LastIndexOf('\\');
            string safeFileName = filename.Substring(index + 1, filename.Length - index - 1);

            if (filename.EndsWith(".mtp"))
            {
                //HandleRecentFileList(filename);

                string extractPath = "C:\\temp\\MA\\" + safeFileName;
                this.Text = "MTP Renderer - " + extractPath;

                extractPath = extractPath.Substring(0, extractPath.Length - 4);

                try
                {
                    var dir = new DirectoryInfo(extractPath);
                    dir.Delete(true);
                }
                catch (IOException exception)
                {
                    string error = exception.Message;
                }

                ZipFile.ExtractToDirectory(filename, extractPath);
                _amlFileName = extractPath + "\\Manifest.aml";

            }

            _xmlDocument = new XmlDocument();
            _xmlDocument.Load(_amlFileName);

            GetMtpVersion(_amlFileName);

            imagePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\Shapes\\";
            parseMtpXml2(_amlFileName);

            if (_mtpVersion == 1)
            {
                parseDataAssemblyItems();
                ParseCommunicationSetNode();
            }
            else
            {
                ParseDataAssemblySetNode();
            }

            //PictureObject pictureObject = mtpPictureObjects.First();
            //RenderGraphic2(pictureObject);
            //_parentForm.SetTabPageName(0, pictureObject.Name);

            //PictureContainer pictureContainer = FindForm() as PictureContainer;

            /*foreach (PictureObject pictureObject1 in mtpPictureObjects.Skip(1))
            {
                _parentForm.AddForm2Tab(pictureObject1);
            }*/

            //RenderGraphic2(pictureObject);
            //toolStripButton5.Enabled = true;
        }

        private void parseMtpXml2(string fileName)
        {
            mtpPictureObjects.Clear();

            var pictures = GetPictureNodes();
            if (pictures == null || pictures.Count == 0)
            {
                System.Windows.Forms.MessageBox.Show("The MTP does not contain any process display aka Picture Node");
                return;
            }

            TreeNode treeNodePictureObjects = SelectedNode.Nodes.Add("Pictures");

            // Parse each Picture node and aggregate elements into shared lists
            foreach (XmlNode pictureNode in pictures)
            {
                String pictureName = pictureNode.Attributes["Name"]?.Value;
                TreeNode treeNodePictureObject = treeNodePictureObjects.Nodes.Add(pictureName);
                TreeNode treeNodeVisualObjects = treeNodePictureObject.Nodes.Add("VisualObjects");
                PictureObject pictureObject = new PictureObject();
                pictureObject.Name = pictureName;
                foreach (XmlNode node in pictureNode.ChildNodes)
                {
                    XmlNode temp = node;
                    string attrValue;

                    if (temp.Name == "Attribute")
                    {
                        //temp1 = temp.InnerXml;
                        attrValue = node.Attributes["Name"].Value;
                        if (attrValue == "Width")
                        {
                            string tempValue = GetValue(node);
                            if (tempValue != string.Empty)
                            {
                                // pictureWidth is local to this picture (not used later), so we don't store globally
                                int pictureWidth = int.Parse(tempValue);
                                pictureObject.Width = pictureWidth;
                            }
                        }
                        else if (attrValue == "Height")
                        {
                            string tempValue = GetValue(node);
                            if (tempValue != string.Empty)
                            {
                                int pictureHeight = int.Parse(tempValue);
                                pictureObject.Height = pictureHeight;
                            }
                        }
                    }
                    if (temp.Name == "InternalElement")
                    {
                        attrValue = node.Attributes["RefBaseSystemUnitPath"]?.Value ?? string.Empty;
                        if (attrValue == "MTPHMISUCLib/SemanticGroup")
                        {
                            List<MtpObject> mtpObjects = ParseSemanticGroup(node);
                            pictureObject.MtpObjects.AddRange(mtpObjects);
                            foreach(MtpObject m in mtpObjects)
                            {
                                TreeNode n = treeNodeVisualObjects.Nodes.Add(m.Name);
                                n.Tag = m.ID;
                            }
                        }
                        else if (attrValue == "MTPHMISUCLib/VisualObject")
                        {
                            String id = node.Attributes["ID"]?.Value ?? string.Empty;
                            MtpObject mtpObject = new MtpObject();
                            mtpObject.ID = id;  
                            mtpObject.RefBaseSystemUnitPath = attrValue;
                            mtpObject.Name = node?.Attributes?["Name"]?.Value;
                            TreeNode n = treeNodeVisualObjects.Nodes.Add(mtpObject.Name);
                            n.Tag = id;
                           
                            foreach (XmlNode loopNodeVar in node.ChildNodes)
                            {
                                XmlNode node10 = loopNodeVar;
                                if (node10 == null || node10.Attributes == null)
                                {
                                    continue;
                                }
                                if (node10.Name == "RoleRequirements" || node10.Name == "Description" || node10.Name == "SupportedRoleClass")
                                {
                                    continue;
                                }

                                if (node10?.Attributes?["Name"]?.Value == "RefID")
                                {
                                    mtpObject.RefID = GetValue(node10);
                                    //n.Tag = mtpObject.RefID;
                                }
                                else if (node10?.Attributes?["Name"]?.Value == "eClassVersion")
                                {
                                    mtpObject.EClassVersion = GetValue(node10);
                                }
                                else if (node10?.Attributes?["Name"]?.Value == "eClassClassificationClass")
                                {
                                    string tempValue = GetValue(node10);
                                    if (tempValue == null || tempValue.Trim() == string.Empty)
                                    {
                                        mtpObject.EClassClassificationClass = "";
                                    }
                                    else
                                    {
                                        int tempValue2 = 0;
                                        tempValue2 = int.Parse(tempValue.Trim());
                                        if (tempValue2 == 0)
                                        {
                                            mtpObject.EClassClassificationClass = "";
                                        }
                                        else
                                        {
                                            mtpObject.EClassClassificationClass = tempValue;
                                            _eClassesUsed.Add(tempValue.ToString());
                                        }
                                    }
                                }

                                else if (node10.Attributes["Name"]?.Value == "Width")
                                {
                                    string tempValue = GetValue(node10);
                                    if (tempValue != string.Empty)
                                    {
                                        mtpObject.Width = Scale(int.Parse(tempValue));
                                    }
                                }

                                else if (node10.Attributes["Name"]?.Value == "Height")
                                {
                                    string tempValue = GetValue(node10);
                                    if (tempValue != string.Empty)
                                    {
                                        mtpObject.Height = Scale(int.Parse(tempValue));
                                    }
                                }
                                else if (node10.Attributes["Name"]?.Value == "X")
                                {
                                    string tempValue = GetValue(node10);
                                    if (tempValue != string.Empty)
                                    {
                                        mtpObject.X = Scale(int.Parse(tempValue));
                                    }
                                }
                                else if (node10.Attributes["Name"].Value == "Y")
                                {
                                    string tempValue = GetValue(node10);
                                    if (tempValue != string.Empty)
                                    {
                                        mtpObject.Y = Scale(int.Parse(tempValue));
                                    }
                                }
                                else if (node10.Attributes["Name"].Value == "ZIndex")
                                {
                                    string tempValue = GetValue(node10);
                                    if (tempValue != string.Empty)
                                    {
                                        mtpObject.ZIndex = int.Parse(tempValue);
                                    }
                                }
                                else if (node10.Attributes["Name"].Value == "Rotation")
                                {
                                    string tempValue = GetValue(node10);
                                    if (tempValue != string.Empty)
                                    {
                                        mtpObject.Rotation = int.Parse(tempValue);
                                    }
                                }

                                
                            }
                            if ((mtpObject.RefID == null || mtpObject.RefID == "") && (mtpObject.EClassClassificationClass == null || mtpObject.EClassClassificationClass == ""))
                            {
                                Trace.WriteLine("MTP Renderer -> Warning: VisualObject " + mtpObject.Name + " has no RefID");
                                mtpErrors.Add("VisualObject " + mtpObject.Name + " has no RefID and no eClassClassificationClass");
                            }
                            pictureObject.MtpObjects.Add(mtpObject);
                        }
                        else if (attrValue == "MTPHMISUCLib/Connection/MeasurementLine" || attrValue == "MTPHMISUCLib/Connection/Pipe"
                            || attrValue == "MTPHMISUCLib/Connection/FunctionLine")
                        {
                            MtpConnectionObject connectionObject = new MtpConnectionObject();
                            connectionObject.RefBaseSystemUnitPath = attrValue;
                            connectionObject.Name = node.Attributes["Name"].Value;
                            foreach (XmlNode loopNodeVar in node.ChildNodes)
                            {
                                XmlNode node10 = loopNodeVar;
                                if (node10.Name == "RoleRequirements")// || node10.Attributes["Name"].Value == "Directed")
                                {
                                    continue;
                                }

                                if (node10.Name == "Description" || node10.Name == "SupportedRoleClass")
                                {
                                    continue;
                                }


                                if (node10.Attributes["Name"].Value == "Directed")
                                {
                                    continue;
                                }

                                if (node10.Attributes["Name"].Value == "Edgepath")
                                {
                                    connectionObject.Edgepath = GetValue(node10);
                                }
                                else if (node10.Attributes["RefBaseSystemUnitPath"] != null && node10.Attributes["RefBaseSystemUnitPath"].Value == "MTPHMISUCLib/PortObject/Nozzle")
                                {
                                    MtpPortObject mtpPortObject = new MtpPortObject();
                                    mtpPortObject.Name = node10.Attributes["Name"].Value;
                                    mtpPortObject.RefBaseSystemUnitPath = "MTPHMISUCLib/PortObject/Nozzle";
                                    foreach (XmlNode loopNodeVar1 in node10.ChildNodes)
                                    {
                                        XmlNode node100 = loopNodeVar1;
                                        if (node100.Name == "RoleRequirements" || node100.Name == "Connector"
                                            || node100.Name == "Connector_Connector" || node100.Name == "Description"
                                            || node100.Name == "SupportedRoleClass")
                                        {
                                            continue;
                                        }
                                        if (node100.Attributes["Name"].Value == "X")
                                        {
                                            string tempValue = GetValue(node100);
                                            if (tempValue != string.Empty)
                                            {
                                                mtpPortObject.X = int.Parse(tempValue);
                                            }
                                        }
                                        else if (node100.Attributes["Name"].Value == "Y")
                                        {
                                            string tempValue = GetValue(node100);
                                            if (tempValue != string.Empty)
                                            {
                                                mtpPortObject.Y = int.Parse(tempValue);
                                            }
                                        }
                                    }
                                    pictureObject.MtpPortObjects.Add(mtpPortObject);
                                }

                                else if (node10.Attributes["RefBaseSystemUnitPath"] != null && node10.Attributes["RefBaseSystemUnitPath"].Value == "MTPHMISUCLib/PortObject/LogicalPort")
                                {
                                    MtpPortObject mtpPortObject = new MtpPortObject();
                                    mtpPortObject.Name = node10.Attributes["Name"].Value;
                                    mtpPortObject.RefBaseSystemUnitPath = "MTPHMISUCLib/PortObject/LogicalPort";
                                    foreach (XmlNode loopNodeVar1 in node10.ChildNodes)
                                    {
                                        XmlNode node100 = loopNodeVar1;
                                        if (node100.Name == "RoleRequirements" || node100.Name == "Connector"
                                            || node100.Name == "Connector_Connector" || node100.Name == "Description"
                                            || node100.Name == "SupportedRoleClass")
                                        {
                                            continue;
                                        }
                                        if (node100.Attributes["Name"].Value == "X")
                                        {
                                            string tempValue = GetValue(node100);
                                            if (tempValue != string.Empty)
                                            {
                                                mtpPortObject.X = int.Parse(tempValue);
                                            }
                                        }
                                        else if (node100.Attributes["Name"].Value == "Y")
                                        {
                                            string tempValue = GetValue(node100);
                                            if (tempValue != string.Empty)
                                            {
                                                mtpPortObject.Y = int.Parse(tempValue);
                                            }
                                        }
                                    }
                                    pictureObject.MtpPortObjects.Add(mtpPortObject);
                                }
                            }
                            pictureObject.MtpConnectionObjects.Add(connectionObject);
                        }
                        else if (attrValue == "MTPHMISUCLib/TopologyObject/Termination")
                        {
                            MtpSinkObject mtpSinkObject = new MtpSinkObject();
                            mtpSinkObject.RefBaseSystemUnitPath = attrValue;
                            mtpSinkObject.Name = node.Attributes["Name"].Value;

                            foreach (XmlNode loopNodeVar in node.ChildNodes)
                            {
                                XmlNode node10 = loopNodeVar;
                                if (node10.Name == "RoleRequirements" || node10.Name == "TermID" || node10.Name == "ExternalInterface"
                                    || node10.Name == "Description" || node10.Name == "SupportedRoleClass")
                                {
                                    continue;
                                }
                                if (node10.Attributes["Name"].Value == "X")
                                {
                                    string tempValue = GetValue(node10);
                                    if (tempValue != string.Empty)
                                    {
                                        mtpSinkObject.X = int.Parse(tempValue);
                                    }
                                }
                                else if (node10.Attributes["Name"].Value == "Y")
                                {
                                    string tempValue = GetValue(node10);
                                    if (tempValue != string.Empty)
                                    {
                                        mtpSinkObject.Y = int.Parse(tempValue);
                                    }
                                }
                                else if (node10.Attributes["RefBaseSystemUnitPath"] != null && node10.Attributes["RefBaseSystemUnitPath"].Value == "MTPHMISUCLib/PortObject/LogicalPort")
                                {
                                    MtpPortObject mtpPortObject = new MtpPortObject();
                                    mtpPortObject.Name = node10.Attributes["Name"].Value;
                                    mtpPortObject.RefBaseSystemUnitPath = "MTPHMISUCLib/PortObject/LogicalPort";
                                    foreach (XmlNode loopNodeVar1 in node10.ChildNodes)
                                    {
                                        XmlNode node100 = loopNodeVar1;
                                        if (node100.Name == "RoleRequirements" || node100.Name == "Connector"
                                            || node100.Name == "Connector_Connector" || node100.Name == "Description"
                                            || node100.Name == "SupportedRoleClass")
                                        {
                                            continue;
                                        }
                                        if (node100.Attributes["Name"].Value == "X")
                                        {
                                            string tempValue = GetValue(node100);
                                            if (tempValue != string.Empty)
                                            {
                                                mtpPortObject.X = int.Parse(tempValue);
                                            }
                                        }
                                        else if (node100.Attributes["Name"].Value == "Y")
                                        {
                                            string tempValue = GetValue(node100);
                                            if (tempValue != string.Empty)
                                            {
                                                mtpPortObject.Y = int.Parse(tempValue);
                                            }
                                        }
                                    }
                                    pictureObject.MtpPortObjects.Add(mtpPortObject);
                                }
                            }
                            pictureObject.MtpSinkObjects.Add(mtpSinkObject);

                        }
                        else if (attrValue == "MTPHMISUCLib/TopologyObject/Termination/Source")
                        {
                            MtpSourceObject mtpSourceObject = new MtpSourceObject();
                            mtpSourceObject.RefBaseSystemUnitPath = attrValue;
                            mtpSourceObject.Name = node.Attributes["Name"].Value;

                            foreach (XmlNode loopNodeVar in node.ChildNodes)
                            {
                                XmlNode node10 = loopNodeVar;
                                if (node10.Name == "RoleRequirements" || node10.Name == "TermID" || node10.Name == "ExternalInterface"
                                    || node10.Name == "Description" || node10.Name == "SupportedRoleClass")
                                {
                                    continue;
                                }
                                if (node10.Attributes["Name"].Value == "X")
                                {
                                    string tempValue = GetValue(node10);
                                    if (tempValue != string.Empty)
                                    {
                                        mtpSourceObject.X = int.Parse(tempValue);
                                    }
                                }
                                else if (node10.Attributes["Name"].Value == "Y")
                                {
                                    string tempValue = GetValue(node10);
                                    if (tempValue != string.Empty)
                                    {
                                        mtpSourceObject.Y = int.Parse(tempValue);
                                    }
                                }
                            }
                            //mtpSourceObjects.Add(mtpSourceObject);
                            pictureObject.MtpSourceObjects.Add(mtpSourceObject);
                        }

                        else if (attrValue == "MTPHMISUCLib/TopologyObject/Termination/Sink")
                        {
                            MtpSinkObject mtpSinkObject = new MtpSinkObject();
                            mtpSinkObject.RefBaseSystemUnitPath = attrValue;
                            mtpSinkObject.Name = node.Attributes["Name"].Value;

                            foreach (XmlNode loopNodeVar in node.ChildNodes)
                            {
                                XmlNode node10 = loopNodeVar;
                                if (node10.Name == "RoleRequirements" || node10.Name == "TermID" || node10.Name == "ExternalInterface"
                                    || node10.Name == "Description" || node10.Name == "SupportedRoleClass")
                                {
                                    continue;
                                }
                                if (node10.Attributes["Name"].Value == "X")
                                {
                                    string tempValue = GetValue(node10);
                                    if (tempValue != string.Empty)
                                    {
                                        mtpSinkObject.X = int.Parse(tempValue);
                                    }
                                }
                                else if (node10.Attributes["Name"].Value == "Y")
                                {
                                    string tempValue = GetValue(node10);
                                    if (tempValue != string.Empty)
                                    {
                                        mtpSinkObject.Y = int.Parse(tempValue);
                                    }
                                }
                            }
                            //mtpSinkObjects.Add(mtpSinkObject);
                            pictureObject.MtpSinkObjects.Add(mtpSinkObject);
                        }

                        else if (attrValue == "MTPHMISUCLib/PortObject/Nozzle" || attrValue == "MTPHMISUCLib/PortObject/MeasurementPoint"
                            || attrValue == "MTPHMISUCLib/PortObject/LogicalPort")
                        {
                            MtpPortObject mtpPortObject = new MtpPortObject();
                            mtpPortObject.RefBaseSystemUnitPath = attrValue;
                            foreach (XmlNode loopNodeVar in node.ChildNodes)
                            {
                                XmlNode node10 = loopNodeVar;
                                if (node10.Name == "RoleRequirements" || node10.Name == "InternalLink" || node10.Name == "ExternalInterface")
                                {
                                    continue;
                                }
                                if (node10.Attributes["Name"].Value == "X")
                                {
                                    string tempValue = GetValue(node10);
                                    if (tempValue != string.Empty)
                                    {
                                        mtpPortObject.X = int.Parse(tempValue);
                                    }
                                }
                                else if (node10.Attributes["Name"].Value == "Y")
                                {
                                    string tempValue = GetValue(node10);
                                    if (tempValue != string.Empty)
                                    {
                                        mtpPortObject.Y = int.Parse(tempValue);
                                    }
                                }
                            }
                            //mtpPortObjects.Add(mtpPortObject); //todo: this can be removed later if the line below works
                            pictureObject.MtpPortObjects.Add(mtpPortObject);
                        }
                        else if (attrValue == "MTPHMISUCLib/TopologyObject/Junction")
                        {
                            MtpJunctionObject mtpJunctionObject = new MtpJunctionObject();
                            mtpJunctionObject.RefBaseSystemUnitPath = attrValue;
                            mtpJunctionObject.Name = node.Attributes["Name"].Value;
                            foreach (XmlNode loopNodeVar in node.ChildNodes)
                            {
                                XmlNode node10 = loopNodeVar;
                                if (node10.Name == "RoleRequirements" || node10.Name == "InternalLink" || node10.Name == "ExternalInterface"
                                    || node10.Name == "Description" || node10.Name == "SupportedRoleClass")
                                {
                                    continue;
                                }
                                if (node10.Attributes["Name"].Value == "X")
                                {
                                    string tempValue = GetValue(node10);
                                    if (tempValue != string.Empty)
                                    {
                                        mtpJunctionObject.X = int.Parse(tempValue);
                                    }
                                }
                                else if (node10.Attributes["Name"].Value == "Y")
                                {
                                    string tempValue = GetValue(node10);
                                    if (tempValue != string.Empty)
                                    {
                                        mtpJunctionObject.Y = int.Parse(tempValue);
                                    }
                                }
                            }
                            //mtpJunctionObjects.Add(mtpJunctionObject);
                            pictureObject.MtpJunctionObjects.Add(mtpJunctionObject);
                        }
                    }
                }
                mtpPictureObjects.Add(pictureObject); // Add the parsed picture object to the list
            }
        }

        private List<XmlNode> GetPictureNodes()
        {
            List<XmlNode> found = new List<XmlNode>();
            XmlNode caexNode = null;

            foreach (XmlNode n in _xmlDocument.ChildNodes)
            {
                if (string.Equals(n.Name, "CAEXFile", StringComparison.OrdinalIgnoreCase))
                {
                    caexNode = n;
                    GetVendorName(n);
                    break;
                }
            }

            XmlNodeList caexChilds = caexNode?.ChildNodes ?? _xmlDocument.ChildNodes;

            foreach (XmlNode xmlNode in caexChilds)
            {
                if (string.Equals(xmlNode.Name, "InstanceHierarchy", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (XmlNode m in xmlNode.ChildNodes)
                    {
                        if (m.Name == "InternalElement")
                        {
                            var refAttr = m.Attributes?["RefBaseSystemUnitPath"];
                            if (refAttr != null && refAttr.Value == "MTPHMISUCLib/Picture")
                            {
                                found.Add(m);
                                // preserve first picture as _pictureNode for backward compatibility
                                //if (_pictureNode == null) _pictureNode = m;
                            }
                            else
                            {
                                // keep searching for CommunicationSet node (unchanged behavior)
                                foreach (XmlNode y in m.ChildNodes)
                                {
                                    if (y.Name == "InternalElement")
                                    {
                                        var x1 = y.Attributes?["RefBaseSystemUnitPath"];

                                        // for MTP 1.0 and 1.1
                                        if (x1 != null && x1.Value == "MTPSUCLib/CommunicationSet")
                                        {
                                            _communicationSetNode = y;
                                        }
                                        // for MTP 2.0
                                        else if (x1 != null && x1.Value == "MTPDataAssemblySUCLib/DataAssemblySet")
                                        {
                                            _dataAssemblySetNode.Add(y);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // populate the pictureNodes field for other consumers
            _pictureNodes.Clear();
            _pictureNodes.AddRange(found);
            return found;
        }

        private void GetVendorName(XmlNode node)
        {
            foreach (XmlNode xmlNode in node.ChildNodes)
            {
                if (xmlNode.Name.ToUpper() == "SourceDocumentInformation".ToUpper())
                {
                    if (xmlNode.Attributes["OriginVendor"] != null)
                    {
                        _vendorName = xmlNode.Attributes["OriginVendor"].Value;
                    }
                    break;
                }
            }
        }

        private List<MtpObject> ParseSemanticGroup(XmlNode node)
        {
            List<MtpObject> mtpObjects = new List<MtpObject>();

            foreach (XmlNode n in node.ChildNodes)
            {
                if (n.Name == "RoleRequirements")
                    continue;

                if (n.Name != "InternalElement")
                    continue;

                string refBasePath =
                    n.Attributes["RefBaseSystemUnitPath"]?.Value ?? string.Empty;

                // VisualObject
                if (refBasePath == "MTPHMISUCLib/VisualObject")
                {
                    String id = n.Attributes["ID"]?.Value ?? string.Empty;
                    MtpObject mtpObject = ParseVisualObject(n);
                    if (mtpObject != null)
                        mtpObject.ID = id;

                    if (mtpObject != null)
                        mtpObjects.Add(mtpObject);
                }

                // Nested SemanticGroup
                else if (refBasePath == "MTPHMISUCLib/SemanticGroup")
                {
                    mtpObjects.AddRange(ParseSemanticGroup(n));
                }
            }

            return mtpObjects;
        }


        private List<MtpObject> ParseSemanticGroupOld(XmlNode node)
        {
            List<MtpObject> mtpObjects = new List<MtpObject>();

            foreach (XmlNode n in node.ChildNodes)
            {
                if (n.Name == "RoleRequirements")
                {
                    continue;
                }
                if (n.Name == "InternalElement" && n.Attributes["RefBaseSystemUnitPath"].Value == "MTPHMISUCLib/VisualObject")
                {
                    MtpObject mtpObject = new MtpObject();
                    mtpObject.RefBaseSystemUnitPath = n.Attributes["RefBaseSystemUnitPath"].Value;
                    mtpObject.Name = n.Attributes["Name"].Value;
                    foreach (XmlNode loopNodeVar in n.ChildNodes)
                    {
                        XmlNode node10 = loopNodeVar;
                        if (node10.Name == "RoleRequirements" || node10.Name == "Description" || node10.Name == "SupportedRoleClass")
                        {
                            continue;
                        }

                        if (node10.Attributes["Name"].Value == "RefID")
                        {
                            mtpObject.RefID = GetValue(node10);
                        }
                        else if (node10.Attributes["Name"].Value == "eClassVersion")
                        {
                            mtpObject.EClassVersion = GetValue(node10);
                        }
                        else if (node10.Attributes["Name"].Value == "eClassClassificationClass")
                        {
                            string tempValue = GetValue(node10);
                            if (tempValue == null || tempValue.Trim() == string.Empty)
                            {
                                mtpObject.EClassClassificationClass = "";
                            }
                            else
                            {
                                int tempValue2 = 0;
                                tempValue2 = int.Parse(tempValue.Trim());
                                if (tempValue2 == 0)
                                {
                                    mtpObject.EClassClassificationClass = "";
                                }
                                else
                                {
                                    mtpObject.EClassClassificationClass = tempValue;
                                    _eClassesUsed.Add(tempValue2.ToString());
                                }
                            }
                        }

                        else if (node10.Attributes["Name"].Value == "Width")
                        {
                            string tempValue = GetValue(node10);
                            if (tempValue != string.Empty)
                            {
                                mtpObject.Width = int.Parse(tempValue);
                            }
                        }

                        else if (node10.Attributes["Name"].Value == "Height")
                        {
                            string tempValue = GetValue(node10);
                            if (tempValue != string.Empty)
                            {
                                mtpObject.Height = int.Parse(tempValue);
                            }
                        }
                        else if (node10.Attributes["Name"].Value == "X")
                        {
                            string tempValue = GetValue(node10);
                            if (tempValue != string.Empty)
                            {
                                mtpObject.X = int.Parse(tempValue);
                            }
                        }
                        else if (node10.Attributes["Name"].Value == "Y")
                        {
                            string tempValue = GetValue(node10);
                            if (tempValue != string.Empty)
                            {
                                mtpObject.Y = int.Parse(tempValue);
                            }
                        }
                        else if (node10.Attributes["Name"].Value == "ZIndex")
                        {
                            string tempValue = GetValue(node10);
                            if (tempValue != string.Empty)
                            {
                                mtpObject.ZIndex = int.Parse(tempValue);
                            }
                        }
                        else if (node10.Attributes["Name"].Value == "Rotation")
                        {
                            string tempValue = GetValue(node10);
                            if (tempValue != string.Empty)
                            {
                                mtpObject.Rotation = int.Parse(tempValue);
                            }
                        }
                    }
                    mtpObjects.Add(mtpObject);
                }
            }

            return mtpObjects;
        }

        private MtpObject ParseVisualObject(XmlNode n)
        {
            MtpObject mtpObject = new MtpObject();

            mtpObject.RefBaseSystemUnitPath = n.Attributes["RefBaseSystemUnitPath"]?.Value;

            mtpObject.Name = n.Attributes["Name"]?.Value;

            foreach (XmlNode node10 in n.ChildNodes)
            {
                if (node10.Name == "RoleRequirements" ||
                    node10.Name == "Description" ||
                    node10.Name == "SupportedRoleClass")
                {
                    continue;
                }

                string attributeName =
                    node10.Attributes["Name"]?.Value ?? string.Empty;

                switch (attributeName)
                {
                    case "RefID":
                        mtpObject.RefID = GetValue(node10);
                        break;

                    case "Width":
                        int.TryParse(GetValue(node10), out int width);
                        mtpObject.Width = width;
                        break;

                    case "Height":
                        int.TryParse(GetValue(node10), out int height);
                        mtpObject.Height = height;
                        break;

                    case "X":
                        int.TryParse(GetValue(node10), out int x);
                        mtpObject.X = x;
                        break;

                    case "Y":
                        int.TryParse(GetValue(node10), out int y);
                        mtpObject.Y = y;
                        break;

                    case "eClassVersion":
                        mtpObject.EClassVersion = GetValue(node10);
                        break;

                    case "eClassClassificationClass":
                        string tempValue = GetValue(node10);
                        if (tempValue == null || tempValue.Trim() == string.Empty)
                        {
                            mtpObject.EClassClassificationClass = "";
                        }
                        else
                        {
                            int tempValue2 = 0;
                            tempValue2 = int.Parse(tempValue.Trim());
                            if (tempValue2 == 0)
                            {
                                mtpObject.EClassClassificationClass = "";
                            }
                            else
                            {
                                mtpObject.EClassClassificationClass = tempValue;
                                _eClassesUsed.Add(tempValue2.ToString());
                            }
                        }
                        break;

                    case "ZIndex":
                        string tempValue1 = GetValue(node10);
                        if (tempValue1 != string.Empty)
                        {
                            mtpObject.ZIndex = int.Parse(tempValue1);
                        }
                        break;

                    case "Rotation":

                        string tempValue3 = GetValue(node10);
                        if (tempValue3 != string.Empty)
                        {
                            mtpObject.Rotation = int.Parse(tempValue3);
                        }
                        break;
                }
            }

            return mtpObject;
        }

        private void ParseDataAssemblySetNode()
        {
            XmlNode dataAssemblySetNode = _dataAssemblySetNode.First();

            var ns = new XmlNamespaceManager(_xmlDocument.NameTable);
            ns.AddNamespace("caex", "http://www.dke.de/CAEX");

            //string aspectRef = dataAssemblySetNode.SelectSingleNode(".//caex:ExternalInterface[@Name='AspectRef']/" + "caex:Attribute[@Name='AspectRef']/" + "caex:Value", ns)?.InnerText;
            string aspectRef = dataAssemblySetNode.SelectSingleNode(".//caex:Attribute[@Name='AspectRef']/caex:Value", ns)?.InnerText;

            if (!string.IsNullOrEmpty(aspectRef))
            {
                XmlNode instanceHierarchy = _xmlDocument.SelectSingleNode($"//caex:InstanceHierarchy[@ID='{aspectRef}']", ns);

                if (instanceHierarchy != null)
                {
                    XmlNodeList internalElements = instanceHierarchy?.SelectNodes("./caex:InternalElement", ns);

                    foreach (XmlNode internalElement in internalElements)
                    {
                        string path = internalElement.Attributes["RefBaseSystemUnitPath"]?.Value;

                        if (string.IsNullOrEmpty(path))
                        {
                            continue;
                        }

                        if (path.EndsWith("/PeaInformationLabel"))
                        {
                            ParsePeaInformationLabel(internalElement, ns);
                        }
                        else if (path.EndsWith("/WebServerUrlInfo"))
                        {
                            ParseWebServerUrlInfo(internalElement, ns);
                        }

                        else if (path.Contains("/IndicatorElement/"))
                        {
                            AddElement(internalElement, _indicatorElementList);
                        }
                        else if (path.Contains("/ActiveElement/"))
                        {
                            AddElement(internalElement, _activeElementList);
                        }
                        else if (path.Contains("/OperationElement/"))
                        {
                            AddElement(internalElement, _operationElementList);
                        }
                        else if (path.Contains("/ServiceElement/ParameterElement/"))
                        {
                            AddElement(internalElement, _parameterElementList);
                        }

                        else if (path.Contains("/InputElement/"))
                        {
                            AddElement(internalElement, _inputElementList);
                        }

                        else if (path.Contains("/ServiceElement/"))
                        {
                            AddElement(internalElement, _serviceElementList);
                        }

                        else if (path.Contains("/DiagnosticElement/"))
                        {
                            AddElement(internalElement, _diagnosticElementList);
                        }
                    }
                }
            }
        }

        private void parseDataAssemblyItems()
        {
            var doc1 = CAEXDocument.LoadFromFile(_amlFileName).CAEXFile;
            foreach (InstanceHierarchyType ih in doc1.InstanceHierarchy)
            {
                if (ih.Name == "ModuleTypePackage")
                {
                    var it = ih.InternalElement[0].InternalElement.FirstOrDefault(ie => ie.RefBaseSystemUnitPath.Equals("MTPSUCLib/CommunicationSet"));
                    if (it != null)
                    {
                        foreach (InternalElementType iele in it)
                        {
                            if (iele.Name == "InstanceList")
                            {
                                foreach (InternalElementType temp1 in iele.InternalElement)
                                {
                                    if (temp1.Attribute["V"] != null) // for AnaMon etc
                                    {
                                        string refId = temp1.Attribute["RefID"].Value;
                                        string valueAttribute = temp1.Attribute["V"].Value!;
                                        dataAssemblyDictionary.Add(refId, valueAttribute);
                                    }
                                    else if (temp1.Attribute["Pos"] != null) // Control Valves
                                    {
                                        string refId = temp1.Attribute["RefID"].Value;
                                        string valueAttribute = temp1.Attribute["Pos"].Value!;
                                        dataAssemblyDictionary.Add(refId, valueAttribute);
                                    }
                                }

                                break;
                            }
                        }
                    }
                    else
                    {
                        mtpErrors.Add("MTP ERROR -> CommunicationSet not found in the MTP file.");
                    }
                    break;
                }
            }
        }

        private void ParsePeaInformationLabel(XmlNode node, XmlNamespaceManager ns)
        {
            PeaInformation = new PeaInformation
            {
                RefID = GetAttributeValue(node, "RefID", ns),
                TagName = GetAttributeValue(node, "TagName", ns),
                TagDescription = GetAttributeValue(node, "TagDescription", ns),
                AssetId = GetAttributeValue(node, "AssetId", ns),
                ComponentName = GetAttributeValue(node, "ComponentName", ns),
                DeviceClass = GetAttributeValue(node, "DeviceClass", ns),
                DeviceHealth = GetAttributeValue(node, "DeviceHealth", ns),
                DeviceRevision = GetAttributeValue(node, "DeviceRevision", ns),
                HardwareRevision = GetAttributeValue(node, "HardwareRevision", ns),
                Manufacturer = GetAttributeValue(node, "Manufacturer", ns),
                ManufacturerUri = GetAttributeValue(node, "ManufacturerUri", ns),
                Model = GetAttributeValue(node, "Model", ns),
                ProductCode = GetAttributeValue(node, "ProductCode", ns),
                ProductInstanceUri = GetAttributeValue(node, "ProductInstanceUri", ns),
                RevisionCounter = GetAttributeValue(node, "RevisionCounter", ns),
                SerialNumber = GetAttributeValue(node, "SerialNumber", ns),
                SoftwareRevision = GetAttributeValue(node, "SoftwareRevision", ns)
            };
        }

        private void ParseWebServerUrlInfo(XmlNode node, XmlNamespaceManager ns)
        {
            WebServerInfo = new WebServerInfo
            {
                Url = GetAttributeValue(node, "Url", ns),
                RefID = GetAttributeValue(node, "RefID", ns),
                TagName = GetAttributeValue(node, "TagName", ns),
                TagDescription = GetAttributeValue(node, "TagDescription", ns)
            };
        }

        private string GetAttributeValue(XmlNode element, string attributeName, XmlNamespaceManager ns)
        {
            return element.SelectSingleNode($"./caex:Attribute[@Name='{attributeName}']/caex:Value", ns)?.InnerText;
        }

        private void AddElement(XmlNode internalElement, ICollection<ElementInfo> targetCollection)
        {
            string path = internalElement.Attributes["RefBaseSystemUnitPath"]?.Value;
            string symbolName = path?.Split('/').Last();

            XmlElement refIdAttribute = internalElement.ChildNodes.OfType<XmlElement>().FirstOrDefault(x => x.Name == "Attribute" && x.Attributes["Name"]?.Value == "RefID");

            if (refIdAttribute == null)
            {
                return;
            }

            var elementInfo = new ElementInfo
            {
                Name = internalElement.Attributes["Name"]?.Value,
                SymbolName = symbolName,
                RefId = GetValue(refIdAttribute),
                Value = refIdAttribute.Value
            };

            targetCollection.Add(elementInfo);
        }

        private string GetValue(XmlNode xmlNode)
        {
            if (xmlNode.ChildNodes.Count == 0)
            {
                return "";
            }
            foreach (XmlNode xmlNode1 in xmlNode.ChildNodes)
            {
                if (xmlNode1.Name == "Value")
                {
                    return xmlNode1.InnerText;
                }
            }
            return "";
        }

        private void ParseCommunicationSetNode()
        {
            if (_communicationSetNode == null)
            {
                return;
            }

            foreach (XmlElement xmlNode in _communicationSetNode)
            {
                if (xmlNode.Name == "RoleRequirements")
                {
                    continue;
                }
                if (xmlNode.Name == "InternalElement" && xmlNode.Attributes["RefBaseSystemUnitPath"].Value == "MTPSUCLib/CommunicationSet/InstanceList")
                {
                    foreach (XmlElement x in xmlNode.ChildNodes)
                    {
                        if (x.Name == "RoleRequirements" || x.Name != "InternalElement")
                        {
                            continue;
                        }

                        string path = x.Attributes["RefBaseSystemUnitPath"]?.Value;

                        if (path.Contains("/IndicatorElement/"))
                        {
                            AddElement(x, _indicatorElementList);
                        }

                        else if (path.Contains("/ActiveElement/"))
                        {
                            AddElement(x, _activeElementList);
                        }

                        else if (path.Contains("/ServiceElement/ParameterElement/"))
                        {
                            AddElement(x, _parameterElementList);
                        }

                        else if (path.Contains("/ServiceElement/"))
                        {
                            AddElement(x, _serviceElementList);
                        }

                        else if (path.Contains("/InputElement/"))
                        {
                            AddElement(x, _inputElementList);
                        }

                        else if (path.Contains("/OperationElement/"))
                        {
                            AddElement(x, _operationElementList);
                        }

                        else if (path.Contains("/DiagnosticElement/"))
                        {
                            AddElement(x, _diagnosticElementList);
                        }

                        else if (path.Contains("/ServiceControl"))
                        {
                            AddElement(x, _serviceElementList);
                        }
                    }
                }
            }
        }

        private void GetMtpVersion(String fileName)
        {
            string version = _xmlDocument.SelectSingleNode("//*[local-name()='Document']")?.Attributes["Version"]?.Value;

            if (version != null)
            {
                _mtpVersion = int.Parse(version.Split('.')[0]);
            }
        }

        private int Scale(int value) => (int)(value * scaleFactor);
        string imagePath = "";
        double scaleFactor = 1.0;

        XmlDocument _xmlDocument = new XmlDocument();
        private string _amlFileName = "";
        List<PictureObject> mtpPictureObjects = new List<PictureObject>();
        private static readonly List<ElementInfo> _indicatorElementList = new();
        private static readonly List<ElementInfo> _activeElementList = new();
        private static readonly List<ElementInfo> _parameterElementList = new();
        private static readonly List<ElementInfo> _operationElementList = new();
        private static readonly List<ElementInfo> _inputElementList = new();
        private static readonly List<ElementInfo> _serviceElementList = new();
        private static readonly List<ElementInfo> _diagnosticElementList = new();
        Dictionary<string, string> dataAssemblyDictionary = new Dictionary<string, string>();
        List<GraphicsPath> _graphicsPathsForPipe = new List<GraphicsPath>();
        List<GraphicsPath> _graphicsPathsForMeasurementLine = new List<GraphicsPath>();
        List<GraphicsPath> _graphicsPathsForFunctionLine = new List<GraphicsPath>();
        XmlNode _communicationSetNode; // used in MTP 1.0 and MTP 1.1
        List<XmlNode> _dataAssemblySetNode = new List<XmlNode>(); // used in MTP 2.0
        public WebServerInfo WebServerInfo { get; private set; }
        public PeaInformation PeaInformation { get; private set; }
        int _mtpVersion = 0;
        List<String> mtpErrors = new List<string>();
        List<string> mtpWarnings = new List<string>();
        List<String> _eClassesUsed = new List<string>();

        string _vendorName = "";
        List<XmlNode> _pictureNodes = new List<XmlNode>();
        public TreeNode SelectedNode { get; set; }
    }
}
