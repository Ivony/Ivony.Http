using Ivony.Http;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

var builder = WebHost.CreateDefaultBuilder();

builder.UseKestrel( kestrel => kestrel.ListenLocalhost( 5000, options =>
{
  options.UseConnectionLogging();
  options.Use( continuation => async context =>
  {

    if ( await options.ApplicationServices.GetRequiredService<HttpProxy>().TryHandleConnection( context ) == false )
      await continuation( context );
  } );
} ) );


builder.ConfigureServices( services =>
{

  services.AddLogging();
  services.AddHttpLogging( options => options.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All );
  services.AddTransient<HttpProxy>();

} );
builder.Configure( builder =>
{
  builder.UseHttpLogging();
} );
var host = builder.Build();
await host.RunAsync();


