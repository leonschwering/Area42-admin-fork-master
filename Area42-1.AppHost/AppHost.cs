var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.Area42_1_ApiService>("apiservice");

builder.AddProject<Projects.Area42_1_Web>("webfrontend")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
