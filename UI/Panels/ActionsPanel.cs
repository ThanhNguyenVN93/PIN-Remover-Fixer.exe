using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using frm_pin_remover.Core;
using frm_pin_remover.Models;
using frm_pin_remover.Services;

namespace frm_pin_remover.UI.Panels
{
    internal sealed class ActionsPanel : Panel
    {
        private readonly Panel _scrollArea;
        private DiagnosticReport _report;

        public ActionsPanel()
        {
            BackColor = Theme.Background;
            _scrollArea = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Theme.Background };
            Controls.Add(_scrollArea);

            RenderEmptyState();

            // The panel starts hidden (user must click the nav tab), so its ClientSize can still be
            // stale/zero the first time SetReport() runs. Re-render with a fresh width whenever it
            // actually becomes visible or the window is resized while it's the active tab.
            VisibleChanged += (s, e) => { if (Visible) Render(); };
            _scrollArea.Resize += (s, e) => { if (Visible) Render(); };
            Localization.LanguageChanged += () => Render();
        }

        public void SetReport(DiagnosticReport report)
        {
            _report = report;
            Render();
        }

        private void RenderEmptyState()
        {
            _scrollArea.Controls.Clear();
            var label = new Label
            {
                Text = Localization.T("actions.emptyState"),
                AutoSize = true,
                Location = new Point(4, 4),
                ForeColor = Theme.TextMuted,
                Font = Theme.FontRegular
            };
            _scrollArea.Controls.Add(label);
        }

        private void Render()
        {
            _scrollArea.SuspendLayout();
            _scrollArea.Controls.Clear();

            if (_report == null)
            {
                RenderEmptyState();
                _scrollArea.ResumeLayout();
                return;
            }

            int width = Math.Max(360, _scrollArea.ClientSize.Width - 4);
            int top = 0;

            foreach (var action in _report.RecommendedActions)
            {
                var card = BuildActionCard(action, width);
                card.Top = top;
                card.Left = 0;
                _scrollArea.Controls.Add(card);
                top += card.Height + 16;
            }

            _scrollArea.ResumeLayout();
        }

        private Guna2Panel BuildActionCard(RecommendedAction action, int width)
        {
            bool blockedByBitLocker = action.RequiresBitLockerOff && _report.BitLockerProtectionOn;

            var card = new Guna2Panel { Width = width, Height = 10 };
            Theme.StyleCard(card);

            var applyButton = new Guna2Button { Text = Localization.T("actions.apply"), Width = 110, Height = 36, Margin = new Padding(8, 0, 0, 0) };
            Theme.StylePrimaryButton(applyButton);

            Guna2Button undoButton = null;
            if (action.Kind == ActionKind.FixRegistryPolicy)
            {
                undoButton = new Guna2Button
                {
                    Text = Localization.T("actions.undo"),
                    Width = 100,
                    Height = 36,
                    Margin = new Padding(8, 0, 0, 0),
                    Enabled = RegistryFixService.HasBackup()
                };
                Theme.StyleSecondaryButton(undoButton);
                applyButton.Click += async (s, e) => await ApplyRegistryFixAsync(applyButton);
                undoButton.Click += async (s, e) => await RollbackRegistryAsync(undoButton);
            }
            else if (action.Kind == ActionKind.DeleteNgcFolder)
            {
                if (blockedByBitLocker)
                {
                    applyButton.Enabled = false;
                }
                else
                {
                    applyButton.Click += async (s, e) => await ApplyNgcFixAsync(applyButton);
                }
            }

            int buttonsWidth = 20 + applyButton.Width + applyButton.Margin.Left + (undoButton != null ? undoButton.Width + undoButton.Margin.Left : 0);

            var buttonsFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                Width = buttonsWidth,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                AutoSize = false,
                Padding = new Padding(0, 14, 20, 0)
            };
            buttonsFlow.Controls.Add(applyButton);
            if (undoButton != null)
            {
                buttonsFlow.Controls.Add(undoButton);
            }

            var titleLabel = new Label
            {
                Text = action.Title,
                Dock = DockStyle.Fill,
                Padding = new Padding(20, 0, 0, 0),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = Theme.FontHeading,
                ForeColor = Theme.TextPrimary
            };

            var headerRow = new Panel { Dock = DockStyle.Top, Height = 64 };
            headerRow.Controls.Add(titleLabel);
            headerRow.Controls.Add(buttonsFlow);

            // Guna2HtmlLabel clips text under its own left/right Padding instead of insetting it,
            // so the Padding must live on the wrapping Panel instead.
            var descriptionLabel = new Guna2HtmlLabel
            {
                Text = action.Description,
                AutoSize = false,
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Font = Theme.FontRegular,
                ForeColor = Theme.TextMuted
            };
            var descriptionRow = new Panel { Dock = DockStyle.Top, Height = 46, Padding = new Padding(20, 0, 20, 0) };
            descriptionRow.Controls.Add(descriptionLabel);

            int cardHeight = headerRow.Height + descriptionRow.Height + 12;

            card.Controls.Add(descriptionRow);

            if (action.Kind == ActionKind.DeleteNgcFolder && blockedByBitLocker)
            {
                var warningBlock = BuildBitLockerWarningBlock(width);
                var warningRow = new Panel { Dock = DockStyle.Top, Height = warningBlock.Height + 16 };
                warningBlock.Location = new Point(20, 0);
                warningRow.Controls.Add(warningBlock);
                card.Controls.Add(warningRow);
                cardHeight += warningRow.Height;
            }

            card.Controls.Add(headerRow);
            card.Height = cardHeight;

            return card;
        }

        private Guna2Panel BuildBitLockerWarningBlock(int cardWidth)
        {
            int panelWidth = cardWidth - 40;

            var panel = new Guna2Panel
            {
                Width = panelWidth,
                Height = 152,
                FillColor = Theme.WarningBg,
                BackColor = Theme.WarningBg,
                BorderColor = Theme.Warning,
                BorderThickness = 1,
                BorderRadius = 8
            };

            var message = new Label
            {
                Text = Localization.T("actions.bitlockerBlockedMessage"),
                Location = new Point(16, 12),
                AutoSize = true,
                BackColor = Color.Transparent,
                Font = Theme.FontSemibold,
                ForeColor = Theme.Warning,
                MaximumSize = new Size(panelWidth - 32, 0)
            };
            panel.Controls.Add(message);

            var commandBox = new Guna2TextBox
            {
                Text = BitLockerActionService.BuildSuspendCommandText(),
                ReadOnly = true,
                Location = new Point(16, 42),
                Width = panelWidth - 166,
                Height = 36
            };
            Theme.StyleTextBox(commandBox);
            panel.Controls.Add(commandBox);

            var copyButton = new Guna2Button { Text = Localization.T("actions.copyCommand"), Location = new Point(panelWidth - 140, 41), Width = 124, Height = 36 };
            Theme.StyleSecondaryButton(copyButton);
            copyButton.Click += (s, e) =>
            {
                Clipboard.SetText(commandBox.Text);
                MessageBox.Show(this, Localization.T("actions.copiedMessage"), Localization.T("actions.copiedTitle"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            panel.Controls.Add(copyButton);

            var hint = new Label
            {
                Text = Localization.T("actions.suspendHint"),
                Location = new Point(16, 88),
                AutoSize = true,
                BackColor = Color.Transparent,
                Font = Theme.FontRegular,
                ForeColor = Theme.TextMuted,
                MaximumSize = new Size(panelWidth - 32, 0)
            };
            panel.Controls.Add(hint);

            var resumeButton = new Guna2Button { Text = Localization.T("actions.resumeButton"), Location = new Point(16, 112), Width = 200, Height = 32 };
            Theme.StyleSecondaryButton(resumeButton);
            resumeButton.Click += async (s, e) => await ResumeBitLockerAsync(resumeButton);
            panel.Controls.Add(resumeButton);

            return panel;
        }

        private async Task ApplyRegistryFixAsync(Guna2Button button)
        {
            var confirm = MessageBox.Show(this,
                Localization.T("actions.confirmApplyRegistry"),
                Localization.T("actions.confirmTitle"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            button.Enabled = false;
            var result = await Task.Run(() => RegistryFixService.Apply());
            button.Enabled = true;

            MessageBox.Show(this, result.Message, result.Success ? Localization.T("common.success") : Localization.T("common.failed"),
                MessageBoxButtons.OK, result.Success ? MessageBoxIcon.Information : MessageBoxIcon.Error);

            Render();
        }

        private async Task RollbackRegistryAsync(Guna2Button button)
        {
            var confirm = MessageBox.Show(this,
                Localization.T("actions.confirmRollback"),
                Localization.T("actions.confirmTitle"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            button.Enabled = false;
            var result = await Task.Run(() => RegistryFixService.Rollback());
            button.Enabled = true;

            MessageBox.Show(this, result.Message, result.Success ? Localization.T("common.success") : Localization.T("common.failed"),
                MessageBoxButtons.OK, result.Success ? MessageBoxIcon.Information : MessageBoxIcon.Error);

            Render();
        }

        private async Task ApplyNgcFixAsync(Guna2Button button)
        {
            var confirm = MessageBox.Show(this,
                Localization.T("actions.confirmApplyNgc"),
                Localization.T("actions.confirmTitle"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return;

            button.Enabled = false;
            var result = await Task.Run(() => NgcFolderFixService.Apply());
            button.Enabled = true;

            MessageBox.Show(this, result.Message, result.Success ? Localization.T("common.success") : Localization.T("common.failed"),
                MessageBoxButtons.OK, result.Success ? MessageBoxIcon.Information : MessageBoxIcon.Error);
        }

        private async Task ResumeBitLockerAsync(Guna2Button button)
        {
            var confirm = MessageBox.Show(this,
                Localization.T("actions.confirmResumeBitlocker"),
                Localization.T("actions.confirmTitle"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            button.Enabled = false;
            var result = await Task.Run(() => BitLockerActionService.ResumeNow());
            button.Enabled = true;

            MessageBox.Show(this, result.Message, result.Success ? Localization.T("common.success") : Localization.T("common.failed"),
                MessageBoxButtons.OK, result.Success ? MessageBoxIcon.Information : MessageBoxIcon.Error);
        }
    }
}
