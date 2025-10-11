~~~sh
dotnet ef migrations add --project App.EF --startup-project WebApp --context AppDbContext  InitialCreate

dotnet ef migrations --project App.EF --startup-project WebApp remove

dotnet ef database --project App.EF --startup-project WebApp update
dotnet ef database --project App.EF --startup-project WebApp drop
~~~