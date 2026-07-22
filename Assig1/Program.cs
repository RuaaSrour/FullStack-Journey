using Assig1.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllers();



// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Connect AppDbContext to SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
{
    // Read the connection string from appsettings.json
    string? connectionString =
        builder.Configuration.GetConnectionString("DefaultConnection");

    // Tell the project to use SQL Server
    options.UseSqlServer(connectionString);
});

var app = builder.Build();






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
