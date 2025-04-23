# .NET MAUI Restaurant Management App

[![.NET MAUI](https://img.shields.io/badge/.NET-MAUI-purple?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/apps/maui)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=flat-square)](https://opensource.org/licenses/MIT)

A cross-platform application built with .NET MAUI for managing various aspects of a restaurant's operations. This application demonstrates concepts like MVVM, data persistence with Entity Framework Core, user authentication, role-based access control, and more.

## Features

*   **User Authentication:**
    *   Login & Registration for Customers and Staff.
    *   Secure password hashing using BCrypt.
    *   Role-based access control (Customer, Staff, Admin).
*   **Menu Management (Staff/Admin):**
    *   Create, Read, Update, Delete (CRUD) food Categories.
    *   CRUD Dishes (Plats), including name, description, price, and category assignment.
*   **Table Management (Staff/Admin):**
    *   CRUD Restaurant Tables, including table number and capacity.
*   **Reservations:**
    *   Customers can create/view/manage their own reservations.
    *   Staff/Admin can view/manage all reservations.
    *   Basic time-slot conflict detection.
    *   Notification system for reservation creation/status changes (saved to DB).
*   **Ordering (Customer):**
    *   Browse dishes.
    *   Add dishes to a shopping cart.
    *   Adjust item quantities in the cart.
    *   Simulated checkout process marking the cart as 'Ordered'.
*   **Statistics (Staff/Admin):**
    *   View total revenue from ordered carts.
    *   View most popular dishes based on ordered quantity.
*   **User Profile:**
    *   View and edit own username and email.
    *   Logout functionality.
*   **Notifications:**
    *   View notifications (e.g., reservation status).
    *   Mark notifications as read.

## Technologies Used

*   **.NET MAUI:** Cross-platform UI framework for Android, iOS, macOS, and Windows.
*   **C#:** Programming language.
*   **XAML:** UI markup language.
*   **MVVM (Model-View-ViewModel):** Architectural pattern for UI separation.
    *   **CommunityToolkit.Mvvm:** MVVM library for source generators (`[ObservableProperty]`, `[RelayCommand]`).
*   **Entity Framework Core:** Object-Relational Mapper (ORM) for data access.
    *   **Pomelo.EntityFrameworkCore.MySql:** EF Core provider for MySQL.
*   **MySQL:** Relational database management system.
*   **Dependency Injection:** Managed by .NET MAUI's host builder.
*   **BCrypt.Net-Next:** Library for secure password hashing.


## Setup and Configuration

1.  **Prerequisites:**
    *   .NET SDK (Version compatible with MAUI, e.g., .NET 8)
    *   Visual Studio 2022 (or later) with the ".NET Multi-platform App UI development" workload installed.
    *   MySQL Server instance (local or remote).
    *   MySQL client tool (e.g., MySQL Workbench, DBeaver) - Optional but helpful.

2.  **Database Setup:**
    *   Ensure your MySQL server is running.
    *   Create a new database (e.g., `amine` as used in the code).
    *   Update the connection string details in **two** places:
        *   `MauiProgram.cs`: For the application's runtime connection.
        *   `Data/RestaurantDbContextFactory.cs`: For EF Core design-time tools (migrations).
        ```csharp
        const string server = "YOUR_MYSQL_SERVER_IP_OR_HOSTNAME"; // e.g., "localhost", "192.168.1.100"
        const string port = "3306"; // Default MySQL port
        const string database = "YOUR_DATABASE_NAME";      // e.g., "amine", "restaurant_db"
        const string user = "YOUR_MYSQL_USERNAME";        // e.g., "root", "app_user"
        const string password = "YOUR_MYSQL_PASSWORD"; // Your strong password
        ```

3.  **Database Migrations:**
    *   Open the Package Manager Console (Tools > NuGet Package Manager > Package Manager Console).
    *   Ensure the default project is set to your MAUI project (`newRestaurant`).
    *   Run the following commands:
        ```powershell
        Add-Migration InitialCreate # Or a descriptive name for your first migration
        Update-Database
        ```
    *   This will create the necessary tables in your MySQL database based on the `RestaurantDbContext`. If you make changes to your `Models` later, run `Add-Migration YourChangeName` and `Update-Database` again.

4.  **Build and Run:**
    *   Select your target platform (Windows Machine, Android Emulator/Device, iOS Emulator/Device) in Visual Studio.
    *   Build the solution (Build > Build Solution).
    *   Run the application (Debug > Start Debugging or F5).

## Usage

1.  **Register/Login:** Start the app and register a new user (choosing Customer or Staff) or log in with existing credentials.
2.  **Navigation:** Use the flyout menu (hamburger icon) to navigate between different sections based on your role.
3.  **Customers:** Can browse dishes, manage their cart, make reservations, view their profile, and see notifications.
4.  **Staff/Admin:** Can manage categories, dishes, tables, view all reservations, view statistics, view their profile, and see notifications.

## Potential Future Enhancements

*   Implement real payment gateway integration.
*   Real-time notifications (SignalR).
*   More detailed statistics and reporting.
*   Order history tracking.
*   Admin dashboard for user management.
*   Image handling for dishes.
*   Unit and integration testing.
*   UI/UX improvements.
*   Password change/reset functionality.
*   Deployment configurations.
