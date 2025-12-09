using System.Linq;

namespace UdpMonitor
{
    internal class TabPageClient : System.Windows.Forms.TabPage
    {
        public System.Net.IPAddress Address { get; private set; }
        Message msgText;

        public TabPageClient(System.Net.IPAddress addr) : base()
        {
            msgText = new Message() { Dock = System.Windows.Forms.DockStyle.Fill };
            this.Controls.Add(msgText);
            this.Text = addr.ToString();
            this.Enter += (sender, e) => msgText.OnEnterParent();
            Address = addr;
        }

        public void Add(int port, byte[] buffer)
        {
            msgText.Add(port, buffer);
            {
                var nm = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), string.Join("", Address.GetAddressBytes().Select(b => b.ToString().PadLeft(3, '0'))));
                foreach(var ext in new [] { "bat", "exe"}) {
                    var hookFile = new System.IO.FileInfo(nm + "." + ext);
                    if(hookFile.Exists && hookFile.Length > 0)
                    {
                        var psInfo = new System.Diagnostics.ProcessStartInfo()
                        {
                            FileName = hookFile.FullName,
                            CreateNoWindow = true,
                            UseShellExecute = false,
                            Arguments = new System.Net.IPEndPoint(Address, port).ToString() + " " + System.Convert.ToBase64String(buffer),
                        };
                        try
                        {
                            System.Diagnostics.Process.Start(psInfo);
                            break;
                        }
                        catch (System.Exception)
                        {
                            ;
                        }
                    }
                }
            }
        } 

        public void Clear() => msgText.Clear();

        public void ToClipboard() {
            msgText.ToClipboard(); 
        }

        public int Count()
        {
            return msgText.Count();
        }
    }


    internal class TabControlClients : System.Windows.Forms.TabControl
    {
        public TabControlClients() : base() {
            this.MouseDoubleClick += (sender, e) =>
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Right && this.SelectedTab is TabPageClient tabPage && SelectedTab != null)
                {
                    if(tabPage.Count() > 0)
                    {
                        tabPage.ToClipboard();
                        tabPage.Clear();
                    }
                    else
                    {
                        this.TabPages.Remove(tabPage);
                    }
                }
            };
        }

        static System.UInt64 GetAddress(System.Net.IPAddress ip)
        {
            System.UInt64 result = 0;
            var addressBytes = ip.GetAddressBytes();
            for (int i = 0; i < addressBytes.Length; i++)
                result += ((System.UInt64)addressBytes[i] << (8 * (3 - i)));
            return result;
        }

        public void Add(System.Net.IPEndPoint ep, byte[] buffer)
        {
            var tab = this.TabPages.OfType<TabPageClient>().Where(t => t.Address.Equals(ep.Address)).FirstOrDefault();
            if (tab == null)
            {
                tab = new TabPageClient(ep.Address);
                var insertIndex = TabPages.OfType<TabPageClient>().SkipWhile(t => GetAddress(System.Net.IPAddress.Parse(t.Text)) <  GetAddress(ep.Address));
                if (!insertIndex.Any())
                    TabPages.Add(tab);
                else
                {
                    var insPos = TabPages.IndexOf(insertIndex.First());
                    TabPages.Insert(insPos, tab);
                }
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
