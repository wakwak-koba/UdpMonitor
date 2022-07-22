using System.Linq;

namespace UdpMonitor
{
    internal class TabPageClient : System.Windows.Forms.TabPage
    {
        public System.Net.IPAddress Address { get; private set; }
        Message msgText;

        public TabPageClient(System.Net.IPAddress addr) : base()
        {
            msgText = new Message();
            this.Controls.Add(msgText);
            Address = addr;
        }

        public void Add(int port, byte[] buffer) => msgText.Add(port, buffer);
    }

    internal class TabControlClients : System.Windows.Forms.TabControl
    {
        public void Add(System.Net.IPEndPoint ep, byte[] buffer)
        {
            var tab = this.TabPages.OfType<TabPageClient>().Where(t => t.Address.Equals(ep.Address)).FirstOrDefault();
            if (tab == null)
            {
                tab = new TabPageClient(ep.Address);
                TabPages.Add(tab);
            }
            tab.Add(ep.Port, buffer);
        }
    }

    internal class TabPage : System.Windows.Forms.TabPage
    {
        TabControlClients Clients;

        public System.Net.IPEndPoint endPoint { get; private set; }
        System.Net.Sockets.UdpClient udpClient;

        public TabPage(System.Net.IPEndPoint ep) : base()
        {
            Clients = new TabControlClients() { Dock = System.Windows.Forms.DockStyle.Fill };
            this.Controls.Add(Clients);
            this.Text = ep.ToString();
            endPoint = ep;
        }

        public TabControlPort TabControl { get => this.Parent as TabControlPort; }

        public async void Start()
        {
            udpClient = new System.Net.Sockets.UdpClient(endPoint);
            for (; ; )
            {
                var result = await udpClient.ReceiveAsync();
                Clients.Add(result.RemoteEndPoint, result.Buffer);
            }
        }

        public void Close() => udpClient?.Close();
    }

    /// <summary>
    /// リッスンポート
    /// </summary>
    internal class TabControlPort : System.Windows.Forms.TabControl
    {
        public TabControlPort() : base() {; }

        public TabPage AddPort(System.Net.IPEndPoint ep, bool select = true)
        {
            var tab = this.TabPages.OfType<TabPage>().Where(t => t.endPoint == ep).FirstOrDefault();
            if(tab == null) {
                tab = new TabPage(ep);
                tab.Start();
                TabPages.Add(tab);
            }
            if (select)
                this.SelectedTab = tab;
            return tab;
        }
    }
}
