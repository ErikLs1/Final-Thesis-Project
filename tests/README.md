# Test Projects

This repository now has two test projects:

- `tests/App.UnitTests`: fast, isolated tests for small logic units.
- `tests/App.IntegrationTests`: tests that exercise repository + EF Core behavior using in-memory SQLite.

## Run locally

```bash
dotnet test tests/App.UnitTests/App.UnitTests.csproj
dotnet test tests/App.IntegrationTests/App.IntegrationTests.csproj
```

Or run all tests in the solution:

```bash
dotnet test FinalThesisProject.sln
```

## CI baseline

A simple pipeline step can be:

```bash
dotnet restore FinalThesisProject.sln
dotnet build FinalThesisProject.sln --configuration Release --no-restore
dotnet test FinalThesisProject.sln --configuration Release --no-build
```
