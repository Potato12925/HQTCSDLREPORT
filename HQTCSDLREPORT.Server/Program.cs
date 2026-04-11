using DevExpress.AspNetCore;
using DevExpress.AspNetCore.Reporting;
using DevExpress.XtraReports.Web.Extensions;
using HQTCSDL.Services;
using HQTCSDLREPORT.Server.Services;
using System;

var builder = WebApplication.CreateBuilder(args);

// ================= SERVICES =================
builder.Services.AddControllers();
builder.Services.AddDevExpressControls();
builder.Services.ConfigureReportingServices(configurator =>
{
    if (builder.Environment.IsDevelopment())
    {
        configurator.UseDevelopmentMode();
    }

    configurator.ConfigureWebDocumentViewer(viewerConfigurator =>
    {
        viewerConfigurator.UseCachedReportSourceBuilder();
    });
});
builder.Services.AddScoped<ReportStorageWebExtension, CustomReportStorageWebExtension>();
builder.Services.AddSingleton<SqlReportStore>();
builder.Services.AddSingleton<MetadataService>();

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
app.UseDevExpressControls();

app.Use(async (context, next) =>
{
    var acceptHeader = context.Request.Headers.Accept.ToString();
    var isDirectViewerNavigation =
        HttpMethods.IsGet(context.Request.Method) &&
        context.Request.Path.Equals("/DXXRDV", StringComparison.OrdinalIgnoreCase) &&
        !context.Request.QueryString.HasValue &&
        acceptHeader.Contains("text/html", StringComparison.OrdinalIgnoreCase);

    if (isDirectViewerNavigation)
    {
        context.Response.Redirect("/");
        return;
    }

    await next();
});

app.UseRouting();

app.UseCors("AllowAll");

app.UseSession();   // phải trước MapControllers nếu dùng session

app.UseAuthorization();

// ================= ENDPOINTS =================
app.MapControllers();
app.MapFallbackToFile("/index.html");

app.Run();
