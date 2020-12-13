using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace Trader
{
    public sealed class ControlPanel : Form
    {
        public TableLayoutPanel Panel { get; private set; }

        public ControlPanel()
        {
            SuspendLayout();
            Panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
            };
            Controls.Add(Panel);
            var topMostCheckBox = CreateTopMostCheckBox();
            Panel.Controls.Add(topMostCheckBox);
            Shown += (s, e) =>
            {
                TopMost = Const.CONTROL_PANEL_TOP_MOST;
                topMostCheckBox.Checked = TopMost;
            };
            ResumeLayout(false);
        }

        private CheckBox CreateTopMostCheckBox()
        {
            var topMostCheckBox = new CheckBox
            {
                Text = "Always on top",
                Dock = DockStyle.Top
            };
            topMostCheckBox.CheckedChanged += (s, e) =>
            {
                TopMost = topMostCheckBox.Checked;
            };
            return topMostCheckBox;
        }
        public void Add(Form form)
        {
            SuspendLayout();
            var checkBox = new CheckBox
            {
                Text = form.Text,
                Checked = form.Visible,
                Dock = DockStyle.Top,
            };
            checkBox.CheckedChanged += (s, e) =>
            {
                form.Visible = checkBox.Checked;
            };
            form.FormClosing += (s, e) =>
            {
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    e.Cancel = true;
                    form.Hide();
                    checkBox.Checked = false;
                }
            };
            Panel.Controls.Add(checkBox);
            ResumeLayout(false);
        }
    }
}
