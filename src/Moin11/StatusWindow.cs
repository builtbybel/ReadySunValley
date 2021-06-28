using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Moin11
{
    public partial class StatusWindow : Form
    {
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
    (
    int nLeftRect,
    int nTopRect,
    int nRightRect,
    int nBottomRect,
    int nWidthEllipse,
    int nHeightEllipse
    );

        public string StatusText
        {
            get
            {
                return this.status.Text;
            }
            set
            {
                this.status.Text = value;
            }
        }

        public StatusWindow()
        {
            InitializeComponent();
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 5, 5));
        }

        private void StatusWindow_Load(object sender, EventArgs e)
        {
        }
    }
}