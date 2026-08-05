using System;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using frm_pin_remover.Core;
using frm_pin_remover.Models;

namespace frm_pin_remover.UI.Panels
{
    internal sealed class LogPanel : Panel
    {
        private readonly Guna2DataGridView _grid;

        public LogPanel()
        {
            BackColor = Theme.Background;
            Padding = new Padding(0, 8, 0, 0);

            _grid = new Guna2DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Theme.Card,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                EnableHeadersVisualStyles = false,
                GridColor = Theme.Border,
                Font = Theme.FontRegular
            };

            _grid.ColumnHeadersDefaultCellStyle.BackColor = Theme.Sidebar;
            _grid.ColumnHeadersDefaultCellStyle.ForeColor = Theme.TextPrimary;
            _grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Theme.Sidebar;
            _grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = Theme.TextPrimary;
            _grid.ColumnHeadersDefaultCellStyle.Font = Theme.FontSemibold;
            _grid.ColumnHeadersHeight = 36;

            _grid.DefaultCellStyle.BackColor = Theme.Card;
            _grid.DefaultCellStyle.ForeColor = Theme.TextPrimary;
            _grid.DefaultCellStyle.SelectionBackColor = Theme.Primary;
            _grid.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.White;
            _grid.DefaultCellStyle.Padding = new Padding(4);

            _grid.AlternatingRowsDefaultCellStyle.BackColor = Theme.Background;
            _grid.AlternatingRowsDefaultCellStyle.ForeColor = Theme.TextPrimary;
            _grid.AlternatingRowsDefaultCellStyle.SelectionBackColor = Theme.Primary;
            _grid.AlternatingRowsDefaultCellStyle.SelectionForeColor = System.Drawing.Color.White;

            _grid.Columns.Add("Time", Localization.T("log.colTime"));
            _grid.Columns.Add("Action", Localization.T("log.colAction"));
            _grid.Columns.Add("Detail", Localization.T("log.colDetail"));
            _grid.Columns.Add("Result", Localization.T("log.colResult"));
            _grid.Columns["Detail"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            Controls.Add(_grid);

            foreach (var entry in AppLogger.AllEntries)
            {
                AddRow(entry);
            }

            AppLogger.EntryAdded += OnEntryAdded;
            Localization.LanguageChanged += RetranslateColumns;
        }

        private void RetranslateColumns()
        {
            _grid.Columns["Time"].HeaderText = Localization.T("log.colTime");
            _grid.Columns["Action"].HeaderText = Localization.T("log.colAction");
            _grid.Columns["Detail"].HeaderText = Localization.T("log.colDetail");
            _grid.Columns["Result"].HeaderText = Localization.T("log.colResult");
        }

        private void OnEntryAdded(ActionLogEntry entry)
        {
            if (IsDisposed) return;

            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => AddRow(entry)));
            }
            else
            {
                AddRow(entry);
            }
        }

        private void AddRow(ActionLogEntry entry)
        {
            _grid.Rows.Add(entry.TimestampUtc.ToLocalTime().ToString("HH:mm:ss dd/MM"), entry.Action, entry.Detail, entry.Result);
        }
    }
}
