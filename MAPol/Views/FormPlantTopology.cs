using MAPol.Models;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MAPol.Views
{
    public partial class FormPlantTopology : Form
    {
        public FormPlantTopology()
        {
            InitializeComponent();
            this.KeyPreview = true;
        }

        private void FormPlantTopology_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(TreeNode)))
            {
                TreeNode draggedNode = (TreeNode)e.Data.GetData(typeof(TreeNode));
                if(draggedNode.Level != 2)
                {
                    return;
                }
                e.Effect = DragDropEffects.Move; // Allow the drop
            }
            else
            {
                e.Effect = DragDropEffects.None; // Disallow the drop
            }
        }

        private void FormPlantTopology_DragDrop(object sender, DragEventArgs e)
        {
            TreeNode droppedNode = (TreeNode)e.Data.GetData(typeof(TreeNode));

            String id = Guid.NewGuid().ToString();

            Label label = new Label();
            label.Text = droppedNode.Text;
            label.Location = PointToClient(new Point(e.X, e.Y));
            label.Tag = id;
            label.AutoSize = true;
            Controls.Add(label);

            Point p = new Point(e.X, e.Y);
            Point formClientPosition = this.PointToClient(p);

            int topoObjectWidth = label.Width > 200 ? label.Width + 5 : 200;

            System.Drawing.Rectangle rectangle = new System.Drawing.Rectangle(formClientPosition.X, formClientPosition.Y, topoObjectWidth, 100);

            Controls.Remove(label);

            TopologyObject topologyObject = new TopologyObject();
            topologyObject.id = id;
            topologyObject.Name = droppedNode.Text;
            topologyObject.Bounds = rectangle;
            topologyObject.InputPoints = new List<Point>();
            topologyObject.InputPoints.Add(new Point(rectangle.X - 3, rectangle.Y + (rectangle.Height / 2)));
            topologyObject.OutputPoints = new List<Point>();
            topologyObject.OutputPoints.Add(new Point(rectangle.X + rectangle.Width - 2, rectangle.Y + rectangle.Height / 2));
            
            topologyObject.topologyObjectLabel = label;

            topologyObjects.Add(topologyObject);

            Refresh();
        }

        private void FormPlantTopology_Paint(object sender, PaintEventArgs e)
        {
            Graphics _graphics = e.Graphics;
            

            using (Pen bluePen = new Pen(_rectangleColor, 2))
            {
                foreach (TopologyObject topologyObject in topologyObjects)
                {
                    int x = topologyObject.Bounds.X;
                    int y = topologyObject.Bounds.Y;
                    int width = topologyObject.Bounds.Width;
                    int height = topologyObject.Bounds.Height;
                    _graphics.DrawRectangle(bluePen, x, y, width, height);
                    Label label = topologyObject.topologyObjectLabel;
                    label.Location = new Point(x+2, y+2);
                    label.AutoSize = true;
                    this.Controls.Add(label);

                    if (!_moveTopologyObject)
                    {
                        foreach (Point point in topologyObject.InputPoints)
                        {
                            _graphics.FillEllipse(new SolidBrush(Color.Red), point.X, point.Y, InputOutputPointsWidth, InputOutputPointsWidth);
                        }

                        foreach (Point point in topologyObject.OutputPoints)
                        {
                            _graphics.FillEllipse(new SolidBrush(Color.Red), point.X, point.Y, InputOutputPointsWidth, InputOutputPointsWidth);
                        }
                    }
                }

                if (selectedRectangle.HasValue && !_moveTopologyObject)
                {
                    PointF topLeft = new PointF(selectedRectangle.Value.X, selectedRectangle.Value.Y);
                    PointF topRight = new PointF(selectedRectangle.Value.X + selectedRectangle.Value.Width, selectedRectangle.Value.Y);
                    PointF bottomLeft = new PointF(selectedRectangle.Value.X, selectedRectangle.Value.Y + selectedRectangle.Value.Height);
                    PointF bottomRight = new PointF(selectedRectangle.Value.X + selectedRectangle.Value.Width, selectedRectangle.Value.Y + selectedRectangle.Value.Height);
                    e.Graphics.DrawRectangle(new Pen(Color.Black, 3), topLeft.X - 2, topLeft.Y - 2, 4, 4);
                    e.Graphics.DrawRectangle(new Pen(Color.Black, 3), topRight.X - 2, topRight.Y - 2, 4, 4);
                    e.Graphics.DrawRectangle(new Pen(Color.Black, 3), bottomRight.X - 2, bottomRight.Y - 2, 4, 4);
                    e.Graphics.DrawRectangle(new Pen(Color.Black, 3), bottomLeft.X - 2, bottomLeft.Y - 2, 4, 4);
                }
            }

            using (Pen pen = new Pen(_connectingLineColor, 2))
            {
                foreach (ConnectingLine connectingLine in _connectingLines)
                {
                    Point p1 = connectingLine.TopologyObject2.InputPoints[connectingLine.InputPointIndex];
                    Point p2 = connectingLine.TopologyObject1.OutputPoints[connectingLine.OutputPointIndex];
                    e.Graphics.DrawLine(pen, p1, p2);
                }
            }
        }

        private void FormPlantTopology_MouseClick(object sender, MouseEventArgs e)
        {
            selectedRectangle = null; // Clear previous selection

            foreach (TopologyObject topologyObject in topologyObjects)
            {
                if (topologyObject.Bounds.Contains(e.Location))
                {
                    selectedRectangle = topologyObject.Bounds;
                    break;
                }
            }

            Invalidate();
        }

        private ConnectingLine isPointWithinInputPoints2(Point point, ConnectingLine connectingLine)
        {
            foreach (TopologyObject topologyObject in topologyObjects)
            {
                //if the topology object has only one input point, dropping the mouse anywhere within the topology object should be fine
                if(topologyObject.InputPoints.Count == 1)
                {
                    if(topologyObject.Bounds.Contains(point) && connectingLine.TopologyObject1 != topologyObject)
                    {
                        connectingLine.TopologyObject2 = topologyObject;
                        connectingLine.InputPointIndex = 0;
                        return connectingLine;
                    }
                }

                for (int i = 0; i < topologyObject.InputPoints.Count; i++)
                {
                    Point p = topologyObject.InputPoints[i];
                    System.Drawing.Rectangle rect = new System.Drawing.Rectangle(p.X, p.Y, 10, 10);
                    if (rect.Contains(point) && connectingLine.TopologyObject1 != topologyObject)
                    {
                        connectingLine.TopologyObject2 = topologyObject;
                        connectingLine.InputPointIndex = i;
                        return connectingLine;
                    }
                }
                
            }
            connectingLine.InputPointIndex = -1;
            return connectingLine;
        }

        private bool isPointWithinOutputPoints(Point point)
        {
            foreach (TopologyObject topologyObject in topologyObjects)
            {
                foreach (Point p in topologyObject.OutputPoints)
                {
                    System.Drawing.Rectangle rect = new System.Drawing.Rectangle(p.X, p.Y, 10, 10);
                    if (rect.Contains(point))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private ConnectingLine isPointWithinOutputPoints2(Point point)
        {
            ConnectingLine connectingLine = new ConnectingLine();
            foreach (TopologyObject topologyObject in topologyObjects)
            {
                for(int i=0; i < topologyObject.OutputPoints.Count; i++)
                {
                    Point p = topologyObject.OutputPoints[i];
                    System.Drawing.Rectangle rect = new System.Drawing.Rectangle(p.X, p.Y, 10, 10);
                    if (rect.Contains(point))
                    {
                        connectingLine.TopologyObject1 = topologyObject;
                        connectingLine.OutputPointIndex = i;
                        return connectingLine;
                    }
                }
                
            }
            connectingLine.OutputPointIndex = -1;
            return connectingLine;
        }

        private ConnectingLine _currentConnectingLine;

        private void FormPlantTopology_MouseDown(object sender, MouseEventArgs e)
        {
            ConnectingLine connectingLine = isPointWithinOutputPoints2(e.Location);
            if(connectingLine.OutputPointIndex != -1)
            {
                _currentConnectingLine = connectingLine;
                _isDrawingLine = true;
            }

            /*foreach (TopologyObject topologyObject in topologyObjects)
            {
                foreach (Point p in topologyObject.OutputPoints)
                {
                    System.Drawing.Rectangle rect = new System.Drawing.Rectangle(p.X, p.Y, 10, 10);
                    if (rect.Contains(e.Location))
                    {
                        _isDrawingLine = true;
                        _startPoint = p;
                        break;
                    }
                }
            }*/

            // to move the topology object

            foreach (TopologyObject topologyObject in topologyObjects)
            {
                if (topologyObject.Bounds.Contains(e.Location))
                {
                    _moveTopologyObject = true;
                    _topologyObjectBeingMoved = topologyObject;
                    lastMousePos = e.Location;
                }
            }
        }

        private void FormPlantTopology_MouseMove(object sender, MouseEventArgs e)
        {
            if (isPointWithinOutputPoints(e.Location))
            {
                Cursor.Current = Cursors.Cross;
            }
            else
            {

            }
            if (_isDrawingLine)
            {
                //Invalidate();
                //_endPoint = e.Location;
            }

            if(_moveTopologyObject)
            {
                int dx = e.X - lastMousePos.X;
                int dy = e.Y - lastMousePos.Y;
                lastMousePos = e.Location;
                
                System.Drawing.Rectangle bounds = new System.Drawing.Rectangle(dx + _topologyObjectBeingMoved.Bounds.X, dy + _topologyObjectBeingMoved.Bounds.Y, _topologyObjectBeingMoved.Bounds.Width, 100);
                _topologyObjectBeingMoved.Bounds = bounds;

                // Move the red input/output connecting points
                List<Point> tempCollection = new List<Point>();
                tempCollection.AddRange(_topologyObjectBeingMoved.InputPoints);
                int i = 0;
                foreach (Point p in _topologyObjectBeingMoved.InputPoints)
                {
                    tempCollection[i] = new Point(p.X + dx, p.Y + dy);
                }
                _topologyObjectBeingMoved.InputPoints.Clear();
                _topologyObjectBeingMoved.InputPoints.AddRange(tempCollection);

                tempCollection.Clear();

                tempCollection.AddRange(_topologyObjectBeingMoved.OutputPoints);

                foreach (Point p in _topologyObjectBeingMoved.OutputPoints)
                {
                    tempCollection[i] = new Point(p.X + dx, p.Y + dy);
                }

                _topologyObjectBeingMoved.OutputPoints.Clear();
                _topologyObjectBeingMoved.OutputPoints.AddRange(tempCollection);
                tempCollection.Clear();

                Invalidate();
            }
        }

        private void FormPlantTopology_MouseUp(object sender, MouseEventArgs e)
        {
            ConnectingLine connectingLine = isPointWithinInputPoints2(e.Location, _currentConnectingLine);
            if (connectingLine.InputPointIndex != -1 && connectingLine.OutputPointIndex != -1 && _isDrawingLine)
            {
                _connectingLines.Add(connectingLine);
                _isDrawingLine = false;
                Invalidate();
            }

            if(_moveTopologyObject)
            {
                _moveTopologyObject = false;
                Invalidate();
            }
        }

        private void FormPlantTopology_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                if (selectedRectangle != null)
                {
                    foreach (TopologyObject topologyObject in topologyObjects)
                    {
                        if (selectedRectangle.Value.X == topologyObject.Bounds.X && selectedRectangle.Value.Y == topologyObject.Bounds.Y)
                        {
                            topologyObjects.Remove(topologyObject);
                            Invalidate();
                            return;
                        }
                    }
                }
            }
        }

        public void DeleteSelectedObject()
        {
            if(selectedRectangle == null)
            {
                return;
            }


            foreach (TopologyObject topologyObject in topologyObjects)
            {
                if (selectedRectangle.Value.X == topologyObject.Bounds.X && selectedRectangle.Value.Y == topologyObject.Bounds.Y)
                {
                    var temp1 = _connectingLines;
                    topologyObjects.Remove(topologyObject);

                    foreach (Label label in this.Controls)
                    {
                        if(label.Tag.ToString() == topologyObject.id)
                        {
                            Controls.Remove(label);
                            break;
                        }
                    }

                    foreach(ConnectingLine connectingLine in _connectingLines)
                    {
                        if(connectingLine.TopologyObject1 == topologyObject)
                        {
                            _connectingLines.Remove(connectingLine);
                            break;
                        }
                    }

                    foreach (ConnectingLine connectingLine in _connectingLines)
                    {
                           
                        if (connectingLine.TopologyObject2 == topologyObject)
                        {
                            _connectingLines.Remove(connectingLine);
                            break;
                        }
                    }
                    var temp2 = _connectingLines;
                    selectedRectangle = null;
                    Invalidate();
                    return;
                }
            }
        }

        private void clearAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DialogResult.Yes == MessageBox.Show("Are you sure to clear all the topology drawings?", "Clear All", MessageBoxButtons.YesNo))
            {
                topologyObjects.Clear();
                _connectingLines.Clear();
                lines.Clear();
                Controls.Clear();
                selectedRectangle = null;
                Invalidate();
                
            }
        }

        List<TopologyObject> topologyObjects = new List<TopologyObject>();
        private System.Drawing.Rectangle? selectedRectangle = null;
        int InputOutputPointsWidth = 6;
        bool _isDrawingLine = false;
        List<Line> lines = new List<Line>();
        Color _rectangleColor = Color.Black;
        Color _connectingLineColor = Color.Blue;

        bool _moveTopologyObject = false;
        TopologyObject _topologyObjectBeingMoved;
        private Point lastMousePos;

        struct Line
        {
            public Point p1;
            public Point p2;
        }

        struct ConnectingLine
        {
            public TopologyObject TopologyObject1;
            public TopologyObject TopologyObject2;
            public int InputPointIndex; //p1
            public int OutputPointIndex; //p2
        }

        List<ConnectingLine> _connectingLines = new List<ConnectingLine>();
    }
}
