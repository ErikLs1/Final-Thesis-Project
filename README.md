Migration Commands:
~~~sh
dotnet ef migrations add --project App.EF --startup-project WebApp --context AppDbContext  InitialCreate

dotnet ef migrations --project App.EF --startup-project WebApp remove

dotnet ef database --project App.EF --startup-project WebApp update
dotnet ef database --project App.EF --startup-project WebApp drop
~~~

[TODO: Change Later]
Run this before starting the app: 
~~~sh
insert into public.languages("IsDefaultLanguage", "LanguageName", "LanguageTag")
values
(true, 'English', 'en'),
(false, 'English (United States)', 'en-US'),
(false, 'Eeesti (Eesti)', 'et-EE'),
(false, 'Русский', 'ru');
~~~
