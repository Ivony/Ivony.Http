using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;

internal class HttpProxy : IMiddleware
{
  private readonly ILogger<HttpProxy> _logger;

  public HttpProxy( ILogger<HttpProxy> logger )
  {
    _logger = logger;
  }

  public async Task InvokeAsync( HttpContext context, RequestDelegate next )
  {

    var request = context.Features.Get<IHttpRequestFeature>()!;
    if ( string.Equals( request.Method, "CONNECT", StringComparison.OrdinalIgnoreCase ) )
    {

      var connect = context.Features.Get<IHttpExtendedConnectFeature>()!;
      _logger.LogInformation( "CONNECT" );
      var stream = await connect.AcceptAsync();


      stream.Dispose();

    }


  }
}