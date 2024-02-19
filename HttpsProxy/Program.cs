using Ivony.Http;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

var builder = WebHost.CreateDefaultBuilder();

builder.UseKestrel( kestrel => kestrel.ListenLocalhost( 5000, options =>
{
  options.UseConnectionLogging();
  options.Use( continuation => async context =>
  {

    var reader = new HttpReader( context.Transport.Input );
    var line = await reader.TryReadLine();
    if ( line != null )
    {

      var request = new HttpRequestLine( line );
      if ( request.IsMethod( HttpMethod.Connect ) )
        await HttpProxy.ProcessConnection( context );

      return;

    }
    await continuation( context );
  } );
} ) );


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


