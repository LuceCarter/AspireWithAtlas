var builder = DistributedApplication.CreateBuilder(args);

var mongoDB = builder.AddMongoDB("drinksDB").AddDatabase("drinks");

builder.AddProject<Projects.AspireWithAtlas_Web>("aspirewithatlas-web");

builder.Build().Run();
