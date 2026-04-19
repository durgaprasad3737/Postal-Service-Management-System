// ParcelsView.cs
// PostalMS - Postal Service Management System
// CST2550 Coursework - Middlesex University
//
// Parcels page - My Parcels list and Send form.
// Dashboard removed - parcels shown in clean grid.
// Stamps section added with prices and symbols.
// Drop off locations open in Google Maps (free, no API key needed).
// Price formula: (ServicePrice + Weight * 1.2) * SizeMultiplier * InternationalMultiplier

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using static System.Windows.Forms.AxHost;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;

namespace PostalServiceWinForms.Forms
{
    public class ParcelsView : UserControl
    {
        // Parcel grid
        private DataGridView dgvParcels;

        // Send form controls
        private TextBox txtRcvName, txtRcvAddr, txtWeight, txtSenderEmail;
        private ComboBox cboSize, cboCountry;
        private Label lblPrice, lblTrackGen, lblEstDays;
        private Panel pnlIntlSection;
        private RadioButton rbDomestic, rbInternational;

        // Mail vs Package selection
        private Panel pnlMailCard, pnlPkgCard;
        private bool isMail = false;

        // Service selection panels
        private Panel pnlMailSvc, pnlPkgSvc;
        private Panel pnlMailFirst, pnlMailPriority, pnlMailExpress;
        private int mailSvcIndex = 0;
        private Panel pnlPkgGround, pnlPkgPriority, pnlPkgExpress, pnlPkgNextDay;
        private int pkgSvcIndex = 0;

        // Main panels
        private Panel pnlList, pnlSend, pnlRefund;
        private Button btnList, btnSend, btnRefund;

        private DatabaseHelper db;
        private string userID, userName;
        private bool _parcelPayOnline = true;
        private string _parcelContents = "";
        private Color Red = Color.FromArgb(180, 30, 30);
        private Color DarkRed = Color.FromArgb(140, 20, 20);
        private Color Bg = Color.FromArgb(245, 245, 245);

        // Drop off locations with Google Maps links
        private (string name, string address, string hours, string mapsUrl)[] dropOffLocations =
        {
            ("PostalMS Hendon Centre",   "The Burroughs, Hendon, London NW4 4BT",          "Mon-Fri 9am-5:30pm, Sat 9am-1pm", "https://maps.google.com/?q=The+Burroughs+Hendon+London+NW4+4BT"),
            ("PostalMS Cat Hill Centre", "Cat Hill, East Barnet, London EN4 8HT",           "Mon-Fri 9am-5:30pm, Sat 9am-1pm", "https://maps.google.com/?q=Cat+Hill+East+Barnet+London+EN4+8HT"),
            ("PostalMS Archway Point",   "2 Junction Road, Archway, London N19 5QU",        "Mon-Fri 9am-6pm, Sat 10am-2pm",   "https://maps.google.com/?q=2+Junction+Road+Archway+London+N19+5QU"),
            ("PostalMS Wembley Centre",  "Engineers Way, Wembley, London HA9 0ED",          "Mon-Fri 9am-5:30pm, Sat 9am-1pm", "https://maps.google.com/?q=Engineers+Way+Wembley+London+HA9+0ED"),
            ("WH Smith Brent Cross",     "Brent Cross Shopping Centre, London NW4 3FP",     "Mon-Sat 9am-8pm, Sun 11am-5pm",   "https://maps.google.com/?q=Brent+Cross+Shopping+Centre+London+NW4+3FP"),
            ("Tesco Golders Green",      "186 Golders Green Road, London NW11 9AA",         "Open daily 7am-11pm",             "https://maps.google.com/?q=186+Golders+Green+Road+London+NW11+9AA"),
            ("PostalMS Archway Shop",    "12 Holloway Road, London N7 8JH",                 "Mon-Sat 8am-7pm",                 "https://maps.google.com/?q=12+Holloway+Road+London+N7+8JH"),
            ("Rymans Stationers",        "44 Finchley Road, London NW3 5EL",                "Mon-Sat 9am-6pm",                 "https://maps.google.com/?q=44+Finchley+Road+London+NW3+5EL"),
        };

        // International countries with zone multiplier and estimated days
        private Dictionary<string, (double mult, int days)> countries = new Dictionary<string, (double, int)>
        {
            {"France",(1.8,5)},{"Germany",(1.8,5)},{"Spain",(1.8,5)},{"Italy",(1.8,5)},
            {"Ireland",(1.8,4)},{"Netherlands",(1.8,5)},{"Belgium",(1.8,5)},{"Portugal",(1.8,6)},
            {"Poland",(2.2,7)},{"Sweden",(2.2,7)},{"Norway",(2.2,6)},{"Denmark",(2.2,6)},
            {"United States",(3.0,10)},{"Canada",(3.0,10)},{"Mexico",(3.2,12)},
            {"Australia",(3.8,14)},{"New Zealand",(3.8,14)},{"Japan",(3.8,12)},
            {"China",(3.5,12)},{"India",(3.2,12)},{"UAE",(2.8,8)},
            {"South Africa",(3.5,14)},{"Brazil",(3.8,16)},{"Singapore",(3.5,10)},
        };

        public ParcelsView(string uid, string name, DatabaseHelper dbHelper)
        {
            userID = uid; userName = name; db = dbHelper;
            this.Dock = DockStyle.Fill;
            this.BackColor = Bg;
            Build();
        }

        private void Build()
        {
            // Top navigation bar
            Panel topBar = new Panel { Dock = DockStyle.Top, Height = 110, BackColor = Color.White };
            topBar.Paint += (s, e) => e.Graphics.DrawLine(new Pen(Color.FromArgb(218, 218, 218)), 0, 109, topBar.Width, 109);
            this.Controls.Add(topBar);

            topBar.Controls.Add(new Label
            {
                Text = "Parcels",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Red,
                Location = new Point(10, 65),
                Size = new Size(115, 28)
            });

            // My Parcels tab button
            btnList = new Button
            {
                Text = "My Parcels",
                Location = new Point(132, 60),
                Size = new Size(140, 38),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Red,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnList.FlatAppearance.BorderSize = 0;
            btnList.Click += (s, e) => Switch("list");
            topBar.Controls.Add(btnList);

            // Send Mail or Package tab button
            btnSend = new Button
            {
                Text = "Send Mail or Package",
                Location = new Point(280, 60),
                Size = new Size(210, 38),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(240, 240, 240),
                ForeColor = Red,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSend.FlatAppearance.BorderColor = Color.FromArgb(210, 190, 190);
            btnSend.FlatAppearance.BorderSize = 1;
            btnSend.Click += (s, e) => Switch("send");
            topBar.Controls.Add(btnSend);

            // Refund Request tab button
            btnRefund = new Button
            {
                Text = "Refund Request",
                Location = new Point(498, 60),
                Size = new Size(180, 38),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(240, 240, 240),
                ForeColor = Red,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRefund.FlatAppearance.BorderColor = Color.FromArgb(210, 190, 190);
            btnRefund.FlatAppearance.BorderSize = 1;
            btnRefund.Click += (s, e) => Switch("refund");
            topBar.Controls.Add(btnRefund);

            // My Parcels panel
            pnlList = new Panel { Dock = DockStyle.Fill, BackColor = Bg, Visible = true };
            this.Controls.Add(pnlList);
            BuildParcelList();

            // Send form panel
            pnlSend = new Panel { Dock = DockStyle.Fill, BackColor = Bg, AutoScroll = true, Visible = false };
            this.Controls.Add(pnlSend);
            BuildSendForm();

            // Refund panel
            pnlRefund = new Panel { Dock = DockStyle.Fill, BackColor = Bg, AutoScroll = true, Visible = false };
            this.Controls.Add(pnlRefund);
            BuildRefundForm();

            RefreshParcels();
        }

        // Switch between tabs
        private void Switch(string which)
        {
            pnlList.Visible = which == "list";
            pnlSend.Visible = which == "send";
            pnlRefund.Visible = which == "refund";

            if (which == "list") pnlList.BringToFront();
            if (which == "send") pnlSend.BringToFront();
            if (which == "refund") pnlRefund.BringToFront();

            btnList.BackColor = which == "list" ? Red : Color.FromArgb(240, 240, 240);
            btnList.ForeColor = which == "list" ? Color.White : Red;
            btnSend.BackColor = which == "send" ? Red : Color.FromArgb(240, 240, 240);
            btnSend.ForeColor = which == "send" ? Color.White : Red;
            btnRefund.BackColor = which == "refund" ? Red : Color.FromArgb(240, 240, 240);
            btnRefund.ForeColor = which == "refund" ? Color.White : Red;

            if (which == "list") RefreshParcels();
            if (which == "send") RefreshTrackID();
        }

        // ============================================================
        // MY PARCELS LIST
        // ============================================================
        private void BuildParcelList()
        {
            pnlList.Controls.Add(new Label
            {
                Text = "All My Parcels",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Red,
                Location = new Point(0, 10),
                Size = new Size(400, 28)
            });

            pnlList.Controls.Add(new Label
            {
                Text = "All your submitted parcels and their current status are shown below.",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                Location = new Point(0, 42),
                Size = new Size(800, 20)
            });

            // Search box
            TextBox srch = new TextBox
            {
                Location = new Point(0, 72),
                Size = new Size(420, 32),
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle,
                Text = "Search tracking ID or receiver...",
                ForeColor = Color.LightGray
            };
            srch.Enter += (s, e) => { if (srch.ForeColor == Color.LightGray) { srch.Text = ""; srch.ForeColor = Color.Black; } };
            srch.Leave += (s, e) => { if (srch.Text == "") { srch.Text = "Search tracking ID or receiver..."; srch.ForeColor = Color.LightGray; } };
            srch.TextChanged += (s, e) => { if (srch.ForeColor != Color.LightGray) FilterParcels(srch.Text); };
            pnlList.Controls.Add(srch);

            // Parcel grid
            dgvParcels = new DataGridView
            {
                Location = new Point(0, 112),
                Size = new Size(1280, 460),
                ReadOnly = true,
                BackgroundColor = Color.White,
                RowHeadersVisible = false,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            dgvParcels.ColumnHeadersDefaultCellStyle.BackColor = Red;
            dgvParcels.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvParcels.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvParcels.ColumnHeadersHeight = 44;
            dgvParcels.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            dgvParcels.RowTemplate.Height = 42;
            dgvParcels.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(255, 248, 248);
            dgvParcels.CellFormatting += StatusFmt;
            dgvParcels.SelectionChanged += DgvSelect;
            pnlList.Controls.Add(dgvParcels);

            // Detail panel
            Panel det = new Panel
            {
                Location = new Point(0, 580),
                Size = new Size(1280, 0),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Name = "pnlDetail"
            };
            pnlList.Controls.Add(det);
        }

        private void RefreshParcels()
        {
            try { dgvParcels.DataSource = db.GetParcelsByCustomer(userID); HideColumns(); }
            catch { }
        }

        private void HideColumns()
        {
            string[] hide = { "IsInternational", "RefundRequested", "RefundReason", "CustomerID", "SenderAddress", "ReceiverAddress" };
            foreach (string col in hide)
                if (dgvParcels.Columns.Contains(col))
                    dgvParcels.Columns[col].Visible = false;
            if (dgvParcels.Columns.Contains("DateSent"))
                dgvParcels.Columns["DateSent"].DefaultCellStyle.Format = "dd/MM/yyyy";
            if (dgvParcels.Columns.Contains("EstimatedDelivery"))
                dgvParcels.Columns["EstimatedDelivery"].DefaultCellStyle.Format = "dd/MM/yyyy";
            if (dgvParcels.Columns.Contains("Price"))
                dgvParcels.Columns["Price"].DefaultCellStyle.Format = "GBP 0.00";
        }

        private void FilterParcels(string query)
        {
            try
            {
                DataTable full = db.GetParcelsByCustomer(userID);
                if (string.IsNullOrEmpty(query)) { dgvParcels.DataSource = full; HideColumns(); return; }
                DataTable filtered = full.Clone();
                foreach (DataRow row in full.Rows)
                    if (row["TrackingID"].ToString().ToLower().Contains(query.ToLower()) ||
                        row["ReceiverName"].ToString().ToLower().Contains(query.ToLower()))
                        filtered.ImportRow(row);
                dgvParcels.DataSource = filtered;
                HideColumns();
            }
            catch { }
        }

        private void DgvSelect(object sender, EventArgs e)
        {
            if (dgvParcels.CurrentRow == null) return;
            Panel det = null;
            foreach (Control c in pnlList.Controls)
                if (c.Name == "pnlDetail") { det = c as Panel; break; }
            if (det == null) return;

            det.Controls.Clear();
            det.Size = new Size(1280, 200);

            var row = dgvParcels.CurrentRow;
            string tid = row.Cells["TrackingID"].Value?.ToString() ?? "--";
            string st = row.Cells["Status"].Value?.ToString() ?? "--";

            det.Controls.Add(new Label { Text = "Details -- " + tid, Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Red, Location = new Point(14, 10), Size = new Size(500, 24) });

            void DR(string l, string v, int x, int y2)
            {
                det.Controls.Add(new Label { Text = l, Font = new Font("Segoe UI", 9), ForeColor = Color.Gray, Location = new Point(x, y2), Size = new Size(160, 18) });
                det.Controls.Add(new Label { Text = v, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(40, 20, 20), Location = new Point(x, y2 + 18), Size = new Size(210, 22) });
            }

            DR("Tracking ID", tid, 14, 42);
            DR("Receiver", row.Cells["ReceiverName"].Value?.ToString() ?? "--", 240, 42);
            DR("Type", row.Cells["ParcelType"].Value?.ToString() ?? "--", 460, 42);
            DR("Service", row.Cells["ServiceType"].Value?.ToString() ?? "--", 640, 42);
            DR("Price", "GBP " + row.Cells["Price"].Value?.ToString(), 820, 42);
            DR("Status", st, 14, 108);

            // Status progress bar
            string[] stages = { "Pending", "In Transit", "Out for Delivery", "Delivered" };
            int cur = Array.IndexOf(stages, st);
            for (int i = 0; i < 4; i++)
            {
                bool done = i <= cur;
                det.Controls.Add(new Label
                {
                    Text = (done ? "OK " : "-- ") + stages[i],
                    Font = new Font("Segoe UI", 9, done ? FontStyle.Bold : FontStyle.Regular),
                    ForeColor = done ? Color.FromArgb(20, 130, 65) : Color.LightGray,
                    Location = new Point(240 + i * 248, 108),
                    Size = new Size(240, 36)
                });
            }

            // Drop Off button -- shown when status is Pending
            if (st == "Pending")
            {
                Button bdrop = new Button { Text = "I Have Dropped Off My Parcel", Location = new Point(14, 158), Size = new Size(280, 36), Font = new Font("Segoe UI", 9, FontStyle.Bold), BackColor = Color.FromArgb(20, 130, 65), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
                bdrop.FlatAppearance.BorderSize = 0;
                string dropTid = tid;
                bdrop.Click += (s2, e2) =>
                {
                    var dr = MessageBox.Show(
                        "Confirm you have dropped off parcel " + dropTid + " at a PostalMS location.

Once confirmed the status will update to In Transit.",
                        "Confirm Drop Off", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (dr == DialogResult.Yes)
                    {
                        db.UpdateParcelStatus(dropTid, "In Transit");
                        MessageBox.Show(
                            "Drop off confirmed!

Your parcel " + dropTid + " is now In Transit.
You can track it in My Parcels.

Estimated updates:
  In Transit-- now
  Out for Delivery-- 1 - 2 days before delivery
  Delivered-- on your estimated delivery date",
                            "Parcel In Transit", MessageBoxButtons.OK, MessageBoxIcon.Information) ;
                        RefreshParcels();
                    }
                };
                det.Controls.Add(bdrop);
            }

            // Refund button
            if (st == "Failed" || st == "Delivered")
            {
                Button br = new Button { Text = "Request Refund", Location = new Point(1056, 106), Size = new Size(150, 36), Font = new Font("Segoe UI", 9, FontStyle.Bold), BackColor = Red, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
                br.FlatAppearance.BorderSize = 0;
                br.Click += (s2, e2) => ShowRefund(tid);
                det.Controls.Add(br);
            }
        }

        private void ShowRefund(string tid)
        {
            Form f = new Form { Text = "Refund -- " + tid, Size = new Size(480, 300), StartPosition = FormStartPosition.CenterParent, BackColor = Color.White };
            f.Controls.Add(new Label { Text = "Refund Request", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Red, Location = new Point(20, 18), Size = new Size(400, 28) });
            f.Controls.Add(new Label { Text = "Describe the reason:", Font = new Font("Segoe UI", 10), ForeColor = Color.Gray, Location = new Point(20, 52), Size = new Size(430, 22) });
            TextBox r2 = new TextBox { Location = new Point(20, 78), Size = new Size(430, 95), Font = new Font("Segoe UI", 10), Multiline = true, BorderStyle = BorderStyle.FixedSingle };
            f.Controls.Add(r2);
            Button sub = new Button { Text = "Submit", Location = new Point(20, 213), Size = new Size(130, 40), Font = new Font("Segoe UI", 10, FontStyle.Bold), BackColor = Red, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            sub.FlatAppearance.BorderSize = 0;
            sub.Click += (s2, e2) => { if (string.IsNullOrWhiteSpace(r2.Text)) { MessageBox.Show("Please enter a reason."); return; } MessageBox.Show(db.RequestRefund(tid, r2.Text.Trim())); f.Close(); RefreshParcels(); };
            f.Controls.Add(sub);
            f.ShowDialog();
        }

        private void StatusFmt(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvParcels.Columns.Count == 0) return;
            if (!dgvParcels.Columns.Contains("Status")) return;
            if (e.ColumnIndex != dgvParcels.Columns["Status"].Index) return;
            string v = e.Value?.ToString() ?? "";
            e.CellStyle.ForeColor = v == "Delivered" ? Color.FromArgb(20, 130, 65) :
                                    v == "In Transit" ? Color.FromArgb(30, 100, 180) :
                                    v == "Failed" ? Color.FromArgb(200, 30, 30) :
                                    v == "Pending" ? Color.FromArgb(160, 100, 0) :
                                    Color.Black;
            e.CellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        }

        // ============================================================
        // SEND FORM
        // ============================================================
        private void BuildSendForm()
        {
            int y = 8;

            // Auto-generated tracking ID display
            Panel tb = new Panel { Location = new Point(10, y), Size = new Size(1240, 38), BackColor = Color.FromArgb(235, 245, 255) };
            pnlSend.Controls.Add(tb);
            lblTrackGen = new Label { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Red, Location = new Point(12, 9), Size = new Size(1010, 22), BackColor = Color.Transparent };
            tb.Controls.Add(lblTrackGen);
            y += 48;

            // Step 1 - What are you sending
            SH("Step 1 -- What are you sending?", y); y += 36;
            pnlMailCard = MakeTypeCard("Mail", "Send Mail", "Letters, documents, postcards. Max 2kg. From GBP 1.99", 10, y, 590, () => SelectType(true));
            pnlPkgCard = MakeTypeCard("Package", "Send a Package", "Parcels, gifts, goods. Max 30kg. From GBP 2.99", 620, y, 630, () => SelectType(false));
            pnlSend.Controls.AddRange(new Control[] { pnlMailCard, pnlPkgCard });
            y += 118;

            // Step 2 - Choose a service
            SH("Step 2 -- Choose a Service", y); y += 36;

            pnlMailSvc = new Panel { Location = new Point(10, y), Size = new Size(1240, 90), BackColor = Color.Transparent, Visible = false };
            pnlSend.Controls.Add(pnlMailSvc);
            pnlPkgSvc = new Panel { Location = new Point(10, y), Size = new Size(1240, 90), BackColor = Color.Transparent, Visible = true };
            pnlSend.Controls.Add(pnlPkgSvc);

            pnlMailFirst = SvcCard(pnlMailSvc, "First Class", "Up to 100g", "GBP 1.99", "3-5 days", 0, () => { mailSvcIndex = 0; RefreshMailSvc(); Recalc(); });
            pnlMailPriority = SvcCard(pnlMailSvc, "Priority", "Up to 2kg", "GBP 3.99", "1-2 days", 310, () => { mailSvcIndex = 1; RefreshMailSvc(); Recalc(); });
            pnlMailExpress = SvcCard(pnlMailSvc, "Express", "Guaranteed", "GBP 6.99", "Next day", 620, () => { mailSvcIndex = 2; RefreshMailSvc(); Recalc(); });

            pnlPkgGround = SvcCard(pnlPkgSvc, "Ground", "Up to 30kg", "GBP 2.99+", "3-5 days", 0, () => { pkgSvcIndex = 0; RefreshPkgSvc(); Recalc(); });
            pnlPkgPriority = SvcCard(pnlPkgSvc, "Priority", "Faster", "GBP 5.99+", "1-2 days", 310, () => { pkgSvcIndex = 1; RefreshPkgSvc(); Recalc(); });
            pnlPkgExpress = SvcCard(pnlPkgSvc, "Express", "Guaranteed", "GBP 9.99+", "Next day", 620, () => { pkgSvcIndex = 2; RefreshPkgSvc(); Recalc(); });
            pnlPkgNextDay = SvcCard(pnlPkgSvc, "Next Day", "By 1pm", "GBP 14.99+", "By 1pm", 930, () => { pkgSvcIndex = 3; RefreshPkgSvc(); Recalc(); });

            mailSvcIndex = 0; pkgSvcIndex = 0;
            RefreshMailSvc(); RefreshPkgSvc();
            y += 100;

            // Step 3 - Where is it going
            SH("Step 3 -- Where is it going?", y); y += 36;
            rbDomestic = new RadioButton { Text = "UK Domestic", Font = new Font("Segoe UI", 11), Location = new Point(10, y), Size = new Size(200, 28), Checked = true };
            rbInternational = new RadioButton { Text = "International", Font = new Font("Segoe UI", 11), Location = new Point(220, y), Size = new Size(200, 28) };
            pnlSend.Controls.AddRange(new Control[] { rbDomestic, rbInternational });
            y += 36;

            // Step 3 destination container -- fixed height so Step 4 never overlaps
            Panel pnlStep3 = new Panel
            {
                Location = new Point(10, y),
                Size = new Size(1240, 120),
                BackColor = Color.Transparent
            };
            pnlSend.Controls.Add(pnlStep3);

            // UK city label and dropdown -- inside fixed container
            var lblUKCity2 = new Label { Text = "DESTINATION CITY (UK)", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.Gray, Location = new Point(0, 0), Size = new Size(300, 16), Visible = true };
            var cboUKCity = new ComboBox
            {
                Location = new Point(0, 18),
                Size = new Size(360, 34),
                Font = new Font("Segoe UI", 11),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Name = "cboUKCity",
                Visible = true
            };
            string[] ukCities = {
                "London", "Manchester", "Birmingham", "Leeds", "Liverpool",
                "Sheffield", "Bristol", "Edinburgh", "Glasgow", "Cardiff",
                "Newcastle", "Nottingham", "Leicester", "Southampton", "Portsmouth",
                "Oxford", "Cambridge", "Brighton", "Norwich", "Derby",
                "Coventry", "Reading", "Sunderland", "Wolverhampton", "Bradford"
            };
            foreach (string city in ukCities) cboUKCity.Items.Add(city);
            cboUKCity.SelectedIndex = 0;
            pnlStep3.Controls.AddRange(new Control[] { lblUKCity2, cboUKCity });

            // International country label and dropdown -- inside fixed container
            var lblIntl2 = new Label { Text = "DESTINATION COUNTRY", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.Gray, Location = new Point(0, 0), Size = new Size(300, 16), Visible = false };
            cboCountry = new ComboBox
            {
                Location = new Point(0, 18),
                Size = new Size(360, 34),
                Font = new Font("Segoe UI", 11),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Visible = false
            };
            foreach (var k in countries.Keys) cboCountry.Items.Add(k);
            cboCountry.SelectedIndex = 0;
            cboCountry.SelectedIndexChanged += (s, e) => Recalc();
            pnlStep3.Controls.AddRange(new Control[] { lblIntl2, cboCountry });

            // International zone info -- inside fixed container
            pnlIntlSection = new Panel
            {
                Location = new Point(0, 64),
                Size = new Size(1220, 40),
                BackColor = Color.FromArgb(242, 255, 242),
                Visible = false
            };
            pnlIntlSection.Controls.Add(new Label
            {
                Text = "Zone 1 Europe x1.8  |  Zone 2 N.Europe x2.2  |  Zone 3 USA/UAE x3.0  |  Zone 4 World x3.5-3.8",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(20, 80, 40),
                Location = new Point(12, 10),
                Size = new Size(1200, 20),
                BackColor = Color.Transparent
            });
            pnlStep3.Controls.Add(pnlIntlSection);

            // Toggle between UK and International -- only affects controls inside pnlStep3
            rbDomestic.CheckedChanged += (s, e) =>
            {
                bool dom = rbDomestic.Checked;
                lblUKCity2.Visible = dom;
                cboUKCity.Visible = dom;
                lblIntl2.Visible = !dom;
                cboCountry.Visible = !dom;
                pnlIntlSection.Visible = !dom;
                Recalc();
            };
            rbInternational.CheckedChanged += (s, e) =>
            {
                bool dom = rbDomestic.Checked;
                lblUKCity2.Visible = dom;
                cboUKCity.Visible = dom;
                lblIntl2.Visible = !dom;
                cboCountry.Visible = !dom;
                pnlIntlSection.Visible = !dom;
                Recalc();
            };

            // Step 4 always starts exactly 130px below Step 3 heading
            y += 130;

            // Step 4 - Full Receiver Details
            SH("Step 4 -- Receiver Details", y); y += 36;

            // Row 1 - Receiver Name and Phone
            pnlSend.Controls.Add(new Label { Text = "RECEIVER FULL NAME", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.Gray, Location = new Point(10, y), Size = new Size(300, 16) });
            pnlSend.Controls.Add(new Label { Text = "RECEIVER PHONE NUMBER", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.Gray, Location = new Point(450, y), Size = new Size(300, 16) });
            y += 18;
            txtRcvName = new TextBox { Location = new Point(10, y), Size = new Size(430, 34), Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle };
            var txtRcvPhone = new TextBox { Location = new Point(450, y), Size = new Size(350, 34), Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle };
            pnlSend.Controls.AddRange(new Control[] { txtRcvName, txtRcvPhone });
            y += 46;

            // Row 2 - Receiver Email
            pnlSend.Controls.Add(new Label { Text = "RECEIVER EMAIL ADDRESS", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.Gray, Location = new Point(10, y), Size = new Size(400, 16) });
            y += 18;
            var txtRcvEmail = new TextBox { Location = new Point(10, y), Size = new Size(560, 34), Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle };
            pnlSend.Controls.Add(txtRcvEmail);
            y += 46;

            // Row 3 - Receiver Address
            pnlSend.Controls.Add(new Label { Text = "RECEIVER STREET ADDRESS", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.Gray, Location = new Point(10, y), Size = new Size(400, 16) });
            y += 18;
            txtRcvAddr = new TextBox { Location = new Point(10, y), Size = new Size(1240, 34), Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle };
            pnlSend.Controls.Add(txtRcvAddr);
            y += 46;

            // Row 4 - Receiver City and Postcode
            pnlSend.Controls.Add(new Label { Text = "RECEIVER CITY", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.Gray, Location = new Point(10, y), Size = new Size(200, 16) });
            pnlSend.Controls.Add(new Label { Text = "RECEIVER POSTCODE", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.Gray, Location = new Point(350, y), Size = new Size(200, 16) });
            y += 18;
            var txtRcvCity = new TextBox { Location = new Point(10, y), Size = new Size(330, 34), Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle };
            var txtRcvPost = new TextBox { Location = new Point(350, y), Size = new Size(200, 34), Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle };
            pnlSend.Controls.AddRange(new Control[] { txtRcvCity, txtRcvPost });
            y += 46;

            // Row 5 - Special delivery instructions
            pnlSend.Controls.Add(new Label { Text = "SPECIAL DELIVERY INSTRUCTIONS (optional)", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.Gray, Location = new Point(10, y), Size = new Size(600, 16) });
            y += 18;
            var txtRcvInstructions = new TextBox { Location = new Point(10, y), Size = new Size(1240, 34), Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle, Text = "e.g. Leave at door, ring bell twice, deliver to neighbour at No.12", ForeColor = Color.LightGray };
            txtRcvInstructions.Enter += (s, e) => { if (txtRcvInstructions.ForeColor == Color.LightGray) { txtRcvInstructions.Text = ""; txtRcvInstructions.ForeColor = Color.Black; } };
            txtRcvInstructions.Leave += (s, e) => { if (txtRcvInstructions.Text == "") { txtRcvInstructions.Text = "e.g. Leave at door, ring bell twice, deliver to neighbour at No.12"; txtRcvInstructions.ForeColor = Color.LightGray; } };
            pnlSend.Controls.Add(txtRcvInstructions);
            y += 46;

            // Row 6 - Sender email
            pnlSend.Controls.Add(new Label { Text = "YOUR EMAIL (must be @gmail.com) -- order confirmation sent here", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.Gray, Location = new Point(10, y), Size = new Size(700, 16) });
            y += 18;
            txtSenderEmail = new TextBox { Location = new Point(10, y), Size = new Size(560, 34), Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle };
            pnlSend.Controls.Add(txtSenderEmail);
            y += 46;

            // Step 5 - Weight and size
            SH("Step 5 -- Weight and Size", y); y += 36;
            pnlSend.Controls.Add(new Label { Text = "WEIGHT (kg)", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.Gray, Location = new Point(10, y), Size = new Size(200, 16) });
            pnlSend.Controls.Add(new Label { Text = "SIZE", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.Gray, Location = new Point(240, y), Size = new Size(200, 16) });
            y += 18;

            txtWeight = new TextBox { Location = new Point(10, y), Size = new Size(220, 34), Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle, Text = "1.0" };
            txtWeight.TextChanged += (s, e) => Recalc();

            cboSize = new ComboBox { Location = new Point(240, y), Size = new Size(220, 34), Font = new Font("Segoe UI", 11), DropDownStyle = ComboBoxStyle.DropDownList };
            cboSize.Items.AddRange(new object[] { "Small", "Medium", "Large" });
            cboSize.SelectedIndex = 0;
            cboSize.SelectedIndexChanged += (s, e) => Recalc();
            pnlSend.Controls.AddRange(new Control[] { txtWeight, cboSize });
            y += 46;

            // Price and estimate display
            lblPrice = new Label { Location = new Point(10, y), Size = new Size(600, 32), Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Red };
            lblEstDays = new Label { Location = new Point(10, y + 36), Size = new Size(600, 22), Font = new Font("Segoe UI", 10), ForeColor = Color.Gray };
            pnlSend.Controls.AddRange(new Control[] { lblPrice, lblEstDays });
            y += 70;

            // Drop off locations section
            SH("Drop Off Locations -- Click to open in Google Maps", y); y += 36;

            // Info note
            Panel note = new Panel { Location = new Point(10, y), Size = new Size(1240, 40), BackColor = Color.FromArgb(235, 245, 255) };
            note.Controls.Add(new Label { Text = "Click any location below to get directions in Google Maps -- completely free, no account needed.", Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(20, 60, 140), Location = new Point(12, 10), Size = new Size(1200, 20), BackColor = Color.Transparent });
            pnlSend.Controls.Add(note);
            y += 50;

            // Location cards with Google Maps links
            int lx = 10; int ly = y;
            for (int i = 0; i < dropOffLocations.Length; i++)
            {
                var loc = dropOffLocations[i];
                Panel card = new Panel { Location = new Point(lx, ly), Size = new Size(295, 110), BackColor = Color.White, Cursor = Cursors.Hand };
                card.Controls.Add(new Panel { Location = new Point(0, 0), Size = new Size(295, 4), BackColor = Red });

                // Location name
                card.Controls.Add(new Label { Text = loc.name, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Red, Location = new Point(10, 10), Size = new Size(275, 18) });

                // Address
                card.Controls.Add(new Label { Text = loc.address, Font = new Font("Segoe UI", 8), ForeColor = Color.FromArgb(60, 60, 60), Location = new Point(10, 30), Size = new Size(275, 32) });

                // Hours
                card.Controls.Add(new Label { Text = loc.hours, Font = new Font("Segoe UI", 8, FontStyle.Italic), ForeColor = Color.Gray, Location = new Point(10, 64), Size = new Size(275, 18) });

                // Open Maps link
                string url = loc.mapsUrl;
                LinkLabel lnk = new LinkLabel { Text = "Get Directions", Font = new Font("Segoe UI", 8, FontStyle.Bold), LinkColor = Red, Location = new Point(10, 84), Size = new Size(275, 18) };
                lnk.Click += (s, e) => { try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); } catch { MessageBox.Show("Could not open browser. Please visit: " + url); } };
                card.Controls.Add(lnk);

                // Clicking the card also opens maps
                string cardUrl = loc.mapsUrl;
                card.Click += (s, e) => { try { Process.Start(new ProcessStartInfo(cardUrl) { UseShellExecute = true }); } catch { } };

                pnlSend.Controls.Add(card);

                lx += 305;
                if (lx > 1200) { lx = 10; ly += 120; }
            }
            y = ly + 130;

            // Step 6 - Parcel Contents (Packing Slip)
            SH("Step 6 -- What is in the parcel?", y); y += 36;
            pnlSend.Controls.Add(new Label { Text = "CONTENTS DESCRIPTION (e.g. clothing, books, electronics)", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.Gray, Location = new Point(10, y), Size = new Size(700, 16) });
            y += 18;
            var txtContents = new TextBox
            {
                Location = new Point(10, y),
                Size = new Size(900, 60),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                Multiline = true,
                Text = "Describe the contents of your parcel...",
                ForeColor = Color.LightGray
            };
            txtContents.Enter += (s, e) => { if (txtContents.ForeColor == Color.LightGray) { txtContents.Text = ""; txtContents.ForeColor = Color.Black; } };
            txtContents.Leave += (s, e) => { if (txtContents.Text == "") { txtContents.Text = "Describe the contents of your parcel..."; txtContents.ForeColor = Color.LightGray; } };
            pnlSend.Controls.Add(txtContents);
            y += 76;

            // Step 7 - Payment Method
            SH("Step 7 -- How would you like to pay?", y); y += 36;

            // Pay Now card
            Panel pnlPayNowCard = new Panel { Location = new Point(10, y), Size = new Size(340, 90), BackColor = Color.White, Cursor = Cursors.Hand };
            pnlPayNowCard.Controls.Add(new Panel { Location = new Point(0, 0), Size = new Size(340, 5), BackColor = Red, Name = "bar" });
            pnlPayNowCard.Controls.Add(new Label { Text = "Pay Now Online", Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Red, Location = new Point(14, 14), Size = new Size(312, 26), BackColor = Color.Transparent });
            pnlPayNowCard.Controls.Add(new Label { Text = "Pay by credit or debit card right now.", Font = new Font("Segoe UI", 9), ForeColor = Color.Gray, Location = new Point(14, 42), Size = new Size(312, 18), BackColor = Color.Transparent });
            pnlPayNowCard.Controls.Add(new Label { Text = "Card charged immediately on submission.", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(20, 130, 65), Location = new Point(14, 62), Size = new Size(312, 18), BackColor = Color.Transparent });
            pnlSend.Controls.Add(pnlPayNowCard);

            // Pay In Store card
            Panel pnlPayStoreCard = new Panel { Location = new Point(370, y), Size = new Size(340, 90), BackColor = Color.White, Cursor = Cursors.Hand };
            pnlPayStoreCard.Controls.Add(new Panel { Location = new Point(0, 0), Size = new Size(340, 5), BackColor = Color.FromArgb(200, 180, 180), Name = "bar" });
            pnlPayStoreCard.Controls.Add(new Label { Text = "Pay In Store", Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Red, Location = new Point(14, 14), Size = new Size(312, 26), BackColor = Color.Transparent });
            pnlPayStoreCard.Controls.Add(new Label { Text = "Reserve now and pay when you drop off.", Font = new Font("Segoe UI", 9), ForeColor = Color.Gray, Location = new Point(14, 42), Size = new Size(312, 18), BackColor = Color.Transparent });
            pnlPayStoreCard.Controls.Add(new Label { Text = "Pay by cash or card at the PostalMS store.", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(180, 100, 0), Location = new Point(14, 62), Size = new Size(312, 18), BackColor = Color.Transparent });
            pnlSend.Controls.Add(pnlPayStoreCard);

            bool parcelPayOnline = true;
            pnlPayNowCard.Controls["bar"].BackColor = Red;
            pnlPayStoreCard.Controls["bar"].BackColor = Color.FromArgb(200, 180, 180);

            Action selectPayNow = () =>
            {
                parcelPayOnline = true;
                pnlPayNowCard.Controls["bar"].BackColor = Red;
                pnlPayStoreCard.Controls["bar"].BackColor = Color.FromArgb(200, 180, 180);
            };
            Action selectPayStore = () =>
            {
                parcelPayOnline = false;
                pnlPayNowCard.Controls["bar"].BackColor = Color.FromArgb(200, 180, 180);
                pnlPayStoreCard.Controls["bar"].BackColor = Red;
            };

            pnlPayNowCard.Click += (s, e) => selectPayNow();
            pnlPayStoreCard.Click += (s, e) => selectPayStore();
            foreach (Control c in pnlPayNowCard.Controls) c.Click += (s, e) => selectPayNow();
            foreach (Control c in pnlPayStoreCard.Controls) c.Click += (s, e) => selectPayStore();
            y += 110;

            // Pay in store info
            Panel payStoreInfo = new Panel { Location = new Point(10, y), Size = new Size(700, 46), BackColor = Color.FromArgb(255, 245, 220) };
            payStoreInfo.Controls.Add(new Label { Text = "Bring your parcel to any PostalMS location. Show your tracking ID at the counter and pay by cash or card. Staff will pack and process your parcel.", Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(140, 80, 0), Location = new Point(12, 10), Size = new Size(676, 26), BackColor = Color.Transparent });
            pnlSend.Controls.Add(payStoreInfo);
            y += 56;

            // Submit button
            Button btnSubmit = new Button
            {
                Text = "Submit Parcel ->",
                Location = new Point(10, y),
                Size = new Size(300, 52),
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                BackColor = Red,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSubmit.FlatAppearance.BorderSize = 0;
            btnSubmit.MouseEnter += (s, e) => btnSubmit.BackColor = DarkRed;
            btnSubmit.MouseLeave += (s, e) => btnSubmit.BackColor = Red;
            btnSubmit.Click += (s, e) =>
            {
                // Store payment choice and contents for Submit_Click to use
                _parcelPayOnline = parcelPayOnline;
                _parcelContents = txtContents.ForeColor == Color.LightGray ? "" : txtContents.Text.Trim();
                Submit_Click(s, e);
            };
            pnlSend.Controls.Add(btnSubmit);

            Recalc();
        }

        // ============================================================
        // REFUND FORM
        // ============================================================
        private void BuildRefundForm()
        {
            int y = 20;

            // Page header
            pnlRefund.Controls.Add(new Label { Text = "Refund Request", Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = Red, Location = new Point(20, y), Size = new Size(500, 36) });
            y += 44;
            pnlRefund.Controls.Add(new Label { Text = "Please fill in your tracking ID and describe what happened. We will review your request and contact you within 2-3 working days.", Font = new Font("Segoe UI", 10), ForeColor = Color.Gray, Location = new Point(20, y), Size = new Size(860, 40) });
            y += 54;

            // White card
            Panel card = new Panel { Location = new Point(20, y), Size = new Size(900, 460), BackColor = Color.White };
            card.Controls.Add(new Panel { Location = new Point(0, 0), Size = new Size(900, 5), BackColor = Red });
            pnlRefund.Controls.Add(card);

            int cy = 20;

            // Tracking ID field
            card.Controls.Add(new Label { Text = "TRACKING ID", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.Gray, Location = new Point(20, cy), Size = new Size(300, 16) });
            cy += 18;
            var txtRefundTrack = new TextBox { Location = new Point(20, cy), Size = new Size(400, 34), Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle };
            txtRefundTrack.Text = "e.g. PS-2026-00001";
            txtRefundTrack.ForeColor = Color.LightGray;
            txtRefundTrack.Enter += (s, e) => { if (txtRefundTrack.ForeColor == Color.LightGray) { txtRefundTrack.Text = ""; txtRefundTrack.ForeColor = Color.Black; } };
            txtRefundTrack.Leave += (s, e) => { if (txtRefundTrack.Text == "") { txtRefundTrack.Text = "e.g. PS-2026-00001"; txtRefundTrack.ForeColor = Color.LightGray; } };
            card.Controls.Add(txtRefundTrack);
            cy += 50;

            // Reason dropdown
            card.Controls.Add(new Label { Text = "REASON FOR REFUND", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.Gray, Location = new Point(20, cy), Size = new Size(300, 16) });
            cy += 18;
            var cboRefundReason = new ComboBox { Location = new Point(20, cy), Size = new Size(400, 34), Font = new Font("Segoe UI", 11), DropDownStyle = ComboBoxStyle.DropDownList };
            cboRefundReason.Items.AddRange(new object[] {
                "Parcel not delivered",
                "Parcel arrived damaged",
                "Wrong item delivered",
                "Parcel lost in transit",
                "Delivery significantly delayed",
                "Parcel delivered to wrong address",
                "Other"
            });
            cboRefundReason.SelectedIndex = 0;
            card.Controls.Add(cboRefundReason);
            cy += 50;

            // What happened text area
            card.Controls.Add(new Label { Text = "DESCRIBE WHAT HAPPENED", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.Gray, Location = new Point(20, cy), Size = new Size(500, 16) });
            cy += 18;
            var txtRefundNote = new TextBox
            {
                Location = new Point(20, cy),
                Size = new Size(860, 100),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Text = "Please describe what happened with your parcel in as much detail as possible...",
                ForeColor = Color.LightGray
            };
            txtRefundNote.Enter += (s, e) => { if (txtRefundNote.ForeColor == Color.LightGray) { txtRefundNote.Text = ""; txtRefundNote.ForeColor = Color.Black; } };
            txtRefundNote.Leave += (s, e) => { if (txtRefundNote.Text == "") { txtRefundNote.Text = "Please describe what happened with your parcel in as much detail as possible..."; txtRefundNote.ForeColor = Color.LightGray; } };
            card.Controls.Add(txtRefundNote);
            cy += 116;

            // Email for contact
            card.Controls.Add(new Label { Text = "YOUR EMAIL (must be @gmail.com) -- we will contact you here", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.Gray, Location = new Point(20, cy), Size = new Size(700, 16) });
            cy += 18;
            var txtRefundEmail = new TextBox { Location = new Point(20, cy), Size = new Size(500, 34), Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle };
            card.Controls.Add(txtRefundEmail);
            cy += 50;

            // Info note
            Panel infoNote = new Panel { Location = new Point(20, cy), Size = new Size(860, 46), BackColor = Color.FromArgb(235, 245, 255) };
            infoNote.Controls.Add(new Label { Text = "Refund requests are reviewed within 2-3 working days. Approved refunds are processed within 5-7 working days back to your original payment method.", Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(20, 60, 140), Location = new Point(12, 14), Size = new Size(836, 18), BackColor = Color.Transparent });
            card.Controls.Add(infoNote);
            cy += 60;

            // Submit button
            Button btnSubmitRefund = new Button
            {
                Text = "Submit Refund Request ->",
                Location = new Point(20, cy),
                Size = new Size(280, 50),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                BackColor = Red,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSubmitRefund.FlatAppearance.BorderSize = 0;
            btnSubmitRefund.MouseEnter += (s, e) => btnSubmitRefund.BackColor = DarkRed;
            btnSubmitRefund.MouseLeave += (s, e) => btnSubmitRefund.BackColor = Red;
            btnSubmitRefund.Click += (s, e) =>
            {
                // Validate tracking ID
                string trackId = txtRefundTrack.ForeColor == Color.LightGray ? "" : txtRefundTrack.Text.Trim();
                if (string.IsNullOrEmpty(trackId))
                {
                    MessageBox.Show("Please enter your tracking ID.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Validate note
                string note = txtRefundNote.ForeColor == Color.LightGray ? "" : txtRefundNote.Text.Trim();
                if (string.IsNullOrEmpty(note))
                {
                    MessageBox.Show("Please describe what happened with your parcel.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Validate email
                string email = txtRefundEmail.Text.Trim();
                if (!email.EndsWith("@gmail.com") || email.Length <= "@gmail.com".Length)
                {
                    MessageBox.Show("Please enter a valid Gmail address so we can contact you.", "Invalid Email", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Show confirmation message
                MessageBox.Show(
                    "Your refund request has been submitted successfully!\n\n" +
                    "Tracking ID: " + trackId + "\n" +
                    "Reason: " + cboRefundReason.SelectedItem.ToString() + "\n\n" +
                    "We will review your request and contact you at:\n" + email + "\n\n" +
                    "Please expect a response within 2-3 working days.",
                    "Refund Request Submitted",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                // Reset the form
                txtRefundTrack.Text = "e.g. PS-2026-00001";
                txtRefundTrack.ForeColor = Color.LightGray;
                txtRefundNote.Text = "Please describe what happened with your parcel in as much detail as possible...";
                txtRefundNote.ForeColor = Color.LightGray;
                txtRefundEmail.Text = "";
                cboRefundReason.SelectedIndex = 0;
            };
            card.Controls.Add(btnSubmitRefund);
        }

        // Section heading helper for send form
        private void SH(string text, int y)
        {
            pnlSend.Controls.Add(new Label { Text = text, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Red, Location = new Point(10, y), Size = new Size(800, 24) });
            pnlSend.Controls.Add(new Panel { Location = new Point(10, y + 26), Size = new Size(1240, 1), BackColor = Color.FromArgb(220, 180, 180) });
        }

        // Make mail/package type card
        private Panel MakeTypeCard(string type, string title, string desc, int x, int y, int w, Action onClick)
        {
            Panel card = new Panel { Location = new Point(x, y), Size = new Size(w, 108), BackColor = Color.White, Cursor = Cursors.Hand };
            card.Controls.Add(new Panel { Location = new Point(0, 0), Size = new Size(w, 5), BackColor = Color.FromArgb(220, 200, 200) });
            card.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Red, Location = new Point(16, 14), Size = new Size(w - 32, 28) });
            card.Controls.Add(new Label { Text = desc, Font = new Font("Segoe UI", 9), ForeColor = Color.Gray, Location = new Point(16, 46), Size = new Size(w - 32, 48) });
            card.Click += (s, e) => onClick();
            foreach (Control c in card.Controls) c.Click += (s, e) => onClick();
            return card;
        }

        // Select mail or package
        private void SelectType(bool mail)
        {
            isMail = mail;
            pnlMailCard.Controls[0].BackColor = mail ? Red : Color.FromArgb(220, 200, 200);
            pnlPkgCard.Controls[0].BackColor = !mail ? Red : Color.FromArgb(220, 200, 200);
            pnlMailSvc.Visible = mail;
            pnlPkgSvc.Visible = !mail;
            Recalc();
        }

        // Make service selection card
        private Panel SvcCard(Panel parent, string name, string sub, string price, string days, int x, Action onClick)
        {
            Panel card = new Panel { Location = new Point(x, 0), Size = new Size(300, 88), BackColor = Color.White, Cursor = Cursors.Hand };
            card.Controls.Add(new Panel { Location = new Point(0, 0), Size = new Size(300, 4), BackColor = Color.FromArgb(220, 200, 200), Name = "bar" });
            card.Controls.Add(new Label { Text = name, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Red, Location = new Point(12, 10), Size = new Size(276, 22) });
            card.Controls.Add(new Label { Text = sub, Font = new Font("Segoe UI", 9), ForeColor = Color.Gray, Location = new Point(12, 34), Size = new Size(276, 18) });
            card.Controls.Add(new Label { Text = price + "  --  " + days, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(50, 50, 50), Location = new Point(12, 56), Size = new Size(276, 18) });
            card.Click += (s, e) => onClick();
            foreach (Control c in card.Controls) c.Click += (s, e) => onClick();
            parent.Controls.Add(card);
            return card;
        }

        private void RefreshMailSvc()
        {
            Panel[] cards = { pnlMailFirst, pnlMailPriority, pnlMailExpress };
            for (int i = 0; i < cards.Length; i++)
                if (cards[i] != null)
                    cards[i].Controls["bar"].BackColor = i == mailSvcIndex ? Red : Color.FromArgb(220, 200, 200);
        }

        private void RefreshPkgSvc()
        {
            Panel[] cards = { pnlPkgGround, pnlPkgPriority, pnlPkgExpress, pnlPkgNextDay };
            for (int i = 0; i < cards.Length; i++)
                if (cards[i] != null)
                    cards[i].Controls["bar"].BackColor = i == pkgSvcIndex ? Red : Color.FromArgb(220, 200, 200);
        }

        private void RefreshTrackID()
        {
            int count = db.GetParcelCount() + 1;
            string tid = "PS-" + DateTime.Now.Year + "-" + count.ToString("D5");
            if (lblTrackGen != null)
                lblTrackGen.Text = "Tracking ID that will be assigned: " + tid;
        }

        // Recalculate price based on selections
        private void Recalc()
        {
            try
            {
                if (!double.TryParse(txtWeight?.Text ?? "1", out double w)) w = 1.0;
                string sz = cboSize?.SelectedItem?.ToString() ?? "Small";
                double sm = sz == "Small" ? 1.0 : sz == "Medium" ? 1.5 : 2.5;
                double svc, im = 1.0; int days;

                if (isMail)
                {
                    double[] prices = { 1.99, 3.99, 6.99 };
                    int[] d2 = { 4, 2, 1 };
                    svc = prices[mailSvcIndex]; days = d2[mailSvcIndex];
                }
                else
                {
                    double[] prices = { 2.99, 5.99, 9.99, 14.99 };
                    int[] d2 = { 4, 2, 1, 1 };
                    svc = prices[pkgSvcIndex]; days = d2[pkgSvcIndex];
                }

                if (rbInternational != null && rbInternational.Checked && cboCountry?.SelectedItem != null)
                {
                    var info = countries[cboCountry.SelectedItem.ToString()];
                    im = info.mult; days = info.days;
                }

                double price = Math.Round((svc + w * 1.2) * sm * im, 2);
                if (lblPrice != null) lblPrice.Text = "Estimated Price: GBP " + price.ToString("0.00");
                if (lblEstDays != null) lblEstDays.Text = "Estimated delivery: " + days + " working day(s)";
            }
            catch { }
        }

        // Submit parcel
        private void Submit_Click(object sender, EventArgs e)
        {
            // Validate email
            string email = txtSenderEmail?.Text.Trim() ?? "";
            if (!email.EndsWith("@gmail.com") || email.Length <= "@gmail.com".Length)
            { MessageBox.Show("Please enter a valid Gmail address.", "Invalid Email", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            if (string.IsNullOrWhiteSpace(txtRcvName?.Text))
            { MessageBox.Show("Please enter the receiver name.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            if (string.IsNullOrWhiteSpace(txtRcvAddr?.Text))
            { MessageBox.Show("Please enter the receiver address.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            if (!double.TryParse(txtWeight?.Text, out double w) || w <= 0)
            { MessageBox.Show("Please enter a valid weight in kg.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            // If paying online show bank details form first
            if (_parcelPayOnline)
            {
                bool paid = ShowParcelPaymentForm();
                if (!paid) return;
            }

            // Build parcel details
            string sz = cboSize.SelectedItem.ToString();
            bool isIntl = rbInternational.Checked;
            string country = isIntl ? cboCountry.SelectedItem?.ToString() : null;
            double sm = sz == "Small" ? 1.0 : sz == "Medium" ? 1.5 : 2.5;
            double svc; string svcName; int days;

            if (isMail)
            {
                string[] names = { "First Class", "Priority Mail", "Express Mail" };
                double[] prices = { 1.99, 3.99, 6.99 };
                int[] d2 = { 4, 2, 1 };
                svcName = names[mailSvcIndex];
                svc = prices[mailSvcIndex];
                days = d2[mailSvcIndex];
            }
            else
            {
                string[] names = { "Ground", "Priority", "Express", "Next Day" };
                double[] prices = { 2.99, 5.99, 9.99, 14.99 };
                int[] d2 = { 4, 2, 1, 1 };
                svcName = names[pkgSvcIndex];
                svc = prices[pkgSvcIndex];
                days = d2[pkgSvcIndex];
            }

            double im = 1.0;
            if (isIntl && country != null) { im = countries[country].mult; days = countries[country].days; }

            double price = Math.Round((svc + w * 1.2) * sm * im, 2);
            string tid = "PS-" + DateTime.Now.Year + "-" + (db.GetParcelCount() + 1).ToString("D5");
            string delID = "DEL-" + (db.GetParcelCount() + 1).ToString("D3");
            DateTime estDel = DateTime.Now.AddDays(days);
            string contents = string.IsNullOrEmpty(_parcelContents) ? "Not specified" : _parcelContents;
            string payment = _parcelPayOnline ? "Paid Online" : "Pay In Store";

            string result = db.AddParcel(tid, userID,
                isMail ? "Mail" : "Package",
                userName, email,
                txtRcvName.Text.Trim(), txtRcvAddr.Text.Trim(),
                w, sz, svcName, isIntl, country,
                "Pending", price, estDel);

            if (result.Contains("successfully"))
            {
                db.AutoAssignDelivery(tid, delID);

                // Build packing slip and instructions
                string msg =
                    "Parcel registered successfully!

" +
                    "Tracking ID:       " + tid + "
" +
                    "Type:              " + (isMail ? "Mail" : "Package") + "
" +
                    "Contents:          " + contents + "
" +
                    "Service:           " + svcName + "
" +
                    "Price:             GBP " + price.ToString("0.00") + "
" +
                    "Payment:           " + payment + "
" +
                    "Est. Delivery:     " + estDel.ToString("dd/MM/yyyy") + "

" +
                    "--- NEXT STEPS ---
" +
                    "1. Take your parcel to any PostalMS location
" +
                    "2. Show your Tracking ID: " + tid + "
" +
                    (_parcelPayOnline
                       ? "3. Staff will scan and process your parcel
"
                       : "3. Pay GBP " + price.ToString("0.00") + " by cash or card at the counter
") +
                    "4. You will receive status updates in My Parcels

" +
                    "Click 'I Have Dropped Off My Parcel' in My Parcels
once you have handed it to the store.";

                MessageBox.Show(msg, "Parcel Registered", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Reset form
                txtRcvName.Text = "";
                txtRcvAddr.Text = "";
                txtSenderEmail.Text = "";
                txtWeight.Text = "1.0";
                _parcelContents = "";
                Recalc();
                RefreshTrackID();
                Switch("list");
                RefreshParcels();
            }
            else
            {
                MessageBox.Show(result, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Payment form for parcel submission
        private bool ShowParcelPaymentForm()
        {
            Form f = new Form
            {
                Text = "PostalMS -- Secure Payment",
                Size = new Size(480, 530),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.FromArgb(245, 245, 245)
            };
            Panel hdr = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Red };
            hdr.Controls.Add(new Label { Text = "Secure Payment", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.White, Location = new Point(16, 14), Size = new Size(300, 28), BackColor = Color.Transparent });
            hdr.Controls.Add(new Label { Text = "Your card details are encrypted and secure", Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(255, 200, 200), Location = new Point(16, 38), Size = new Size(400, 18), BackColor = Color.Transparent });
            f.Controls.Add(hdr);

            Panel card = new Panel { Location = new Point(20, 76), Size = new Size(432, 350), BackColor = Color.White };
            f.Controls.Add(card);

            card.Controls.Add(new Label { Text = "CARDHOLDER NAME", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.Gray, Location = new Point(16, 16), Size = new Size(400, 16) });
            var txtName = new TextBox { Location = new Point(16, 34), Size = new Size(400, 34), Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle, Text = "Full name as it appears on card", ForeColor = Color.LightGray };
            txtName.Enter += (s, e2) => { if (txtName.ForeColor == Color.LightGray) { txtName.Text = ""; txtName.ForeColor = Color.Black; } };
            card.Controls.Add(txtName);

            card.Controls.Add(new Label { Text = "CARD NUMBER", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.Gray, Location = new Point(16, 84), Size = new Size(400, 16) });
            var txtNum = new TextBox { Location = new Point(16, 102), Size = new Size(400, 34), Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle, Text = "1234 5678 9012 3456", ForeColor = Color.LightGray };
            txtNum.Enter += (s, e2) => { if (txtNum.ForeColor == Color.LightGray) { txtNum.Text = ""; txtNum.ForeColor = Color.Black; } };
            card.Controls.Add(txtNum);

            card.Controls.Add(new Label { Text = "EXPIRY DATE", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.Gray, Location = new Point(16, 152), Size = new Size(190, 16) });
            var txtExp = new TextBox { Location = new Point(16, 170), Size = new Size(190, 34), Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle, Text = "MM/YY", ForeColor = Color.LightGray };
            txtExp.Enter += (s, e2) => { if (txtExp.ForeColor == Color.LightGray) { txtExp.Text = ""; txtExp.ForeColor = Color.Black; } };
            card.Controls.Add(txtExp);

            card.Controls.Add(new Label { Text = "CVV", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.Gray, Location = new Point(220, 152), Size = new Size(190, 16) });
            var txtCVV = new TextBox { Location = new Point(220, 170), Size = new Size(190, 34), Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle, Text = "CVV", ForeColor = Color.LightGray, PasswordChar = '*' };
            txtCVV.Enter += (s, e2) => { if (txtCVV.ForeColor == Color.LightGray) { txtCVV.Text = ""; txtCVV.ForeColor = Color.Black; } };
            card.Controls.Add(txtCVV);

            card.Controls.Add(new Label { Text = "BILLING POSTCODE", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.Gray, Location = new Point(16, 220), Size = new Size(300, 16) });
            var txtPost = new TextBox { Location = new Point(16, 238), Size = new Size(200, 34), Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle, Text = "e.g. NW4 4BT", ForeColor = Color.LightGray };
            txtPost.Enter += (s, e2) => { if (txtPost.ForeColor == Color.LightGray) { txtPost.Text = ""; txtPost.ForeColor = Color.Black; } };
            card.Controls.Add(txtPost);

            Panel sec = new Panel { Location = new Point(16, 288), Size = new Size(400, 36), BackColor = Color.FromArgb(235, 245, 255) };
            sec.Controls.Add(new Label { Text = "Secure encrypted payment. We do not store your card details.", Font = new Font("Segoe UI", 8), ForeColor = Color.FromArgb(20, 60, 140), Location = new Point(10, 10), Size = new Size(380, 16), BackColor = Color.Transparent });
            card.Controls.Add(sec);

            bool paid = false;
            // Calculate price for button label
            double bw = 1.0; double.TryParse(txtWeight?.Text ?? "1", out bw);
            string bsz = cboSize?.SelectedItem?.ToString() ?? "Small";
            double bsm = bsz == "Small" ? 1.0 : bsz == "Medium" ? 1.5 : 2.5;
            double bsvc = isMail ? new double[] { 1.99, 3.99, 6.99 }[mailSvcIndex] : new double[] { 2.99, 5.99, 9.99, 14.99 }[pkgSvcIndex];
            double bim = 1.0;
            if (rbInternational.Checked && cboCountry.SelectedItem != null) bim = countries[cboCountry.SelectedItem.ToString()].mult;
            double bprice = Math.Round((bsvc + bw * 1.2) * bsm * bim, 2);

            Button btnPay = new Button { Text = "Pay Now  GBP " + bprice.ToString("0.00"), Location = new Point(20, 442), Size = new Size(432, 50), Font = new Font("Segoe UI", 12, FontStyle.Bold), BackColor = Red, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnPay.FlatAppearance.BorderSize = 0;
            btnPay.Click += (s, e2) =>
            {
                string cn = txtName.ForeColor == Color.LightGray ? "" : txtName.Text.Trim();
                string cc = txtNum.ForeColor == Color.LightGray ? "" : txtNum.Text.Trim();
                string ce = txtExp.ForeColor == Color.LightGray ? "" : txtExp.Text.Trim();
                string cv = txtCVV.ForeColor == Color.LightGray ? "" : txtCVV.Text.Trim();
                string cp = txtPost.ForeColor == Color.LightGray ? "" : txtPost.Text.Trim();
                if (string.IsNullOrEmpty(cn)) { MessageBox.Show("Please enter the cardholder name.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                if (string.IsNullOrEmpty(cc)) { MessageBox.Show("Please enter your card number.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                if (string.IsNullOrEmpty(ce)) { MessageBox.Show("Please enter the expiry date.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                if (string.IsNullOrEmpty(cv)) { MessageBox.Show("Please enter the CVV.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                if (string.IsNullOrEmpty(cp)) { MessageBox.Show("Please enter your billing postcode.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                paid = true;
                f.Close();
            };
            f.Controls.Add(btnPay);

            Button btnCancel = new Button { Text = "Cancel", Location = new Point(358, 452), Size = new Size(94, 30), Font = new Font("Segoe UI", 9), BackColor = Color.FromArgb(240, 240, 240), ForeColor = Red, FlatStyle = FlatStyle.Flat };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e2) => f.Close();
            f.Controls.Add(btnCancel);

            f.ShowDialog();
            return paid;
        }
    }
}