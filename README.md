# Community Library Desk

Student Number: 76990
Student Name: Lucas Madeira Maranho
This project is an ASP.NET Core MVC application CA1 in Modern Programming Principles and Practice.

## Overview
The system allows staff to manage:
- Books
- Members
- Loans
- Admin roles

## Technologies

- ASP.NET Core MVC
- Entity Framework Core
- SQL Server / LocalDB
- ASP.NET Core Identity
- xUnit
- Bogus
- GitHub Actions

## Features
- CRUD for Books
- CRUD for Members
- Loan creation and return workflow
- Prevents lending a book already on active loan
- Search by Title or Author
- Filter by Category
- Filter by Availability
- Admin-only Role Management page
- Seeded fake data with Bogus
- xUnit tests
- GitHub Actions CI pipeline

## Entities:

### Book
- Id
- Title
- Author
- Isbn
- Category
- IsAvailable

### Member
- Id
- FullName
- Email
- Phone

### Loan
- Id
- BookId
- MemberId
- LoanDate
- DueDate
- ReturnedDate

## Admin Login
Seeded Admin account:
- Email: admin@library.com
- Password: Admin123!

## How to Run
1. Restore packages:
   dotnet restore

2. Update database:
   dotnet ef database update

3. Run the application:
   dotnet run

## How to Run
dotnet test