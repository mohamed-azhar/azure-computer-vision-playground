using Serilog;
using MicrosoftAzureComputerVisionPlayground.Configuration;
using MicrosoftAzureComputerVisionPlayground.Services;

var builder = WebApplication.CreateBuilder(args);

//DI setup with the required services
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog((context, loggingConfiguration) => loggingConfiguration
    .WriteTo.Console()
    .ReadFrom.Configuration(context.Configuration));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<ComputerVision>(builder.Configuration.GetSection("ComputerVision"));

builder.Services.AddScoped<IComputerVisionService, ComputerVisionService>();

var app = builder.Build();

//middleware pipeline setup
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
