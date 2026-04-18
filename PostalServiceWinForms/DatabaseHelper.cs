// DatabaseHelper.cs
// PostalMS - Postal Service Management System
// CST2550 Coursework - Middlesex University
//
// References:
// Microsoft (2024a) Stored Procedures (Database Engine). Available at:
//   https://learn.microsoft.com/en-us/sql/relational-databases/stored-procedures
// Microsoft (2024b) SqlConnection class. Available at:
//   https://learn.microsoft.com/en-us/dotnet/api/system.data.sqlclient.sqlconnection
//
// Handles all database operations using SQL Server stored procedures and direct queries.
// Also manages the three custom data structures (Hash Table, BST, Queue) which are
// loaded from the database on startup and kept in sync with every add/update/delete.
//
// The three data structures serve different purposes:
//   Hash Table - fast O(1) parcel lookup by tracking ID
//   BST        - sorted O(log n) parcel storage for dashboard display
//   Queue      - FIFO O(1) delivery assignment processing

using System;
using System.Data;
using System.Data.SqlClient;
using PostalServiceWinForms.DataStructures;

namespace PostalServiceWinForms
{
    public class DatabaseHelper
    {
        private string connectionString =
            "Server=localhost;Database=PostalServiceDB;Trusted_Connection=True;TrustServerCertificate=True;";

        // -- Custom data structures loaded on startup
        public CustomHashTable ParcelHashTable = new CustomHashTable(100);
        public CustomBST ParcelBST = new CustomBST();
        public CustomQueue DeliveryQueue = new CustomQueue();

        public DatabaseHelper() { LoadDS(); }

        // Load all parcels into Hash Table + BST, pending deliveries into Queue
        public void LoadDS()
        {
            try
            {
                using (SqlConnection c = new SqlConnection(connectionString))
                {
                    c.Open();
                    var r1 = new SqlCommand("SELECT TrackingID,Status FROM Parcel", c).ExecuteReader();
                    while (r1.Read()) { string t = r1["TrackingID"].ToString(); string s = r1["Status"].ToString(); ParcelHashTable.Add(t, s); ParcelBST.Insert(t, s); }
                    r1.Close();
                    var r2 = new SqlCommand("SELECT DeliveryID FROM Delivery WHERE DeliveryStatus='Assigned'", c).ExecuteReader();
                    while (r2.Read()) DeliveryQueue.Enqueue(r2["DeliveryID"].ToString());
                    r2.Close();
                }
            }
            catch { }
        }

        // Public query helper for ProfileView and other classes
        public DataTable Q_Public(string sql, params (string n, object v)[] p) => Q(sql, p);

        // Generic query helper
        private DataTable Q(string sql, params (string n, object v)[] p)
        {
            try
            {
                using (SqlConnection c = new SqlConnection(connectionString))
                {
                    c.Open();
                    SqlCommand cmd = new SqlCommand(sql, c);
                    foreach (var pm in p) cmd.Parameters.AddWithValue(pm.n, pm.v ?? DBNull.Value);
                    DataTable dt = new DataTable(); dt.Load(cmd.ExecuteReader()); return dt;
                }
            }
            catch { return new DataTable(); }
        }

        // == USER ==
        public DataTable LoginUser(string email, string pwd)
        {
            try
            {
                using (SqlConnection c = new SqlConnection(connectionString))
                {
                    c.Open(); SqlCommand cmd = new SqlCommand("LoginUser", c) { CommandType = CommandType.StoredProcedure };
                    cmd.Parameters.AddWithValue("@email", email); cmd.Parameters.AddWithValue("@password", pwd);
                    DataTable dt = new DataTable(); dt.Load(cmd.ExecuteReader()); return dt;
                }
            }
            catch (Exception ex) { DataTable dt = new DataTable(); dt.Columns.Add("Message"); dt.Rows.Add("Error: " + ex.Message); return dt; }
        }

        public string RegisterUser(string uid, string name, string email, string phone, string addr, string post, string role, string pwd, string city = "London")
        {
            try
            {
                using (SqlConnection c = new SqlConnection(connectionString))
                {
                    c.Open(); SqlCommand cmd = new SqlCommand("RegisterUser", c) { CommandType = CommandType.StoredProcedure };
                    cmd.Parameters.AddWithValue("@userID", uid); cmd.Parameters.AddWithValue("@fullName", name);
                    cmd.Parameters.AddWithValue("@email", email); cmd.Parameters.AddWithValue("@phone", phone);
                    cmd.Parameters.AddWithValue("@address", addr); cmd.Parameters.AddWithValue("@postCode", post);
                    cmd.Parameters.AddWithValue("@role", role); cmd.Parameters.AddWithValue("@password", pwd);
                    cmd.Parameters.AddWithValue("@city", string.IsNullOrEmpty(city) ? "London" : city);
                    return cmd.ExecuteScalar()?.ToString() ?? "Account created successfully";
                }
            }
            catch (Exception ex) { return "Error: " + ex.Message; }
        }

        public int GetUserCount() { try { using (var c = new SqlConnection(connectionString)) { c.Open(); return Convert.ToInt32(new SqlCommand("SELECT COUNT(*) FROM Users", c).ExecuteScalar()); } } catch { return 0; } }

        public DataTable GetUserByID(string uid)
        {
            // Try with extra columns first, fall back to basic if columns don't exist
            try { return Q("SELECT UserID,FullName,Email,Phone,Address,PostCode,Role,CreatedDate,PreferredName,DOB,Bio FROM Users WHERE UserID=@id", ("@id", uid)); }
            catch { return Q("SELECT UserID,FullName,Email,Phone,Address,PostCode,Role,CreatedDate FROM Users WHERE UserID=@id", ("@id", uid)); }
        }

        public string UpdateUserProfile(string uid, string name, string phone, string addr, string post)
        {
            try { using (var c = new SqlConnection(connectionString)) { c.Open(); var cmd = new SqlCommand("UPDATE Users SET FullName=@n,Phone=@p,Address=@a,PostCode=@pc WHERE UserID=@id", c); cmd.Parameters.AddWithValue("@n", name); cmd.Parameters.AddWithValue("@p", phone); cmd.Parameters.AddWithValue("@a", addr); cmd.Parameters.AddWithValue("@pc", post); cmd.Parameters.AddWithValue("@id", uid); cmd.ExecuteNonQuery(); return "Profile updated successfully"; } }
            catch (Exception ex) { return "Error: " + ex.Message; }
        }

        // Save extra profile fields -- PreferredName, DOB, Bio
        public string UpdateUserExtras(string uid, string prefName, string dob, string bio)
        {
            try
            {
                using (var c = new SqlConnection(connectionString))
                {
                    c.Open();
                    // Add columns if they don't exist yet
                    try { new SqlCommand("IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name='PreferredName' AND Object_ID=Object_ID('Users')) ALTER TABLE Users ADD PreferredName VARCHAR(100) NULL", c).ExecuteNonQuery(); } catch { }
                    try { new SqlCommand("IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name='DOB'           AND Object_ID=Object_ID('Users')) ALTER TABLE Users ADD DOB           VARCHAR(20)  NULL", c).ExecuteNonQuery(); } catch { }
                    try { new SqlCommand("IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name='Bio'           AND Object_ID=Object_ID('Users')) ALTER TABLE Users ADD Bio           VARCHAR(500) NULL", c).ExecuteNonQuery(); } catch { }

                    var cmd = new SqlCommand("UPDATE Users SET PreferredName=@pn, DOB=@db, Bio=@bio WHERE UserID=@id", c);
                    cmd.Parameters.AddWithValue("@pn", string.IsNullOrEmpty(prefName) ? (object)DBNull.Value : prefName);
                    cmd.Parameters.AddWithValue("@db", string.IsNullOrEmpty(dob) ? (object)DBNull.Value : dob);
                    cmd.Parameters.AddWithValue("@bio", string.IsNullOrEmpty(bio) ? (object)DBNull.Value : bio);
                    cmd.Parameters.AddWithValue("@id", uid);
                    cmd.ExecuteNonQuery();
                    return "Extras saved";
                }
            }
            catch (Exception ex) { return "Error: " + ex.Message; }
        }

        // == DASHBOARD ==
        public DataTable GetDashboardStats() => Q("SELECT * FROM DashboardStats");
        public DataTable GetRecentParcels() => Q("SELECT TOP 10 TrackingID,ParcelType,SenderName,ReceiverName,Status,DateSent,Price FROM Parcel ORDER BY DateSent DESC");

        // User-specific stats (BUG FIX: filtered by userID)
        public DataTable GetUserStats(string uid) => Q("SELECT * FROM UserDashboardStats WHERE CustomerID=@uid", ("@uid", uid));

        // == PARCEL ==
        public DataTable SearchParcel(string tid)
        {
            var hit = ParcelHashTable.Search(tid); // O(1)
            if (hit != null) Console.WriteLine($"[HashTable O(1)] {tid}={hit}");
            return Q("SELECT p.*,d.DriverName,d.DeliveryStatus,d.AttemptCount,d.Notes FROM Parcel p LEFT JOIN Delivery d ON p.TrackingID=d.TrackingID WHERE p.TrackingID=@t", ("@t", tid));
        }

        // BUG FIX: Returns only this user's parcels -- new users get empty result
        public DataTable GetParcelsByCustomer(string uid) =>
            Q("SELECT TrackingID,ParcelType,SenderName,ReceiverName,Status,DateSent,ServiceType,Price,Weight,IsInternational,DestinationCountry,EstimatedDelivery,RefundRequested,RefundStatus FROM Parcel WHERE CustomerID=@uid ORDER BY DateSent DESC", ("@uid", uid));

        public int GetParcelCount() { try { using (var c = new SqlConnection(connectionString)) { c.Open(); return Convert.ToInt32(new SqlCommand("SELECT COUNT(*) FROM Parcel", c).ExecuteScalar()); } } catch { return 0; } }

        public string AddParcel(string tid, string cid, string ptype, string sn, string sa, string rn, string ra, double w, string sz, string sv, bool isIntl, string country, string status, double price, DateTime estDel)
        {
            try
            {
                using (SqlConnection c = new SqlConnection(connectionString))
                {
                    c.Open();
                    SqlCommand cmd = new SqlCommand(
                        "INSERT INTO Parcel(TrackingID,CustomerID,ParcelType,SenderName,SenderAddress,ReceiverName,ReceiverAddress,Weight,Size,ServiceType,IsInternational,DestinationCountry,Status,DateSent,Price,EstimatedDelivery) " +
                        "VALUES(@t,@c,@pt,@sn,@sa,@rn,@ra,@w,@sz,@sv,@ii,@co,@st,GETDATE(),@pr,@ed)", c);
                    cmd.Parameters.AddWithValue("@t", tid); cmd.Parameters.AddWithValue("@c", cid);
                    cmd.Parameters.AddWithValue("@pt", ptype); cmd.Parameters.AddWithValue("@sn", sn);
                    cmd.Parameters.AddWithValue("@sa", sa); cmd.Parameters.AddWithValue("@rn", rn);
                    cmd.Parameters.AddWithValue("@ra", ra); cmd.Parameters.AddWithValue("@w", w);
                    cmd.Parameters.AddWithValue("@sz", sz); cmd.Parameters.AddWithValue("@sv", sv);
                    cmd.Parameters.AddWithValue("@ii", isIntl ? 1 : 0);
                    cmd.Parameters.AddWithValue("@co", (object)country ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@st", status); cmd.Parameters.AddWithValue("@pr", price);
                    cmd.Parameters.AddWithValue("@ed", estDel);
                    if (cmd.ExecuteNonQuery() > 0) { ParcelHashTable.Add(tid, status); ParcelBST.Insert(tid, status); return "Parcel added successfully"; }
                    return "Failed to add";
                }
            }
            catch (Exception ex) { return "Error: " + ex.Message; }
        }

        public string UpdateParcelStatus(string tid, string status)
        {
            try { using (var c = new SqlConnection(connectionString)) { c.Open(); var cmd = new SqlCommand("UpdateParcelStatus", c) { CommandType = CommandType.StoredProcedure }; cmd.Parameters.AddWithValue("@trackID", tid); cmd.Parameters.AddWithValue("@newStatus", status); var r = cmd.ExecuteScalar(); if (ParcelHashTable.ContainsKey(tid)) ParcelHashTable.Add(tid, status); return r?.ToString() ?? "Updated"; } }
            catch (Exception ex) { return "Error: " + ex.Message; }
        }

        public string DeleteParcel(string tid)
        {
            try { using (var c = new SqlConnection(connectionString)) { c.Open(); var cmd = new SqlCommand("DeleteParcel", c) { CommandType = CommandType.StoredProcedure }; cmd.Parameters.AddWithValue("@trackID", tid); var r = cmd.ExecuteScalar(); ParcelHashTable.Delete(tid); ParcelBST.Delete(tid); return r?.ToString() ?? "Deleted"; } }
            catch (Exception ex) { return "Error: " + ex.Message; }
        }

        public string RequestRefund(string tid, string reason)
        {
            try { using (var c = new SqlConnection(connectionString)) { c.Open(); var cmd = new SqlCommand("RequestRefund", c) { CommandType = CommandType.StoredProcedure }; cmd.Parameters.AddWithValue("@trackID", tid); cmd.Parameters.AddWithValue("@reason", reason); return cmd.ExecuteScalar()?.ToString() ?? "Refund request submitted successfully"; } }
            catch (Exception ex) { return "Error: " + ex.Message; }
        }

        // == DELIVERY ==
        // BUG FIX: Only return deliveries for this user's parcels
        public DataTable GetDeliveriesForUser(string uid) =>
            Q("SELECT d.DeliveryID,d.TrackingID,ISNULL(d.DriverName,'Auto-Assigned') AS DriverName,ISNULL(d.Route,'Auto') AS Route,d.DeliveryStatus,d.AttemptCount,d.AssignedDate,d.LastAttemptDate,ISNULL(d.Notes,'No notes') AS Notes,p.SenderName,p.ReceiverName,p.ParcelType FROM Delivery d JOIN Parcel p ON d.TrackingID=p.TrackingID WHERE p.CustomerID=@uid ORDER BY d.AssignedDate DESC", ("@uid", uid));

        public DataTable GetDeliveriesForUserByStatus(string uid, string status) =>
            Q("SELECT d.DeliveryID,d.TrackingID,ISNULL(d.DriverName,'Auto-Assigned') AS DriverName,ISNULL(d.Route,'Auto') AS Route,d.DeliveryStatus,d.AttemptCount,d.AssignedDate,d.LastAttemptDate,ISNULL(d.Notes,'No notes') AS Notes,p.SenderName,p.ReceiverName,p.ParcelType FROM Delivery d JOIN Parcel p ON d.TrackingID=p.TrackingID WHERE p.CustomerID=@uid AND d.DeliveryStatus=@s ORDER BY d.AssignedDate DESC", ("@uid", uid), ("@s", status));

        public DataTable GetAllDeliveries() =>
            Q("SELECT d.DeliveryID,d.TrackingID,ISNULL(d.DriverName,'Auto') AS DriverName,d.DeliveryStatus,d.AttemptCount,d.AssignedDate,d.LastAttemptDate,ISNULL(d.Notes,'--') AS Notes,p.SenderName,p.ReceiverName FROM Delivery d JOIN Parcel p ON d.TrackingID=p.TrackingID ORDER BY d.AssignedDate DESC");

        public string UpdateDeliveryStatus(string id, string status)
        {
            try { using (var c = new SqlConnection(connectionString)) { c.Open(); var cmd = new SqlCommand("UpdateDeliveryStatus", c) { CommandType = CommandType.StoredProcedure }; cmd.Parameters.AddWithValue("@delID", id); cmd.Parameters.AddWithValue("@newStatus", status); return cmd.ExecuteScalar()?.ToString() ?? "Updated"; } }
            catch (Exception ex) { return "Error: " + ex.Message; }
        }

        public string AutoAssignDelivery(string tid, string delID)
        {
            try { using (var c = new SqlConnection(connectionString)) { c.Open(); var cmd = new SqlCommand("AutoAssignDelivery", c) { CommandType = CommandType.StoredProcedure }; cmd.Parameters.AddWithValue("@trackID", tid); cmd.Parameters.AddWithValue("@newDelID", delID); var r = cmd.ExecuteScalar(); DeliveryQueue.Enqueue(delID); return r?.ToString() ?? "Created"; } }
            catch (Exception ex) { return "Error: " + ex.Message; }
        }
    }
}