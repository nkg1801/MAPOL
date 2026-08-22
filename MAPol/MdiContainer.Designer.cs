namespace MAPol
{
    partial class MdiContainer
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            TreeNode treeNode1 = new TreeNode("MTP Libraries");
            TreeNode treeNode2 = new TreeNode("Plant Topology");
            TreeNode treeNode3 = new TreeNode("Recipes");
            TreeNode treeNode4 = new TreeNode("Project Name", new TreeNode[] { treeNode1, treeNode2, treeNode3 });
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MdiContainer));
            contextMenuStrip1 = new ContextMenuStrip(components);
            toolStripMenuItem1 = new ToolStripMenuItem();
            removeAllMTPsToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem4 = new ToolStripSeparator();
            openAllProcessDisplaysToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            recentMTPFilesToolStripMenuItem = new ToolStripMenuItem();
            contextMenuStripTopology = new ContextMenuStrip(components);
            toolStripMenuItem2 = new ToolStripMenuItem();
            contextMenuStripRecipe = new ContextMenuStrip(components);
            toolStripMenuItem3 = new ToolStripMenuItem();
            toolStrip1 = new ToolStrip();
            toolStripLabel1 = new ToolStripLabel();
            toolStripSeparator3 = new ToolStripSeparator();
            toolStripLabel6 = new ToolStripLabel();
            toolStripLabel7 = new ToolStripLabel();
            toolStripSeparator2 = new ToolStripSeparator();
            toolStripButton1 = new ToolStripButton();
            toolStripButtonErrorList = new ToolStripButton();
            toolStripLabel2 = new ToolStripLabel();
            toolStripLabel3 = new ToolStripLabel();
            toolStripLabel4 = new ToolStripLabel();
            toolStripLabel5 = new ToolStripLabel();
            splitContainer1 = new SplitContainer();
            labelProjectView = new Label();
            treeViewProject = new TreeView();
            imageList1 = new ImageList(components);
            tabControl1 = new TabControl();
            openFileDialog1 = new OpenFileDialog();
            contextMenuStripTabPage = new ContextMenuStrip(components);
            toolStripMenuItemCloseAllButThis = new ToolStripMenuItem();
            toolStripMenuItemCloseAllTabs = new ToolStripMenuItem();
            contextMenuStripMtpFile = new ContextMenuStrip(components);
            mTPInfoToolStripMenuItem = new ToolStripMenuItem();
            openManifestXMLToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem5 = new ToolStripSeparator();
            removeToolStripMenuItem = new ToolStripMenuItem();
            contextMenuStrip1.SuspendLayout();
            contextMenuStripTopology.SuspendLayout();
            contextMenuStripRecipe.SuspendLayout();
            toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            contextMenuStripTabPage.SuspendLayout();
            contextMenuStripMtpFile.SuspendLayout();
            SuspendLayout();
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.ImageScalingSize = new Size(20, 20);
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { toolStripMenuItem1, removeAllMTPsToolStripMenuItem, toolStripMenuItem4, openAllProcessDisplaysToolStripMenuItem, toolStripSeparator1, recentMTPFilesToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(207, 104);
            contextMenuStrip1.Opening += contextMenuStrip1_Opening;
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new Size(206, 22);
            toolStripMenuItem1.Text = "Import MTP File(s)";
            toolStripMenuItem1.Click += toolStripMenuItem1_Click;
            // 
            // removeAllMTPsToolStripMenuItem
            // 
            removeAllMTPsToolStripMenuItem.Name = "removeAllMTPsToolStripMenuItem";
            removeAllMTPsToolStripMenuItem.Size = new Size(206, 22);
            removeAllMTPsToolStripMenuItem.Text = "Remove all MTPs";
            removeAllMTPsToolStripMenuItem.Click += removeAllMTPsToolStripMenuItem_Click;
            // 
            // toolStripMenuItem4
            // 
            toolStripMenuItem4.Name = "toolStripMenuItem4";
            toolStripMenuItem4.Size = new Size(203, 6);
            // 
            // openAllProcessDisplaysToolStripMenuItem
            // 
            openAllProcessDisplaysToolStripMenuItem.Name = "openAllProcessDisplaysToolStripMenuItem";
            openAllProcessDisplaysToolStripMenuItem.Size = new Size(206, 22);
            openAllProcessDisplaysToolStripMenuItem.Text = "Open all process displays";
            openAllProcessDisplaysToolStripMenuItem.Click += openAllProcessDisplaysToolStripMenuItem_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(203, 6);
            // 
            // recentMTPFilesToolStripMenuItem
            // 
            recentMTPFilesToolStripMenuItem.Name = "recentMTPFilesToolStripMenuItem";
            recentMTPFilesToolStripMenuItem.Size = new Size(206, 22);
            recentMTPFilesToolStripMenuItem.Text = "Recent MTP Files";
            // 
            // contextMenuStripTopology
            // 
            contextMenuStripTopology.ImageScalingSize = new Size(20, 20);
            contextMenuStripTopology.Items.AddRange(new ToolStripItem[] { toolStripMenuItem2 });
            contextMenuStripTopology.Name = "contextMenuStripTopology";
            contextMenuStripTopology.Size = new Size(150, 26);
            // 
            // toolStripMenuItem2
            // 
            toolStripMenuItem2.Name = "toolStripMenuItem2";
            toolStripMenuItem2.Size = new Size(149, 22);
            toolStripMenuItem2.Text = "Add Topology";
            // 
            // contextMenuStripRecipe
            // 
            contextMenuStripRecipe.ImageScalingSize = new Size(20, 20);
            contextMenuStripRecipe.Items.AddRange(new ToolStripItem[] { toolStripMenuItem3 });
            contextMenuStripRecipe.Name = "contextMenuStripRecipe";
            contextMenuStripRecipe.Size = new Size(135, 26);
            // 
            // toolStripMenuItem3
            // 
            toolStripMenuItem3.Name = "toolStripMenuItem3";
            toolStripMenuItem3.Size = new Size(134, 22);
            toolStripMenuItem3.Text = "Add Recipe";
            // 
            // toolStrip1
            // 
            toolStrip1.ImageScalingSize = new Size(64, 64);
            toolStrip1.Items.AddRange(new ToolStripItem[] { toolStripLabel1, toolStripSeparator3, toolStripLabel6, toolStripLabel7, toolStripSeparator2, toolStripButton1, toolStripButtonErrorList, toolStripLabel2, toolStripLabel3, toolStripLabel4, toolStripLabel5 });
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(1100, 81);
            toolStrip1.TabIndex = 1;
            toolStrip1.Text = "toolStrip1";
            toolStrip1.ItemClicked += toolStrip1_ItemClicked;
            // 
            // toolStripLabel1
            // 
            toolStripLabel1.Image = Properties.Resources.import_mtp_icon;
            toolStripLabel1.Name = "toolStripLabel1";
            toolStripLabel1.Padding = new Padding(10, 0, 10, 0);
            toolStripLabel1.Size = new Size(84, 78);
            toolStripLabel1.TextImageRelation = TextImageRelation.Overlay;
            toolStripLabel1.Click += toolStripLabel1_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(6, 81);
            // 
            // toolStripLabel6
            // 
            toolStripLabel6.Image = Properties.Resources._operator;
            toolStripLabel6.Name = "toolStripLabel6";
            toolStripLabel6.Padding = new Padding(10, 0, 10, 0);
            toolStripLabel6.Size = new Size(84, 78);
            toolStripLabel6.Visible = false;
            toolStripLabel6.Click += toolStripLabel6_Click;
            // 
            // toolStripLabel7
            // 
            toolStripLabel7.Image = Properties.Resources.engineer;
            toolStripLabel7.Name = "toolStripLabel7";
            toolStripLabel7.Padding = new Padding(10, 0, 10, 0);
            toolStripLabel7.Size = new Size(84, 78);
            toolStripLabel7.Visible = false;
            toolStripLabel7.Click += toolStripLabel7_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(6, 81);
            // 
            // toolStripButton1
            // 
            toolStripButton1.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton1.Image = Properties.Resources.info;
            toolStripButton1.ImageTransparentColor = Color.Magenta;
            toolStripButton1.Name = "toolStripButton1";
            toolStripButton1.Padding = new Padding(10, 0, 10, 0);
            toolStripButton1.Size = new Size(88, 78);
            toolStripButton1.Text = "toolStripButton1";
            toolStripButton1.Click += toolStripButton1_Click;
            // 
            // toolStripButtonErrorList
            // 
            toolStripButtonErrorList.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButtonErrorList.Image = Properties.Resources.errors;
            toolStripButtonErrorList.ImageTransparentColor = Color.Magenta;
            toolStripButtonErrorList.Name = "toolStripButtonErrorList";
            toolStripButtonErrorList.Padding = new Padding(10, 5, 10, 5);
            toolStripButtonErrorList.Size = new Size(88, 78);
            toolStripButtonErrorList.Text = "toolStripButtonErrorList";
            // 
            // toolStripLabel2
            // 
            toolStripLabel2.Name = "toolStripLabel2";
            toolStripLabel2.Size = new Size(106, 78);
            toolStripLabel2.Text = "Topology Designer";
            toolStripLabel2.Visible = false;
            // 
            // toolStripLabel3
            // 
            toolStripLabel3.Name = "toolStripLabel3";
            toolStripLabel3.Size = new Size(42, 78);
            toolStripLabel3.Text = "Recipe";
            toolStripLabel3.Visible = false;
            // 
            // toolStripLabel4
            // 
            toolStripLabel4.Name = "toolStripLabel4";
            toolStripLabel4.Size = new Size(37, 78);
            toolStripLabel4.Text = "Views";
            toolStripLabel4.Visible = false;
            // 
            // toolStripLabel5
            // 
            toolStripLabel5.Name = "toolStripLabel5";
            toolStripLabel5.Size = new Size(93, 78);
            toolStripLabel5.Text = "Process Displays";
            toolStripLabel5.Visible = false;
            toolStripLabel5.Click += toolStripLabel5_Click;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 81);
            splitContainer1.Margin = new Padding(3, 2, 3, 2);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(labelProjectView);
            splitContainer1.Panel1.Controls.Add(treeViewProject);
            splitContainer1.Panel1.Resize += splitContainer1_Panel1_Resize;
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(tabControl1);
            splitContainer1.Size = new Size(1100, 470);
            splitContainer1.SplitterDistance = 230;
            splitContainer1.SplitterWidth = 9;
            splitContainer1.TabIndex = 3;
            // 
            // labelProjectView
            // 
            labelProjectView.BackColor = Color.FromArgb(192, 192, 255);
            labelProjectView.Location = new Point(0, 0);
            labelProjectView.Name = "labelProjectView";
            labelProjectView.Size = new Size(228, 20);
            labelProjectView.TabIndex = 1;
            labelProjectView.Text = "Project Explorer";
            labelProjectView.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // treeViewProject
            // 
            treeViewProject.AllowDrop = true;
            treeViewProject.ImageKey = "document-management.png";
            treeViewProject.ImageList = imageList1;
            treeViewProject.Location = new Point(0, 22);
            treeViewProject.Margin = new Padding(3, 2, 3, 2);
            treeViewProject.Name = "treeViewProject";
            treeNode1.ContextMenuStrip = contextMenuStrip1;
            treeNode1.ImageKey = "mtp-logo-2.png";
            treeNode1.Name = "NodeMtpLibraries";
            treeNode1.SelectedImageKey = "mtp-logo-2.png";
            treeNode1.Text = "MTP Libraries";
            treeNode2.ContextMenuStrip = contextMenuStripTopology;
            treeNode2.ImageKey = "topology-structure.png";
            treeNode2.Name = "NodePlantTopology";
            treeNode2.SelectedImageKey = "topology-structure.png";
            treeNode2.Text = "Plant Topology";
            treeNode3.ContextMenuStrip = contextMenuStripRecipe;
            treeNode3.ImageKey = "sfc.png";
            treeNode3.Name = "NodeRecipes";
            treeNode3.SelectedImageKey = "sfc.png";
            treeNode3.Text = "Recipes";
            treeNode4.ImageIndex = 2;
            treeNode4.Name = "NodeProjectName";
            treeNode4.SelectedImageKey = "document-management.png";
            treeNode4.Text = "Project Name";
            treeViewProject.Nodes.AddRange(new TreeNode[] { treeNode4 });
            treeViewProject.SelectedImageIndex = 0;
            treeViewProject.Size = new Size(225, 480);
            treeViewProject.TabIndex = 0;
            treeViewProject.ItemDrag += treeViewProject_ItemDrag;
            treeViewProject.NodeMouseDoubleClick += treeViewProject_NodeMouseDoubleClick;
            treeViewProject.Click += treeViewProject_Click;
            treeViewProject.DragDrop += treeViewProject_DragDrop;
            treeViewProject.DragEnter += treeViewProject_DragEnter;
            treeViewProject.MouseDown += treeViewProject_MouseDown;
            // 
            // imageList1
            // 
            imageList1.ColorDepth = ColorDepth.Depth32Bit;
            imageList1.ImageStream = (ImageListStreamer)resources.GetObject("imageList1.ImageStream");
            imageList1.TransparentColor = Color.Transparent;
            imageList1.Images.SetKeyName(0, "mtp logo.png");
            imageList1.Images.SetKeyName(1, "topology-structure.png");
            imageList1.Images.SetKeyName(2, "document-management.png");
            imageList1.Images.SetKeyName(3, "sfc.png");
            imageList1.Images.SetKeyName(4, "mtp-logo-2.png");
            imageList1.Images.SetKeyName(5, "import-mtp-icon.png");
            // 
            // tabControl1
            // 
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabControl1.HotTrack = true;
            tabControl1.ItemSize = new Size(230, 40);
            tabControl1.Location = new Point(0, 0);
            tabControl1.Margin = new Padding(3, 2, 3, 2);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(861, 470);
            tabControl1.SizeMode = TabSizeMode.Fixed;
            tabControl1.TabIndex = 0;
            tabControl1.DrawItem += tabControl1_DrawItem_1;
            tabControl1.MouseClick += tabControl1_MouseClick;
            tabControl1.MouseDown += tabControl1_MouseDown;
            tabControl1.MouseUp += tabControl1_MouseUp;
            // 
            // openFileDialog1
            // 
            openFileDialog1.FileName = "openFileDialog1";
            // 
            // contextMenuStripTabPage
            // 
            contextMenuStripTabPage.ImageScalingSize = new Size(20, 20);
            contextMenuStripTabPage.Items.AddRange(new ToolStripItem[] { toolStripMenuItemCloseAllButThis, toolStripMenuItemCloseAllTabs });
            contextMenuStripTabPage.Name = "contextMenuStripTabPage";
            contextMenuStripTabPage.Size = new Size(187, 48);
            contextMenuStripTabPage.Opening += contextMenuStripTabPage_Opening;
            // 
            // toolStripMenuItemCloseAllButThis
            // 
            toolStripMenuItemCloseAllButThis.Name = "toolStripMenuItemCloseAllButThis";
            toolStripMenuItemCloseAllButThis.Size = new Size(186, 22);
            toolStripMenuItemCloseAllButThis.Text = "Close all tabs but this";
            toolStripMenuItemCloseAllButThis.Click += toolStripMenuItemCloseAllButThis_Click;
            // 
            // toolStripMenuItemCloseAllTabs
            // 
            toolStripMenuItemCloseAllTabs.Name = "toolStripMenuItemCloseAllTabs";
            toolStripMenuItemCloseAllTabs.Size = new Size(186, 22);
            toolStripMenuItemCloseAllTabs.Text = "Close All tabs";
            toolStripMenuItemCloseAllTabs.Click += toolStripMenuItemCloseAllTabs_Click;
            // 
            // contextMenuStripMtpFile
            // 
            contextMenuStripMtpFile.ImageScalingSize = new Size(20, 20);
            contextMenuStripMtpFile.Items.AddRange(new ToolStripItem[] { mTPInfoToolStripMenuItem, openManifestXMLToolStripMenuItem, toolStripMenuItem5, removeToolStripMenuItem });
            contextMenuStripMtpFile.Name = "contextMenuStripMtpFile";
            contextMenuStripMtpFile.Size = new Size(180, 76);
            // 
            // mTPInfoToolStripMenuItem
            // 
            mTPInfoToolStripMenuItem.Name = "mTPInfoToolStripMenuItem";
            mTPInfoToolStripMenuItem.Size = new Size(179, 22);
            mTPInfoToolStripMenuItem.Text = "MTP Info";
            mTPInfoToolStripMenuItem.Click += mTPInfoToolStripMenuItem_Click;
            // 
            // openManifestXMLToolStripMenuItem
            // 
            openManifestXMLToolStripMenuItem.Name = "openManifestXMLToolStripMenuItem";
            openManifestXMLToolStripMenuItem.Size = new Size(179, 22);
            openManifestXMLToolStripMenuItem.Text = "Open Manifest XML";
            openManifestXMLToolStripMenuItem.Click += openManifestXMLToolStripMenuItem_Click;
            // 
            // toolStripMenuItem5
            // 
            toolStripMenuItem5.Name = "toolStripMenuItem5";
            toolStripMenuItem5.Size = new Size(176, 6);
            // 
            // removeToolStripMenuItem
            // 
            removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            removeToolStripMenuItem.Size = new Size(179, 22);
            removeToolStripMenuItem.Text = "Remove";
            removeToolStripMenuItem.Click += removeToolStripMenuItem_Click;
            // 
            // MdiContainer
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1100, 551);
            Controls.Add(splitContainer1);
            Controls.Add(toolStrip1);
            IsMdiContainer = true;
            KeyPreview = true;
            Margin = new Padding(3, 2, 3, 2);
            Name = "MdiContainer";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Modular Automation - POL";
            WindowState = FormWindowState.Maximized;
            Load += Form1_Load;
            KeyDown += MdiContainer_KeyDown;
            Resize += Form1_Resize;
            contextMenuStrip1.ResumeLayout(false);
            contextMenuStripTopology.ResumeLayout(false);
            contextMenuStripRecipe.ResumeLayout(false);
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            contextMenuStripTabPage.ResumeLayout(false);
            contextMenuStripMtpFile.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ToolStrip toolStrip1;
        private ToolStripLabel toolStripLabel5;
        private ToolStripLabel toolStripLabel1;
        private ToolStripLabel toolStripLabel2;
        private ToolStripLabel toolStripLabel3;
        private ToolStripLabel toolStripLabel4;
        private SplitContainer splitContainer1;
        private Label labelProjectView;
        private TreeView treeViewProject;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem toolStripMenuItem1;
        private OpenFileDialog openFileDialog1;
        private ImageList imageList1;
        private ContextMenuStrip contextMenuStripTopology;
        private ToolStripMenuItem toolStripMenuItem2;
        private ContextMenuStrip contextMenuStripRecipe;
        private ToolStripMenuItem toolStripMenuItem3;
        private TabControl tabControl1;
        private ToolStripMenuItem openAllProcessDisplaysToolStripMenuItem;
        private ToolStripLabel toolStripLabel6;
        private ToolStripLabel toolStripLabel7;
        private ContextMenuStrip contextMenuStripTabPage;
        private ToolStripMenuItem toolStripMenuItemCloseAllButThis;
        private ToolStripMenuItem toolStripMenuItemCloseAllTabs;
        private ToolStripMenuItem removeAllMTPsToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem4;
        private ContextMenuStrip contextMenuStripMtpFile;
        private ToolStripMenuItem removeToolStripMenuItem;
        private ToolStripMenuItem mTPInfoToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem5;
        private ToolStripMenuItem openManifestXMLToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem recentMTPFilesToolStripMenuItem;
        private ToolStripButton toolStripButton1;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripButton toolStripButtonErrorList;
    }
}
