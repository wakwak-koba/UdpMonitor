using System.Linq;

namespace UdpMonitor
{
    public partial class Form1 : System.Windows.Forms.Form
    {
        public Form1() : base()
        {
            InitializeComponent();

            if(UdpMonitor.Properties.Settings.Default.Size != null && !System.Drawing.Size.Empty.Equals(UdpMonitor.Properties.Settings.Default.Size))
                this.Size = UdpMonitor.Properties.Settings.Default.Size;

            tabPorts.AddPort(new System.Net.IPEndPoint(System.Net.IPAddress.Any, 514));

            txtPort.Enter += (sender, e) => this.AcceptButton = btnAddPort;

            txtPort.Leave += (sender, e) => this.AcceptButton = null;

            btnAddPort.Click += (sender, e) =>
            {
                System.UInt16 portnumber;
                if (System.UInt16.TryParse(txtPort.Text, out portnumber))
                {
                    tabPorts.AddPort(new System.Net.IPEndPoint(System.Net.IPAddress.Any, portnumber));
                    txtPort.Clear();
                    txtPort.Focus();
                }
            };

            this.SizeChanged += (sender1, e1) =>
            {
                if (this.WindowState == System.Windows.Forms.FormWindowState.Normal)
                {
                    UdpMonitor.Properties.Settings.Default.Size = this.Size;
                    UdpMonitor.Properties.Settings.Default.Save();
                }
            };
        }
    }
}





