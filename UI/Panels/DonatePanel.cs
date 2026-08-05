using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using frm_pin_remover.Core;

namespace frm_pin_remover.UI.Panels
{
    internal sealed class DonatePanel : Panel
    {
        private const string KofiUrl = "https://ko-fi.com/thanhnguyen150993";
        private const int CardWidth = 640;
        private const int DescriptionLeft = 96;

        private readonly Guna2Panel _thankYouCard;
        private readonly Guna2Panel _infoPanel;
        private readonly Label _title;
        private readonly Label _description;
        private readonly Label _momoLabel;
        private readonly Label _tcbLabel;
        private readonly Label _kofiHint;
        private readonly Guna2Button _kofiButton;

        public DonatePanel()
        {
            BackColor = Theme.Background;

            var scrollArea = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Theme.Background };
            Controls.Add(scrollArea);

            _thankYouCard = new Guna2Panel { Width = CardWidth, Location = new Point(0, 0) };
            Theme.StyleCard(_thankYouCard);

            // AutoSize under-measures the ink extent of large color-emoji glyphs like a heart, so the
            // glyph can visually bleed past its own Bounds into whatever sits right next to it. Give it
            // a fixed, generously-sized box instead of AutoSize, and leave extra clearance before the
            // next label.
            var heart = new Label
            {
                Text = "❤",
                Location = new Point(20, 18),
                AutoSize = false,
                Size = new Size(44, 44),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 17f),
                ForeColor = Theme.Danger
            };
            _thankYouCard.Controls.Add(heart);

            _title = new Label
            {
                Text = Localization.T("donate.title"),
                Location = new Point(DescriptionLeft, 20),
                AutoSize = true,
                Font = Theme.FontHeading,
                ForeColor = Theme.TextPrimary
            };
            _thankYouCard.Controls.Add(_title);

            _description = new Label
            {
                Text = Localization.T("donate.description"),
                Location = new Point(DescriptionLeft, 48),
                AutoSize = true,
                MaximumSize = new Size(CardWidth - DescriptionLeft - 24, 0),
                Font = Theme.FontRegular,
                ForeColor = Theme.TextMuted
            };
            _thankYouCard.Controls.Add(_description);

            scrollArea.Controls.Add(_thankYouCard);

            _infoPanel = new Guna2Panel { Width = CardWidth, Height = 400 };
            Theme.StyleCard(_infoPanel);

            _momoLabel = new Label
            {
                Text = Localization.T("donate.momoLabel"),
                Location = new Point(30, 20),
                AutoSize = true,
                Font = Theme.FontSemibold,
                ForeColor = Theme.TextPrimary
            };
            _infoPanel.Controls.Add(_momoLabel);

            var momoBox = new PictureBox
            {
                Image = EmbeddedResources.LoadImage("momo.jpg"),
                SizeMode = PictureBoxSizeMode.Zoom,
                Location = new Point(30, 50),
                Size = new Size(250, 270),
                BackColor = Color.White
            };
            _infoPanel.Controls.Add(momoBox);

            _tcbLabel = new Label
            {
                Text = Localization.T("donate.techcombankLabel"),
                Location = new Point(320, 20),
                AutoSize = true,
                Font = Theme.FontSemibold,
                ForeColor = Theme.TextPrimary
            };
            _infoPanel.Controls.Add(_tcbLabel);

            var tcbBox = new PictureBox
            {
                Image = EmbeddedResources.LoadImage("tcb.jpg"),
                SizeMode = PictureBoxSizeMode.Zoom,
                Location = new Point(320, 50),
                Size = new Size(250, 270),
                BackColor = Color.White
            };
            _infoPanel.Controls.Add(tcbBox);

            _kofiHint = new Label
            {
                Text = Localization.T("donate.kofiHint"),
                Location = new Point(30, 335),
                AutoSize = true,
                Font = Theme.FontRegular,
                ForeColor = Theme.TextMuted
            };
            _infoPanel.Controls.Add(_kofiHint);

            _kofiButton = new Guna2Button { Text = Localization.T("donate.kofiButton"), Location = new Point(30, 358), Width = 170, Height = 36 };
            Theme.StylePrimaryButton(_kofiButton);
            _kofiButton.Click += (s, e) => OpenUrl(KofiUrl);
            _infoPanel.Controls.Add(_kofiButton);

            var kofiLink = new Guna2TextBox
            {
                Text = KofiUrl,
                ReadOnly = true,
                Location = new Point(210, 358),
                Width = 360,
                Height = 36
            };
            Theme.StyleTextBox(kofiLink);
            _infoPanel.Controls.Add(kofiLink);

            scrollArea.Controls.Add(_infoPanel);

            ResizeThankYouCard();
            Localization.LanguageChanged += Retranslate;
        }

        // The thank-you card's height is driven by its actual text content (which varies by language),
        // so the info card right below it always sits snugly instead of leaving a big empty gap.
        private void ResizeThankYouCard()
        {
            int descriptionBottom = _description.Location.Y + _description.Height;
            int heartBottom = 18 + 44;
            int cardHeight = Math.Max(descriptionBottom, heartBottom) + 20;

            _thankYouCard.Height = cardHeight;
            _infoPanel.Location = new Point(0, cardHeight + 16);
        }

        private void Retranslate()
        {
            _title.Text = Localization.T("donate.title");
            _description.Text = Localization.T("donate.description");
            _momoLabel.Text = Localization.T("donate.momoLabel");
            _tcbLabel.Text = Localization.T("donate.techcombankLabel");
            _kofiHint.Text = Localization.T("donate.kofiHint");
            _kofiButton.Text = Localization.T("donate.kofiButton");

            ResizeThankYouCard();
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
