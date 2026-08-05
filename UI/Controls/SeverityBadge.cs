using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using frm_pin_remover.Models;

namespace frm_pin_remover.UI.Controls
{
    internal sealed class SeverityBadge : Guna2Panel
    {
        private readonly Label _label;

        public SeverityBadge()
        {
            AutoSize = false;
            BorderRadius = 10;
            Height = 22;

            _label = new Label
            {
                AutoSize = true,
                Location = new Point(10, 3),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI Semibold", 8f)
            };

            Controls.Add(_label);
        }

        public void SetSeverity(DiagnosticSeverity severity)
        {
            var (fg, bg) = Theme.SeverityColors(severity);
            FillColor = bg;
            _label.ForeColor = fg;
            _label.Text = Theme.SeverityLabel(severity);

            Size labelSize = _label.PreferredSize;
            _label.Size = labelSize;
            Width = labelSize.Width + 20;
            Height = labelSize.Height + 6;
        }
    }
}
