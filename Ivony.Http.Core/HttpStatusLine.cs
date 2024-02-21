using System.Buffers;
using System.Collections;
using System.Runtime.CompilerServices;

namespace Ivony.Http;

public readonly struct HttpStatusLine : IEnumerable<ReadOnlyMemory<char>>
{
  private readonly ReadOnlyMemory<char> _version;
  private readonly int _code;
  private readonly ReadOnlyMemory<char> _phrase;

  public ReadOnlySpan<char> ProtocolVersion => _version.Span;

  public int Code => _code;

  public ReadOnlySpan<char> Phrase => _phrase.Span;



  public HttpStatusLine( ReadOnlyMemory<char> version, int code, ReadOnlyMemory<char> phrase )
  {
    if ( code < 100 || code >= 600 )
      throw new ArgumentOutOfRangeException( nameof( code ) );

    _version = version;
    _code = code;
    _phrase = phrase;
  }

  public HttpStatusLine( string line ) : this( line.AsMemory() ) { }


  private readonly ReadOnlyMemory<char> _rawMemory = ReadOnlyMemory<char>.Empty;

  public HttpStatusLine( ReadOnlyMemory<char> line )
  {
    if ( line.Span.Contains( HttpProtocalConstant.Newline.AsSpan(), StringComparison.Ordinal ) )
      throw new FormatException();

    _rawMemory = line;

    var span = line.Span;
    var index = span.IndexOf( ' ' );
    if ( index < 0 )
      throw new FormatException();

    _version = line[..index++];
    span = span[index..];

    index = span.IndexOf( ' ' );
    if ( index < 0 )
    {
      _code = int.Parse( span );
      return;
    }

    _code = int.Parse( span[..index++] );
    span = span[index..];

    _phrase = line[^span.Length..];

  }

  public int Length => _rawMemory.IsEmpty == false ? _rawMemory.Length : _version.Length + _phrase.Length + 2 + 3;//2 of sprator whitespace and 3 of code


  public static readonly HttpStatusLine OK = new HttpStatusLine( "HTTP/1.1 200 OK" );


  public IEnumerator<ReadOnlyMemory<char>> GetEnumerator()
  {
    yield return _version;
    yield return HttpProtocalConstant.WhiteSpace.AsMemory();
    yield return _code.ToString().AsMemory();
    yield return HttpProtocalConstant.WhiteSpace.AsMemory();
    yield return _phrase;
  }


  IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
