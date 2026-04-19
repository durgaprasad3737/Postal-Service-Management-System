# PostalMS 

A desktop postal service management app built with C# WinForms and SQL Server for our CST2550 Software Engineering Management and Development coursework at Middlesex University.

--------------------------------------------------------------------------------------------------------------------------------------------

## Who built this

**The Leftovers** — Middlesex University, April 2026

- **Durga** (M01038074) — Team lead, built pretty much everything
- **Meet** (M01000862) — Documentation and secretary
- **Tapan** (M01050963) — Development support and video demo
- **Forhad** (M01044942) — Testing, wrote all 78 unit tests

--------------------------------------------------------------------------------------------------------------------------------------------

## What can it do

**Send parcels** — domestic UK or international to 24 countries. Price calculates live as you fill in the form. Choose to pay online with a card or pay in store when you drop it off.

**Track deliveries** — see the full delivery timeline, who the driver is, how many attempts were made and what happened. Confirm your drop off directly from the app.

**Order stamps** — pick your stamp type, choose delivery or click and collect, pay online or pay in store. Miss your collection slot? No problem, it auto-reschedules after 48 hours.

**Find Us** — 76 PostalMS locations across London, Manchester, Birmingham, Leeds, Bristol, Edinburgh and Glasgow. Opens Google Maps when you tap any location. Auto-detects your city from your profile.

**Request a refund** — fill in your tracking ID, pick a reason, describe what happened. We get back to you within 2-3 working days.

**AI Assistant** — ask it anything. Track a parcel, get a price estimate, find a drop-off point. All offline, no internet needed.

--------------------------------------------------------------------------------------------------------------------------------------------

## The three custom data structures

We built all three from scratch — no standard library collections.

**Hash Table** — instant parcel lookup by tracking ID. O(1) average. Auto-resizes when load hits 75%.

**Binary Search Tree** — parcels sorted alphabetically by tracking ID. O(log n) average insert and search.

**Queue** — deliveries processed in the order they come in. O(1) for everything.

--------------------------------------------------------------------------------------------------------------------------------------------

## 78 unit tests, all passing

HashTableTests (12) · BSTTests (13) · QueueTests (10) · PriceCalculationTests (9) · TrackingIDTests (6) · IntegrationTests (5) · GmailValidationTests (9) · StampPriceTests (6) · CityLocationTests (8)

--------------------------------------------------------------------------------------------------------------------------------------------

## How to run it

1. Install Visual Studio 2022 and SQL Server 2022
2. Open SSMS and run `PostalMS-Database.sql` to set up the database
3. Open `PostalServiceWinForms.sln` and press F5

**Test accounts**

- `john@gmail.com` / `john123`
- `alice@gmail.com` / `alice123`
- `admin@gmail.com` / `admin123`

> All accounts must use Gmail. That's enforced throughout the app.

--------------------------------------------------------------------------------------------------------------------------------------------

## Video demonstration

The video demonstration is located in the root of the repository as `PostalMS_Demo.mp4`

--------------------------------------------------------------------------------------------------------------------------------------------

## Stack

C# · .NET Framework 4.7.2 · WinForms · SQL Server 2022 · MSTest · Visual Studio 2022

--------------------------------------------------------------------------------------------------------------------------------------------

## Connection string

```
Server=localhost;Database=PostalServiceDB;Trusted_Connection=True;TrustServerCertificate=True;
```

--------------------------------------------------------------------------------------------------------------------------------------------

## Repository structure

```
📁 Postal-Service-Management-System
│
├── 📁 PostalServiceWinForms          ← Main application source code
│   ├── 📁 DataStructures
│   │   ├── CustomHashTable.cs
│   │   ├── CustomBST.cs
│   │   └── CustomQueue.cs
│   ├── 📁 Forms
│   │   ├── HomeView.cs
│   │   ├── ParcelsView.cs
│   │   ├── DeliveriesView.cs
│   │   ├── StampsView.cs
│   │   ├── FindUsView.cs
│   │   ├── ProfileView.cs
│   │   ├── HelpView.cs
│   │   ├── InfoView.cs
│   │   ├── DataStructuresView.cs
│   │   └── AIAssistantPanel.cs
│   ├── DatabaseHelper.cs
│   ├── LoginForm.cs
│   ├── RegisterForm.cs
│   ├── MainForm.cs
│   └── Program.cs
│
├── 📁 PostalServiceWinForms.Tests    ← Unit tests
│   └── PostalMSTests.cs
│
├── 📁 design                         ← Design documents
│   ├── PostalMS-Pseudocode-3.0.pdf
│   ├── PostalMS-TimeComplexity-3.0.pdf
│   ├── ClassDiagram.png
│   └── Database.erd
│
├── 📁 docs                           ← Project management documents
│   ├── PostalMS-SprintPlanning-3.0.pdf
│   └── PostalMS-UnitTesting-Report-3.0.pdf
│
├── 📁 meetings                       ← Meeting minutes
│   └── PostalMS_MeetingMinutes.pdf
│
├── PostalMS-Database.sql             ← SQL database script
├── PostalMS_Demo.mp4                 ← Video demonstration
├── PostalMS-Report-3.0.pdf               ← Full project report
└── README.md
```
