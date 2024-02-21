using System.Text;

using Ivony.Http;

using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;


internal class HttpProxy
{
  private readonly ILogger<HttpProxy> _logger;

  public HttpProxy( ILogger<HttpProxy> logger )
  {
    _logger = logger;
  }


  internal async ValueTask<bool> TryHandleConnection( ConnectionContext context )
  {
    var reader = new HttpReader( context.Transport.Input, true );
    var request = await reader.ReadHttpRequestLine();
    if ( request.IsMethod( HttpMethod.Connect ) )
    {
      await ConnectTo( context, request.PathAndQuery.ToString(), await reader.ReadHttpHeaderLines() );

      return true;
    }
    else
      return false;


  }

  /// <summary>
  /// 连接到指定的地址
  /// </summary>
  /// <param name="context"></param>
  /// <param name="path"></param>
  /// <returns></returns>
  /// <exception cref="NotImplementedException"></exception>
  protected virtual async Task ConnectTo( ConnectionContext context, string path, IReadOnlyList<HttpHeaderLine> headers )
  {

    var writer = new HttpWriter( context.Transport.Output );
    writer.WriteLine( HttpStatusLine.OK );
    writer.WriteLine();


    while ( true )
    {
      var result = await context.Transport.Input.ReadAsync();

      if ( result.IsCompleted )
        return;

      Console.Write( Encoding.ASCII.GetString( result.Buffer ) );
      context.Transport.Input.AdvanceTo( result.Buffer.End );

    }
  }
}
