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


REDIS HASHES

[SOME OPERATION (GET/SET)] [HASH KEY] [KEY] [VALUE] [KEY] [VALUE] ETC.
HSET UI:TRANSLATION:EN translation_key translation_value

On application start we insert all the keys but then after then later when we have tu update something or something is missing we perform and update only specific keys.


DOCKERFILE BUILD
~~~sh
docker build -t {IMAGE_NAME}:local .
~~~

DOCKER COMPOSE BUILD + REMOVE
~~~sh
docker compose up --build
docker compose down
~~~
