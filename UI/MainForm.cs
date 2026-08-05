using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using frm_pin_remover.Core;
using frm_pin_remover.Models;
using frm_pin_remover.UI.Panels;

namespace frm_pin_remover.UI
{
    internal sealed class MainForm : Form
    {
        private const int ResizeMargin = 6;

        private readonly bool _startMinimized;
        private bool _visibleOnce;
        private bool _allowExit;

        private readonly TrayContext _tray;
        private readonly Guna2ProgressBar _headerProgress;
        private readonly Label _headerTitle;

        private Panel _titleBar;
        private Panel _titleBarButtonsHost;
        private Guna2Button _maximizeButton;

        private readonly DiagnosticsPanel _diagnosticsPanel;
        private readonly ActionsPanel _actionsPanel;
        private readonly LogPanel _logPanel;
        private readonly SettingsPanel _settingsPanel;
        private readonly FaqPanel _faqPanel;
        private readonly FeedbackPanel _feedbackPanel;
        private readonly DonatePanel _donatePanel;

        private readonly List<(Guna2Button Button, Control Panel, string Icon, string LabelKey)> _navItems =
            new List<(Guna2Button, Control, string, string)>();

        private Label _sidebarTagline;
        private int _selectedNavIndex;

        public MainForm(bool startMinimized)
        {
            _startMinimized = startMinimized;

            Text = AppConstants.MainWindowTitle;
            FormBorderStyle = FormBorderStyle.None;
            ClientSize = new Size(1040, 660);
            MinimumSize = new Size(900, 580);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Theme.Border;
            Padding = new Padding(1);
            Icon = AppIcon.Current;
            Font = Theme.FontRegular;

            var root = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Background };
            Controls.Add(root);

            // A Dock=Fill sibling only shrinks around Top/Left/Right/Bottom siblings that are added
            // to the Controls collection AFTER it — added before, it ends up spanning the full parent
            // and everything inside it renders underneath the sidebar/title bar. So contentHost (Fill)
            // must be added to root.Controls first, then the title bar and sidebar.
            var contentHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Theme.Background,
                Padding = new Padding(24)
            };
            root.Controls.Add(contentHost);

            _titleBar = BuildTitleBar();
            root.Controls.Add(_titleBar);

            var sidebar = BuildSidebar();
            root.Controls.Add(sidebar);

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 48,
                BackColor = Theme.Background
            };

            _headerTitle = new Label
            {
                Dock = DockStyle.Left,
                AutoSize = false,
                Width = 400,
                Font = Theme.FontTitle,
                ForeColor = Theme.TextPrimary,
                TextAlign = ContentAlignment.MiddleLeft
            };
            header.Controls.Add(_headerTitle);

            _headerProgress = new Guna2ProgressBar
            {
                Dock = DockStyle.Right,
                Width = 220,
                Height = 6,
                Style = ProgressBarStyle.Marquee,
                ProgressColor = Theme.Primary,
                Visible = false
            };
            header.Controls.Add(_headerProgress);

            var body = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Theme.Background
            };

            contentHost.Controls.Add(body);
            contentHost.Controls.Add(header);

            _diagnosticsPanel = new DiagnosticsPanel();
            _actionsPanel = new ActionsPanel();
            _logPanel = new LogPanel();
            _settingsPanel = new SettingsPanel();
            _faqPanel = new FaqPanel();
            _feedbackPanel = new FeedbackPanel();
            _donatePanel = new DonatePanel();

            foreach (var panel in new Control[]
                     {
                         _diagnosticsPanel, _actionsPanel, _logPanel, _settingsPanel,
                         _faqPanel, _feedbackPanel, _donatePanel
                     })
            {
                panel.Dock = DockStyle.Fill;
                panel.Visible = false;
                body.Controls.Add(panel);
            }

            _diagnosticsPanel.ScanStarted += () => _headerProgress.Visible = true;
            _diagnosticsPanel.ReportReady += OnReportReady;

            AddNavButton(sidebar, "🔎", "nav.diagnostics", _diagnosticsPanel, 0);
            AddNavButton(sidebar, "🛠", "nav.recommendations", _actionsPanel, 1);
            AddNavButton(sidebar, "📋", "nav.log", _logPanel, 2);
            AddNavButton(sidebar, "⚙", "nav.settings", _settingsPanel, 3);
            AddNavButton(sidebar, "❓", "nav.faq", _faqPanel, 4);
            AddNavButton(sidebar, "💬", "nav.feedback", _feedbackPanel, 5);
            AddNavButton(sidebar, "❤", "nav.donate", _donatePanel, 6);

            SelectNav(0);

            _tray = new TrayContext();
            _tray.OpenRequested += RestoreFromTray;
            _tray.RescanRequested += () => _diagnosticsPanel.RunScanAsync();
            _tray.ExitRequested += () =>
            {
                _allowExit = true;
                // Application.Exit() alone still left the process running in the background in testing.
                // Hide/dispose the tray icon synchronously, then force the process to actually terminate.
                _tray.Dispose();
                Environment.Exit(0);
            };

            Load += (s, e) => _diagnosticsPanel.RunScanAsync();
            FormClosed += (s, e) => _tray.Dispose();

            Localization.LanguageChanged += OnLanguageChanged;
        }

        private void OnLanguageChanged()
        {
            _sidebarTagline.Text = Localization.T("sidebar.tagline");
            foreach (var item in _navItems)
            {
                item.Button.Text = "   " + item.Icon + "  " + Localization.T(item.LabelKey);
            }
            _headerTitle.Text = Localization.T(_navItems[_selectedNavIndex].LabelKey);

            // Diagnostic findings and recommended actions are built by the Services layer at scan
            // time, so the only way to get already-shown results into the new language is to scan again.
            _diagnosticsPanel.RunScanAsync();
        }

        private Panel BuildTitleBar()
        {
            var titleBar = new Panel { Dock = DockStyle.Top, Height = 36, BackColor = Theme.TitleBar };

            var icon = new PictureBox
            {
                Dock = DockStyle.Left,
                Width = 40,
                Image = AppIcon.Current.ToBitmap(),
                SizeMode = PictureBoxSizeMode.CenterImage
            };

            var titleLabel = new Label
            {
                Text = AppConstants.MainWindowTitle,
                Dock = DockStyle.Fill,
                Padding = new Padding(6, 0, 0, 0),
                ForeColor = Theme.TextPrimary,
                Font = Theme.FontSemibold,
                TextAlign = ContentAlignment.MiddleLeft
            };

            _titleBarButtonsHost = new Panel { Dock = DockStyle.Right, Width = 138, BackColor = Theme.TitleBar };

            var closeButton = BuildTitleBarButton("×", 16f);
            closeButton.HoverState.FillColor = Theme.Danger;
            closeButton.Click += (s, e) => Close();

            _maximizeButton = BuildTitleBarButton("□", 11f);
            _maximizeButton.Click += (s, e) => ToggleMaximize();

            var minimizeButton = BuildTitleBarButton("−", 13f);
            minimizeButton.Click += (s, e) => WindowState = FormWindowState.Minimized;

            _titleBarButtonsHost.Controls.Add(closeButton);
            _titleBarButtonsHost.Controls.Add(_maximizeButton);
            _titleBarButtonsHost.Controls.Add(minimizeButton);

            titleBar.Controls.Add(titleLabel);
            titleBar.Controls.Add(icon);
            titleBar.Controls.Add(_titleBarButtonsHost);

            titleBar.DoubleClick += (s, e) => ToggleMaximize();

            return titleBar;
        }

        private Guna2Button BuildTitleBarButton(string glyph, float fontSize)
        {
            var button = new Guna2Button
            {
                Text = glyph,
                Dock = DockStyle.Left,
                Width = 46,
                FillColor = Theme.TitleBar,
                ForeColor = Theme.TextPrimary,
                BorderRadius = 0,
                Font = new Font("Segoe UI", fontSize),
                Cursor = Cursors.Hand
            };
            button.HoverState.FillColor = Theme.SidebarHover;
            return button;
        }

        private void ToggleMaximize()
        {
            WindowState = WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == NativeMethods.WM_GETMINMAXINFO)
            {
                ApplyMaximizedWorkingArea(ref m);
            }

            base.WndProc(ref m);

            if (m.Msg == NativeMethods.WM_NCHITTEST && (int)m.Result == NativeMethods.HTCLIENT)
            {
                HandleNonClientHitTest(ref m);
            }
        }

        private void ApplyMaximizedWorkingArea(ref Message m)
        {
            var mmi = (MINMAXINFO)Marshal.PtrToStructure(m.LParam, typeof(MINMAXINFO));

            Screen screen = Screen.FromHandle(Handle);
            Rectangle workingArea = screen.WorkingArea;
            Rectangle bounds = screen.Bounds;

            mmi.ptMaxPosition.X = workingArea.Left - bounds.Left;
            mmi.ptMaxPosition.Y = workingArea.Top - bounds.Top;
            mmi.ptMaxSize.X = workingArea.Width;
            mmi.ptMaxSize.Y = workingArea.Height;

            Marshal.StructureToPtr(mmi, m.LParam, true);
        }

        private void HandleNonClientHitTest(ref Message m)
        {
            Point screenPoint = new Point(unchecked((short)(long)m.LParam), unchecked((short)((long)m.LParam >> 16)));
            Point clientPoint = PointToClient(screenPoint);

            if (WindowState == FormWindowState.Normal)
            {
                bool onLeft = clientPoint.X <= ResizeMargin;
                bool onRight = clientPoint.X >= ClientSize.Width - ResizeMargin;
                bool onTop = clientPoint.Y <= ResizeMargin;
                bool onBottom = clientPoint.Y >= ClientSize.Height - ResizeMargin;

                if (onTop && onLeft) { m.Result = (IntPtr)NativeMethods.HTTOPLEFT; return; }
                if (onTop && onRight) { m.Result = (IntPtr)NativeMethods.HTTOPRIGHT; return; }
                if (onBottom && onLeft) { m.Result = (IntPtr)NativeMethods.HTBOTTOMLEFT; return; }
                if (onBottom && onRight) { m.Result = (IntPtr)NativeMethods.HTBOTTOMRIGHT; return; }
                if (onLeft) { m.Result = (IntPtr)NativeMethods.HTLEFT; return; }
                if (onRight) { m.Result = (IntPtr)NativeMethods.HTRIGHT; return; }
                if (onTop) { m.Result = (IntPtr)NativeMethods.HTTOP; return; }
                if (onBottom) { m.Result = (IntPtr)NativeMethods.HTBOTTOM; return; }
            }

            if (_titleBar == null) return;

            Point titleBarPoint = _titleBar.PointToClient(screenPoint);
            bool insideTitleBar = _titleBar.ClientRectangle.Contains(titleBarPoint);
            bool insideButtons = _titleBarButtonsHost.Bounds.Contains(titleBarPoint);

            if (insideTitleBar && !insideButtons)
            {
                m.Result = (IntPtr)NativeMethods.HTCAPTION;
            }
        }

        private Guna2Panel BuildSidebar()
        {
            var sidebar = new Guna2Panel
            {
                Dock = DockStyle.Left,
                Width = 240,
                FillColor = Theme.Sidebar,
                BackColor = Theme.Sidebar,
                BorderRadius = 0
            };

            // The title bar already shows the tool's name, so the sidebar header shows what it *does*
            // instead of repeating the name.
            _sidebarTagline = new Label
            {
                Text = Localization.T("sidebar.tagline"),
                Dock = DockStyle.Top,
                Height = 80,
                Font = Theme.FontTitle,
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0)
            };
            sidebar.Controls.Add(_sidebarTagline);
            _sidebarTagline.BringToFront();

            return sidebar;
        }

        private void AddNavButton(Guna2Panel sidebar, string icon, string labelKey, Control targetPanel, int index)
        {
            var button = new Guna2Button
            {
                Text = "   " + icon + "  " + Localization.T(labelKey),
                Dock = DockStyle.Top,
                Height = 48,
                TextAlign = HorizontalAlignment.Left,
                Font = Theme.FontSemibold,
                FillColor = Theme.Sidebar,
                ForeColor = Color.White,
                BorderRadius = 0,
                Cursor = Cursors.Hand
            };
            button.HoverState.FillColor = Theme.SidebarHover;

            int capturedIndex = index;
            button.Click += (s, e) => SelectNav(capturedIndex);

            sidebar.Controls.Add(button);
            button.BringToFront();

            _navItems.Add((button, targetPanel, icon, labelKey));
        }

        private void SelectNav(int index)
        {
            _selectedNavIndex = index;
            for (int i = 0; i < _navItems.Count; i++)
            {
                var item = _navItems[i];
                bool selected = i == index;
                item.Panel.Visible = selected;
                item.Button.FillColor = selected ? Theme.Primary : Theme.Sidebar;
                if (selected)
                {
                    item.Panel.BringToFront();
                    _headerTitle.Text = Localization.T(item.LabelKey);
                }
            }
        }

        private void OnReportReady(DiagnosticReport report)
        {
            _headerProgress.Visible = false;
            _actionsPanel.SetReport(report);

            if (report.BitLockerProtectionOn)
            {
                _tray.ShowBalloon(Localization.T("mainform.bitlockerWarningTitle"), Localization.T("mainform.bitlockerWarningText"), ToolTipIcon.Warning);
            }
            else
            {
                _tray.ShowBalloon(AppConstants.MainWindowTitle, Localization.T("mainform.scanCompletePrefix") + report.RootCauseSummary);
            }
        }

        private void RestoreFromTray()
        {
            Show();
            WindowState = FormWindowState.Normal;
            BringToFront();
            Activate();
        }

        protected override void SetVisibleCore(bool value)
        {
            if (!_visibleOnce && _startMinimized)
            {
                value = false;
                if (!IsHandleCreated)
                {
                    CreateHandle();
                }
            }

            _visibleOnce = true;
            base.SetVisibleCore(value);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!_allowExit && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
                _tray.ShowBalloon(AppConstants.MainWindowTitle, Localization.T("mainform.stillRunningTray"));
                return;
            }

            base.OnFormClosing(e);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (_maximizeButton != null)
            {
                _maximizeButton.Text = WindowState == FormWindowState.Maximized ? "❐" : "□";
            }

            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
            }
        }
    }
}
