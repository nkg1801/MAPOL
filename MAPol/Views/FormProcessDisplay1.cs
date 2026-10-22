using Aml.Engine.Adapter;
using Aml.Engine.AmlObjects;
using Aml.Engine.CAEX;
using MAPol.Models;
using MAPol.Views;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;

namespace MAPol
{
    public partial class FormProcessDisplay1 : Form
    {
        public FormProcessDisplay1()
        {
            InitializeComponent();

        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.Clear(BackColor);

            // draw the lines
            using (System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Color.DarkGray, 6))
            {
                foreach (GraphicsPath path in _graphicsPathsForPipe)
                {
                    PointF[] temp = path.PathPoints;
                    e.Graphics.DrawLines(pen, temp);
                }
            }

            using (System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Color.DarkGray, 4))
            {
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                foreach (GraphicsPath path in _graphicsPathsForMeasurementLine)
                {
                    PointF[] temp = path.PathPoints;
                    e.Graphics.DrawLines(pen, temp);
                }
            }

            using (System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Color.LightBlue, 3))
            {
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                foreach (GraphicsPath path in _graphicsPathsForFunctionLine)
                {
                    PointF[] temp = path.PathPoints;
                    e.Graphics.DrawLines(pen, temp);
                }
            }

            //Render port object
            using (Pen pen = new System.Drawing.Pen(System.Drawing.Color.DarkGray, 4))
            {
                foreach (MtpPortObject mtpPortObject in mtpPortObjects)
                {
                    if (mtpPortObject.RefBaseSystemUnitPath == "MTPHMISUCLib/PortObject/Nozzle")
                    {
                        e.Graphics.FillEllipse(new SolidBrush(System.Drawing.Color.DarkGray), mtpPortObject.X + leftMargin - 5, mtpPortObject.Y + topMargin - 5, 10, 10);
                    }
                    else if (mtpPortObject.RefBaseSystemUnitPath == "MTPHMISUCLib/PortObject/MeasurementPoint")
                    {
                        e.Graphics.FillEllipse(new SolidBrush(System.Drawing.Color.DarkGray), mtpPortObject.X + leftMargin - 5, mtpPortObject.Y + topMargin - 5, 10, 10);
                    }
                    else if (mtpPortObject.RefBaseSystemUnitPath == "MTPHMISUCLib/PortObject/LogicalPort")
                    {
                        e.Graphics.FillEllipse(new SolidBrush(System.Drawing.Color.LightBlue), mtpPortObject.X + leftMargin - 4, mtpPortObject.Y + topMargin - 4, 10, 8);
                    }
                }
            }

            //Render Junction object
            using (System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Color.Black, 4))
            {
                Trace.WriteLine("MTP Renderer -> Number of Junction Objects: " + mtpJunctionObjects.Count);
                foreach (MtpJunctionObject mtpJunctionObject in mtpJunctionObjects)
                {
                    e.Graphics.FillEllipse(new SolidBrush(System.Drawing.Color.DarkGray), mtpJunctionObject.X + leftMargin - 10, mtpJunctionObject.Y + topMargin - 10, 20, 20);
                    e.Graphics.FillEllipse(new SolidBrush(System.Drawing.Color.Black), mtpJunctionObject.X + leftMargin - 5, mtpJunctionObject.Y + topMargin - 5, 10, 10);
                }
            }

            RepositionVendorAndDisplayName();
        }

        private void FormProcessDisplay1_Load(object sender, EventArgs e)
        {
            OpenFile(mtpFileName);
            isOnline = true;
            //timerReadTimer.Enabled = false;
            //timer1.Enabled = false;
        }

        private void CreateSourceObject(MtpSourceObject mtpSourceObject)
        {
            Button button = new Button();
            button.Left = mtpSourceObject.X + leftMargin;
            button.Top = mtpSourceObject.Y + topMargin;
            button.Width = 100;
            button.Height = 50;
            button.Text = mtpSourceObject.Name;
            Controls.Add(button);
        }

        private void CreateServiceControl(MtpObject mtpObject)
        {
            Button button = new Button();
            button.Left = Convert.ToInt32((mtpObject.X + leftMargin) * scaleFactor);
            button.Top = mtpObject.Y + topMargin;
            button.Width = 100;
            button.Height = 50;
            button.Text = mtpObject.Name;
            button.BackColor = Color.DarkGray;
            Controls.Add(button);
        }

        private void CreateSinkObject(MtpSinkObject mtpObject)
        {
            Button button = new Button();
            button.Left = mtpObject.X + leftMargin;
            button.Top = mtpObject.Y + topMargin;
            button.Width = 100;
            button.Height = 50;
            button.Text = mtpObject.Name;
            Controls.Add(button);
        }

        private void CreatePictureBox(MtpObject mtpObject)
        {
            string imageFileName = imagePath + "\\" + _defaultImageFileName;

            if (mtpObject.EClassClassificationClass != null && mtpObject.EClassClassificationClass.Length != 0)
            {
                int eClassNumber = int.Parse(mtpObject.EClassClassificationClass);
                if (_eClassMappingForMD.ContainsKey(eClassNumber))
                {
                    imageFileName = _eClassMappingForMD[eClassNumber];
                }
                else
                {
                    _missingEClassesMapping.Add(mtpObject.EClassClassificationClass);
                    //imageFileName = _defaultImageFileName;
                    string visualElement = FindReferencedElement(mtpObject.RefID);
                    if (visualElement != string.Empty && !visualElement.Equals("ServiceControl"))
                    {
                        imageFileName = visualElement;
                    }
                }

                imageFileName = imagePath + imageFileName;
                if (!File.Exists(imageFileName))
                {
                    imageFileName = imagePath + "\\" + _defaultImageFileName;
                }
            }
            else
            {
                Trace.WriteLine("MTP Renderer -> Missing eClass for: " + mtpObject.Name);
                _missingEClasses.Add(mtpObject.Name);
                if (mtpObject.RefID != null)
                {
                    string visualElement = FindReferencedElement(mtpObject.RefID);
                    if(visualElement.Equals("ServiceControl"))
                    {
                        CreateServiceControl(mtpObject);
                        return;
                    }
                    if (visualElement != string.Empty)
                    {
                        imageFileName = imagePath + visualElement;
                    }
                    else
                    {
                        imageFileName = imagePath + "\\" + _defaultImageFileName;
                    }
                }
            }

            PictureBox pictureBox = new PictureBox();
            pictureBox.Name = mtpObject.RefID;
            ToolTip toolTip = new ToolTip();
            toolTip.Tag = mtpObject.RefID;
            //string toolTipText = makeTooltipText(mtpObject.Name, mtpObject.EClassClassificationClass, mtpObject.X, mtpObject.Y, mtpObject.Width, mtpObject.Height);
            string toolTipText = mtpObject.Name;
            toolTip.SetToolTip(pictureBox, toolTipText);
            Controls.Add(pictureBox);

            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            MyImage = new Bitmap(imageFileName);
            MyImage.RotateFlip(GetFlipType(mtpObject.Rotation));
            pictureBox.ClientSize = new System.Drawing.Size(mtpObject.Width, mtpObject.Height);
            pictureBox.Location = new System.Drawing.Point(mtpObject.X + leftMargin, mtpObject.Y + topMargin);
            pictureBox.Image = MyImage;
            pictureBox.SendToBack();
            //RotateTransform rotateTransform = new RotateTransform(mtpObject.Rotation);
        }

        private RotateFlipType GetFlipType(int angle)
        {
            switch (angle)
            {
                case 0:
                case 360:
                default:
                    return RotateFlipType.RotateNoneFlipNone;
                case 90:
                    return RotateFlipType.Rotate90FlipNone;
                case 180:
                    return RotateFlipType.Rotate180FlipNone;
                case 270:
                    return RotateFlipType.Rotate270FlipNone;
            }
        }

        private string FindReferencedElement(string refId)
        {
            var collections = new[]
            {
                _indicatorElementList,
                _activeElementList,
                _serviceElementList,
                _inputElementList,
                _operationElementList,
                _parameterElementList,
                _diagnosticElementList
            };

            foreach (var collection in collections)
            {
                ElementInfo matchingElement = collection.FirstOrDefault(x => x.RefId == refId);

                if (matchingElement == null)
                {
                    continue;
                }

                if (SymbolImages.TryGetValue(matchingElement.SymbolName, out string? image))
                {
                    return image;
                }

                _missingSymbols.Add(matchingElement.Name);
                return "default.png";
            }

            return string.Empty;
        }

        private string FindReferencedElementOld(string refId)
        {
            string referencedElement = "";
            foreach (string s in _indicatorElements)
            {
                if (s.Contains(refId))
                {
                    string temp = s.Split(',')[1];
                    switch (temp)
                    {
                        case "AnaView":
                            return "27209090.png";

                        case "BinView":
                            return "BinaryIndicator.png";

                        case "BinMon":
                            return "BinaryIndicator.png";

                        case "AnaMon":
                            return "AnalogIndicator.emf";

                        case "DIntView":
                            return "ArithmeticOperator.emf";

                        case "DIntMon":
                            return "ArithmeticOperator.emf";

                        case "StringView":
                            return "BinaryIndicator.png";

                        default:
                            _missingSymbols.Add(s.Split(',')[0]);
                            return "default.png";
                    }
                }
            }

            foreach (string s in _activeElements)
            {
                if (s.Contains(refId))
                {
                    string temp = s.Split(',')[1];
                    switch (temp)
                    {
                        case "BinVlv":
                            return "BlockValve.emf";

                        case "MonBinVlv":
                            return "BlockValve.emf";

                        case "AnaVlv":
                            return "BlockValve.emf";

                        case "MonBinDrv":
                            return "Mixer.emf";

                        case "AnaDrv":
                            return "pump.emf";

                        case "MonAnaDrv":
                            return "pump.emf";

                        case "PIDCtrl":
                            return "pidctrl.png";

                        case "MonAnaVlv":   
                            return "BlockValve.emf";


                        default:
                            _missingSymbols.Add(s.Split(',')[0]);
                            return "default.png";
                    }
                }
            }

            foreach (string s in _serviceElements)
            {
                if (s.Contains(refId))
                {
                    string temp = s.Split(',')[1];
                    switch (temp)
                    {
                        case "ServiceControl":
                            //return "service_control_button.png";
                            return "ServiceControl";

                        default:
                            _missingSymbols.Add(s.Split(',')[0]);
                            return "default.png";
                    }
                }
            }

            foreach (string s in _inputElements)
            {
                if (s.Contains(refId))
                {
                    string temp = s.Split(',')[1];
                    switch (temp)
                    {
                        case "BinProcessValueIn":
                            return "BlockValve.emf";

                        case "AnaProcessValueIn":
                            return "BlockValve.emf";

                        case "DIntProcessValueIn":
                            return "BlockValve.emf";

                        case "StringProcessValueIn":
                            return "BlockValve.emf";

                        default:
                            _missingSymbols.Add(s.Split(',')[0]);
                            return "default.png";
                    }
                }
            }

            foreach (string s in _operationElements)
            {
                if (s.Contains(refId))
                {
                    string temp = s.Split(',')[1];
                    switch (temp)
                    {
                        case "DIntManInt":
                            return "BlockValve.emf";

                        case "DIntMan":
                            return "BlockValve.emf";

                        case "AnaManInt":
                            return "BlockValve.emf";

                        case "AnaMan":
                            return "BlockValve.emf";

                        case "BinManInt":
                            return "BinaryIndicator.png";

                        default:
                            _missingSymbols.Add(s.Split(',')[0]);
                            return "default.png";
                    }
                }
            }

            foreach (string s in _parameterElements)
            {
                if (s.Contains(refId))
                {
                    string temp = s.Split(',')[1];
                    switch (temp)
                    {
                        case "DIntServParam":
                            return "BlockValve.emf";

                        case "BinServParam":
                            return "BlockValve.emf";

                        case "AnaServParam":
                            return "BlockValve.emf";

                        case "StringServParam":
                            return "BlockValve.emf";

                        default:
                            _missingSymbols.Add(s.Split(',')[0]);
                            return "default.png";
                    }
                }
            }

            foreach (string s in _diagnosticElements)
            {
                if (s.Contains(refId))
                {
                    string temp = s.Split(',')[1];
                    switch (temp)
                    {
                        case "LockView4":
                        case "LockView8":
                        case "LockView16":
                            return "InterlockView.emf";

                        default:
                            _missingSymbols.Add(s.Split(',')[0]);
                            return "default.png";
                    }
                }
            }

            return referencedElement;
        }

        private string FindInCollection(IEnumerable<string> collection)
        {
            foreach (string item in collection)
            {
                string[] parts = item.Split(',');

                if (parts.Length < 2)
                {
                    continue;
                }

                string symbolType = parts[1];

                if (SymbolImages.TryGetValue(symbolType, out string image))
                {
                    return image;
                }

                _missingSymbols.Add(parts[0]);
                return "default.png";
            }

            return null;
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

        private void parseMtpXml2(string fileName)
        {
            mtpObjects.Clear();
            mtpConnectionObjects.Clear();
            mtpSourceObjects.Clear();
            mtpPortObjects.Clear();
            mtpSinkObjects.Clear();
            mtpJunctionObjects.Clear();
            mtpPictureObjects.Clear();

            var pictures = GetPictureNodes(fileName);
            if (pictures == null || pictures.Count == 0)
            {
                System.Windows.Forms.MessageBox.Show("The MTP does not contain any process display aka Picture Node");
                return;
            }

            // Use combined display names (or you can choose the first picture's name)
            _displayName = string.Join(", ", pictures.Select(p => p.Attributes["Name"]?.Value).Where(n => !string.IsNullOrEmpty(n)));

            // Parse each Picture node and aggregate elements into shared lists
            foreach (XmlNode pictureNode in pictures)
            {
                PictureObject pictureObject = new PictureObject();
                foreach (XmlNode node in pictureNode.ChildNodes)
                {
                    XmlNode temp = node;
                    string temp1;
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
                        }
                        else if (attrValue == "MTPHMISUCLib/VisualObject")
                        {
                            MtpObject mtpObject = new MtpObject();
                            mtpObject.RefBaseSystemUnitPath = attrValue;
                            mtpObject.Name = node?.Attributes?["Name"]?.Value;
                            foreach (XmlNode loopNodeVar in node.ChildNodes)
                            {
                                XmlNode node10 = loopNodeVar;
                                if (node10.Name == "RoleRequirements" || node10.Name == "Description" || node10.Name == "SupportedRoleClass")
                                {
                                    continue;
                                }

                                if (node10?.Attributes?["Name"]?.Value == "RefID")
                                {
                                    mtpObject.RefID = GetValue(node10);
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
                                        mtpObject.Width = int.Parse(tempValue);
                                    }
                                }

                                else if (node10.Attributes["Name"]?.Value == "Height")
                                {
                                    string tempValue = GetValue(node10);
                                    if (tempValue != string.Empty)
                                    {
                                        mtpObject.Height = int.Parse(tempValue);
                                    }
                                }
                                else if (node10.Attributes["Name"]?.Value == "X")
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
                                    mtpPortObjects.Add(mtpPortObject);
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
                                    mtpPortObjects.Add(mtpPortObject);
                                    pictureObject.MtpPortObjects.Add(mtpPortObject);
                                }
                            }
                            mtpConnectionObjects.Add(connectionObject);
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
                                    mtpPortObjects.Add(mtpPortObject); //todo: this can be removed later if the line below works
                                    pictureObject.MtpPortObjects.Add(mtpPortObject);
                                }
                            }
                            mtpSinkObjects.Add(mtpSinkObject); //todo: this can be removed later if the line below works
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
                            mtpSourceObjects.Add(mtpSourceObject);
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
                            mtpSinkObjects.Add(mtpSinkObject);
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
                            mtpPortObjects.Add(mtpPortObject);
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
                            mtpJunctionObjects.Add(mtpJunctionObject);
                            pictureObject.MtpJunctionObjects.Add(mtpJunctionObject);
                        }
                    }
                }
                mtpPictureObjects.Add(pictureObject); // Add the parsed picture object to the list
            }
        }

        private List<XmlNode> GetPictureNodes(string fileName)
        {
            //XmlDocument _xmlDocument = new XmlDocument();
            //_xmlDocument.Load(fileName);

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
            pictureNodes.Clear();
            pictureNodes.AddRange(found);
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

        private void ParseDataAssemblySetNode(String fileName)
        {
            //XmlDocument _xmlDocument = new XmlDocument();
            //_xmlDocument.Load(fileName);

            XmlNode dataAssemblySetNode = _dataAssemblySetNode.First();

            var ns = new XmlNamespaceManager(_xmlDocument.NameTable);
            ns.AddNamespace("caex", "http://www.dke.de/CAEX");

            string aspectRef = dataAssemblySetNode.SelectSingleNode(".//caex:ExternalInterface[@Name='AspectRef']/" + "caex:Attribute[@Name='AspectRef']/" + "caex:Value", ns)?.InnerText;

            if (!string.IsNullOrEmpty(aspectRef))
            {
                XmlNode instanceHierarchy = _xmlDocument.SelectSingleNode($"//caex:InstanceHierarchy[@ID='{aspectRef}']",ns);

                if (instanceHierarchy != null)
                {
                    XmlNodeList internalElements = instanceHierarchy?.SelectNodes("./caex:InternalElement", ns);

                    foreach (XmlNode internalElement in internalElements)
                    {
                        string name = internalElement.Attributes["Name"]?.Value;
                        string id = internalElement.Attributes["ID"]?.Value;

                        string path = internalElement.Attributes["RefBaseSystemUnitPath"]?.Value;

                        if (path.Contains("/IndicatorElement/"))
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

        private void AddElementOld(XmlNode internalElement, ICollection<string> targetCollection)
        {
            string path = internalElement.Attributes["RefBaseSystemUnitPath"]?.Value;
            string symbolName = path?.Split('/').Last();

            XmlElement refIdAttribute = internalElement.ChildNodes.OfType<XmlElement>().FirstOrDefault(x => x.Name == "Attribute" && x.Attributes["Name"]?.Value == "RefID");

            if (refIdAttribute == null)
            {
                return;
            }

            string value = $"{internalElement.Attributes["Name"]?.Value}," + $"{symbolName}," + $"{refIdAttribute.Value}{GetValue(refIdAttribute)}";
            targetCollection.Add(value);
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
                        if (x.Name == "RoleRequirements")
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

        private void ParseCommunicationSetNodeOld()
        {
            if(_communicationSetNode == null)
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
                        if (x.Name == "RoleRequirements")
                        {
                            continue;
                        }

                        string path = x.Attributes["RefBaseSystemUnitPath"]?.Value;

                        if (x.Name == "InternalElement" && x.Attributes["RefBaseSystemUnitPath"].Value == "MTPDataObjectSUCLib/DataAssembly/IndicatorElement/AnaView")
                        {
                            string temp = x.Attributes["Name"].Value + ",AnaView";
                            foreach (XmlElement y in x)
                            {
                                if (y.Name == "Attribute" && y.Attributes["Name"].Value == "RefID")
                                {
                                    temp = temp + "," + y.Value + GetValue(y);
                                    _indicatorElements.Add(temp);
                                    //_indicatorElementList.Add(temp);
                                }
                            }
                        }
                        else if (x.Name == "InternalElement" && x.Attributes["RefBaseSystemUnitPath"].Value == "MTPDataObjectSUCLib/DataAssembly/IndicatorElement/AnaView/AnaMon")
                        {
                            string temp = x.Attributes["Name"].Value + ",AnaMon";
                            foreach (XmlElement y in x)
                            {
                                if (y.Name == "Attribute" && y.Attributes["Name"].Value == "RefID")
                                {
                                    temp = temp + "," + y.Value + GetValue(y);
                                    _indicatorElements.Add(temp);
                                }
                            }
                        }
                        else if (x.Name == "InternalElement" && x.Attributes["RefBaseSystemUnitPath"].Value == "MTPDataObjectSUCLib/DataAssembly/IndicatorElement/BinView")
                        {
                            string temp = x.Attributes["Name"].Value + ",BinView";
                            foreach (XmlElement y in x)
                            {
                                if (y.Name == "Attribute" && y.Attributes["Name"].Value == "RefID")
                                {
                                    temp = temp + "," + y.Value + GetValue(y);
                                    _indicatorElements.Add(temp);
                                }
                            }
                        }
                        else if (x.Name == "InternalElement" && x.Attributes["RefBaseSystemUnitPath"].Value == "MTPDataObjectSUCLib/DataAssembly/IndicatorElement/BinView/BinMon")
                        {
                            string temp = x.Attributes["Name"].Value + ",BinMon";
                            foreach (XmlElement y in x)
                            {
                                if (y.Name == "Attribute" && y.Attributes["Name"].Value == "RefID")
                                {
                                    temp = temp + "," + y.Value + GetValue(y);
                                    _indicatorElements.Add(temp);
                                }
                            }
                        }
                        else if (x.Name == "InternalElement" && x.Attributes["RefBaseSystemUnitPath"].Value == "MTPDataObjectSUCLib/DataAssembly/IndicatorElement/DIntView")
                        {
                            string temp = x.Attributes["Name"].Value + ",DIntView";
                            foreach (XmlElement y in x)
                            {
                                if (y.Name == "Attribute" && y.Attributes["Name"].Value == "RefID")
                                {
                                    temp = temp + "," + y.Value + GetValue(y);
                                    _indicatorElements.Add(temp);
                                }
                            }
                        }

                        // active elements
                        else if (x.Name == "InternalElement" && x.Attributes["RefBaseSystemUnitPath"].Value == "MTPDataObjectSUCLib/DataAssembly/IndicatorElement/DIntView/DIntMon")
                        {
                            string temp = x.Attributes["Name"].Value + ",DIntMon";
                            foreach (XmlElement y in x)
                            {
                                if (y.Name == "Attribute" && y.Attributes["Name"].Value == "RefID")
                                {
                                    temp = temp + "," + y.Value + GetValue(y);
                                    _indicatorElements.Add(temp);
                                }
                            }
                        }
                        else if (x.Name == "InternalElement" && x.Attributes["RefBaseSystemUnitPath"].Value == "MTPDataObjectSUCLib/DataAssembly/ActiveElement/BinVlv/MonBinVlv")
                        {
                            string temp = x.Attributes["Name"].Value + ",MonBinVlv";
                            foreach (XmlElement y in x)
                            {
                                if (y.Name == "Attribute" && y.Attributes["Name"].Value == "RefID")
                                {
                                    temp = temp + "," + y.Value + GetValue(y);
                                    _activeElements.Add(temp);
                                }
                            }
                        }
                        else if (x.Name == "InternalElement" && x.Attributes["RefBaseSystemUnitPath"].Value == "MTPDataObjectSUCLib/DataAssembly/ActiveElement/BinDrv/MonBinDrv")
                        {
                            string temp = x.Attributes["Name"].Value + ",MonBinDrv";
                            foreach (XmlElement y in x)
                            {
                                if (y.Name == "Attribute" && y.Attributes["Name"].Value == "RefID")
                                {
                                    temp = temp + "," + y.Value + GetValue(y);
                                    _activeElements.Add(temp);
                                }
                            }
                        }
                        else if (x.Name == "InternalElement" && x.Attributes["RefBaseSystemUnitPath"].Value == "MTPDataObjectSUCLib/DataAssembly/ActiveElement/AnaVlv")
                        {
                            string temp = x.Attributes["Name"].Value + ",AnaVlv";
                            foreach (XmlElement y in x)
                            {
                                if (y.Name == "Attribute" && y.Attributes["Name"].Value == "RefID")
                                {
                                    temp = temp + "," + y.Value + GetValue(y);
                                    _activeElements.Add(temp);
                                }
                            }
                        }

                        else if (x.Name == "InternalElement" && x.Attributes["RefBaseSystemUnitPath"].Value == "MTPDataObjectSUCLib/DataAssembly/ActiveElement/AnaVlv/MonAnaVlv")
                        {
                            string temp = x.Attributes["Name"].Value + ",MonAnaVlv";
                            foreach (XmlElement y in x)
                            {
                                if (y.Name == "Attribute" && y.Attributes["Name"].Value == "RefID")
                                {
                                    temp = temp + "," + y.Value + GetValue(y);
                                    _activeElements.Add(temp);
                                }
                            }
                        }

                        else if (x.Name == "InternalElement" && x.Attributes["RefBaseSystemUnitPath"].Value == "MTPDataObjectSUCLib/DataAssembly/ActiveElement/AnaDrv")
                        {
                            string temp = x.Attributes["Name"].Value + ",AnaDrv";
                            foreach (XmlElement y in x)
                            {
                                if (y.Name == "Attribute" && y.Attributes["Name"].Value == "RefID")
                                {
                                    temp = temp + "," + y.Value + GetValue(y);
                                    _activeElements.Add(temp);
                                }
                            }
                        }

                        else if (x.Name == "InternalElement" && x.Attributes["RefBaseSystemUnitPath"].Value == "MTPDataObjectSUCLib/DataAssembly/ActiveElement/AnaDrv/MonAnaDrv")
                        {
                            string temp = x.Attributes["Name"].Value + ",MonAnaDrv";
                            foreach (XmlElement y in x)
                            {
                                if (y.Name == "Attribute" && y.Attributes["Name"].Value == "RefID")
                                {
                                    temp = temp + "," + y.Value + GetValue(y);
                                    _activeElements.Add(temp);
                                }
                            }
                        }

                        else if (x.Name == "InternalElement" && x.Attributes["RefBaseSystemUnitPath"].Value == "MTPDataObjectSUCLib/DataAssembly/ActiveElement/PIDCtrl")
                        {
                            string temp = x.Attributes["Name"].Value + ",PIDCtrl";
                            foreach (XmlElement y in x)
                            {
                                if (y.Name == "Attribute" && y.Attributes["Name"].Value == "RefID")
                                {
                                    temp = temp + "," + y.Value + GetValue(y);
                                    _activeElements.Add(temp);
                                }
                            }
                        }


                        //MonAnaVlv

                        // service elements

                        else if (x.Name == "InternalElement" && ((x.Attributes["RefBaseSystemUnitPath"].Value == "MTPDataObjectSUCLib/DataAssembly/ServiceElement/ServiceControl")
                            || (x.Attributes["RefBaseSystemUnitPath"].Value == "MTPDataObjectSUCLib/DataAssembly/ServiceControl")))
                        {
                            string temp = x.Attributes["Name"].Value + ",ServiceControl";
                            foreach (XmlElement xmlElement in x)
                            {
                                if (xmlElement.Name == "Attribute" && xmlElement.Attributes["Name"].Value == "RefID")
                                {
                                    temp = temp + "," + xmlElement.Value + GetValue(xmlElement);
                                    _serviceElements.Add(temp);
                                }
                            }
                        }
                        else if (x.Name == "InternalElement" && x.Attributes["RefBaseSystemUnitPath"].Value == "MTPDataObjectSUCLib/DataAssembly/InputElement/BinProcessValueIn")
                        {
                            string temp = x.Attributes["Name"].Value + ",BinProcessValueIn";
                            foreach (XmlElement y in x)
                            {
                                if (y.Name == "Attribute" && y.Attributes["Name"].Value == "RefID")
                                {
                                    temp = temp + "," + y.Value + GetValue(y);
                                    _inputElements.Add(temp);
                                }
                            }
                        }
                        else if (x.Name == "InternalElement" && x.Attributes["RefBaseSystemUnitPath"].Value == "MTPDataObjectSUCLib/DataAssembly/InputElement/DIntProcessValueIn")
                        {
                            string temp = x.Attributes["Name"].Value + ",DIntProcessValueIn";
                            foreach (XmlElement y in x)
                            {
                                if (y.Name == "Attribute" && y.Attributes["Name"].Value == "RefID")
                                {
                                    temp = temp + "," + y.Value + GetValue(y);
                                    _inputElements.Add(temp);
                                }
                            }
                        }
                        else if (x.Name == "InternalElement" && x.Attributes["RefBaseSystemUnitPath"].Value == "MTPDataObjectSUCLib/DataAssembly/InputElement/AnaProcessValueIn")
                        {
                            string temp = x.Attributes["Name"].Value + ",AnaProcessValueIn";
                            foreach (XmlElement y in x)
                            {
                                if (y.Name == "Attribute" && y.Attributes["Name"].Value == "RefID")
                                {
                                    temp = temp + "," + y.Value + GetValue(y);
                                    _inputElements.Add(temp);
                                }
                            }
                        }
                        else if (x.Name == "InternalElement" && x.Attributes["RefBaseSystemUnitPath"].Value == "MTPDataObjectSUCLib/DataAssembly/InputElement/StringProcessValueIn")
                        {
                            string temp = x.Attributes["Name"].Value + ",StringProcessValueIn";
                            foreach (XmlElement y in x)
                            {
                                if (y.Name == "Attribute" && y.Attributes["Name"].Value == "RefID")
                                {
                                    temp = temp + "," + y.Value + GetValue(y);
                                    _inputElements.Add(temp);
                                }
                            }
                        }
                        else if (x.Name == "InternalElement" && x.Attributes["RefBaseSystemUnitPath"].Value == "MTPDataObjectSUCLib/DataAssembly/OperationElement/DIntMan/DIntManInt")
                        {
                            string temp = x.Attributes["Name"].Value + ",DIntManInt";
                            foreach (XmlElement y in x)
                            {
                                if (y.Name == "Attribute" && y.Attributes["Name"].Value == "RefID")
                                {
                                    temp = temp + "," + y.Value + GetValue(y);
                                    _operationElements.Add(temp);
                                }
                            }
                        }

                        else if (x.Name == "InternalElement" && x.Attributes["RefBaseSystemUnitPath"].Value == "MTPDataObjectSUCLib/DataAssembly/OperationElement/DIntMan")
                        {
                            string temp = x.Attributes["Name"].Value + ",DIntMan";
                            foreach (XmlElement y in x)
                            {
                                if (y.Name == "Attribute" && y.Attributes["Name"].Value == "RefID")
                                {
                                    temp = temp + "," + y.Value + GetValue(y);
                                    _operationElements.Add(temp);
                                }
                            }
                        }

                        else if (x.Name == "InternalElement" && x.Attributes["RefBaseSystemUnitPath"].Value == "MTPDataObjectSUCLib/DataAssembly/OperationElement/AnaMan")
                        {
                            string temp = x.Attributes["Name"].Value + ",AnaMan";
                            foreach (XmlElement y in x)
                            {
                                if (y.Name == "Attribute" && y.Attributes["Name"].Value == "RefID")
                                {
                                    temp = temp + "," + y.Value + GetValue(y);
                                    _operationElements.Add(temp);
                                }
                            }
                        }

                        else if (x.Name == "InternalElement" && x.Attributes["RefBaseSystemUnitPath"].Value == "MTPDataObjectSUCLib/DataAssembly/OperationElement/AnaMan/AnaManInt")
                        {
                            string temp = x.Attributes["Name"].Value + ",AnaManInt";
                            foreach (XmlElement y in x)
                            {
                                if (y.Name == "Attribute" && y.Attributes["Name"].Value == "RefID")
                                {
                                    temp = temp + "," + y.Value + GetValue(y);
                                    _operationElements.Add(temp);
                                }
                            }
                        }
                        else if (x.Name == "InternalElement" && x.Attributes["RefBaseSystemUnitPath"].Value == "MTPDataObjectSUCLib/DataAssembly/ServiceElement/ParameterElement/DIntServParam")
                        {
                            string temp = x.Attributes["Name"].Value + ",DIntServParam";
                            foreach (XmlElement y in x)
                            {
                                if (y.Name == "Attribute" && y.Attributes["Name"].Value == "RefID")
                                {
                                    temp = temp + "," + y.Value + GetValue(y);
                                    _parameterElements.Add(temp);
                                }
                            }
                        }
                        else if (x.Name == "InternalElement" && x.Attributes["RefBaseSystemUnitPath"].Value == "MTPDataObjectSUCLib/DataAssembly/ServiceElement/ParameterElement/BinServParam")
                        {
                            string temp = x.Attributes["Name"].Value + ",BinServParam";
                            foreach (XmlElement y in x)
                            {
                                if (y.Name == "Attribute" && y.Attributes["Name"].Value == "RefID")
                                {
                                    temp = temp + "," + y.Value + GetValue(y);
                                    _parameterElements.Add(temp);
                                }
                            }
                        }
                        else if (x.Name == "InternalElement" && x.Attributes["RefBaseSystemUnitPath"].Value == "MTPDataObjectSUCLib/DataAssembly/ServiceElement/ParameterElement/AnaServParam")
                        {
                            string temp = x.Attributes["Name"].Value + ",AnaServParam";
                            foreach (XmlElement y in x)
                            {
                                if (y.Name == "Attribute" && y.Attributes["Name"].Value == "RefID")
                                {
                                    temp = temp + "," + y.Value + GetValue(y);
                                    _parameterElements.Add(temp);
                                }
                            }
                        }
                        else if (x.Name == "InternalElement" && x.Attributes["RefBaseSystemUnitPath"].Value == "MTPDataObjectSUCLib/DataAssembly/ServiceElement/ParameterElement/StringServParam")
                        {
                            string temp = x.Attributes["Name"].Value + ",StringServParam";
                            foreach (XmlElement y in x)
                            {
                                if (y.Name == "Attribute" && y.Attributes["Name"].Value == "RefID")
                                {
                                    temp = temp + "," + y.Value + GetValue(y);
                                    _parameterElements.Add(temp);
                                }
                            }
                        }
                        else if (x.Name == "InternalElement" && x.Attributes["RefBaseSystemUnitPath"].Value == "MTPDataObjectSUCLib/DataAssembly/DiagnosticElement/LockView4")
                        {
                            string temp = x.Attributes["Name"].Value + ",LockView4";
                            foreach (XmlElement y in x)
                            {
                                if (y.Name == "Attribute" && y.Attributes["Name"].Value == "RefID")
                                {
                                    temp = temp + "," + y.Value + GetValue(y);
                                    _diagnosticElements.Add(temp);
                                }
                            }
                        }
                        else if (x.Name == "InternalElement" && x.Attributes["RefBaseSystemUnitPath"].Value == "MTPDataObjectSUCLib/DataAssembly/DiagnosticElement/LockView4/LockView8")
                        {
                            string temp = x.Attributes["Name"].Value + ",LockView8";
                            foreach (XmlElement y in x)
                            {
                                if (y.Name == "Attribute" && y.Attributes["Name"].Value == "RefID")
                                {
                                    temp = temp + "," + y.Value + GetValue(y);
                                    _diagnosticElements.Add(temp);
                                }
                            }
                        }
                        else if (x.Name == "InternalElement" && x.Attributes["RefBaseSystemUnitPath"].Value == "MTPDataObjectSUCLib/DataAssembly/DiagnosticElement/LockView4/LockView8/LockView16")
                        {
                            string temp = x.Attributes["Name"].Value + ",LockView16";
                            foreach (XmlElement y in x)
                            {
                                if (y.Name == "Attribute" && y.Attributes["Name"].Value == "RefID")
                                {
                                    temp = temp + "," + y.Value + GetValue(y);
                                    _diagnosticElements.Add(temp);
                                }
                            }
                        }
                    }
                }
            }
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
                ParseDataAssemblySetNode(_amlFileName);
            }

            RenderGraphic2();
            //toolStripButton5.Enabled = true;
        }

        private void GetMtpVersion(String fileName)
        {
            //XmlDocument xmlDocument = new XmlDocument();
            //xmlDocument.Load(fileName);

            string version = _xmlDocument.SelectSingleNode("//*[local-name()='Document']")?.Attributes["Version"]?.Value;

            if (version != null)
            {
                _mtpVersion = int.Parse(version.Split('.')[0]);
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

        

        private void RenderGraphic2()
        {
            _eClassMappingForMD = EclassMapping.GetMDMapping();
            _missingEClassesMapping.Clear();
            _missingSymbols.Clear();
            PictureObject pictureObject = mtpPictureObjects.First();

            //foreach (PictureObject pictureObject in mtpPictureObjects)
            //{
                RenderVisualObjects(pictureObject);
                RenderConnectionObjects(pictureObject);
                RenderSourceObjects(pictureObject);
                RenderSinkObjects(pictureObject);
                RenderVendorName2();
            //}

            Form mdiParent = this.Parent?.FindForm();

            if (mdiParent is MdiContainer mainForm)
            {
                mainForm.AddMtpError(mtpFileName, mtpErrors);
            }


            /*MdiContainer parent = this.MdiParent as MdiContainer;
            if(parent != null)
            {
                parent.AddMtpError(mtpFileName, mtpErrors);
            }*/
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            /*initialValue += 5;
            dBHandler.UpdateOpcUaItem("ceb2feae-6bed-4e3e-8057-71c1d72c0747", initialValue.ToString());*/
        }

        private void timerReadTimer_Tick(object sender, EventArgs e)
        {
            /*if (isOnline)
            {
                List<OpcUaItem> opcUaItemValues = new List<OpcUaItem>();
                opcUaItemValues = dBHandler.ReadAllOpcUaItems();

                foreach (var item in Controls)
                {
                    ToolTip toolTip = new ToolTip();
                    Control control = item as Control;
                    string refId = control!.Name;

                    if (dataAssemblyDictionary.ContainsKey(refId))
                    {
                        string opcUaRefId = dataAssemblyDictionary[refId];
                        OpcUaItem opcUaItem = opcUaItemValues.Find(item => item.Id == opcUaRefId);
                        if (opcUaItem != null)
                        {
                            //toolTip.GetToolTip(control);
                            toolTip.SetToolTip(control, opcUaItem.Value);
                        }
                    }
                }
            }*/
        }

        private void RenderSinkObjects(PictureObject pictureObject)
        {
            Trace.WriteLine("MTP Renderer -> Number of Sink Objects: " + mtpSinkObjects.Count);
            foreach (MtpSinkObject mtpSinkObject in pictureObject.MtpSinkObjects)
            {
                CreateSinkObject(mtpSinkObject);
            }
        }

        private void RenderSourceObjects(PictureObject pictureObject)
        {
            Trace.WriteLine("MTP Renderer -> Number of Source Objects: " + mtpSourceObjects.Count);

            foreach (MtpSourceObject mtpSourceObject in pictureObject.MtpSourceObjects)
            {
                CreateSourceObject(mtpSourceObject);
            }
        }

        private void RenderConnectionObjects(PictureObject pictureObject)
        {
            foreach (MtpConnectionObject mtpConnectionObject in pictureObject.MtpConnectionObjects)
            {
                if (mtpConnectionObject.RefBaseSystemUnitPath == "MTPHMISUCLib/Connection/Pipe")
                {
                    DrawPolyline(mtpConnectionObject.Edgepath, 5, mtpConnectionObject.Name, ConnectionType.PIPE);
                }
                else if (mtpConnectionObject.RefBaseSystemUnitPath == "MTPHMISUCLib/Connection/MeasurementLine")
                {
                    DrawPolyline(mtpConnectionObject.Edgepath, 1, mtpConnectionObject.Name, ConnectionType.MEASUREMENT_LINE);
                }
                else if (mtpConnectionObject.RefBaseSystemUnitPath == "MTPHMISUCLib/Connection/FunctionLine")
                {
                    DrawPolyline(mtpConnectionObject.Edgepath, 10, mtpConnectionObject.Name, ConnectionType.FUNCTION_LINE);
                }
            }
            Refresh();
        }

        List<String> mtpErrors = new List<string>();
        List<string> mtpWarnings = new List<string>();

        private void DrawPolyline(string edgePath, int strokeThikness, string name, ConnectionType connectionType)
        {
            if (edgePath == null)
            {
                return;
            }
            string temp = edgePath;
            if (edgePath.EndsWith(";"))
            {
                temp = edgePath.Substring(0, edgePath.Length - 1);
                mtpErrors.Add("MTP ERROR -> Edgepath for " + name + " ends with a semicolon. This is not allowed in MTP files.");
            }

            char delimeter = ';';
            if(!temp.Contains(delimeter))
            {
                delimeter = ' '; // this is a forgiving approach, but some MTP files have a space instead of a mandatory semicolon
                mtpErrors.Add("MTP ERROR -> Edgepath for " + name + " does not contain a semicolon delimeter. This is not allowed in MTP files.");
            }

            string[] points = temp.Split(delimeter);
            GraphicsPath myPath = new GraphicsPath();
            List<System.Drawing.Point> points2 = new List<System.Drawing.Point>();
            foreach (string point in points)
            {
                int x = int.Parse(point.Split(',')[0]) + leftMargin;
                int y = int.Parse(point.Split(',')[1]) + topMargin;
                System.Drawing.Point p = new System.Drawing.Point(x, y);
                points2.Add(p);

            }
            System.Drawing.Point[] temp3 = points2.ToArray();

            myPath.AddLines(temp3);
            if (connectionType == ConnectionType.MEASUREMENT_LINE)
            {
                _graphicsPathsForMeasurementLine.Add(myPath);
            }
            else if (connectionType == ConnectionType.PIPE)
            {
                _graphicsPathsForPipe.Add(myPath);
            }
            else if (connectionType == ConnectionType.FUNCTION_LINE)
            {
                _graphicsPathsForFunctionLine.Add(myPath);
            }
            else
            {
                _graphicsPathsForPipe.Add(myPath);
            }

            this.Update();
        }

        private void RenderVisualObjects(PictureObject pictureObject)
        {
            SetMinimumTopAndLeftMargins(pictureObject);
            Trace.WriteLine("MTP Renderer -> Number of Visual Objects: " + mtpObjects.Count);
            foreach (MtpObject mtpObject in pictureObject.MtpObjects)
            {
                if (mtpObject.RefBaseSystemUnitPath == "MTPDataObjectSUCLib/DataAssembly/ServiceControl" || mtpObject.RefBaseSystemUnitPath == "MTPDataObjectSUCLib/DataAssembly")
                {
                    CreateServiceControl(mtpObject);
                }
                else if (mtpObject.RefBaseSystemUnitPath == "MTPHMISUCLib/VisualObject")
                {
                    CreatePictureBox(mtpObject);
                }
            }
        }

        private void SetMinimumTopAndLeftMargins(PictureObject pictureObject)
        {
            leftMargin = 0 - FindMinimumXCoordinate(pictureObject) + 20;
            topMargin = 0 - FindMinimumYCoordinate(pictureObject) + 20;
        }

        private int FindMinimumXCoordinate(PictureObject pictureObject)
        {
            int minX = int.MaxValue;
            foreach (MtpObject mtpObject in pictureObject.MtpObjects)
            {
                if (mtpObject.X < minX)
                {
                    minX = mtpObject.X;
                }
            }
            return minX;
        }

        private int FindMinimumYCoordinate(PictureObject pictureObject)
        {
            int minY = int.MaxValue;
            foreach (MtpObject mtpObject in pictureObject.MtpObjects)
            {
                if (mtpObject.Y < minY)
                {
                    minY = mtpObject.Y;
                }
            }
            return minY;
        }

        private void ReadValuesFromOpcUaServer()
        {

        }

        private void ReadAllOpcUaItemFromManifestFile()
        {

            XDocument doc = XDocument.Load("Manifest.xml");

            XNamespace ns = "http://www.dke.de/CAEX";

            var opcItems = doc
                .Descendants(ns + "ExternalInterface")
                .Where(x => (string)x.Attribute("RefBaseClassPath") ==
                            "MTPCommunicationICLib/DataItem/OPCUAItem");

            foreach (var item in opcItems)
            {
                string name = (string)item.Attribute("Name");
                string id = (string)item.Attribute("ID");

                string identifier = item
                    .Elements(ns + "Attribute")
                    .FirstOrDefault(a => (string)a.Attribute("Name") == "Identifier")
                    ?.Element(ns + "Value")
                    ?.Value;

                string nameSpace = item
                    .Elements(ns + "Attribute")
                    .FirstOrDefault(a => (string)a.Attribute("Name") == "Namespace")
                    ?.Element(ns + "Value")
                    ?.Value;

                string access = item
                    .Elements(ns + "Attribute")
                    .FirstOrDefault(a => (string)a.Attribute("Name") == "Access")
                    ?.Element(ns + "Value")
                    ?.Value;

                //Console.WriteLine($"Name      : {name}");
                //Console.WriteLine($"ID        : {id}");
                //Console.WriteLine($"Identifier: {identifier}");
                //Console.WriteLine($"Namespace : {nameSpace}");
                //Console.WriteLine($"Access    : {access}");
                //Console.WriteLine();
            }
        }

        enum ConnectionType
        {
            PIPE,
            MEASUREMENT_LINE,
            FUNCTION_LINE
        }

        private void RepositionVendorAndDisplayName()
        {
            _labelVendorName.Location = new System.Drawing.Point(0, this.Height - 100);
            _labelDisplayName.Location = new System.Drawing.Point(0, this.Height - 130);
        }

        Label _labelVendorName = new Label();
        Label _labelDisplayName = new Label();
        Dictionary<int, string> _eClassMappingForMD = new Dictionary<int, string>();
        Dictionary<int, string> _eClassMappingForXa = new Dictionary<int, string>();
        private void RenderVendorName2()
        {
            _labelVendorName.ClientSize = new System.Drawing.Size(100, 100);
            System.Drawing.Font LargeFont = new System.Drawing.Font("Arial", 16);


            _labelVendorName.Font = LargeFont;
            _labelVendorName.Text = _vendorName;
            _labelVendorName.AutoSize = true;
            _labelVendorName.ForeColor = System.Drawing.Color.LightGray;

            double w = _vendorName.Length * 30;
            _labelVendorName.Location = new System.Drawing.Point(0, this.Height - 100);
            Controls.Add(_labelVendorName);

            _labelDisplayName.ClientSize = new System.Drawing.Size(100, 100);
            System.Drawing.Font LargeFont2 = new System.Drawing.Font("Arial", 14);

            _labelDisplayName.Font = LargeFont2;
            _labelDisplayName.Text = _displayName;
            _labelDisplayName.AutoSize = true;
            _labelDisplayName.ForeColor = System.Drawing.Color.LightGray;

            _labelDisplayName.Location = new System.Drawing.Point(0, this.Height - 130);
            Controls.Add(_labelDisplayName);
        }



        XmlDocument _xmlDocument;

        List<MtpObject> mtpObjects = new List<MtpObject>();
        List<MtpConnectionObject> mtpConnectionObjects = new List<MtpConnectionObject>();
        List<MtpSourceObject> mtpSourceObjects = new List<MtpSourceObject>();
        List<MtpPortObject> mtpPortObjects = new List<MtpPortObject>();
        List<MtpJunctionObject> mtpJunctionObjects = new List<MtpJunctionObject>();
        List<MtpSinkObject> mtpSinkObjects = new List<MtpSinkObject>();
        List<MtpSinkObject> mtpTerminationObjects = new List<MtpSinkObject>();
        List<PictureObject> mtpPictureObjects = new List<PictureObject>();
        List<XmlNode> pictureNodes = new List<XmlNode>();
        XmlNode _communicationSetNode; // used in MTP 1.0 and MTP 1.1
        List<XmlNode> _dataAssemblySetNode = new List<XmlNode>(); // used in MTP 2.0

        string imagePath = "";

        int topMargin = 100;
        int leftMargin = 0;
        List<string> _missingEClasses = new List<string>();
        List<string> _missingEClassesMapping = new List<string>();
        List<String> _missingSymbols = new List<string>();
        List<String> _eClassesUsed = new List<string>();
        string _vendorName = "";
        private string _displayName = "";
        private string _defaultImageFileName = "default.png";
        private string _amlFileName = "";

        List<string> _indicatorElements = [];
        List<string> _activeElements = [];
        List<string> _operationElements = new List<string>();
        List<string> _inputElements = new List<string>();
        List<string> _serviceElements = new List<string>();
        List<string> _parameterElements = new List<string>();
        List<string> _diagnosticElements = new List<string>();

        private readonly List<ElementInfo> _indicatorElementList = new();
        private readonly List<ElementInfo> _activeElementList = new();
        private readonly List<ElementInfo> _parameterElementList = new();
        private readonly List<ElementInfo> _operationElementList = new();
        private readonly List<ElementInfo> _inputElementList = new();
        private readonly List<ElementInfo> _serviceElementList = new();
        private readonly List<ElementInfo> _diagnosticElementList = new();

        private Bitmap MyImage;
        public string mtpFileName;
        Dictionary<string, string> dataAssemblyDictionary = new Dictionary<string, string>();
        List<GraphicsPath> _graphicsPathsForPipe = new List<GraphicsPath>();
        List<GraphicsPath> _graphicsPathsForMeasurementLine = new List<GraphicsPath>();
        List<GraphicsPath> _graphicsPathsForFunctionLine = new List<GraphicsPath>();
        int initialValue = 0;
        //DBHandler dBHandler = new();
        bool isOnline = true;
        double scaleFactor = 1.0;
        int _mtpVersion = 0;


        private static readonly Dictionary<string, string> SymbolImages = new()
        {
        // Indicators
        ["AnaView"] = "27209090.png",
        ["BinView"] = "BinaryIndicator.png",
        ["BinMon"] = "BinaryIndicator.png",
        ["AnaMon"] = "AnalogIndicator.emf",
        ["DIntView"] = "ArithmeticOperator.emf",
        ["DIntMon"] = "ArithmeticOperator.emf",
        ["StringView"] = "BinaryIndicator.png",

        // Active
        ["BinVlv"] = "BlockValve.emf",
        ["MonBinVlv"] = "BlockValve.emf",
        ["AnaVlv"] = "BlockValve.emf",
        ["MonAnaVlv"] = "BlockValve.emf",
        ["MonBinDrv"] = "Mixer.emf",
        ["AnaDrv"] = "pump.emf",
        ["MonAnaDrv"] = "pump.emf",
        ["PIDCtrl"] = "pidctrl.png",

        // Service
        ["ServiceControl"] = "ServiceControl",

        // Operation
        ["DIntManInt"] = "BlockValve.emf",
        ["DIntMan"] = "BlockValve.emf",
        ["AnaManInt"] = "BlockValve.emf",
        ["AnaMan"] = "BlockValve.emf",
        ["BinManInt"] = "BinaryIndicator.png",

        // Parameters
        ["DIntServParam"] = "BlockValve.emf",
        ["BinServParam"] = "BlockValve.emf",
        ["AnaServParam"] = "BlockValve.emf",
        ["StringServParam"] = "BlockValve.emf",

        // Diagnostics
        ["LockView4"] = "InterlockView.emf",
        ["LockView8"] = "InterlockView.emf",
        ["LockView16"] = "InterlockView.emf",

        // Inputs
        ["BinProcessValueIn"] = "BlockValve.emf",
        ["AnaProcessValueIn"] = "BlockValve.emf",
        ["DIntProcessValueIn"] = "BlockValve.emf",
        ["StringProcessValueIn"] = "BlockValve.emf"
    };


    }
}
