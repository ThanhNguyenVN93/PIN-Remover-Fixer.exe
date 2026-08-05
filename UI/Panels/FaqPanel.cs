using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using frm_pin_remover.Core;

namespace frm_pin_remover.UI.Panels
{
    internal sealed class FaqPanel : Panel
    {
        private static readonly (string QuestionKey, string AnswerKey)[] EntryKeys =
        {
            ("faq.q1", "faq.a1"),
            ("faq.q2", "faq.a2"),
            ("faq.q3", "faq.a3"),
            ("faq.q4", "faq.a4"),
            ("faq.q5", "faq.a5"),
            ("faq.q6", "faq.a6"),
        };

        private readonly Panel _scrollArea;
        private int _width = 900;

        public FaqPanel()
        {
            BackColor = Theme.Background;

            _scrollArea = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Theme.Background };
            Controls.Add(_scrollArea);

            Relayout();

            VisibleChanged += (s, e) => { if (Visible) RelayoutIfWidthChanged(); };
            _scrollArea.Resize += (s, e) => { if (Visible) RelayoutIfWidthChanged(); };
            Localization.LanguageChanged += Relayout;
        }

        private void RelayoutIfWidthChanged()
        {
            int newWidth = System.Math.Max(400, _scrollArea.ClientSize.Width - 20);
            if (newWidth == _width) return;
            _width = newWidth;
            Relayout();
        }

        private void Relayout()
        {
            _scrollArea.SuspendLayout();
            _scrollArea.Controls.Clear();
            int top = 0;
            foreach (var entry in EntryKeys)
            {
                var card = BuildCard(Localization.T(entry.QuestionKey), Localization.T(entry.AnswerKey), _width);
                card.Top = top;
                card.Left = 0;
                _scrollArea.Controls.Add(card);
                top += card.Height + 16;
            }
            _scrollArea.ResumeLayout();
        }

        private static Guna2Panel BuildCard(string question, string answer, int width)
        {
            int answerWidth = width - 40;
            Size measured = TextRenderer.MeasureText(answer, Theme.FontRegular, new Size(answerWidth, int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.Left);

            var card = new Guna2Panel { Width = width, Height = 34 + measured.Height + 18 + 12 };
            Theme.StyleCard(card);

            var questionLabel = new Label
            {
                Text = question,
                Dock = DockStyle.Top,
                Height = 34,
                Padding = new Padding(20, 8, 20, 0),
                Font = Theme.FontSemibold,
                ForeColor = Theme.TextPrimary
            };

            var answerLabel = new Guna2HtmlLabel
            {
                Text = answer,
                AutoSize = false,
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Font = Theme.FontRegular,
                ForeColor = Theme.TextMuted
            };
            var answerWrap = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 4, 20, 12), BackColor = Color.Transparent };
            answerWrap.Controls.Add(answerLabel);

            card.Controls.Add(answerWrap);
            card.Controls.Add(questionLabel);

            return card;
        }
    }
}
