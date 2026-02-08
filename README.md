Migration Commands:
~~~sh
dotnet ef migrations add --project App.EF --startup-project WebApp --context AppDbContext  InitialCreate

dotnet ef migrations --project App.EF --startup-project WebApp remove

dotnet ef database --project App.EF --startup-project WebApp update
dotnet ef database --project App.EF --startup-project WebApp drop
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

share cache only between specific usrs

            migrationBuilder.Sql(@"
                insert into languages(""IsDefaultLanguage"", ""LanguageName"", ""LanguageTag"")
                values 
                (true, 'English', 'en'),
                (false, 'English (United States)', 'en-US'),
                (false, 'Eesti (Eesti)', 'et-EE'),
                (false, 'Русский', 'ru')
                on conflict (""LanguageTag"") do nothing;
            ");