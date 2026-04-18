# Postal Service Management System

PostalMS is a desktop application built using C# WinForms and Microsoft SQL Server.
It allows users to send mail and packages, track deliveries in real time, request
refunds, and use an offline AI assistant for parcel support. The system uses three
custom-built data structures — a Hash Table, Binary Search Tree and Queue ,all
implemented from scratch without any standard template libraries.

---

## Module Information

- Module: CST2550 Software Engineering Management and Development
- University: Middlesex University London
- Team Name: The Leftovers


---

## Team Members

**Durga Prasad Avala**
Student ID: M01038074
Role: Team Leader and Developer
Email: da1162@live.mdx.ac.uk

**Meet Sanjay Mande**
Student ID: M01000862
Role: Secretary
Email: MM4353@live.mdx.ac.uk

**Tapan Keyurbhai Patel**
Student ID: M01050963
Role: Developer
Email: tp588@live.mdx.ac.uk

**Forhad Hossain**
Student ID: M01044942
Role: Tester
Email: FH455@live.mdx.ac.uk

---

## System Requirements

- Windows 10 or Windows 11
- Visual Studio 2022
- Microsoft SQL Server 2022 Developer Edition
- SQL Server Management Studio (SSMS) 22
- .NET Framework 4.7.2 or higher

---

## Database Setup

Before running the application you must set up the database first.

1. Open **SQL Server Management Studio (SSMS)**
2. Connect to **localhost** using Windows Authentication
3. Click **New Query** to open a query window
4. Open the file `src/database-script.sql` from this repository
5. Copy the entire contents and paste into the query window
6. Click **Execute** or press **F5**
7. You should see the message **PostalServiceDB created successfully**
8. The database is now ready with sample data loaded

---

## How to Open and Run the Project

1. Clone this repository or download it as a ZIP file
2. Extract the ZIP if downloaded
3. Open **Visual Studio 2022**
4. Go to **File** then **Open** then **Project / Solution**
5. Navigate to the `src` folder and open `PostalServiceWinForms.sln`
6. Make sure **SQL Server is running** on localhost before continuing
7. Press **F5** or click the green **Start** button to build and run
8. The PostalMS login screen will appear

---

## Test Login Credentials

Three accounts are pre-loaded in the database for testing:

**John Smith**
Email: john@email.com
Password: john123
Notes: Has 5 sample parcels and deliveries

**Alice Brown**
Email: alice@email.com
Password: alice123
Notes: Empty account for testing

**Admin**
Email: admin@postalms.com
Password: admin123
Notes: Admin account

**Recommended:** Log in as John Smith to see the full system with data.

---

## How to Run the Unit Tests

The unit tests are written using MSTest for .NET Framework and cover all three
custom data structures, the pricing logic, tracking ID generation and integration.

1. Open the solution in Visual Studio 2022
2. Make sure the **PostalServiceWinForms.Tests** project is visible in Solution Explorer
3. If it is not there, right-click the solution and click **Add Existing Project**
   and navigate to the Tests project folder
4. Click the **Test** menu at the top of Visual Studio
5. Click **Run All Tests**
6. The **Test Explorer** window will open
7. All **59 tests** should show as passed with a green tick

**Expected result:** 59 Passed, 0 Failed, 0 Skipped in approximately 66 ms

---
## Project Structure

- **design** — Class diagram, ERD, pseudocode, time complexity analysis
- **docs** — PDF report, unit testing report, sprint planning documents
- **media** — MP4 video demonstration
- **meetings** — All team meeting minutes
- **src** — All C# source code, SQL database script, unit test project
- **README.md** — This file

---
  
## Key Features
- Send mail and packages domestically and internationally
- Real-time parcel tracking using a custom Hash Table for O(1) lookup
- Sorted parcel dashboard using a custom BST for O(log n) insertion
- Delivery queue management using a custom FIFO Queue
- Offline AI assistant for tracking, price estimation and delivery prediction
- Refund request system
- User profile management with photo upload
- 59 passing unit tests covering all data structures and business logic

---

## Data Structures Used

**Custom Hash Table**
Purpose: Parcel lookup by tracking ID
Time Complexity: O(1) average search

**Custom Binary Search Tree (BST)**
Purpose: Sorted parcel display on dashboard
Time Complexity: O(log n) insert, O(n) traversal

**Custom Queue**
Purpose: Delivery processing in FIFO order
Time Complexity: O(1) enqueue and dequeue

All three data structures were built from scratch without using any
standard template library collections.


