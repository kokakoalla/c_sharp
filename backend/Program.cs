using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Добавляем службы
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173") // URL фронтенда
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
builder.Services.AddHttpClient();

var app = builder.Build();

// Настраиваем конвейер
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseRouting();
app.UseCors();

app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogInformation($"Request: {context.Request.Method} {context.Request.Path}");
    await next.Invoke();
    logger.LogInformation($"Response: {context.Response.StatusCode}");
});

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
