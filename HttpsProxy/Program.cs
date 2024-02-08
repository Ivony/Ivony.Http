using System.Net;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

var builder = WebHost.CreateDefaultBuilder();

builder.UseKestrel( kestrel => kestrel.ListenLocalhost( 5000, options => options.UseConnectionLogging() ) );
builder.ConfigureServices( services =>
{

  services.AddLogging();
  services.AddHttpLogging( options => options.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All );
  services.AddSingleton<HttpProxy>();

} );
builder.Configure( builder =>
{

  builder.UseMiddleware<HttpProxy>();
  builder.UseHttpLogging();


} );
var host = builder.Build();
await host.RunAsync();


