using System.Drawing;
using System.Windows.Forms;

namespace MuncipalityServiceRequestApp
{
    // Colour palette blends the City of Tshwane's gold/black identity with the
    // black/white/green tricolour from the municipal flag. No official logo or
    // crest graphic is used here — the City has publicly restricted reuse of
    // its actual logo/crest, so branding is expressed through colour and text only.
    public static class AppTheme
    {
        public static readonly Color Black = ColorTranslator.FromHtml("#1A1A1A");
        public static readonly Color Gold = ColorTranslator.FromHtml("#F2A900");
        public static readonly Color Green = ColorTranslator.FromHtml("#2E7D32");
        public static readonly Color White = Color.White;
        public static readonly Color LightBackground = ColorTranslator.FromHtml("#F5F5F0");

        public const int HeaderHeight = 96; // 90 header + 6 stripe

        public static Panel CreateHeaderPanel(string title, string subtitle)
        {
            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = HeaderHeight,
                BackColor = Black
            };

            Label titleLabel = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Gold,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Top,
                Height = 45,
                Padding = new Padding(20, 12, 0, 0)
            };

            Label subtitleLabel = new Label
            {
                Text = subtitle,
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = White,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Top,
                Height = 25,
                Padding = new Padding(20, 0, 0, 0)
            };

            // Thin flag-inspired stripe: black / white / green
            Panel stripePanel = new Panel { Dock = DockStyle.Bottom, Height = 6 };
            TableLayoutPanel stripeLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1 };
            stripeLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            stripeLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));
            stripeLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            stripeLayout.Controls.Add(new Panel { Dock = DockStyle.Fill, BackColor = Black }, 0, 0);
            stripeLayout.Controls.Add(new Panel { Dock = DockStyle.Fill, BackColor = White }, 1, 0);
            stripeLayout.Controls.Add(new Panel { Dock = DockStyle.Fill, BackColor = Green }, 2, 0);
            stripePanel.Controls.Add(stripeLayout);

            headerPanel.Controls.Add(subtitleLabel);
            headerPanel.Controls.Add(titleLabel);
            headerPanel.Controls.Add(stripePanel);

            return headerPanel;
        }

        public static void StylePrimaryButton(Button button)
        {
            button.BackColor = Gold;
            button.ForeColor = Black;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = Black;
            button.FlatAppearance.BorderSize = 1;
            button.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            button.Cursor = Cursors.Hand;
        }

        public static void StyleSecondaryButton(Button button)
        {
            button.BackColor = White;
            button.ForeColor = Black;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = Green;
            button.FlatAppearance.BorderSize = 2;
            button.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            button.Cursor = Cursors.Hand;
        }

        public static void StyleDisabledButton(Button button)
        {
            button.BackColor = Color.Gainsboro;
            button.ForeColor = Color.DimGray;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = Color.Silver;
            button.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
        }
    }
}