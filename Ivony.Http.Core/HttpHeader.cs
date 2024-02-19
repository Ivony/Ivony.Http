using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Ivony.Http;


/// <summary>
/// 定义 HTTP 头部行
/// </summary>
/// <param name="Name"></param>
/// <param name="Value"></param>
public readonly struct HttpHeaderLine
{
  private readonly int delimiterIndex;

  public ReadOnlySpan<char> Name => RawText.AsSpan( ..delimiterIndex );
  public ReadOnlySpan<char> Value => RawText.AsSpan( (delimiterIndex + 1).. );

  public string RawText { get; }

  public HttpHeaderLine( string headerLine )
  {
    RawText = headerLine ?? throw new ArgumentNullException( nameof( headerLine ) );
    delimiterIndex = headerLine.IndexOf( ':' );

    if ( delimiterIndex < 0 )
      throw new FormatException();
  }
}

/// <summary>
/// 定义 HTTP 请求行
/// </summary>
/// <param name="Method"></param>
/// <param name="Url"></param>
/// <param name="Version"></param>
public readonly struct HttpRequestLine
{

  public string RawText { get; }
  private readonly int delimiter1;
  private readonly int delimiter2;

  public HttpRequestLine( string requestLine )
  {
    RawText = requestLine ?? throw new ArgumentNullException( nameof( requestLine ) );

    delimiter1 = requestLine.IndexOf( ' ' );
    delimiter2 = requestLine.IndexOf( ' ', delimiter1 + 1 );
  }

  public ReadOnlySpan<char> Method => RawText.AsSpan( ..delimiter1 );
  public ReadOnlySpan<char> Path => RawText.AsSpan( (delimiter1 + 1)..delimiter2 );
  public ReadOnlySpan<char> Version => RawText.AsSpan( (delimiter2 + 1).. );


  public bool IsMethod( HttpMethod method ) => string.Equals( new string( Method ), method.ToString(), StringComparison.OrdinalIgnoreCase );
}
