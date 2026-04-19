// HomeView.cs
// PostalMS - Postal Service Management System
// CST2550 Coursework - Middlesex University
//
// Home dashboard shown after login.
// Displays welcome message, quick stats and navigation shortcuts.

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;

namespace PostalServiceWinForms.Forms
{
    public class HomeView : UserControl
    {
        private MainForm parent;
        private Color Red = Color.FromArgb(180, 30, 30);
        private Color DarkRed = Color.FromArgb(140, 20, 20);

        private string userID;
        private DatabaseHelper db;

        public HomeView(MainForm p, string uid = "", DatabaseHelper dbHelper = null)
        {
            parent = p;
            userID = uid;
            db = dbHelper;
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(245, 245, 245);
            this.AutoScroll = true;
            Build();
        }

        private void Build()
        {
            int y = 0;

            // -- Hero banner
            Panel hero = new Panel { Location = new Point(0, y), Size = new Size(1400, 220), BackColor = Red };
            hero.Paint += (s, e) =>
            {
                using (var b = new System.Drawing.Drawing2D.LinearGradientBrush(hero.ClientRectangle,
                    Color.FromArgb(155, 18, 18), Color.FromArgb(215, 65, 40),
                    System.Drawing.Drawing2D.LinearGradientMode.ForwardDiagonal))
                    e.Graphics.FillRectangle(b, hero.ClientRectangle);
            };
            this.Controls.Add(hero);

            hero.Controls.Add(new Label { Text = "", Font = new Font("Segoe UI", 52), ForeColor = Color.White, Location = new Point(40, 45), Size = new Size(100, 85), BackColor = Color.Transparent });
            hero.Controls.Add(new Label { Text = "PostalMS", Font = new Font("Segoe UI", 36, FontStyle.Bold), ForeColor = Color.White, Location = new Point(148, 42), Size = new Size(500, 58), BackColor = Color.Transparent });
            hero.Controls.Add(new Label { Text = "Your Trusted Postal Service Management System", Font = new Font("Segoe UI", 14), ForeColor = Color.FromArgb(255, 210, 210), Location = new Point(148, 102), Size = new Size(700, 28), BackColor = Color.Transparent });
            hero.Controls.Add(new Label { Text = "Send mail and packages locally or internationally -- with real-time tracking, AI assistance and full refund protection.", Font = new Font("Segoe UI", 11), ForeColor = Color.FromArgb(255, 225, 225), Location = new Point(148, 134), Size = new Size(800, 48), BackColor = Color.Transparent });

            // CTA buttons
            Button btnStart = new Button { Text = " Get Started -- Send a Parcel", Location = new Point(148, 178), Size = new Size(270, 38), Font = new Font("Segoe UI", 10, FontStyle.Bold), BackColor = Color.White, ForeColor = Red, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnStart.FlatAppearance.BorderSize = 0;
            btnStart.Click += (s, e) => parent.ShowParcels();
            hero.Controls.Add(btnStart);

            Button btnLearn = new Button { Text = " How It Works", Location = new Point(428, 178), Size = new Size(170, 38), Font = new Font("Segoe UI", 10), BackColor = Color.Transparent, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnLearn.FlatAppearance.BorderColor = Color.FromArgb(220, 150, 150); btnLearn.FlatAppearance.BorderSize = 1;
            btnLearn.Click += (s, e) => parent.ShowInfo();
            hero.Controls.Add(btnLearn);
            y += 230;

            // Parcel status notification banner
            try
            {
                if (db != null && !string.IsNullOrEmpty(userID))
                {
                    var dt = db.GetParcelsByCustomer(userID);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        // Find most recent parcel that is not delivered
                        string notifTid = "--"; string notifStatus = "--";
                        foreach (System.Data.DataRow dr in dt.Rows)
                        {
                            string s2 = dr["Status"]?.ToString() ?? "";
                            if (s2 != "Delivered" && s2 != "Failed")
                            {
                                notifTid = dr["TrackingID"]?.ToString() ?? "--";
                                notifStatus = s2;
                                break;
                            }
                        }

                        if (notifTid != "--")
                        {
                            Color notifColor = notifStatus == "In Transit" ? Color.FromArgb(235, 245, 255) :
                                               notifStatus == "Out for Delivery" ? Color.FromArgb(235, 255, 235) :
                                               Color.FromArgb(255, 245, 220);
                            Color notifText = notifStatus == "In Transit" ? Color.FromArgb(20, 60, 140) :
                                               notifStatus == "Out for Delivery" ? Color.FromArgb(20, 100, 40) :
                                               Color.FromArgb(140, 80, 0);

                            Panel notif = new Panel { Location = new Point(20, y), Size = new Size(900, 52), BackColor = notifColor };
                            notif.Controls.Add(new Label { Text = "Parcel Update:  " + notifTid + "  is currently  " + notifStatus + ".  Go to My Parcels to view full details or confirm drop off.", Font = new Font("Segoe UI", 10), ForeColor = notifText, Location = new Point(14, 16), Size = new Size(870, 20), BackColor = Color.Transparent });
                            this.Controls.Add(notif);
                            y += 62;
                        }
                    }
                }
            }
            catch { }


            // -- Trust stats bar
            Panel trust = new Panel { Location = new Point(0, y), Size = new Size(1400, 78), BackColor = Color.White };
            this.Controls.Add(trust);
            TrustStat(trust, "50,000+", "Parcels Delivered", 60);
            TrustStat(trust, "20+", "Countries Covered", 280);
            TrustStat(trust, "99.2%", "Delivery Success Rate", 480);
            TrustStat(trust, "24/7", "AI Support Available", 680);
            TrustStat(trust, "100%", "Refund Protection", 880);
            y += 88;

            // -- Features
            SHead("Why Choose PostalMS?", y); y += 44;
            Panel feats = new Panel { Location = new Point(0, y), Size = new Size(1200, 210), BackColor = Color.Transparent };
            this.Controls.Add(feats);
            FCard(feats, "", "Mail & Packages", "Send letters, documents and packages.\nStandard, Express or Next Day.", 0);
            FCard(feats, "", "International", "Ship to 20+ countries across 4 zones.\nTransparent costs before you commit.", 305);
            FCard(feats, "", "Real-Time Tracking", "Track any parcel with a unique ID.\nSee the full delivery journey live.", 610);
            FCard(feats, "", "Refund Protection", "Damaged or lost parcel?\nRequest a refund directly from the app.", 915);
            y += 225;

            Panel feats2 = new Panel { Location = new Point(0, y), Size = new Size(1200, 210), BackColor = Color.Transparent };
            this.Controls.Add(feats2);
            FCard(feats2, "", "AI Assistant", "Track parcels, estimate prices and\npredict delivery -- all offline.", 0);
            FCard(feats2, "", "Secure Accounts", "Your data stays private. Each user\nonly sees their own parcels.", 305);
            FCard(feats2, "", "Parcel Dashboard", "See all your parcel stats and history\nat a glance in one clean view.", 610);
            FCard(feats2, "", "Easy to Use", "Simple, clean interface designed\nfor everyone.", 915);
            y += 225;

            // -- How it works
            SHead("How It Works -- 4 Simple Steps", y); y += 44;
            Panel steps = new Panel { Location = new Point(0, y), Size = new Size(1200, 155), BackColor = Color.Transparent };
            this.Controls.Add(steps);
            Step(steps, "1", "Register & Login", "Create your free account and sign in securely.", 0);
            Step(steps, "2", "Choose Mail or Package", "Select what you're sending and fill in the details.", 305);
            Step(steps, "3", "Submit & Track", "Submit your order -- a tracking ID is generated automatically.", 610);
            Step(steps, "4", "Delivered!", "Track in real time. Request a refund if anything goes wrong.", 915);
            y += 170;

            // -- Testimonials
            SHead("Trusted by Our Customers", y); y += 44;
            Panel quotes = new Panel { Location = new Point(0, y), Size = new Size(1200, 130), BackColor = Color.Transparent };
            this.Controls.Add(quotes);
            Quote(quotes, "\"PostalMS made sending my first international parcel so easy. The tracking is amazing!\"", "-- Sarah M., London", 0);
            Quote(quotes, "\"The AI assistant answered all my questions about delivery times instantly. Brilliant service.\"", "-- James K., Manchester", 400);
            Quote(quotes, "\"Had a damaged package -- the refund process was quick and hassle-free. Highly recommend!\"", "-- Alice B., Birmingham", 800);
            y += 145;

            // -- Bottom CTA
            Panel cta = new Panel { Location = new Point(0, y), Size = new Size(1400, 90), BackColor = Red };
            cta.Paint += (s, e) => { using (var b = new System.Drawing.Drawing2D.LinearGradientBrush(cta.ClientRectangle, Color.FromArgb(155, 18, 18), Color.FromArgb(210, 55, 35), 0f)) e.Graphics.FillRectangle(b, cta.ClientRectangle); };
            this.Controls.Add(cta);
            cta.Controls.Add(new Label { Text = "Ready to send your first parcel?", Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Color.White, Location = new Point(40, 26), Size = new Size(500, 34), BackColor = Color.Transparent });
            Button ctaBtn = new Button { Text = " Send Now ->", Location = new Point(560, 22), Size = new Size(200, 42), Font = new Font("Segoe UI", 12, FontStyle.Bold), BackColor = Color.White, ForeColor = Red, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            ctaBtn.FlatAppearance.BorderSize = 0;
            ctaBtn.Click += (s, e) => parent.ShowParcels();
            cta.Controls.Add(ctaBtn);
        }

        private void TrustStat(Panel p, string val, string lbl, int x)
        {
            p.Controls.Add(new Label { Text = val, Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = Red, Location = new Point(x, 8), Size = new Size(180, 30) });
            p.Controls.Add(new Label { Text = lbl, Font = new Font("Segoe UI", 9), ForeColor = Color.Gray, Location = new Point(x, 40), Size = new Size(180, 20) });
        }

        private void SHead(string t, int y)
        {
            this.Controls.Add(new Label { Text = t, Font = new Font("Segoe UI", 17, FontStyle.Bold), ForeColor = Red, Location = new Point(0, y), Size = new Size(800, 34) });
            this.Controls.Add(new Panel { Location = new Point(0, y + 36), Size = new Size(1200, 2), BackColor = Color.FromArgb(230, 180, 180) });
        }

        private void FCard(Panel p, string icon, string title, string desc, int x)
        {
            Panel c = new Panel { Location = new Point(x, 0), Size = new Size(290, 205), BackColor = Color.White };
            c.Controls.Add(new Panel { Location = new Point(0, 0), Size = new Size(290, 5), BackColor = Red });
            c.Controls.Add(new Label { Text = icon, Font = new Font("Segoe UI", 26), Location = new Point(16, 18), Size = new Size(52, 44), BackColor = Color.Transparent });
            c.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Red, Location = new Point(16, 66), Size = new Size(258, 26) });
            c.Controls.Add(new Label { Text = desc, Font = new Font("Segoe UI", 9.5f), ForeColor = Color.FromArgb(60, 70, 90), Location = new Point(16, 96), Size = new Size(258, 80) });
            p.Controls.Add(c);
        }

        private void Step(Panel p, string num, string title, string desc, int x)
        {
            Panel c = new Panel { Location = new Point(x, 0), Size = new Size(290, 145), BackColor = Color.White };
            Panel circ = new Panel { Location = new Point(16, 16), Size = new Size(44, 44) };
            circ.Paint += (s, e) => { e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias; e.Graphics.FillEllipse(new SolidBrush(Red), 0, 0, 43, 43); e.Graphics.DrawString(num, new Font("Segoe UI", 16, FontStyle.Bold), Brushes.White, 11, 9); };
            c.Controls.Add(circ);
            c.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Red, Location = new Point(16, 68), Size = new Size(258, 24) });
            c.Controls.Add(new Label { Text = desc, Font = new Font("Segoe UI", 9), ForeColor = Color.Gray, Location = new Point(16, 95), Size = new Size(258, 44) });
            p.Controls.Add(c);
        }

        private void Quote(Panel p, string text, string author, int x)
        {
            Panel c = new Panel { Location = new Point(x, 0), Size = new Size(380, 122), BackColor = Color.FromArgb(255, 245, 245) };
            c.Controls.Add(new Panel { Location = new Point(0, 0), Size = new Size(5, 122), BackColor = Red });
            c.Controls.Add(new Label { Text = text, Font = new Font("Segoe UI", 9.5f, FontStyle.Italic), ForeColor = Color.FromArgb(60, 40, 40), Location = new Point(18, 16), Size = new Size(350, 70) });
            c.Controls.Add(new Label { Text = author, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Red, Location = new Point(18, 94), Size = new Size(350, 20) });
            p.Controls.Add(c);
        }
    }
}