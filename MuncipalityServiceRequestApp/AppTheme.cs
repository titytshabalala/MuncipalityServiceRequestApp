using System;
using System.Drawing;
using System.Windows.Forms;

namespace MuncipalityServiceRequestApp
{
    // Colour palette blends the City of Tshwane's gold/black identity with the
    // black/white/green tricolour from the municipal flag. No official logo or
    // crest graphic is used - branding is expressed through colour and text only.
    public static class AppTheme
    {
        // ---- Colour palette ----
        public static readonly Color Black = ColorTranslator.FromHtml("#1A1A1A");
        public static readonly Color Gold = ColorTranslator.FromHtml("#F2A900");
        public static readonly Color GoldHover = ColorTranslator.FromHtml("#FFC94D");
        public static readonly Color GoldPressed = ColorTranslator.FromHtml("#D99A00");
        public static readonly Color Green = ColorTranslator.FromHtml("#2E7D32");
        public static readonly Color GreenHoverTint = ColorTranslator.FromHtml("#EAF5EC");
        public static readonly Color White = Color.White;
        public static readonly Color LightBackground = ColorTranslator.FromHtml("#F5F5F0");
        public static readonly Color ErrorRed = ColorTranslator.FromHtml("#C62828");

        public const int HeaderHeight = 96; // 90 header + 6 stripe

        // ---- Typography scale ----
        public static readonly Font FontDisplay = new Font("Segoe UI", 16F, FontStyle.Bold);
        public static readonly Font FontSubheading = new Font("Segoe UI", 10.5F, FontStyle.Bold);
        public static readonly Font FontBody = new Font("Segoe UI", 9.5F, FontStyle.Regular);
        public static readonly Font FontLabel = new Font("Segoe UI", 9F, FontStyle.Bold);
        public static readonly Font FontCaption = new Font("Segoe UI", 8.5F, FontStyle.Regular);
        public static readonly Font FontCaptionItalic = new Font("Segoe UI", 8.5F, FontStyle.Italic);

        // ---- Spacing scale (8px grid) ----
        public const int SpaceXs = 4;
        public const int SpaceSm = 8;
        public const int SpaceMd = 16;
        public const int SpaceLg = 24;

        /// <summary>
        /// Common window set-up: DPI-aware scaling, centred, themed, and never larger than
        /// the screen's working area so small laptop screens still show the whole form.
        /// </summary>
        public static void InitForm(Form form, string title, int width, int height, int minWidth, int minHeight)
        {
            form.AutoScaleDimensions = new SizeF(96F, 96F);
            form.AutoScaleMode = AutoScaleMode.Dpi;
            form.Text = title;
            form.Font = FontBody;
            form.BackColor = LightBackground;
            form.StartPosition = FormStartPosition.CenterScreen;

            Rectangle area = Screen.PrimaryScreen.WorkingArea;
            form.MinimumSize = new Size(Math.Min(minWidth, area.Width), Math.Min(minHeight, area.Height));
            form.Size = new Size(Math.Min(width, area.Width - 20), Math.Min(height, area.Height - 20));
        }

        // ---- Header panel (used at the top of every form) ----
        public static Panel CreateHeaderPanel(string title, string subtitle)
        {
            Panel headerPanel = new Panel { Dock = DockStyle.Top, Height = HeaderHeight, BackColor = Black };

            Label titleLabel = new Label
            {
                Text = title,
                Font = FontDisplay,
                ForeColor = Gold,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Top,
                Height = 45,
                Padding = new Padding(SpaceLg, 12, 0, 0)
            };

            Label subtitleLabel = new Label
            {
                Text = subtitle,
                Font = FontCaptionItalic,
                ForeColor = White,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Top,
                Height = 25,
                Padding = new Padding(SpaceLg, 0, 0, 0)
            };

            Panel stripePanel = new Panel { Dock = DockStyle.Bottom, Height = 6 };
            TableLayoutPanel stripeLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, Margin = Padding.Empty };
            stripeLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            stripeLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));
            stripeLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            stripeLayout.Controls.Add(new Panel { Dock = DockStyle.Fill, BackColor = Black, Margin = Padding.Empty }, 0, 0);
            stripeLayout.Controls.Add(new Panel { Dock = DockStyle.Fill, BackColor = White, Margin = Padding.Empty }, 1, 0);
            stripeLayout.Controls.Add(new Panel { Dock = DockStyle.Fill, BackColor = Green, Margin = Padding.Empty }, 2, 0);
            stripePanel.Controls.Add(stripeLayout);

            headerPanel.Controls.Add(subtitleLabel);
            headerPanel.Controls.Add(titleLabel);
            headerPanel.Controls.Add(stripePanel);
            return headerPanel;
        }

        // ---- Buttons, with native hover/pressed states via FlatAppearance ----
        public static void StylePrimaryButton(Button button)
        {
            button.BackColor = Gold;
            button.ForeColor = Black;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = Black;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.MouseOverBackColor = GoldHover;
            button.FlatAppearance.MouseDownBackColor = GoldPressed;
            button.Font = FontSubheading;
            button.Cursor = Cursors.Hand;
        }

        public static void StyleSecondaryButton(Button button)
        {
            button.BackColor = White;
            button.ForeColor = Black;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = Green;
            button.FlatAppearance.BorderSize = 2;
            button.FlatAppearance.MouseOverBackColor = GreenHoverTint;
            button.FlatAppearance.MouseDownBackColor = ColorTranslator.FromHtml("#D5EAD8");
            button.Font = FontSubheading;
            button.Cursor = Cursors.Hand;
        }

        public static void StyleDisabledButton(Button button)
        {
            button.BackColor = Color.Gainsboro;
            button.ForeColor = Color.DimGray;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = Color.Silver;
            button.FlatAppearance.BorderSize = 1;
            button.Font = FontSubheading;
            button.Cursor = Cursors.Default;
        }

        /// <summary>Enables/disables a primary button and swaps its look to match.</summary>
        public static void SetPrimaryEnabled(Button button, bool enabled)
        {
            button.Enabled = enabled;
            if (enabled) StylePrimaryButton(button); else StyleDisabledButton(button);
        }

        // ---- Grids ----
        public static void StyleGrid(DataGridView grid)
        {
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;
            grid.RowHeadersVisible = false;
            grid.MultiSelect = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            grid.BackgroundColor = White;
            grid.BorderStyle = BorderStyle.FixedSingle;
            grid.GridColor = Color.Gainsboro;
            grid.Font = FontBody;

            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.ColumnHeadersHeight = 30;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Black;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Gold;
            grid.ColumnHeadersDefaultCellStyle.Font = FontLabel;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Black;

            grid.DefaultCellStyle.BackColor = White;
            grid.DefaultCellStyle.ForeColor = Black;
            grid.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            grid.RowTemplate.MinimumHeight = 28; // breathing room without cell padding
            grid.DefaultCellStyle.SelectionBackColor = GoldHover;
            grid.DefaultCellStyle.SelectionForeColor = Black;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 245);
        }

        public static DataGridViewTextBoxColumn GridColumn(string name, string header, float fillWeight, string format = null, bool sortable = false)
        {
            DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn
            {
                Name = name,
                HeaderText = header,
                FillWeight = fillWeight,
                MinimumWidth = 60,
                SortMode = sortable ? DataGridViewColumnSortMode.Automatic : DataGridViewColumnSortMode.NotSortable
            };
            if (format != null) column.DefaultCellStyle.Format = format;
            return column;
        }

        public static Color PriorityColor(EventPriority priority)
        {
            switch (priority)
            {
                case EventPriority.Urgent: return ColorTranslator.FromHtml("#C62828");
                case EventPriority.High: return ColorTranslator.FromHtml("#E65100");
                case EventPriority.Normal: return Green;
                default: return Color.Gray;
            }
        }
    }
}