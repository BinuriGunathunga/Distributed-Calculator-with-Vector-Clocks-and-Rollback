// using CalculatorServer.Services;

// var builder = WebApplication.CreateBuilder(args);

// // Parse port from command line arguments
// var port = args.Length > 0 && int.TryParse(args[0], out var p) ? p : 5000;

// builder.WebHost.ConfigureKestrel(options =>
// {
//     options.ListenLocalhost(port, o => o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2);
// });

// // Add services to the container
// builder.Services.AddGrpc();

// var app = builder.Build();

// // Configure the HTTP request pipeline
// app.MapGrpcService<CalculatorService>();

// Console.WriteLine($"🚀 Calculator server running on port {port}");
// Console.WriteLine("Press Ctrl+C to shutdown");

// app.Run();



using CalculatorServer.Services;

var builder = WebApplication.CreateBuilder(args);

// Parse port from command line arguments
var port = args.Length > 0 && int.TryParse(args[0], out var p) ? p : 5000;

// 🔧 FIXED: Configure Kestrel for HTTP/2 without TLS
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(port, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
    });
});

// Add services to the container
builder.Services.AddGrpc();

var app = builder.Build();

// Configure the HTTP request pipeline
app.MapGrpcService<CalculatorService>();

Console.WriteLine($"🚀 Calculator server running on HTTP port {port}");
Console.WriteLine("Press Ctrl+C to shutdown");

app.Run();