using System;
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
    public partial class FormMtpInfo : Form
    {
        public FormMtpInfo()
        {
            InitializeComponent();
        }

        public string fileName;

        private void FormMtpInfo_Load(object sender, EventArgs e)
        {
            this.Text = Path.GetFileName(fileName);
            textBox1.Text = InfoText;
        }

        private void FormMtpInfo_Resize(object sender, EventArgs e)
        {
            textBox1.Width = this.Width - 40;
            textBox1.Height = this.Height - 80;
        }

        private string infoText;

        public string InfoText { get { return infoText; } set { infoText = value; } }
    }
}
