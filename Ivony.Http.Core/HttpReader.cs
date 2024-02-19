using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Net.Http.Headers;
using System.Text;

namespace Ivony.Http;
public class HttpReader
{


  private static byte CR = (byte) '\r';
  private static byte LF = (byte) '\n';



  public HttpReader( PipeReader reader )
  {
    HttpPipeReader = reader;
  }

  public PipeReader HttpPipeReader { get; }



  public HttpReaderState ReaderState { get; private set; }


  public async ValueTask<HttpRequestLine> ReadHttpRequestLine( CancellationToken cancellationToken = default )
  {


    if ( ReaderState != HttpReaderState.NotStarted )
      throw new InvalidOperationException();

    var line = await TryReadLine( true, cancellationToken );
    if ( line == null )
      throw new FormatException();

    ReaderState = HttpReaderState.Headers;
    return new HttpRequestLine( line );
  }

  public async ValueTask<HttpHeaderLine?> TryReadHttpHeaderLine( CancellationToken cancellationToken = default )
  {
    if ( ReaderState != HttpReaderState.Headers )
      throw new InvalidOperationException();

    var line = await TryReadLine( true, cancellationToken );
    if ( line == null )
      throw new FormatException();


    if ( line == "" )
    {
      ReaderState = HttpReaderState.Body;
      return null;
    }
    else
      return new HttpHeaderLine( line );
  }

  public async ValueTask<IReadOnlyList<HttpHeaderLine>> ReadHttpHeaderLines( CancellationToken cancellationToken = default )
  {

    var list = new List<HttpHeaderLine>();

    while ( true )
    {
      cancellationToken.ThrowIfCancellationRequested();
      var header = await TryReadHttpHeaderLine( cancellationToken );
      if ( header == null )
        break;
      else
        list.Add( header.Value );
    }

    return list.AsReadOnly();
  }

  public async ValueTask<string?> TryReadLine( bool advance = true, CancellationToken cancellationToken = default )
  {
    while ( true )
    {
      var result = await HttpPipeReader.ReadAsync( cancellationToken );
      var buffer = result.Buffer;

      if ( TryReadLine( buffer, out var line ) )
        return line;

      if ( result.IsCompleted )
        return null;
    }
  }

  public static bool TryReadLine( ReadOnlySequence<byte> buffer, [NotNullWhen( true )] out string? line )
  {
    line = null;

    var reader = new SequenceReader<byte>( buffer );
    if ( reader.TryReadTo( out ReadOnlySequence<byte> result, CR, LF ) == false )
      return false;

    line = Encoding.ASCII.GetString( result );
    return true;
  }

}
