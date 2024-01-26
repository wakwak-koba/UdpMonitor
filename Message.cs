using System.Linq;

namespace UdpMonitor
{
    internal class Message : System.Windows.Forms.ListBox
    {
        public Message() : base() {
            Dock = System.Windows.Forms.DockStyle.Fill;
            SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            HorizontalScrollbar = true;
            this.MouseUp += (sender, e) =>
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Right)
                {
                    if (this.SelectedIndices.Contains(this.IndexFromPoint(new System.Drawing.Point(e.X, e.Y))))
                        ToClipboard(this.SelectedItems.OfType<string>());
                    else
                        this.ClearSelected();
                }
            };
        }

        public void Add(int port, byte[] buffer)
        {
            var index = Items.Add(string.Join("\t"
                , System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff")
                , port.ToString().PadLeft(5)
                , System.Text.Encoding.UTF8.GetString(buffer)
                ));
            if (this.SelectedIndices.Count == 0)
            {
                this.SelectedIndex = index;
                this.ClearSelected();
            }
        }

        public void Clear() => Items.Clear();

        public void ToClipboard() => ToClipboard(this.Items.OfType<string>());
        public void ToClipboard(System.Collections.Generic.IEnumerable<string> texts)
        {
            var text = string.Join("", texts.Select(s => s + System.Environment.NewLine).ToArray());
            if(!string.IsNullOrEmpty(text))
                System.Windows.Forms.Clipboard.SetText(text);
        }
    }
}
