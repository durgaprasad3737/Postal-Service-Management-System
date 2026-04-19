// FindUsView.cs
// PostalMS - Postal Service Management System
// CST2550 Coursework - Middlesex University
//
// Find Us page showing PostalMS locations near the user.
// Detects user city from their profile and shows local stores.
// Each city has at least 10 PostalMS locations.

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace PostalServiceWinForms.Forms
{
    public class FindUsView : UserControl
    {
        private Color Red = Color.FromArgb(180, 30, 30);
        private Color Dark = Color.FromArgb(30, 30, 30);
        private Color Grey = Color.FromArgb(90, 90, 90);
        private Color LightBg = Color.FromArgb(245, 245, 245);

        private DatabaseHelper db;
        private string userID;

        // All PostalMS locations grouped by city
        private Dictionary<string, List<(string name, string address, string phone, string hours, string travel)>> allLocations
            = new Dictionary<string, List<(string, string, string, string, string)>>(StringComparer.OrdinalIgnoreCase)
            {
                ["London"] = new List<(string, string, string, string, string)>
            {
                ("PostalMS Hendon Centre",       "The Burroughs, Hendon, London NW4 4BT",           "020 8411 5000", "Mon-Fri 9am-5:30pm, Sat 9am-1pm", "Tube: Hendon Central (Northern Line)"),
                ("PostalMS Cat Hill Centre",      "Cat Hill, East Barnet, London EN4 8HT",            "020 8411 6000", "Mon-Fri 9am-5:30pm, Sat 9am-1pm", "Bus: 84, 299, 384"),
                ("PostalMS Archway Point",        "2 Junction Road, Archway, London N19 5QU",         "020 7272 0000", "Mon-Fri 9am-6pm, Sat 10am-2pm",   "Tube: Archway (Northern Line)"),
                ("PostalMS Wembley Centre",       "Engineers Way, Wembley, London HA9 0ED",           "020 8900 0000", "Mon-Fri 9am-5:30pm, Sat 9am-1pm", "Tube: Wembley Park (Metropolitan/Jubilee)"),
                ("PostalMS Brixton Hub",          "472 Brixton Road, London SW9 8EQ",                 "020 7926 0000", "Mon-Fri 8am-6pm, Sat 9am-2pm",    "Tube: Brixton (Victoria Line)"),
                ("PostalMS Canary Wharf",         "1 Canada Square, London E14 5AB",                  "020 7418 0000", "Mon-Fri 8am-7pm, Sat 10am-3pm",   "Tube: Canary Wharf (Jubilee Line)"),
                ("PostalMS Croydon Centre",       "12 North End, Croydon CR0 1TT",                    "020 8686 0000", "Mon-Fri 9am-6pm, Sat 9am-2pm",    "Tram: Church Street stop"),
                ("PostalMS Hackney Hub",          "234 Mare Street, Hackney, London E8 1HE",          "020 8985 0000", "Mon-Fri 9am-5:30pm, Sat 9am-1pm", "Bus: 106, 253, 254"),
                ("PostalMS Ealing Centre",        "58 New Broadway, Ealing, London W5 5AH",           "020 8567 0000", "Mon-Fri 9am-5:30pm, Sat 9am-1pm", "Tube: Ealing Broadway (District/Central)"),
                ("PostalMS Stratford Point",      "The Stratford Centre, London E15 1XQ",             "020 8534 0000", "Mon-Fri 9am-7pm, Sat 9am-3pm",    "Tube: Stratford (Central/Jubilee/DLR)"),
                ("PostalMS Islington Hub",        "321 Upper Street, Islington, London N1 2XQ",       "020 7226 0000", "Mon-Fri 9am-6pm, Sat 10am-2pm",   "Tube: Angel (Northern Line)"),
                ("PostalMS Greenwich Centre",     "6 College Approach, Greenwich, London SE10 9HY",   "020 8853 0000", "Mon-Fri 9am-5:30pm, Sat 9am-1pm", "DLR: Cutty Sark"),
            },
                ["Manchester"] = new List<(string, string, string, string, string)>
            {
                ("PostalMS Manchester City",      "12 Piccadilly, Manchester M1 1HP",                 "0161 200 0001", "Mon-Fri 8am-6pm, Sat 9am-2pm",    "Tram: Piccadilly Gardens"),
                ("PostalMS Salford Centre",       "45 Chapel Street, Salford M3 5JG",                 "0161 200 0002", "Mon-Fri 9am-5:30pm, Sat 9am-1pm", "Bus: 33, 36"),
                ("PostalMS Trafford Park",        "2 Europa Way, Trafford Park M17 1QT",              "0161 200 0003", "Mon-Fri 9am-6pm, Sat 9am-1pm",    "Tram: Trafford Park"),
                ("PostalMS Didsbury Hub",         "764 Wilmslow Road, Didsbury M20 2DR",              "0161 200 0004", "Mon-Fri 9am-5:30pm, Sat 9am-1pm", "Bus: 23, 41, 42"),
                ("PostalMS Stockport Centre",     "18 Merseyway, Stockport SK1 1PL",                  "0161 200 0005", "Mon-Fri 9am-6pm, Sat 9am-2pm",    "Train: Stockport station"),
                ("PostalMS Oldham Hub",           "7 Yorkshire Street, Oldham OL1 1QP",               "0161 200 0006", "Mon-Fri 9am-5:30pm, Sat 9am-1pm", "Tram: Oldham Central"),
                ("PostalMS Rochdale Centre",      "23 The Esplanade, Rochdale OL16 1AE",              "0161 200 0007", "Mon-Fri 9am-5:30pm, Sat 9am-1pm", "Tram: Rochdale Town Centre"),
                ("PostalMS Bury Hub",             "16 Millgate, Bury BL9 0BW",                        "0161 200 0008", "Mon-Fri 9am-5pm, Sat 9am-1pm",    "Tram: Bury interchange"),
                ("PostalMS Altrincham Centre",    "29 Stamford New Road, Altrincham WA14 1EJ",        "0161 200 0009", "Mon-Fri 9am-5:30pm, Sat 9am-1pm", "Tram: Altrincham"),
                ("PostalMS Ashton Hub",           "4 Old Street, Ashton-under-Lyne OL6 7EG",          "0161 200 0010", "Mon-Fri 9am-5pm, Sat 9am-1pm",    "Tram: Ashton-under-Lyne"),
                ("PostalMS Bolton Centre",        "8 Newport Street, Bolton BL1 1DE",                 "0161 200 0011", "Mon-Fri 9am-5:30pm, Sat 9am-1pm", "Bus: X41, 8"),
                ("PostalMS Wythenshawe Hub",      "2 Civic Centre Road, Wythenshawe M22 5RF",         "0161 200 0012", "Mon-Fri 9am-5:30pm, Sat 9am-1pm", "Bus: 43, 44"),
            },
                ["Birmingham"] = new List<(string, string, string, string, string)>
            {
                ("PostalMS Birmingham City",      "14 New Street, Birmingham B2 4DU",                  "0121 200 0001", "Mon-Fri 8am-6pm, Sat 9am-2pm",    "Tram: New Street"),
                ("PostalMS Broad Street Hub",     "88 Broad Street, Birmingham B15 1AU",               "0121 200 0002", "Mon-Fri 9am-6pm, Sat 9am-2pm",    "Bus: 9, 22, 23"),
                ("PostalMS Erdington Centre",     "94 High Street, Erdington B23 6SA",                 "0121 200 0003", "Mon-Fri 9am-5:30pm, Sat 9am-1pm", "Bus: 65, 111"),
                ("PostalMS Solihull Hub",         "12 Mell Square, Solihull B91 3AX",                  "0121 200 0004", "Mon-Fri 9am-5:30pm, Sat 9am-1pm", "Train: Solihull station"),
                ("PostalMS Handsworth Centre",    "34 Soho Road, Handsworth B21 9BH",                  "0121 200 0005", "Mon-Fri 9am-5:30pm, Sat 9am-1pm", "Bus: 74, 79"),
                ("PostalMS Wolverhampton Hub",    "9 Dudley Street, Wolverhampton WV1 3ER",            "0121 200 0006", "Mon-Fri 9am-5:30pm, Sat 9am-1pm", "Tram: Wolverhampton"),
                ("PostalMS Walsall Centre",       "22 Park Street, Walsall WS1 1LZ",                   "0121 200 0007", "Mon-Fri 9am-5:30pm, Sat 9am-1pm", "Train: Walsall station"),
                ("PostalMS Coventry Hub",         "18 Broadgate, Coventry CV1 1NG",                    "0121 200 0008", "Mon-Fri 9am-5:30pm, Sat 9am-1pm", "Train: Coventry station"),
                ("PostalMS Kings Heath Centre",   "106 High Street, Kings Heath B14 7JZ",              "0121 200 0009", "Mon-Fri 9am-5:30pm, Sat 9am-1pm", "Bus: 35, 50"),
                ("PostalMS Sutton Coldfield Hub", "11 The Parade, Sutton Coldfield B72 1PD",           "0121 200 0010", "Mon-Fri 9am-5:30pm, Sat 9am-1pm", "Train: Sutton Coldfield station"),
                ("PostalMS Smethwick Centre",     "242 High Street, Smethwick B66 3PL",                "0121 200 0011", "Mon-Fri 9am-5pm, Sat 9am-1pm",    "Tram: Rolfe Street"),
                ("PostalMS Northfield Hub",       "674 Bristol Road South, Northfield B31 2JS",        "0121 200 0012", "Mon-Fri 9am-5pm, Sat 9am-1pm",    "Bus: 45, 63"),
            },
                ["Leeds"] = new List<(string, string, string, string, string)>
            {
                ("PostalMS Leeds City Centre",    "24 Briggate, Leeds LS1 6HD",                        "0113 200 0001", "Mon-Fri 8am-6pm, Sat 9am-2pm",    "Train: Leeds station"),
                ("PostalMS Headingley Hub",       "83 Otley Road, Headingley LS6 3PS",                 "0113 200 0002", "Mon-Fri 9am-5:30pm, Sat 9am-1pm", "Bus: 28, 96, 97"),
                ("PostalMS Morley Centre",        "11 Queen Street, Morley LS27 9BU",                  "0113 200 0003", "Mon-Fri 9am-5:30pm, Sat 9am-1pm", "Train: Morley station"),
                ("PostalMS Roundhay Hub",         "342 Roundhay Road, Leeds LS8 2HU",                  "0113 200 0004", "Mon-Fri 9am-5:30pm, Sat 9am-1pm", "Bus: 2, 12"),
                ("PostalMS Crossgates Centre",    "4 Station Road, Crossgates LS15 8DU",               "0113 200 0005", "Mon-Fri 9am-5:30pm, Sat 9am-1pm", "Train: Cross Gates station"),
                ("PostalMS Armley Hub",           "56 Town Street, Armley LS12 3AP",                   "0113 200 0006", "Mon-Fri 9am-5pm, Sat 9am-1pm",    "Bus: 16, 17"),
                ("PostalMS Pudsey Centre",        "12 Church Lane, Pudsey LS28 7LG",                   "0113 200 0007", "Mon-Fri 9am-5pm, Sat 9am-1pm",    "Train: Pudsey station"),
                ("PostalMS Yeadon Hub",           "27 High Street, Yeadon LS19 7PP",                   "0113 200 0008", "Mon-Fri 9am-5pm, Sat 9am-1pm",    "Bus: 33, 34"),
                ("PostalMS Chapeltown Centre",    "184 Chapeltown Road, Leeds LS7 4HP",                "0113 200 0009", "Mon-Fri 9am-5:30pm, Sat 9am-1pm", "Bus: 2, 3"),
                ("PostalMS Horsforth Hub",        "8 Town Street, Horsforth LS18 4RJ",                 "0113 200 0010", "Mon-Fri 9am-5pm, Sat 9am-1pm",    "Train: Horsforth station"),
            },
                ["Bristol"] = new List<(string, string, string, string, string)>
            {
                ("PostalMS Bristol City",         "14 Broadmead, Bristol BS1 3HH",                     "0117 200 0001", "Mon-Fri 8am-6pm, Sat 9am-2pm",    "Bus: 1, 2, 8"),
                ("PostalMS Clifton Hub",          "22 Queen's Road, Clifton BS8 1QU",                   "0117 200 0002", "Mon-Fri 9am-5:30pm, Sat 9am-1pm", "Bus: 8, 9"),
                ("PostalMS Bedminster Centre",    "84 East Street, Bedminster BS3 4EX",                 "0117 200 0003", "Mon-Fri 9am-5:30pm, Sat 9am-1pm", "Bus: 24, 25"),
                ("PostalMS Fishponds Hub",        "320 Fishponds Road, Fishponds BS16 3EW",             "0117 200 0004", "Mon-Fri 9am-5pm, Sat 9am-1pm",    "Bus: 5, 48"),
                ("PostalMS Filton Centre",        "6 Northfield Road, Filton BS34 7LW",                 "0117 200 0005", "Mon-Fri 9am-5:30pm, Sat 9am-1pm", "Bus: 73, 73A"),
                ("PostalMS Horfield Hub",         "188 Gloucester Road, Horfield BS7 8NU",              "0117 200 0006", "Mon-Fri 9am-5:30pm, Sat 9am-1pm", "Bus: 75, 76"),
                ("PostalMS Knowle Centre",        "2 Wells Road, Knowle BS4 2QA",                       "0117 200 0007", "Mon-Fri 9am-5pm, Sat 9am-1pm",    "Bus: 35, 36"),
                ("PostalMS Southmead Hub",        "12 Doncaster Road, Southmead BS10 5PW",              "0117 200 0008", "Mon-Fri 9am-5pm, Sat 9am-1pm",    "Bus: 18, 19"),
                ("PostalMS Kingswood Centre",     "High Street, Kingswood BS15 4AA",                    "0117 200 0009", "Mon-Fri 9am-5:30pm, Sat 9am-1pm", "Bus: 43, 44"),
                ("PostalMS Hanham Hub",           "High Street, Hanham BS15 3DL",                       "0117 200 0010", "Mon-Fri 9am-5pm, Sat 9am-1pm",    "Bus: 19, 620"),
            },
                ["Edinburgh"] = new List<(string, string, string, string, string)>
            {
                ("PostalMS Edinburgh City",       "46 Princes Street, Edinburgh EH2 2BY",              "0131 200 0001", "Mon-Fri 8am-6pm, Sat 9am-2pm",    "Tram: Princes Street"),
                ("PostalMS Leith Centre",         "88 Leith Walk, Edinburgh EH6 5HB",                  "0131 200 0002", "Mon-Fri 9am-5:30pm, Sat 9am-1pm", "Bus: 7, 12, 14"),
                ("PostalMS Morningside Hub",      "184 Morningside Road, Edinburgh EH10 4QA",          "0131 200 0003", "Mon-Fri 9am-5:30pm, Sat 9am-1pm", "Bus: 11, 15, 16"),
                ("PostalMS Stockbridge Centre",   "36 Raeburn Place, Edinburgh EH4 1HN",               "0131 200 0004", "Mon-Fri 9am-5:30pm, Sat 9am-1pm", "Bus: 24, 29, 36"),
                ("PostalMS Portobello Hub",       "136 High Street, Portobello EH15 1AJ",              "0131 200 0005", "Mon-Fri 9am-5pm, Sat 9am-1pm",    "Bus: 26, 45"),
                ("PostalMS Corstorphine Centre",  "134 St John's Road, Corstorphine EH12 8AU",         "0131 200 0006", "Mon-Fri 9am-5:30pm, Sat 9am-1pm", "Bus: 12, 26"),
                ("PostalMS Gorgie Hub",           "208 Gorgie Road, Edinburgh EH11 2NU",               "0131 200 0007", "Mon-Fri 9am-5pm, Sat 9am-1pm",    "Bus: 3, 25"),
                ("PostalMS Craigmillar Centre",   "134 Duddingston Road, Edinburgh EH16 4AR",          "0131 200 0008", "Mon-Fri 9am-5pm, Sat 9am-1pm",    "Bus: 42, 44"),
                ("PostalMS Newington Hub",        "116 South Clerk Street, Edinburgh EH8 9PE",         "0131 200 0009", "Mon-Fri 9am-5:30pm, Sat 9am-1pm", "Bus: 2, 7, 8"),
                ("PostalMS Dalry Centre",         "86 Dalry Road, Edinburgh EH11 2AZ",                 "0131 200 0010", "Mon-Fri 9am-5pm, Sat 9am-1pm",    "Bus: 3, 25, 33"),
            },
                ["Glasgow"] = new List<(string, string, string, string, string)>
            {
                ("PostalMS Glasgow City",         "92 Buchanan Street, Glasgow G1 3HA",                "0141 200 0001", "Mon-Fri 8am-6pm, Sat 9am-2pm",    "Subway: Buchanan Street"),
                ("PostalMS Partick Hub",          "44 Dumbarton Road, Partick G11 6PA",                "0141 200 0002", "Mon-Fri 9am-5:30pm, Sat 9am-1pm", "Subway: Partick"),
                ("PostalMS Shawlands Centre",     "136 Kilmarnock Road, Shawlands G41 3NH",            "0141 200 0003", "Mon-Fri 9am-5:30pm, Sat 9am-1pm", "Train: Shawlands station"),
                ("PostalMS Govan Hub",            "12 Govan Road, Govan G51 1JS",                      "0141 200 0004", "Mon-Fri 9am-5pm, Sat 9am-1pm",    "Subway: Govan"),
                ("PostalMS East End Centre",      "18 Duke Street, Glasgow G4 0UL",                    "0141 200 0005", "Mon-Fri 9am-5:30pm, Sat 9am-1pm", "Bus: 40, 61"),
                ("PostalMS Maryhill Hub",         "398 Maryhill Road, Glasgow G20 7YH",                "0141 200 0006", "Mon-Fri 9am-5pm, Sat 9am-1pm",    "Bus: 6, 8"),
                ("PostalMS Rutherglen Centre",    "48 Main Street, Rutherglen G73 2HZ",                "0141 200 0007", "Mon-Fri 9am-5:30pm, Sat 9am-1pm", "Train: Rutherglen station"),
                ("PostalMS Pollokshields Hub",    "120 Albert Drive, Pollokshields G41 2PE",           "0141 200 0008", "Mon-Fri 9am-5pm, Sat 9am-1pm",    "Train: Maxwell Park station"),
                ("PostalMS Springburn Centre",    "44 Springburn Way, Glasgow G21 1TR",                "0141 200 0009", "Mon-Fri 9am-5pm, Sat 9am-1pm",    "Train: Springburn station"),
                ("PostalMS Parkhead Hub",         "22 Tollcross Road, Parkhead G31 4UG",               "0141 200 0010", "Mon-Fri 9am-5pm, Sat 9am-1pm",    "Bus: 61, 62"),
            },
            };

        public FindUsView(string uid = "", DatabaseHelper dbHelper = null)
        {
            userID = uid;
            db = dbHelper;
            this.Dock = DockStyle.Fill;
            this.BackColor = LightBg;
            this.AutoScroll = true;
            Build();
        }

        private void Build()
        {
            int y = 30;

            // Page header
            Panel header = new Panel { Location = new Point(0, y), Size = new Size(1400, 80), BackColor = Red };
            header.Paint += (s, e) =>
            {
                using (var b = new System.Drawing.Drawing2D.LinearGradientBrush(header.ClientRectangle,
                    Color.FromArgb(155, 18, 18), Color.FromArgb(210, 55, 35), 0f))
                    e.Graphics.FillRectangle(b, header.ClientRectangle);
            };
            header.Controls.Add(new Label { Text = "Find Us", Font = new Font("Segoe UI", 24, FontStyle.Bold), ForeColor = Color.White, Location = new Point(20, 18), Size = new Size(600, 42), BackColor = Color.Transparent });
            this.Controls.Add(header);
            y += 100;

            // Detect user city
            string userCity = GetUserCity();

            // City selector
            this.Controls.Add(new Label { Text = "Showing locations near:", Font = new Font("Segoe UI", 11), ForeColor = Grey, Location = new Point(20, y), Size = new Size(240, 26) });

            ComboBox cboCity = new ComboBox { Location = new Point(268, y), Size = new Size(260, 30), Font = new Font("Segoe UI", 11), DropDownStyle = ComboBoxStyle.DropDownList };
            foreach (var city in allLocations.Keys) cboCity.Items.Add(city);

            // Default to user city or London
            int cityIndex = 0;
            int ci = 0;
            foreach (var city in allLocations.Keys)
            {
                if (string.Equals(city, userCity, StringComparison.OrdinalIgnoreCase)) { cityIndex = ci; break; }
                ci++;
            }
            cboCity.SelectedIndex = cityIndex;
            this.Controls.Add(cboCity);

            // User city badge
            Panel badge = new Panel { Location = new Point(540, y), Size = new Size(300, 28), BackColor = Color.FromArgb(230, 255, 240) };
            Label badgeLbl = new Label { Text = string.IsNullOrEmpty(userCity) ? "Set your city in your profile" : "Based on your registered city: " + userCity, Font = new Font("Segoe UI", 8), ForeColor = Color.FromArgb(10, 100, 40), Location = new Point(8, 6), Size = new Size(284, 16), BackColor = Color.Transparent };
            badge.Controls.Add(badgeLbl);
            this.Controls.Add(badge);
            y += 44;

            // Opening hours
            Panel hours = new Panel { Location = new Point(20, y), Size = new Size(960, 44), BackColor = Color.FromArgb(255, 245, 220) };
            hours.Controls.Add(new Panel { Location = new Point(0, 0), Size = new Size(5, 44), BackColor = Color.FromArgb(180, 120, 0) });
            hours.Controls.Add(new Label { Text = "Opening Hours:  Mon-Fri 9am-5:30pm  |  Sat 9am-1pm  |  Sun Closed  |  Hours may vary by location", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(110, 70, 0), Location = new Point(14, 13), Size = new Size(940, 18), BackColor = Color.Transparent });
            this.Controls.Add(hours);
            y += 54;

            // Locations container
            Panel locContainer = new Panel { Location = new Point(0, y), Size = new Size(1400, 2000), BackColor = LightBg, Name = "locContainer" };
            this.Controls.Add(locContainer);

            // Show locations for selected city
            ShowLocations(locContainer, cboCity.SelectedItem?.ToString() ?? "London");

            // Wire up city change
            cboCity.SelectedIndexChanged += (s, e) =>
            {
                ShowLocations(locContainer, cboCity.SelectedItem?.ToString() ?? "London");
            };
        }

        private void ShowLocations(Panel container, string city)
        {
            container.Controls.Clear();
            int y = 10;

            // Section heading
            container.Controls.Add(new Label { Text = "PostalMS Locations in " + city + " (" + (allLocations.ContainsKey(city) ? allLocations[city].Count : 0) + " locations)", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Red, Location = new Point(20, y), Size = new Size(900, 28) });
            container.Controls.Add(new Panel { Location = new Point(20, y + 30), Size = new Size(1360, 2), BackColor = Color.FromArgb(230, 180, 180) });
            y += 46;

            if (!allLocations.ContainsKey(city))
            {
                container.Controls.Add(new Label { Text = "No locations found for " + city + ". Please select another city.", Font = new Font("Segoe UI", 11), ForeColor = Grey, Location = new Point(20, y), Size = new Size(700, 24) });
                return;
            }

            var locations = allLocations[city];
            int lx = 20; int ly = y;

            foreach (var loc in locations)
            {
                Panel card = new Panel { Location = new Point(lx, ly), Size = new Size(430, 190), BackColor = Color.White };
                card.Controls.Add(new Panel { Location = new Point(0, 0), Size = new Size(430, 5), BackColor = Red });

                // Name
                card.Controls.Add(new Label { Text = loc.name, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Red, Location = new Point(14, 12), Size = new Size(402, 22) });

                // Address
                card.Controls.Add(new Label { Text = "Address:", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Grey, Location = new Point(14, 38), Size = new Size(70, 16) });
                card.Controls.Add(new Label { Text = loc.address, Font = new Font("Segoe UI", 9), ForeColor = Dark, Location = new Point(88, 38), Size = new Size(328, 32) });

                // Phone
                card.Controls.Add(new Label { Text = "Phone:", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Grey, Location = new Point(14, 74), Size = new Size(70, 16) });
                card.Controls.Add(new Label { Text = loc.phone, Font = new Font("Segoe UI", 9), ForeColor = Dark, Location = new Point(88, 74), Size = new Size(328, 16) });

                // Hours
                card.Controls.Add(new Label { Text = "Hours:", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Grey, Location = new Point(14, 94), Size = new Size(70, 16) });
                card.Controls.Add(new Label { Text = loc.hours, Font = new Font("Segoe UI", 9), ForeColor = Dark, Location = new Point(88, 94), Size = new Size(328, 16) });

                // Divider
                card.Controls.Add(new Panel { Location = new Point(14, 114), Size = new Size(402, 1), BackColor = Color.FromArgb(220, 220, 220) });

                // Travel
                card.Controls.Add(new Label { Text = "How to get here:", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Grey, Location = new Point(14, 120), Size = new Size(402, 16) });
                card.Controls.Add(new Label { Text = loc.travel, Font = new Font("Segoe UI", 9), ForeColor = Dark, Location = new Point(14, 138), Size = new Size(402, 32) });

                // Get directions link
                string mapsUrl = "https://maps.google.com/?q=" + Uri.EscapeUriString(loc.address);
                LinkLabel lnk = new LinkLabel { Text = "Get Directions in Google Maps", Font = new Font("Segoe UI", 8, FontStyle.Bold), LinkColor = Red, Location = new Point(14, 168), Size = new Size(402, 18) };
                lnk.Click += (s, e) => { try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(mapsUrl) { UseShellExecute = true }); } catch { } };
                card.Controls.Add(lnk);

                container.Controls.Add(card);

                lx += 445;
                if (lx > 1320) { lx = 20; ly += 205; }
            }
        }

        // Get user city from database
        private string GetUserCity()
        {
            try
            {
                if (db == null || string.IsNullOrEmpty(userID)) return "London";

                var dt = db.Q_Public("SELECT City FROM Users WHERE UserID=@id", ("@id", userID));
                if (dt != null && dt.Rows.Count > 0 && dt.Columns.Contains("City"))
                {
                    string city = dt.Rows[0]["City"]?.ToString() ?? "";
                    if (!string.IsNullOrEmpty(city)) return city;
                }

                // Fallback: extract city from Address field
                var dt2 = db.Q_Public("SELECT Address FROM Users WHERE UserID=@id", ("@id", userID));
                if (dt2 != null && dt2.Rows.Count > 0)
                {
                    string addr = dt2.Rows[0]["Address"]?.ToString() ?? "";
                    foreach (var city in allLocations.Keys)
                        if (addr.IndexOf(city, StringComparison.OrdinalIgnoreCase) >= 0)
                            return city;
                }
            }
            catch { }
            return "London";
        }
    }
}