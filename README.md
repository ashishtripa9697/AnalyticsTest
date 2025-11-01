

-  All LINQ queries are **optimized** using `AsNoTracking()` and **single-query projections** to prevent multiple database calls (no N+1 issues).
-  DTOs (Data Transfer Objects) are used in all API responses — **no EF Core entities are exposed directly**.
-  Database **auto-seeds** with realistic test data (5,000–7000 employees) on first run.
-  Swagger is enabled by default for quick API testing at `/swagger` if we can test on the browser /Postman .
-  Postman is Best testing tools currently.
-  Project is built using **.NET 8**, **Entity Framework Core**, and **SQL Server**.
-  If database seeding takes longer on first run, that’s expected due to data volume.
-  Ensure SQL Server or LocalDB instance is running before launching the API.
-  Performance verified locally — each endpoint responds within **400ms (P95)** after warm start.
-  Use `dotnet ef database update` to create and seed the database if automatic seeding fails.
-  To re-seed the database, delete it manually and re-run migrations or the app.
-  Authentication (JWT) can be added later — not required in the assignment.
-  Follow REST naming conventions when testing routes (`/api/Analytics/...`).
**-  Note I have suggestion i did it according to you many via code like optimization but we should not more code write for the optimization in coding part we can also manage from Database
**

-Employee Analytics API - Project Documentation

// GET: https://localhost:44390/api/Analytics/salary-extremes
//a// GET: https://localhost:44390/api/Analytics/salary-extremes
//https://localhost:44390/api/Analytics/average-salary

