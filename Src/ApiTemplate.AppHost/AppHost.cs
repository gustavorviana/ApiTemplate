var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.ApiTemplate_Api>("apitemplate");

builder.Build().Run();
