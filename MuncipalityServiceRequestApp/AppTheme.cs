using System;
using System.Drawing;
using System.Windows.Forms;

namespace MuncipalityServiceRequestApp
{
    // Colour palette blends the City of Tshwane's gold/black identity with the
    // black/white/green tricolour from the municipal flag. No official logo or
    // crest graphic is used — the City has publicly restricted reuse of its
    // actual logo/crest, so branding is expressed through colour and text only.
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
        public static readonly Color CardBorder = ColorTranslator.FromHtml("#E0E0DA");
        public static readonly Color CardHoverTint = ColorTranslator.FromHtml("#FAF7EB");

        public const int HeaderHeight = 96; // 90 header + 6 stripe

        // ---- Typography scale ----
        public static readonly Font FontDisplay = new Font("Segoe UI", 16F, FontStyle.Bold);
        public static readonly Font FontHeading = new Font("Segoe UI", 13F, FontStyle.Bold);
        public static readonly Font FontSubheading = new Font("Segoe UI", 10.5F, FontStyle.Bold);
        public static readonly Font FontBody = new Font("Segoe UI", 9.5F, FontStyle.Regular);
        public static readonly Font FontCaption = new Font("Segoe UI", 8.5F, FontStyle.Regular);
        public static readonly Font FontCaptionItalic = new Font("Segoe UI", 8.5F, FontStyle.Italic);

        public static Font IconFont(float size) => new Font("Segoe MDL2 Assets", size, FontStyle.Regular);

        // ---- Spacing scale (8px grid) ----
        public const int SpaceXs = 4;
        public const int SpaceSm = 8;
        public const int SpaceMd = 16;
        public const int SpaceLg = 24;
        public const int SpaceXl = 32;

        // ---- Icon glyphs (Segoe MDL2 Assets — ships with Windows 10 and 11) ----
        public static class Icons
        {
            public const string Home = "\uE80F";
            public const string ReportFlag = "\uE7C1";
            public const string Megaphone = "\uE789";
            public const string Calendar = "\uE787";
            public const string Warning = "\uE7BA";
            public const string Search = "\uE721";
            public const string Filter = "\uE71C";
            public const string MapPin = "\uE707";
            public const string Attach = "\uE723";
            public const string Accept = "\uE10B";
            public const string Back = "\uE72B";
            public const string Refresh = "\uE72C";
            public const string Cancel = "\uE711";
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
            button.Font = FontSubheading;
        }

        // ---- Menu card: icon + title + subtitle, hoverable/clickable as a unit ----
        public static Panel CreateMenuCard(string icon, string title, string subtitle, bool enabled, int width, EventHandler? onClick)
        {
            Color bgColor = enabled ? White : ColorTranslator.FromHtml("#F0F0F0");
            Color accentColor = enabled ? Gold : Color.Gainsboro;
            Color titleColor = enabled ? Black : Color.Gray;
            Color subtitleColor = enabled ? Color.DimGray : Color.LightGray;

            Panel card = new Panel
            {
                Width = width,
                Height = 76,
                BackColor = bgColor,
                Cursor = enabled ? Cursors.Hand : Cursors.Default,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblIcon = new Label
            {
                Text = icon,
                Font = IconFont(22F),
                ForeColor = accentColor,
                AutoSize = false,
                Left = SpaceMd,
                Top = 14,
                Width = 46,
                Height = 46,
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label lblTitle = new Label
            {
                Text = title,
                Font = FontSubheading,
                ForeColor = titleColor,
                AutoSize = false,
                Left = 74,
                Top = 14,
                Width = width - 90,
                Height = 22,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            Label lblSubtitle = new Label
            {
                Text = subtitle,
                Font = FontCaption,
                ForeColor = subtitleColor,
                AutoSize = false,
                Left = 74,
                Top = 38,
                Width = width - 90,
                Height = 30,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            card.Controls.Add(lblIcon);
            card.Controls.Add(lblTitle);
            card.Controls.Add(lblSubtitle);

            if (enabled && onClick != null)
            {
                Control[] hitTargets = { card, lblIcon, lblTitle, lblSubtitle };
                foreach (Control control in hitTargets)
                {
                    control.Click += onClick;
                    control.MouseEnter += (s, e) => card.BackColor = CardHoverTint;
                    control.MouseLeave += (s, e) => card.BackColor = bgColor;
                }
            }

            return card;
        }

        // ---- Section card: groups related form fields with padding + a border ----
        public static Panel CreateSectionCard(int width, int height)
        {
            return new Panel
            {
                Width = width,
                Height = height,
                BackColor = White,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(SpaceMd)
            };
        }

        // ---- Small pill/badge, e.g. for priority or category tags ----
        public static Label CreateBadge(string text, Color backgroundColor, Color textColor)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                BackColor = backgroundColor,
                ForeColor = textColor,
                Font = FontCaption,
                Padding = new Padding(8, 3, 8, 3),
                TextAlign = ContentAlignment.MiddleCenter
            };
        }

        // A label stacked above its input control — the standard, DPI-safe pattern
        // for form fields (avoids guessing a fixed width for the caption text).
        public static Panel CreateLabeledField(string labelText, Control input, int width)
        {
            Panel wrapper = new Panel { Width = width, Height = 50, Margin = new Padding(0, 0, SpaceMd, 0) };
            Label label = new Label
            {
                Text = labelText,
                Font = FontCaption,
                ForeColor = Color.DimGray,
                AutoSize = false,
                Left = 0,
                Top = 0,
                Width = width,
                Height = 16
            };
            input.Left = 0;
            input.Top = 20;
            input.Width = width;
            wrapper.Controls.Add(label);
            wrapper.Controls.Add(input);
            return wrapper;
        }

        // An icon glyph beside a short piece of text. Labels can only render one font
        // per control, so an icon (Segoe MDL2 Assets) and its caption (Segoe UI) have
        // to be two controls — this pairs them and lays them out automatically.
        public static FlowLayoutPanel CreateIconTextPair(string icon, string text, Color iconColor, Color textColor, Font textFont)
        {
            FlowLayoutPanel pair = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Margin = new Padding(0, 0, SpaceMd, 0)
            };
            Label iconLabel = new Label { Text = icon, Font = IconFont(11F), ForeColor = iconColor, AutoSize = true, Margin = new Padding(0, 1, 4, 0) };
            Label textLabel = new Label { Text = text, Font = textFont, ForeColor = textColor, AutoSize = true, Margin = new Padding(0, 2, 0, 0) };
            pair.Controls.Add(iconLabel);
            pair.Controls.Add(textLabel);
            return pair;
        }

        public static Color PriorityColor(EventPriority priority) => priority switch
        {
            EventPriority.Urgent => ColorTranslator.FromHtml("#C62828"),
            EventPriority.High => ColorTranslator.FromHtml("#E65100"),
            EventPriority.Normal => Green,
            EventPriority.Low => Color.Gray,
            _ => Color.Gray
        };
    }
}