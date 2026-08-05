using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using frm_pin_remover.Core;

namespace frm_pin_remover.UI.Panels
{
    internal sealed class FeedbackPanel : Panel
    {
        private const string FeedbackFormUrl =
            "https://docs.google.com/forms/d/e/1FAIpQLSeo-lrn9p7d1iRsUXW1JCWRhnNCeTpYmA9DSRbewbb5iHnbmA/viewform?usp=dialog";

        private readonly Label _title;
        private readonly Label _description;
        private readonly Guna2Button _openFormButton;

        public FeedbackPanel()
        {
            BackColor = Theme.Background;

            var card = new Guna2Panel { Width = 560, Height = 206, Location = new Point(0, 0) };
            Theme.StyleCard(card);

            _title = new Label
            {
                Text = Localization.T("feedback.title"),
                Location = new Point(24, 20),
                AutoSize = true,
                Font = Theme.FontHeading,
                ForeColor = Theme.TextPrimary
            };
            card.Controls.Add(_title);

            _description = new Label
            {
                Text = Localization.T("feedback.description"),
                Location = new Point(24, 52),
                AutoSize = true,
                MaximumSize = new Size(512, 0),
                Font = Theme.FontRegular,
                ForeColor = Theme.TextMuted
            };
            card.Controls.Add(_description);

            _openFormButton = new Guna2Button
            {
                Text = Localization.T("feedback.openButton"),
                Location = new Point(24, 100),
                Width = 200,
                Height = 40
            };
            Theme.StylePrimaryButton(_openFormButton);
            _openFormButton.Click += (s, e) => OpenUrl(FeedbackFormUrl);
            card.Controls.Add(_openFormButton);

            var linkBox = new Guna2TextBox
            {
                Text = FeedbackFormUrl,
                ReadOnly = true,
                Location = new Point(24, 150),
                Width = 512,
                Height = 32
            };
            Theme.StyleTextBox(linkBox);
            card.Controls.Add(linkBox);

            Controls.Add(card);

            Localization.LanguageChanged += Retranslate;
        }

        private void Retranslate()
        {
            _title.Text = Localization.T("feedback.title");
            _description.Text = Localization.T("feedback.description");
            _openFormButton.Text = Localization.T("feedback.openButton");
        }

        private static void OpenUrl(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show(null, Localization.TF("donate.linkError", ex.Message), Localization.T("donate.linkErrorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
