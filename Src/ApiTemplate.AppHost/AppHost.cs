var builder = DistributedApplication.CreateBuilder(args);

#if (UseDatabase)
#if (UseSqlServer)
var db = builder
            .AddSqlServer("sql")
            .AddDatabase("DefaultConnection");
#elif (UsePostgres)
var db = builder
            .AddPostgres("postgres")
            .AddDatabase("DefaultConnection");
#elif (UseMySQL)
var db = builder
            .AddMySql("mysql")
            .AddDatabase("DefaultConnection");
#endif
builder
    .AddProject<Projects.ApiTemplate_Api>("apitemplate")
    .WithReference(db, "DefaultConnection")
    .WaitFor(db);
#else
builder.AddProject<Projects.ApiTemplate_Api>("apitemplate");
#endif

builder
    .Build()
    .Run();
