using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.EF.Migrations
{
    /// <inheritdoc />
    public partial class SeedLanguages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                insert into languages(""IsDefaultLanguage"", ""LanguageName"", ""LanguageTag"")
                values 
                (true, 'English', 'en'),
                (false, 'English (United States)', 'en-US'),
                (false, 'Eesti (Eesti)', 'et-EE'),
                (false, 'Русский', 'ru')
                on conflict (""LanguageTag"") do nothing;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
