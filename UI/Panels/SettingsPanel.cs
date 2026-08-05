using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using frm_pin_remover.Core;
using frm_pin_remover.Services;

namespace frm_pin_remover.UI.Panels
{
    internal sealed class SettingsPanel : Panel
    {
        private readonly Label _languageLabel;
        private readonly Guna2CheckBox _autostartCheckbox;
        private readonly Label _autostartHint;

        public SettingsPanel()
        {
            BackColor = Theme.Background;

            var card = new Guna2Panel { Width = 480, Height = 110, Location = new Point(0, 0) };
            Theme.StyleCard(card);

            _autostartCheckbox = new Guna2CheckBox
            {
                Text = Localization.T("settings.autostart"),
                Location = new Point(20, 20),
                AutoSize = true,
                ForeColor = Theme.TextPrimary,
                CheckedState = { FillColor = Theme.Primary, BorderColor = Theme.Primary },
                UncheckedState = { BorderColor = Theme.Border },
                Checked = AutoStartService.IsEnabled()
            };
            _autostartCheckbox.CheckedChanged += (s, e) => AutoStartService.SetEnabled(_autostartCheckbox.Checked);
            card.Controls.Add(_autostartCheckbox);

            _autostartHint = new Label
            {
                Text = Localization.T("settings.autostartHint"),
                Location = new Point(20, 55),
                AutoSize = true,
                Font = Theme.FontRegular,
                ForeColor = Theme.TextMuted,
                MaximumSize = new Size(420, 0)
            };
            card.Controls.Add(_autostartHint);

            Controls.Add(card);

            var languageCard = new Guna2Panel { Width = 480, Height = 80, Location = new Point(0, 130) };
            Theme.StyleCard(languageCard);

            _languageLabel = new Label
            {
                Text = Localization.T("settings.languageLabel"),
                Location = new Point(20, 24),
                AutoSize = true,
                Font = Theme.FontSemibold,
                ForeColor = Theme.TextPrimary
            };
            languageCard.Controls.Add(_languageLabel);

            var languageCombo = new Guna2ComboBox
            {
                Location = new Point(150, 20),
                Width = 220,
                Height = 36,
                DropDownStyle = ComboBoxStyle.DropDownList,
                FillColor = Theme.Background,
                ForeColor = Theme.TextPrimary,
                BorderColor = Theme.Border
            };
            // Language names are always shown natively (not translated), so users can find their own
            // language regardless of whichever language is currently active.
            languageCombo.Items.Add("English");
            languageCombo.Items.Add("Tiếng Việt");
            languageCombo.SelectedIndex = Localization.Current == AppLanguage.Vietnamese ? 1 : 0;
            languageCombo.SelectedIndexChanged += (s, e) =>
            {
                Localization.SetLanguage(languageCombo.SelectedIndex == 1 ? AppLanguage.Vietnamese : AppLanguage.English);
            };
            languageCard.Controls.Add(languageCombo);

            Controls.Add(languageCard);

            Localization.LanguageChanged += Retranslate;
        }

        private void Retranslate()
        {
            _autostartCheckbox.Text = Localization.T("settings.autostart");
            _autostartHint.Text = Localization.T("settings.autostartHint");
            _languageLabel.Text = Localization.T("settings.languageLabel");
        }
    }
}
