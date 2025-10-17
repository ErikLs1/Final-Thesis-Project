using App.EF;
using App.Repository.Interface;

namespace App.Repository.Impl;

public class ResxImportRepository : IResxImportRepository
{
    private readonly AppDbContext _db;
    
    public ResxImportRepository(AppDbContext repositoryDbContext)
    {
        _db = repositoryDbContext;
    }
    
    // https://learn.microsoft.com/en-us/dotnet/fundamentals/runtime-libraries/system-resources-resourcereader
    // https://learn.microsoft.com/en-us/dotnet/api/system.resources.resxresourcereader?view=windowsdesktop-9.0
    // https://learn.microsoft.com/en-us/dotnet/api/system.io.directory.enumeratefiles?view=net-9.0
}