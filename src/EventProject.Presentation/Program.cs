using System.Reflection;
using System.Text.Json.Serialization;
using EventProject.Application.Extensions;
using EventProject.Infrastructure.Extensions;
using EventProject.Presentation.BackgroundServices;
using EventProject.Presentation.Middlewares;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException("Connection string 'Default' not found.");

builder.Services.AddInfrastructureServices(connectionString);
builder.Services.AddApplicationServices();
builder.Services.AddHostedService<BookingProcessingService>();

builder.Services
    .AddControllers()
    .AddJsonOptions(options => { options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); });

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
    
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Type = SecuritySchemeType.Http,
        In = ParameterLocation.Header,
    });

    options.AddSecurityRequirement(document =>
    {
        var securityRequirement = new OpenApiSecurityRequirement();
        var securitySchemeReference = new OpenApiSecuritySchemeReference("Bearer", document);

        securityRequirement.Add(securitySchemeReference, new List<string>());

        return securityRequirement;
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.Services.ConfigureInfrastructure();

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();