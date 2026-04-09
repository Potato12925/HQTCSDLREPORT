using System;

var builder = WebApplication.CreateBuilder(args);

// ================= SERVICES =================
builder.Services.AddControllers();

// Swagger (.NET 8)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddDataProtection();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// ================= MIDDLEWARE =================

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("AllowAll");

app.UseSession();   // phải trước MapControllers nếu dùng session

app.UseAuthorization();

// ================= ENDPOINTS =================
app.MapControllers();
app.MapFallbackToFile("/index.html");

app.Run();