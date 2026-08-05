using System;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using frm_pin_remover.Core;
using frm_pin_remover.Models;
using frm_pin_remover.Services;
using frm_pin_remover.UI.Controls;

namespace frm_pin_remover.UI.Panels
{
    internal sealed class DiagnosticsPanel : Panel
    {
        public event Action ScanStarted;
        public event Action<DiagnosticReport> ReportReady;

        private readonly Guna2Button _rescanButton;
        private readonly Guna2Panel _rootCauseCard;
        private readonly Guna2HtmlLabel _rootCauseDetail;
        private readonly Label _rootCauseTitle;
        private readonly Panel _scrollArea;
        private DiagnosticReport _report;

        public DiagnosticsPanel()
        {
            BackColor = Theme.Background;

            var toolbar = new Panel { Dock = DockStyle.Top, Height = 40 };
            _rescanButton = new Guna2Button
            {
                Text = Localization.T("diagnostics.rescanButton"),
                Dock = DockStyle.Right,
                Width = 130,
                Height = 36
            };
            Theme.StyleSecondaryButton(_rescanButton);
            _rescanButton.Click += (s, e) => RunScanAsync();
            toolbar.Controls.Add(_rescanButton);

            _rootCauseCard = new Guna2Panel { Dock = DockStyle.Fill };
            Theme.StyleCard(_rootCauseCard);

            _rootCauseTitle = new Label
            {
                Text = Localization.T("diagnostics.rootCauseLabel"),
                Dock = DockStyle.Top,
                Height = 30,
                Padding = new Padding(20, 12, 0, 0),
                AutoSize = false,
                Font = Theme.FontHeading,
                ForeColor = Theme.TextMuted
            };
            // Guna2HtmlLabel clips text under its own left/right Padding instead of insetting it,
            // so the Padding must live on a plain wrapping Panel instead (see BuildFindingCard too).
            _rootCauseDetail = new Guna2HtmlLabel
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Font = Theme.FontTitle,
                ForeColor = Theme.TextPrimary
            };
            var rootCauseDetailWrap = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 4, 20, 16), BackColor = Color.Transparent };
            rootCauseDetailWrap.Controls.Add(_rootCauseDetail);

            _rootCauseCard.Controls.Add(rootCauseDetailWrap);
            _rootCauseCard.Controls.Add(_rootCauseTitle);

            var topWrap = new Panel { Dock = DockStyle.Top, Height = 90 };
            topWrap.Controls.Add(_rootCauseCard);

            _scrollArea = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Theme.Background
            };

            var toolbarSpacer = new Panel { Dock = DockStyle.Top, Height = 16, BackColor = Theme.Background };

            Controls.Add(_scrollArea);
            Controls.Add(topWrap);
            Controls.Add(toolbarSpacer);
            Controls.Add(toolbar);

            VisibleChanged += (s, e) => { if (Visible && _report != null) RenderCards(); };
            _scrollArea.Resize += (s, e) => { if (Visible && _report != null) RenderCards(); };

            Localization.LanguageChanged += () =>
            {
                _rescanButton.Text = Localization.T("diagnostics.rescanButton");
                _rootCauseTitle.Text = Localization.T("diagnostics.rootCauseLabel");
            };
        }

        public async void RunScanAsync()
        {
            _rescanButton.Enabled = false;
            ScanStarted?.Invoke();

            DiagnosticReport report;
            try
            {
                report = await DiagnosticsEngine.RunAsync();
            }
            finally
            {
                _rescanButton.Enabled = true;
            }

            _report = report;
            _rootCauseDetail.Text = report.RootCauseSummary;
            RenderCards();
            ReportReady?.Invoke(report);
        }

        private void RenderCards()
        {
            _scrollArea.SuspendLayout();
            _scrollArea.Controls.Clear();

            const int interCardGap = 16;
            // Cards are positioned manually (Top/Left), so Panel.Padding on _scrollArea would have no
            // effect on them — the gap before the first card has to be baked into the starting offset.
            const int gapBeforeFirstCard = 32;

            int width = Math.Max(300, _scrollArea.ClientSize.Width - 20);
            int top = gapBeforeFirstCard;
            foreach (var finding in _report.AllFindings)
            {
                var card = BuildFindingCard(finding, width);
                card.Top = top;
                card.Left = 0;
                _scrollArea.Controls.Add(card);
                top += card.Height + interCardGap;
            }

            _scrollArea.ResumeLayout();
        }

        private Guna2Panel BuildFindingCard(DiagnosticFinding finding, int width)
        {
            const int sourceHeight = 26;
            const int titleRowHeight = 34;
            const int detailVerticalPadding = 6 + 12;
            const int cardBottomMargin = 12;

            int detailWidth = Math.Max(100, width - 40);
            Size measured = TextRenderer.MeasureText(finding.Detail, Theme.FontRegular, new Size(detailWidth, int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.Left);
            int detailHeight = Math.Max(40, measured.Height + detailVerticalPadding);

            var card = new Guna2Panel { Width = width, Height = sourceHeight + titleRowHeight + detailHeight + cardBottomMargin };
            Theme.StyleCard(card);

            if (finding.Severity != DiagnosticSeverity.Info)
            {
                var (fg, _) = Theme.SeverityColors(finding.Severity);
                card.BorderColor = fg;
                card.BorderThickness = 1;
            }

            var sourceLabel = new Label
            {
                Text = finding.Source,
                Dock = DockStyle.Top,
                Height = 26,
                Padding = new Padding(20, 10, 0, 0),
                Font = Theme.FontRegular,
                ForeColor = Theme.TextMuted
            };

            var titleRow = new Panel { Dock = DockStyle.Top, Height = 34, Padding = new Padding(20, 4, 20, 0) };
            var badge = new SeverityBadge { Dock = DockStyle.Left };
            badge.SetSeverity(finding.Severity);
            var titleLabel = new Label
            {
                Text = finding.Title,
                Dock = DockStyle.Fill,
                Padding = new Padding(10, 3, 0, 0),
                Font = Theme.FontSemibold,
                ForeColor = Theme.TextPrimary
            };
            titleRow.Controls.Add(titleLabel);
            titleRow.Controls.Add(badge);

            var detailLabel = new Guna2HtmlLabel
            {
                Text = finding.Detail,
                AutoSize = false,
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Font = Theme.FontRegular,
                ForeColor = Theme.TextMuted
            };
            var detailWrap = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 6, 20, 12), BackColor = Color.Transparent };
            detailWrap.Controls.Add(detailLabel);

            card.Controls.Add(detailWrap);
            card.Controls.Add(titleRow);
            card.Controls.Add(sourceLabel);

            return card;
        }
    }
}
