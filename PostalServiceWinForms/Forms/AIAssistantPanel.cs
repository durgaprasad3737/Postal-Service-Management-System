// AIAssistantPanel.cs
// PostalMS - Postal Service Management System
// CST2550 Coursework - Middlesex University
//
// References:
// Russell, S. and Norvig, P. (2021) Artificial Intelligence: A Modern Approach.
//   4th edn. Hoboken, NJ: Pearson.
//   -- Rule-based AI agent design (Chapter 2)
//
// Offline AI assistant for parcel support queries.
// Handles tracking, pricing, delivery estimates, stamps, drop off locations,
// find us queries and FAQs entirely offline using keyword matching.
// No internet connection or external API required.

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;

namespace PostalServiceWinForms.Forms
{
    public class AIAssistantPanel : Form
    {
        private Panel pnlMessages;
        private TextBox txtInput;
        private Button btnSend;
        private Label lblTyping;
        private DatabaseHelper db;
        private string userID, userName;
        private int _msgY = 10;
        private Color Red = Color.FromArgb(180, 30, 30);
        private Color DarkRed = Color.FromArgb(140, 20, 20);
        private Color Bg = Color.FromArgb(255, 248, 248);

        public AIAssistantPanel(string uid, string name, DatabaseHelper dbHelper)
        {
            userID = uid; userName = name; db = dbHelper;
            this.Text = "PostalMS AI Assistant";
            this.Size = new Size(520, 700);
            this.MinimumSize = new Size(480, 500);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.Manual;
            this.BackColor = Bg;
            Rectangle scr = Screen.PrimaryScreen.WorkingArea;
            this.Location = new Point(scr.Right - 540, scr.Bottom - 720);
            BuildUI();

            // Welcome message explaining all capabilities
            AddMsg("ai",
                "Hi " + userName.Split(' ')[0] + "! I am your PostalMS assistant.\n\n" +
                "I can help you with:\n" +
                "  track PS-2026-00001\n" +
                "  how much for 2kg express?\n" +
                "  when will my parcel arrive?\n" +
                "  show my parcels\n" +
                "  where to drop off my parcel\n" +
                "  where to buy stamps\n" +
                "  where to find us\n" +
                "  how do I get a refund?\n\n" +
                "Type below and press Enter.");
        }

        private void BuildUI()
        {
            // Input bar at the bottom
            Panel inputBar = new Panel { Dock = DockStyle.Bottom, Height = 56, BackColor = Color.White };
            inputBar.Paint += (s, e) => e.Graphics.DrawLine(new Pen(Color.FromArgb(215, 215, 215)), 0, 0, inputBar.Width, 0);
            this.Controls.Add(inputBar);

            txtInput = new TextBox
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
                Location = new Point(10, 12),
                Size = new Size(this.ClientSize.Width - 82, 32),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                ForeColor = Color.Gray,
                Text = "Ask me anything..."
            };
            txtInput.Enter += (s, e) => { if (txtInput.ForeColor == Color.Gray) { txtInput.Text = ""; txtInput.ForeColor = Color.Black; } };
            txtInput.Leave += (s, e) => { if (txtInput.Text == "") { txtInput.Text = "Ask me anything..."; txtInput.ForeColor = Color.Gray; } };
            txtInput.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; Send(txtInput.Text.Trim()); } };

            btnSend = new Button
            {
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
                Size = new Size(58, 36),
                Location = new Point(this.ClientSize.Width - 70, 10),
                Text = "Send",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor = Red,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSend.FlatAppearance.BorderSize = 0;
            btnSend.Click += (s, e) => Send(txtInput.Text.Trim());
            inputBar.Controls.AddRange(new Control[] { txtInput, btnSend });

            this.Resize += (s, e) =>
            {
                txtInput.Size = new Size(inputBar.Width - 82, 32);
                btnSend.Location = new Point(inputBar.Width - 70, 10);
            };

            // Typing indicator
            lblTyping = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 22,
                Text = "   PostalMS AI is thinking...",
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = Color.Gray,
                BackColor = Bg,
                Visible = false
            };
            this.Controls.Add(lblTyping);

            // Messages panel
            pnlMessages = new Panel { Dock = DockStyle.Fill, BackColor = Bg, AutoScroll = true };
            this.Controls.Add(pnlMessages);

            // Quick buttons
            Panel qb = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = Color.FromArgb(255, 235, 235) };
            this.Controls.Add(qb);
            string[] hints = { "My parcels", "Track parcel", "Drop off", "Buy stamps", "Find us" };
            int sx = 8;
            foreach (string hint in hints)
            {
                Button hb = new Button { Text = hint, Location = new Point(sx, 8), Size = new Size(92, 28), Font = new Font("Segoe UI", 8), FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = Red, Cursor = Cursors.Hand };
                hb.FlatAppearance.BorderColor = Color.FromArgb(230, 160, 160);
                string h = hint;
                hb.Click += (s, e) => Send(h);
                qb.Controls.Add(hb);
                sx += 98;
            }

            // Header
            Panel hdr = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = Red };
            hdr.Paint += (s, e) =>
            {
                using (var b = new System.Drawing.Drawing2D.LinearGradientBrush(
                    hdr.ClientRectangle, Color.FromArgb(155, 18, 18), Color.FromArgb(210, 60, 40), 0f))
                    e.Graphics.FillRectangle(b, hdr.ClientRectangle);
            };
            hdr.Controls.Add(new Label { Text = "PostalMS AI Assistant", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.White, Location = new Point(16, 10), Size = new Size(400, 26), BackColor = Color.Transparent });
            hdr.Controls.Add(new Label { Text = "Offline  --  No internet required", Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(255, 200, 200), Location = new Point(16, 38), Size = new Size(400, 18), BackColor = Color.Transparent });
            this.Controls.Add(hdr);
        }

        // Add a message bubble to the chat
        private void AddMsg(string role, string text)
        {
            bool isUser = role == "user";
            int maxW = Math.Min(370, pnlMessages.ClientSize.Width - 40);

            // Role label
            Label roleLbl = new Label
            {
                Text = isUser ? userName.Split(' ')[0] : "PostalMS AI",
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = Color.Gray,
                Location = new Point(isUser ? pnlMessages.Width - maxW - 18 : 12, _msgY),
                Size = new Size(150, 14)
            };
            _msgY += 16;

            // Message content label
            Label lbl = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 10),
                ForeColor = isUser ? Color.White : Color.FromArgb(40, 10, 10),
                MaximumSize = new Size(maxW - 24, 0),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            // Bubble panel
            Panel bubble = new Panel { BackColor = isUser ? Red : Color.White };
            bubble.Controls.Add(lbl);
            lbl.Location = new Point(12, 10);
            lbl.CreateControl();
            bubble.Size = new Size(Math.Min(lbl.Width + 28, maxW), lbl.Height + 32);
            bubble.Location = new Point(isUser ? pnlMessages.Width - bubble.Width - 14 : 12, _msgY);

            pnlMessages.Controls.AddRange(new Control[] { roleLbl, bubble });
            _msgY += bubble.Height + 20;
            pnlMessages.AutoScrollPosition = new Point(0, _msgY);
        }

        // Process and send a message
        private void Send(string input)
        {
            if (string.IsNullOrEmpty(input) || txtInput.ForeColor == Color.Gray) return;
            txtInput.Text = "";
            txtInput.ForeColor = Color.Black;
            AddMsg("user", input);
            lblTyping.Visible = true;
            btnSend.Enabled = false;

            // Simulate a short thinking delay
            var timer = new System.Windows.Forms.Timer { Interval = 380 };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                AddMsg("ai", AI(input.ToLower().Trim()));
                lblTyping.Visible = false;
                btnSend.Enabled = true;
            };
            timer.Start();
        }

        // Main AI response logic using keyword matching
        private string AI(string input)
        {
            // Tracking queries
            if (input.Contains("track") || input.Contains("where is") || input.Contains("ps-") ||
               (input.Contains("status") && input.Contains("parcel")))
            {
                string tid = TID(input);
                return string.IsNullOrEmpty(tid)
                    ? "Please provide a tracking ID.\nExample: 'track PS-2026-00001'"
                    : Track(tid);
            }

            // Price queries
            if (input.Contains("price") || input.Contains("cost") || input.Contains("how much") || input.Contains("charge"))
                return Price(input);

            // Arrival prediction queries
            if (input.Contains("when") || input.Contains("arrive") || input.Contains("delivery date"))
            {
                string tid = TID(input);
                return string.IsNullOrEmpty(tid)
                    ? "Please provide a tracking ID.\nExample: 'when will PS-2026-00001 arrive?'"
                    : Predict(tid);
            }

            // My parcels
            if (input.Contains("my parcel") || input == "my parcels" || input.Contains("show my"))
                return MyParcels();

            // Drop off location queries
            if (input.Contains("drop off") || input.Contains("drop") || input.Contains("where to send") || input.Contains("post it"))
                return DropOffInfo();

            // Stamps queries
            if (input.Contains("stamp") || input.Contains("buy stamp") || input.Contains("postage"))
                return StampsInfo();

            // Find us queries
            if (input.Contains("find us") || input.Contains("office") || input.Contains("visit") ||
                input.Contains("location") || input.Contains("address") || input.Contains("where are you"))
                return FindUsInfo();

            // Refund queries
            if (input.Contains("refund"))
                return "To request a refund:\n\n1. Go to Parcels page\n2. Click on your parcel row\n3. Click Request Refund button\n4. Describe the issue and submit\n\nRefunds are reviewed within 3 to 5 working days.\nEligible for failed or damaged deliveries.";

            // International queries
            if (input.Contains("international") || input.Contains("abroad") || input.Contains("overseas"))
                return "International Shipping Zones:\n\nZone 1 -- Europe (x1.8):  5-7 days\nZone 2 -- N.Europe (x2.2):  6-8 days\nZone 3 -- USA/UAE (x3.0):  8-12 days\nZone 4 -- World (x3.5-3.8):  12-16 days\n\nGo to Parcels -> Send -> Select International\nthen choose your destination country.";

            // Register or account queries
            if (input.Contains("register") || input.Contains("sign up") || input.Contains("create account"))
                return "To create a PostalMS account:\n\n1. Close this window\n2. Click Register here on the login screen\n3. Fill in your details\n4. Use a valid Gmail address (@gmail.com)\n5. Click Create Account\n\nRegistration is completely free.";

            // Help
            if (input.Contains("help") || input == "help")
                return "I can help you with:\n\n  track PS-2026-00001\n  how much for 2kg express?\n  when will my parcel arrive?\n  show my parcels\n  where to drop off my parcel\n  where to buy stamps\n  where to find us\n  how do I get a refund?\n  international shipping info";

            // Greetings
            if (input.Contains("hello") || input.Contains("hi") || input.Contains("hey"))
                return "Hello " + userName.Split(' ')[0] + "! How can I help you today?";

            if (input.Contains("thank")) return "You are welcome! Is there anything else I can help you with?";
            if (input.Contains("bye")) return "Goodbye " + userName.Split(' ')[0] + "! Have a great day.";

            // Default response
            return "I am not sure about that.\n\nTry asking:\n  track PS-2026-00001\n  how much for 2kg express?\n  where to drop off my parcel\n  where to buy stamps\n  help";
        }

        // Track a parcel by tracking ID using the Hash Table first then database
        private string Track(string tid)
        {
            try
            {
                // Check Hash Table first for O(1) lookup
                object cached = db.ParcelHashTable.Search(tid);
                string source = cached != null ? "[ Hash Table O(1) lookup ]" : "[ Database query ]";

                DataTable dt = db.SearchParcel(tid);
                if (dt.Rows.Count == 0) return "No parcel found with tracking ID: " + tid + "\nPlease check the ID and try again.";

                DataRow r = dt.Rows[0];
                string s = r["Status"].ToString();
                string em = s == "Delivered" ? "Delivered" : s == "Failed" ? "Failed" : s == "In Transit" ? "In Transit" : s == "Out for Delivery" ? "Out for Delivery" : "Pending";

                return em + "\n\nTracking ID:  " + r["TrackingID"] +
                       "\nType:         " + r["ParcelType"] +
                       "\nSender:       " + r["SenderName"] +
                       "\nReceiver:     " + r["ReceiverName"] +
                       "\nStatus:       " + s +
                       "\nService:      " + r["ServiceType"] +
                       "\nPrice:        GBP " + r["Price"] +
                       "\nDate Sent:    " + DateTime.Parse(r["DateSent"].ToString()).ToString("dd/MM/yyyy") +
                       "\n\n" + source;
            }
            catch (Exception ex) { return "Error tracking parcel: " + ex.Message; }
        }

        // Calculate estimated price based on input keywords
        private string Price(string input)
        {
            // Extract weight from input
            double w = 1.0;
            foreach (var word in input.Split(' '))
                if (double.TryParse(word.Replace("kg", ""), out double wv)) { w = wv; break; }

            // Determine size from weight or keywords
            string sz = input.Contains("large") || w > 5 ? "Large" :
                        input.Contains("medium") || w > 1 ? "Medium" : "Small";

            // Determine service type
            string tp = input.Contains("next day") ? "Next Day" :
                        input.Contains("express") ? "Express" :
                        input.Contains("priority") ? "Priority" : "Standard";

            // Size multiplier
            double sm = sz == "Small" ? 1.0 : sz == "Medium" ? 1.5 : 2.5;

            // Service price
            double svcP = tp == "Next Day" ? 14.99 : tp == "Express" ? 9.99 : tp == "Priority" ? 5.99 : 2.99;

            // International multiplier
            double im = 1.0; string dest = "UK Domestic";
            if (input.Contains("france") || input.Contains("germany") || input.Contains("spain") || input.Contains("europe"))
            { im = 1.8; dest = "Europe (Zone 1)"; }
            else if (input.Contains("usa") || input.Contains("america") || input.Contains("canada"))
            { im = 3.0; dest = "North America (Zone 3)"; }
            else if (input.Contains("australia") || input.Contains("japan") || input.Contains("china"))
            { im = 3.8; dest = "Asia Pacific (Zone 4)"; }

            // Calculate price
            double price = Math.Round((svcP + w * 1.2) * sm * im, 2);
            int days = tp == "Standard" ? 5 : tp == "Priority" ? 2 : tp == "Express" ? 1 : 1;
            if (im > 1) days = im < 2 ? 7 : im < 3 ? 10 : 14;

            return "Price Estimate\n\nWeight:   " + w + "kg\nSize:     " + sz +
                   "\nService:  " + tp + "\nTo:       " + dest +
                   "\n\nEstimated Price:  GBP " + price +
                   "\nDelivery Time:    " + days + " working day(s)" +
                   "\n\nGo to Parcels to send your item!";
        }

        // Predict delivery date based on parcel status
        private string Predict(string tid)
        {
            try
            {
                DataTable dt = db.SearchParcel(tid);
                if (dt.Rows.Count == 0) return "No parcel found with tracking ID: " + tid;

                DataRow r = dt.Rows[0];
                string s = r["Status"].ToString();
                string tp = r["ServiceType"].ToString();

                string pred; int d;
                switch (s)
                {
                    case "Pending": d = tp == "Next Day" ? 2 : tp == "Express" ? 3 : 6; pred = "Not dispatched yet. Estimated " + d + " days."; break;
                    case "In Transit": d = tp == "Express" ? 1 : 2; pred = "On its way! Estimated " + d + " more day(s)."; break;
                    case "Out for Delivery": d = 0; pred = "Out for delivery TODAY!"; break;
                    case "Delivered": d = 0; pred = "Already delivered successfully!"; break;
                    case "Failed": d = 2; pred = "Delivery failed. Re-attempt in 1-2 days."; break;
                    default: d = -1; pred = "Please contact support."; break;
                }

                string eta = d > 0 ? DateTime.Now.AddDays(d).ToString("dddd, dd MMM yyyy") : d == 0 ? "Today" : "N/A";

                return "Delivery Prediction\n\nParcel ID:  " + tid +
                       "\nStatus:     " + s +
                       "\nService:    " + tp +
                       "\n\n" + pred +
                       "\n\nEstimated Arrival:  " + eta;
            }
            catch (Exception ex) { return "Error: " + ex.Message; }
        }

        // Show user's recent parcels
        private string MyParcels()
        {
            try
            {
                DataTable dt = db.GetParcelsByCustomer(userID);
                if (dt.Rows.Count == 0)
                    return "No parcels yet " + userName.Split(' ')[0] + "!\nGo to Parcels -> Send Mail or Package to get started.";

                string res = "Your Parcels (" + dt.Rows.Count + " total)\n\n";
                int shown = 0;
                foreach (DataRow r in dt.Rows)
                {
                    if (shown >= 6) { res += "...and " + (dt.Rows.Count - 6) + " more.\nSee the Parcels page for full list."; break; }
                    string s2 = r["Status"].ToString();
                    res += r["TrackingID"] + "  ->  " + r["ReceiverName"] + "  [" + s2 + "]\n";
                    shown++;
                }
                return res;
            }
            catch { return "Could not load your parcels. Please try again."; }
        }

        // Drop off location information
        private string DropOffInfo()
        {
            return "Drop Off Locations\n\n" +
                   "You can drop off your parcel at any of these locations:\n\n" +
                   "1. PostalMS Hendon Campus Centre\n" +
                   "   The Burroughs, Hendon, London, NW4 4BT\n" +
                   "   Mon-Fri 9am-5:30pm  |  Sat 9am-1pm\n\n" +
                   "2. PostalMS Cat Hill Centre\n" +
                   "   Cat Hill, East Barnet, London, EN4 8HT\n" +
                   "   Mon-Fri 9am-5:30pm  |  Sat 9am-1pm\n\n" +
                   "3. PostalMS Archway Drop-Off Point\n" +
                   "   2 Junction Road, Archway, London, N19 5QU\n" +
                   "   Mon-Fri 9am-6pm  |  Sat 10am-2pm\n\n" +
                   "4. PostalMS Wembley Centre\n" +
                   "   Engineers Way, Wembley, London, HA9 0ED\n" +
                   "   Mon-Fri 9am-5:30pm  |  Sat 9am-1pm\n\n" +
                   "Bring your parcel securely packaged.\nStaff will weigh and label it for you.";
        }

        // Stamps information
        private string StampsInfo()
        {
            return "Buying Stamps\n\n" +
                   "Stamp Prices:\n" +
                   "  First Class:       GBP 1.10\n" +
                   "  Second Class:      GBP 0.75\n" +
                   "  Large Letter:      GBP 1.55\n" +
                   "  International:     GBP 1.85\n\n" +
                   "Where to Buy:\n\n" +
                   "1. PostalMS Hendon Campus Centre\n" +
                   "   The Burroughs, Hendon NW4 4BT\n\n" +
                   "2. PostalMS Cat Hill Centre\n" +
                   "   Cat Hill, East Barnet EN4 8HT\n\n" +
                   "3. WH Smith -- Brent Cross Shopping Centre\n\n" +
                   "4. Tesco Express -- Golders Green Road\n\n" +
                   "Click Stamps in the navigation bar\nfor the full list of locations.";
        }

        // Find us / office location information
        private string FindUsInfo()
        {
            return "Find PostalMS\n\n" +
                   "Main Service Centres:\n\n" +
                   "1. Hendon Campus Centre\n" +
                   "   The Burroughs, Hendon, London NW4 4BT\n" +
                   "   Tel: 020 8411 5000\n" +
                   "   Tube: Hendon Central (Northern Line)\n\n" +
                   "2. Cat Hill Service Centre\n" +
                   "   Cat Hill, East Barnet, London EN4 8HT\n" +
                   "   Tel: 020 8411 6000\n" +
                   "   Bus: 84, 299, 384\n\n" +
                   "3. Archway Drop-Off Point\n" +
                   "   2 Junction Road, Archway, London N19 5QU\n" +
                   "   Tube: Archway (Northern Line)\n\n" +
                   "4. Wembley Service Centre\n" +
                   "   Engineers Way, Wembley HA9 0ED\n" +
                   "   Tube: Wembley Park\n\n" +
                   "Click Find Us in the navigation bar\nfor full details and opening hours.";
        }

        // Extract tracking ID from user input
        private string TID(string input)
        {
            string up = input.ToUpper();
            int idx = up.IndexOf("PS-");
            if (idx < 0) return "";
            string sub = up.Substring(idx);
            int end = sub.IndexOf(' ');
            return end > 0 ? sub.Substring(0, end).Trim() : sub.Trim();
        }
    }
}