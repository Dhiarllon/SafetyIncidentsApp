using Microsoft.OpenApi.Models;
using SafetyIncidentsApp;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
TestHostBuilder.ConfigureServices(builder.Services);

// Add Swagger for development
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Safety Incidents API", Version = "v1" });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Make Program accessible to tests
public partial class Program : IApiMarker { }
