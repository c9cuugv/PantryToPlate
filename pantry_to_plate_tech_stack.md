# Pantry-to-Plate — Technology Stack

---

## 1. UI Design — Google Stitch
Used only in the design phase to rapidly prototype screen layouts using text prompts. You describe the screen, Stitch generates the visual. You then use that as a blueprint to recreate it in XAML. It does not generate any code you actually use in the app.

---

## 2. Core Framework — .NET MAUI (.NET 8/9)
This is the heart of the app. MAUI lets you write your app once in C# and deploy it as a native app on both iOS and Android simultaneously. Without this, you would need two completely separate codebases — one in Swift for iOS and one in Kotlin for Android.

---

## 3. Language — C#
Everything in the app is written in C#. It handles all the logic: checking which recipes are available, deducting ingredients after cooking, generating the shopping list, and talking to the database.

---

## 4. UI Markup — XAML
XAML is the language used inside .NET MAUI to build the actual screens. You define buttons, lists, checkboxes, and layouts in XAML. Think of it as the HTML of the app — C# is the logic, XAML is the structure and look.

---

## 5. Local Database — SQLite
SQLite is a lightweight database that lives directly on the user's phone. It stores everything: the recipe library, the pantry inventory, and the shopping list. There is no server, no internet required. The app works completely offline.

---

## 6. ORM — Entity Framework Core (EF Core)
EF Core sits between your C# code and the SQLite database. Instead of writing raw SQL queries like `SELECT * FROM PantryItems`, you write normal C# code and EF Core translates it for you. It also handles creating the database tables automatically from your C# model classes.

---

## 7. Architecture — MVVM + CommunityToolkit.Mvvm
MVVM (Model-View-ViewModel) is the pattern that keeps your UI code separate from your business logic. The CommunityToolkit.Mvvm NuGet package makes this easy to implement in .NET MAUI. Practically, this means when a user confirms cooking a dish and the pantry updates, the shopping list screen refreshes automatically without you writing manual refresh code.

---

## 8. IDE — Visual Studio 2026 / JetBrains Rider
Your development environment where you write all the code, manage the project, and test the app on iOS and Android emulators before deploying to a real device.

---

## Quick Summary

| What | Tool | Why |
|---|---|---|
| Design screens | Google Stitch | Fast visual prototyping |
| Build the app | .NET MAUI | One codebase for iOS + Android |
| Write logic | C# | Familiar, strongly typed |
| Build UI screens | XAML | Native MAUI UI language |
| Store data on device | SQLite | Offline, no server needed |
| Talk to the database | EF Core | No raw SQL, use C# objects |
| Keep UI and logic separated | MVVM + CommunityToolkit | Clean architecture, auto UI refresh |
| Write and test code | Visual Studio / Rider | Full MAUI dev environment |
