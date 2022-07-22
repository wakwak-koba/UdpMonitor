using System.Linq;

namespace UdpMonitor
{
    internal class Message : System.Windows.Forms.ListBox
    {
        public Message() : base() {
            Dock = System.Windows.Forms.DockStyle.Fill;
            SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            HorizontalScrollbar = true;
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

        protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                if (this.SelectedIndices.Contains(this.IndexFromPoint(new System.Drawing.Point(e.X, e.Y))))
                {
                    var text = string.Join("", this.SelectedItems.OfType<string>().Select(s => s + System.Environment.NewLine).ToArray());
                    System.Windows.Forms.Clipboard.SetText(text);
                }
                else
                    this.ClearSelected();
            }
            else
                base.OnMouseUp(e);
        }
    }
}
