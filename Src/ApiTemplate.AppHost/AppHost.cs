var builder = DistributedApplication.CreateBuilder(args);

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

#if (EnableHangfire)
var hangfireDb = builder
#if (UseSqlServer)
            .AddSqlServer("hangfire-sql")
#elif (UsePostgres)
            .AddPostgres("hangfire-pg")
#elif (UseMySQL)
            .AddMySql("hangfire-mysql")
#endif
            .AddDatabase("Hangfire");
#endif

var api = builder
    .AddProject<Projects.ApiTemplate_Api>("apitemplate")
    .WithReference(db, "DefaultConnection")
    .WaitFor(db);

#if (EnableHangfire)
api.WithReference(hangfireDb, "Hangfire").WaitFor(hangfireDb);

builder
    .AddProject<Projects.ApiTemplate_Jobs>("apitemplate-jobs")
    .WithReference(db, "DefaultConnection")
    .WaitFor(db)
    .WithReference(hangfireDb, "Hangfire")
    .WaitFor(hangfireDb);
#endif

builder
    .Build()
    .Run();
