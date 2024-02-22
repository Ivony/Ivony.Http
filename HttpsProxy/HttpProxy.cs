using System.IO.Pipelines;
using System.Net.Sockets;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.RegularExpressions;

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

    await writer.HttpPipeWriter.FlushAsync();


    Console.WriteLine( $"CONNECT {path}" );

    var stream = await ConnectTo( path );

    Console.WriteLine( $"connected to {path}" );


    var tunnel = new PipeTunnel( context.Transport, stream );
    await tunnel.TransportAsync();
  }


  private Regex hostnameRegex = new Regex( @"(?<host>[\w\.\-]+)(:(?<port>[0-9]+))", RegexOptions.Singleline | RegexOptions.Compiled );

  private async Task<Stream> ConnectTo( string path )
  {
    (string, int) ParseHost( string hostname )
    {
      var match = hostnameRegex.Match( hostname );
      if ( match.Success == false )
        throw new FormatException();

      return (match.Groups["host"].Value, int.Parse( match.Groups["port"].ValueSpan ));
    }

    var (host, port) = ParseHost( path );

    var client = new TcpClient();
    await client.ConnectAsync( host, port );

    return client.GetStream();
  }
}
