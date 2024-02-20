using Ivony.Http;

using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.Extensions.Logging;

internal class HttpProxy
{
  private readonly ILogger<HttpProxy> _logger;

  public HttpProxy( ILogger<HttpProxy> logger )
  {
    _logger = logger;
  }

  public static async Task ProcessConnection( ConnectionContext context )
  {

    var reader = new HttpReader( context.Transport.Input );


  }
}