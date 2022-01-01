



using Microsoft.AspNetCore.HttpOverrides;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
 
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);

 
// Add services to the container.

builder.Services.AddControllers();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      builder =>
                      {
                          builder.WithOrigins("http://localhost:3000" );
                      });
});


var app = builder.Build();
app.UseForwardedHeaders();
// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.UseCors(MyAllowSpecificOrigins);
app.MapControllers();

app.Run();

app.MapDefaultControllerRoute();
//app.UseEndpoints(endpoints =>
//{
//    endpoints.MapControllerRoute(name: "default", pattern: "{controller}/{action=Index}/{id?}");
//});