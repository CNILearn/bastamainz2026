var builder = DistributedApplication.CreateBuilder(args);

var service =  builder.AddProject<Projects._04_SignalR>("signalr");

builder.Build().Run();
