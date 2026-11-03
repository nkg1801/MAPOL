using Aml.Engine.CAEX;
using MAPol.Models;
using MAPol.Views;
using System.Data;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.IO.Compression;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace MAPol
{
    public partial class FormProcessDisplay1 : Form
    {
        public FormProcessDisplay1(PictureObject pictureObject, PictureContainer parentForm)
        {
            InitializeComponent();
            panel1.Paint += PanelCanvas_Paint;
            panel1.MouseWheel += Panel1_MouseWheel;
            _incomingPictureObject = pictureObject;
            _parentForm = parentForm;
            imagePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\Shapes\\";
        }

        private float _zoom = 1.0f;
        PictureContainer _parentForm;

        private void PanelCanvas_Paint(object sender, PaintEventArgs e)
        {
            //e.Graphics.ScaleTransform(_zoom, _zoom);

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
                foreach (MtpPortObject mtpPortObject in _currentPictureObject.MtpPortObjects)
                {
                    if (mtpPortObject.RefBaseSystemUnitPath == "MTPHMISUCLib/PortObject/Nozzle")
                    {
                        //e.Graphics.FillEllipse(new SolidBrush(System.Drawing.Color.DarkGray), Scale(mtpPortObject.X + leftMargin - 5), Scale(mtpPortObject.Y + topMargin - 5), Scale(10), Scale(10));
                        e.Graphics.FillEllipse(new SolidBrush(System.Drawing.Color.DarkGray), mtpPortObject.X + leftMargin - 5, mtpPortObject.Y + topMargin - 5, 10, 10);
                    }
                    else if (mtpPortObject.RefBaseSystemUnitPath == "MTPHMISUCLib/PortObject/MeasurementPoint")
                    {
                        //e.Graphics.FillEllipse(new SolidBrush(System.Drawing.Color.DarkGray), Scale(mtpPortObject.X + leftMargin - 5), Scale(mtpPortObject.Y + topMargin - 5), Scale(10), Scale(10));
                        e.Graphics.FillEllipse(new SolidBrush(System.Drawing.Color.DarkGray), mtpPortObject.X + leftMargin - 5, mtpPortObject.Y + topMargin - 5, 10, 10);
                    }
                    else if (mtpPortObject.RefBaseSystemUnitPath == "MTPHMISUCLib/PortObject/LogicalPort")
                    {
                        //e.Graphics.FillEllipse(new SolidBrush(System.Drawing.Color.LightBlue), Scale(mtpPortObject.X + leftMargin - 4), Scale(mtpPortObject.Y + topMargin - 4), Scale(10), Scale(8));
                        e.Graphics.FillEllipse(new SolidBrush(System.Drawing.Color.LightBlue), mtpPortObject.X + leftMargin - 4, mtpPortObject.Y + topMargin - 4, 10, 8);
                    }
                }
            }

            //Render Junction object
            using (System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Color.Black, 4))
            {
                Trace.WriteLine("MTP Renderer -> Number of Junction Objects: " + _currentPictureObject.MtpJunctionObjects.Count);
                //foreach (MtpJunctionObject mtpJunctionObject in mtpJunctionObjects)
                foreach (MtpJunctionObject mtpJunctionObject in _currentPictureObject.MtpJunctionObjects)
                {
                    //e.Graphics.FillEllipse(new SolidBrush(System.Drawing.Color.DarkGray), Scale(mtpJunctionObject.X + leftMargin - 10), Scale(mtpJunctionObject.Y + topMargin - 10), Scale(20), Scale(20));
                    e.Graphics.FillEllipse(new SolidBrush(System.Drawing.Color.DarkGray), mtpJunctionObject.X + leftMargin - 10, mtpJunctionObject.Y + topMargin - 10, 20, 20);
                    //e.Graphics.FillEllipse(new SolidBrush(System.Drawing.Color.Black), Scale(mtpJunctionObject.X + leftMargin - 5), Scale(mtpJunctionObject.Y + topMargin - 5), Scale(10), Scale(10));
                    e.Graphics.FillEllipse(new SolidBrush(System.Drawing.Color.Black), mtpJunctionObject.X + leftMargin - 5, mtpJunctionObject.Y + topMargin - 5, 10, 10);
                }
            }

            RepositionVendorAndDisplayName();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            /*base.OnPaint(e);

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
                        e.Graphics.FillEllipse(new SolidBrush(System.Drawing.Color.DarkGray), Scale(mtpPortObject.X + leftMargin - 5), Scale(mtpPortObject.Y + topMargin - 5), Scale(10), Scale(10));
                    }
                    else if (mtpPortObject.RefBaseSystemUnitPath == "MTPHMISUCLib/PortObject/MeasurementPoint")
                    {
                        e.Graphics.FillEllipse(new SolidBrush(System.Drawing.Color.DarkGray), Scale(mtpPortObject.X + leftMargin - 5), Scale(mtpPortObject.Y + topMargin - 5), Scale(10), Scale(10));
                    }
                    else if (mtpPortObject.RefBaseSystemUnitPath == "MTPHMISUCLib/PortObject/LogicalPort")
                    {
                        e.Graphics.FillEllipse(new SolidBrush(System.Drawing.Color.LightBlue), Scale(mtpPortObject.X + leftMargin - 4), Scale(mtpPortObject.Y + topMargin - 4), Scale(10), Scale(8));
                    }
                }
            }

            //Render Junction object
            using (System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Color.Black, 4))
            {
                Trace.WriteLine("MTP Renderer -> Number of Junction Objects: " + mtpJunctionObjects.Count);
                foreach (MtpJunctionObject mtpJunctionObject in mtpJunctionObjects)
                {
                    e.Graphics.FillEllipse(new SolidBrush(System.Drawing.Color.DarkGray), Scale(mtpJunctionObject.X + leftMargin - 10), Scale(mtpJunctionObject.Y + topMargin - 10), Scale(20), Scale(20));
                    e.Graphics.FillEllipse(new SolidBrush(System.Drawing.Color.Black), Scale(mtpJunctionObject.X + leftMargin - 5), Scale(mtpJunctionObject.Y + topMargin - 5), Scale(10), Scale(10));
                }
            }

            RepositionVendorAndDisplayName();*/
        }

        private void FormProcessDisplay1_Load(object sender, EventArgs e)
        {
            RenderGraphic2(_incomingPictureObject);
            isOnline = true;
        }

        private void CreateSourceObject(MtpSourceObject mtpSourceObject)
        {
            Button button = new Button();
            button.Left = Scale(mtpSourceObject.X + leftMargin);
            button.Top = Scale(mtpSourceObject.Y + topMargin);
            button.Width = Scale(100);
            button.Height = Scale(50);
            button.Text = mtpSourceObject.Name;
            //button.Tag = mtpSourceObject.RefID;
            panel1.Controls.Add(button);
        }

        private void CreateServiceControl(MtpObject mtpObject)
        {
            Button button = new Button();
            //button.Left = Convert.ToInt32((mtpObject.X + leftMargin) * scaleFactor);
            button.Left = Scale(mtpObject.X + leftMargin);
            button.Top = Scale(mtpObject.Y + topMargin);
            button.Width = Scale(100);
            button.Height = Scale(50);
            button.Text = mtpObject.Name;
            button.BackColor = Color.DarkGray;
            button.Tag = mtpObject.RefID;
            panel1.Controls.Add(button);
        }

        private void CreateSinkObject(MtpSinkObject mtpObject)
        {
            Button button = new Button();
            button.Left = Scale(mtpObject.X + leftMargin);
            button.Top = Scale(mtpObject.Y + topMargin);
            button.Width = Scale(100);
            button.Height = Scale(50);
            button.Text = mtpObject.Name;
            //button.Tag = mtpObject.RefID;
            panel1.Controls.Add(button);
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

                    if (visualElement.Equals("ServiceControl"))
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
            pictureBox.Tag = mtpObject.ID;
            ToolTip toolTip = new ToolTip();
            toolTip.Tag = mtpObject.RefID;
            //string toolTipText = makeTooltipText(mtpObject.Name, mtpObject.EClassClassificationClass, mtpObject.X, mtpObject.Y, mtpObject.Width, mtpObject.Height);
            string toolTipText = mtpObject.Name + ", eClass: " + mtpObject.EClassClassificationClass;
            toolTip.SetToolTip(pictureBox, toolTipText);
            
            panel1.Controls.Add(pictureBox);

            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            MyImage = new Bitmap(imageFileName);
            MyImage.RotateFlip(GetFlipType(mtpObject.Rotation));

            pictureBox.ClientSize = new System.Drawing.Size(mtpObject.Width, mtpObject.Height);
            pictureBox.Location = new System.Drawing.Point(mtpObject.X + leftMargin, mtpObject.Y + topMargin);
            pictureBox.Image = MyImage;
            pictureBox.BringToFront();
            //pictureBox.SendToBack();
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

        PictureObject _currentPictureObject;

        private void RenderGraphic2(PictureObject pictureObject)
        {
            _currentPictureObject = pictureObject;
            _eClassMappingForMD = EclassMapping.GetMDMapping();
            _missingEClassesMapping.Clear();
            _missingSymbols.Clear();
            
            RenderVisualObjects(pictureObject);
            RenderConnectionObjects(pictureObject);
            RenderSourceObjects(pictureObject);
            RenderSinkObjects(pictureObject);
            RenderVendorName2();

            Form mdiParent = this.Parent?.FindForm();

            if (mdiParent is MdiContainer mainForm)
            {
                mainForm.AddMtpError(mtpFileName, mtpErrors);
            }


            MdiContainer parent = this.MdiParent as MdiContainer;
            if(parent != null)
            {
                parent.AddMtpError(mtpFileName, mtpErrors);
            }
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
                //System.Drawing.Point p = new System.Drawing.Point(Scale(x), Scale(y));
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

            //this.Update();
            this.panel1.Invalidate();
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
            leftMargin = 0 - FindMinimumXCoordinate(pictureObject);// + 20;
            topMargin = 0 - FindMinimumYCoordinate(pictureObject);// + 20;
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

            foreach(MtpSourceObject mtpSourceObject in pictureObject.MtpSourceObjects)
            {
                if (mtpSourceObject.X < minX)
                {
                    minX = mtpSourceObject.X;
                }
            }

            foreach(GraphicsPath path in _graphicsPathsForFunctionLine)
            {
                foreach (System.Drawing.PointF point in path.PathPoints)
                {
                    if (point.X < minX)
                    {
                        minX = (int)point.X;
                    }
                }
            }
            

            return minX - 100;
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

            foreach(MtpSourceObject mtpSourceObject in pictureObject.MtpSourceObjects)
            {
                if (mtpSourceObject.Y < minY)
                {
                    minY = mtpSourceObject.Y;
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
               

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if ((ModifierKeys & Keys.Control) == Keys.Control)
            {
                if (e.Delta > 0)
                {
                    scaleFactor *= 1.1f; // Zoom in 10%
                    _zoom *= 1.1f; // Zoom in 10%
                    
                }
                else
                {
                    scaleFactor /= 1.1f; // Zoom out 10%
                    _zoom /= 1.1f; // Zoom out 10%
                }
                this.panel1.Invalidate();

                //RedrawMtpObjects();
                //RenderGraphic2();
            }
        }

        private void Panel1_MouseWheel(object sender, MouseEventArgs e)
        {
            if ((ModifierKeys & Keys.Control) == Keys.Control)
            {
                if (e.Delta > 0)
                {
                    scaleFactor *= 1.1f;
                    _zoom *= 1.1f;
                }
                else
                {
                    scaleFactor /= 1.1f;
                    _zoom /= 1.1f;
                }

                panel1.Controls.Clear();
                _graphicsPathsForFunctionLine.Clear();
                _graphicsPathsForMeasurementLine.Clear();
                _graphicsPathsForPipe.Clear();
                dataAssemblyDictionary.Clear();
                //RenderGraphic2();

                //OpenFile(_amlFileName);

                //panel1.Invalidate();
            }
        }


        private int Scale(int value) => (int)(value * scaleFactor);
        string imagePath = "";


        XmlDocument _xmlDocument;

        List<MtpObject> mtpObjects = new List<MtpObject>();
        List<MtpConnectionObject> mtpConnectionObjects = new List<MtpConnectionObject>();
        List<MtpSourceObject> mtpSourceObjects = new List<MtpSourceObject>();
        List<MtpPortObject> mtpPortObjects = new List<MtpPortObject>();
        List<MtpJunctionObject> mtpJunctionObjects = new List<MtpJunctionObject>();
        List<MtpSinkObject> mtpSinkObjects = new List<MtpSinkObject>();
        //List<MtpSinkObject> mtpTerminationObjects = new List<MtpSinkObject>();
        List<PictureObject> mtpPictureObjects = new List<PictureObject>();
        List<XmlNode> _pictureNodes = new List<XmlNode>();
        
        XmlNode _communicationSetNode; // used in MTP 1.0 and MTP 1.1
        List<XmlNode> _dataAssemblySetNode = new List<XmlNode>(); // used in MTP 2.0

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
        private PictureObject _incomingPictureObject;

        public List<ElementInfo> _indicatorElementList = new();
        public List<ElementInfo> _activeElementList = new();
        public List<ElementInfo> _parameterElementList = new();
        public List<ElementInfo> _operationElementList = new();
        public List<ElementInfo> _inputElementList = new();
        public List<ElementInfo> _serviceElementList = new();
        public List<ElementInfo> _diagnosticElementList = new();

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

        public PeaInformation PeaInformation { get; private set; }

        public WebServerInfo WebServerInfo { get; private set; }


        private static readonly Dictionary<string, string> SymbolImages = new()
        {
        // Indicators
        ["AnaView"] = "27209090.png",
        ["BinView"] = "BinaryIndicator.png",
        ["BinMon"] = "BinaryIndicator.png",
        ["AnaMon"] = "AnalogIndicator.emf",
        ["DIntView"] = "ArithmeticOperator.emf",
        ["DIntMon"] = "ArithmeticOperator.emf",
        ["StringView"] = "AnalogIndicator.emf",

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
        ["BinMan"] = "BinaryIndicator.png",
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
