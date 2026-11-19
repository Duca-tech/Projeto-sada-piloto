using Application;
using Infraestructure;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    // Usa os comentários /// dos controllers na documentação do Swagger.
    var xml = Path.Combine(AppContext.BaseDirectory, $"{typeof(Program).Assembly.GetName().Name}.xml");
    if (File.Exists(xml))
        options.IncludeXmlComments(xml);
});

builder.Services.AddMemoryCache();

// Logging básico: registra no console cada requisição (método, rota, status e tempo de resposta).
builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = HttpLoggingFields.RequestMethod
                          | HttpLoggingFields.RequestPath
                          | HttpLoggingFields.RequestQuery
                          | HttpLoggingFields.ResponseStatusCode
                          | HttpLoggingFields.Duration;
    options.CombineLogs = true;
});

AddDependenciaRepository.AddIndependencieRepositorys(builder.Services);
AddDependenciaApplication.AddIndependencieApplication(builder.Services);

var app = builder.Build();

app.UseHttpLogging();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
