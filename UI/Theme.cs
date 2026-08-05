using System.Drawing;
using Guna.UI2.WinForms;

namespace frm_pin_remover.UI
{
    internal static class Theme
    {
        // Dark theme: the whole app (including the custom title bar) shares this palette so nothing
        // reads as a mismatched native-vs-app color.
        public static readonly Color Background = Color.FromArgb(18, 20, 26);
        public static readonly Color Sidebar = Color.FromArgb(14, 16, 21);
        public static readonly Color SidebarHover = Color.FromArgb(34, 38, 49);
        public static readonly Color TitleBar = Color.FromArgb(14, 16, 21);
        public static readonly Color Card = Color.FromArgb(28, 31, 40);
        public static readonly Color Border = Color.FromArgb(54, 59, 74);
        public static readonly Color TextPrimary = Color.FromArgb(237, 239, 245);
        public static readonly Color TextMuted = Color.FromArgb(149, 155, 173);

        public static readonly Color Primary = Color.FromArgb(78, 130, 245);
        public static readonly Color PrimaryHover = Color.FromArgb(99, 148, 250);
        public static readonly Color Success = Color.FromArgb(74, 222, 128);
        public static readonly Color SuccessBg = Color.FromArgb(20, 46, 30);
        public static readonly Color Info = Color.FromArgb(96, 165, 250);
        public static readonly Color InfoBg = Color.FromArgb(20, 34, 59);
        public static readonly Color Warning = Color.FromArgb(245, 165, 36);
        public static readonly Color WarningBg = Color.FromArgb(58, 42, 18);
        public static readonly Color Danger = Color.FromArgb(248, 113, 113);
        public static readonly Color DangerBg = Color.FromArgb(58, 20, 20);

        public static readonly Font FontRegular = new Font("Segoe UI", 9.5f, FontStyle.Regular);
        public static readonly Font FontSemibold = new Font("Segoe UI Semibold", 10f, FontStyle.Regular);
        public static readonly Font FontTitle = new Font("Segoe UI Semibold", 14f, FontStyle.Regular);
        public static readonly Font FontHeading = new Font("Segoe UI Semibold", 11f, FontStyle.Regular);

        public static void StyleCard(Guna2Panel panel)
        {
            panel.FillColor = Card;
            // Plain child Panels/Labels that don't set their own BackColor ambiently inherit this
            // (not FillColor), so it must match FillColor or they'd show the wrong shade behind text.
            panel.BackColor = Card;
            panel.BorderColor = Border;
            panel.BorderThickness = 1;
            panel.BorderRadius = 10;
            panel.ShadowDecoration.Enabled = true;
            panel.ShadowDecoration.Mode = Guna.UI2.WinForms.Enums.ShadowMode.Custom;
            panel.ShadowDecoration.Color = Color.FromArgb(90, 0, 0, 0);
            panel.ShadowDecoration.Depth = 14;
            panel.ShadowDecoration.BorderRadius = 10;
        }

        public static void StylePrimaryButton(Guna2Button button)
        {
            button.FillColor = Primary;
            button.ForeColor = Color.White;
            button.BorderRadius = 8;
            button.Font = FontSemibold;
            button.HoverState.FillColor = PrimaryHover;
            button.Cursor = System.Windows.Forms.Cursors.Hand;
        }

        public static void StyleSecondaryButton(Guna2Button button)
        {
            button.FillColor = Card;
            button.ForeColor = TextPrimary;
            button.BorderColor = Border;
            button.BorderThickness = 1;
            button.BorderRadius = 8;
            button.Font = FontSemibold;
            button.HoverState.FillColor = SidebarHover;
            button.Cursor = System.Windows.Forms.Cursors.Hand;
        }

        public static void StyleTextBox(Guna2TextBox textBox)
        {
            textBox.FillColor = Background;
            textBox.ForeColor = TextPrimary;
            textBox.BorderColor = Border;
            textBox.FocusedState.BorderColor = Primary;
        }

        public static void StyleDangerButton(Guna2Button button)
        {
            button.FillColor = Danger;
            button.ForeColor = Color.White;
            button.BorderRadius = 8;
            button.Font = FontSemibold;
            button.HoverState.FillColor = Color.FromArgb(185, 28, 28);
            button.Cursor = System.Windows.Forms.Cursors.Hand;
        }

        public static (Color fg, Color bg) SeverityColors(Models.DiagnosticSeverity severity)
        {
            switch (severity)
            {
                case Models.DiagnosticSeverity.Critical:
                    return (Danger, DangerBg);
                case Models.DiagnosticSeverity.Warning:
                    return (Warning, WarningBg);
                default:
                    return (Info, InfoBg);
            }
        }

        public static string SeverityLabel(Models.DiagnosticSeverity severity)
        {
            switch (severity)
            {
                case Models.DiagnosticSeverity.Critical:
                    return Core.Localization.T("severity.critical");
                case Models.DiagnosticSeverity.Warning:
                    return Core.Localization.T("severity.warning");
                default:
                    return Core.Localization.T("severity.info");
            }
        }
    }
}
