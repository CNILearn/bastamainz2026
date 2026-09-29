using Shipping.SignalR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddSingleton<ShipmentTracker>();

var app = builder.Build();

app.MapHub<ShipmentHub>("/hubs/shipments");
app.MapGet("/", () => "Shipment SignalR hub is running. Connect to /hubs/shipments.");

app.Run();
