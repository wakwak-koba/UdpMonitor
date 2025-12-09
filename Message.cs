using System.Linq;

namespace UdpMonitor
{
    internal class Message : System.Windows.Forms.ContainerControl
    {
        System.Windows.Forms.FlowLayoutPanel pnlStatus;
        System.Windows.Forms.Label lblCount;
        System.Windows.Forms.TabControl tabDays;

        public Message() : base() {
            tabDays = new System.Windows.Forms.TabControl() { Dock = System.Windows.Forms.DockStyle.Fill, Alignment = System.Windows.Forms.TabAlignment.Bottom };
            this.Controls.Add(tabDays);

            pnlStatus = new System.Windows.Forms.FlowLayoutPanel() { Dock = System.Windows.Forms.DockStyle.Top, Height = 16 };
            this.Controls.Add(pnlStatus);
            pnlStatus.Controls.Add(new System.Windows.Forms.Label() { Text = "records:" });
            lblCount = new System.Windows.Forms.Label();
            pnlStatus.Controls.Add(lblCount);
        }

        public void Add(int port, byte[] buffer)
        {
            System.Windows.Forms.ListBox lstMessage;
            var now = System.DateTime.Now;
            var tabKey = now.ToString("yyyyMMdd");
            var tabPage = tabDays.TabPages[tabKey];
            if(tabPage == null)
            {
                tabPage = new System.Windows.Forms.TabPage() { Name=tabKey, Text = tabKey };
                tabDays.TabPages.Insert(0, tabPage);

                lstMessage = new System.Windows.Forms.ListBox() { Dock = System.Windows.Forms.DockStyle.Fill, SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended, HorizontalScrollbar = true };
                tabPage.Controls.Add(lstMessage);

                lstMessage.MouseUp += (sender, e) =>
                {
                    if (e.Button == System.Windows.Forms.MouseButtons.Right)
                    {
                        if (lstMessage.SelectedIndices.Contains(lstMessage.IndexFromPoint(new System.Drawing.Point(e.X, e.Y))))
                            ToClipboard(lstMessage.SelectedItems.OfType<string>());
                        else
                            lstMessage.SelectedIndex = -1;
                    }
                };
                lstMessage.KeyDown += (sender, e) =>
                {
                    if (e.KeyCode == System.Windows.Forms.Keys.Escape)
                    {
                        lstMessage.ClearSelected();
                        lstMessage.ClearSelected();
                    }

                };
            } else
                lstMessage = tabPage.Controls[0] as System.Windows.Forms.ListBox;

            var selected = lstMessage.SelectedIndices.OfType<int>().ToArray();
            var text = string.Join("\t"
                , now.ToString("yyyy/MM/dd HH:mm:ss.fff")
                , port.ToString().PadLeft(5)
                , System.Text.Encoding.UTF8.GetString(buffer)
                );

            lstMessage.BeginUpdate();
            var index = lstMessage.Items.Add(text);

            if (selected.Length == 0)
            {
                lstMessage.SelectedIndex = index;
                lstMessage.ClearSelected();
                if(tabDays.TabPages.Count > 0)
                    tabDays.SelectTab(0);
            }

            lstMessage.EndUpdate();
            SetDesign();
        }

        public void Clear()
        {
            tabDays.TabPages.Clear();
            SetDesign();
        }

        public int Count()
        {
            return tabDays.TabPages.OfType<System.Windows.Forms.TabPage>().SelectMany(t => t.Controls.OfType<System.Windows.Forms.ListBox>()).Count();
        }

        public void ToClipboard() {
            var st = new System.Text.StringBuilder();
            foreach (var lst in tabDays.TabPages.OfType<System.Windows.Forms.TabPage>().Select(t => t.Controls[0] as System.Windows.Forms.ListBox))
                st.AppendLine(string.Join("", lst.Items.OfType<string>().Select(s => s + System.Environment.NewLine).ToArray()));
            if(st.Length > 0)
                System.Windows.Forms.Clipboard.SetText(st.ToString());
        }

        public void ToClipboard(System.Collections.Generic.IEnumerable<string> texts)
        {
            var text = string.Join("", texts.Select(s => s + System.Environment.NewLine).ToArray());
            if(!string.IsNullOrEmpty(text))
                System.Windows.Forms.Clipboard.SetText(text);
        }

        public void OnEnterParent()
        {
            if(tabDays.SelectedTab != null && tabDays.SelectedTab.Controls[0] is System.Windows.Forms.ListBox listBox && listBox.SelectedIndex < 0)
            {
                // 未選択なら直近タブを選択
                tabDays.SelectTab(tabDays.TabPages[0]);
                listBox = tabDays.TabPages[0].Controls[0] as System.Windows.Forms.ListBox;
                listBox.SelectedIndex = listBox.Items.Count - 1;
                listBox.SelectedIndex = -1;
            }
        }

        public void SetDesign()
        {
            lblCount.Text = tabDays.TabPages.OfType<System.Windows.Forms.TabPage>()
                .SelectMany(c => c.Controls.OfType<System.Windows.Forms.ListBox>())
                .Sum(l => l.Items.Count).ToString();
            ;
        }
    }
}
