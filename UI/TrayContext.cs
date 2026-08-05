using System;
using System.Windows.Forms;
using frm_pin_remover.Core;

namespace frm_pin_remover.UI
{
    internal sealed class TrayContext : IDisposable
    {
        private readonly NotifyIcon _notifyIcon;
        private readonly ToolStripMenuItem _openItem;
        private readonly ToolStripMenuItem _rescanItem;
        private readonly ToolStripMenuItem _exitItem;

        public event Action OpenRequested;
        public event Action RescanRequested;
        public event Action ExitRequested;

        public TrayContext()
        {
            _openItem = new ToolStripMenuItem(Localization.T("tray.open"));
            _openItem.Click += (s, e) => OpenRequested?.Invoke();

            _rescanItem = new ToolStripMenuItem(Localization.T("tray.rescan"));
            _rescanItem.Click += (s, e) => RescanRequested?.Invoke();

            _exitItem = new ToolStripMenuItem(Localization.T("tray.exit"));
            _exitItem.Click += (s, e) => ExitRequested?.Invoke();

            var menu = new ContextMenuStrip();
            menu.Items.Add(_openItem);
            menu.Items.Add(_rescanItem);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(_exitItem);

            _notifyIcon = new NotifyIcon
            {
                Icon = AppIcon.Current,
                Text = AppConstants.MainWindowTitle,
                Visible = true,
                ContextMenuStrip = menu
            };

            _notifyIcon.DoubleClick += (s, e) => OpenRequested?.Invoke();

            Localization.LanguageChanged += Retranslate;
        }

        private void Retranslate()
        {
            _openItem.Text = Localization.T("tray.open");
            _rescanItem.Text = Localization.T("tray.rescan");
            _exitItem.Text = Localization.T("tray.exit");
        }

        public void ShowBalloon(string title, string text, ToolTipIcon icon = ToolTipIcon.Info)
        {
            _notifyIcon.BalloonTipTitle = title;
            _notifyIcon.BalloonTipText = text;
            _notifyIcon.BalloonTipIcon = icon;
            _notifyIcon.ShowBalloonTip(4000);
        }

        public void Dispose()
        {
            Localization.LanguageChanged -= Retranslate;
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
        }
    }
}
