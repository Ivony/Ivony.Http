using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Ivony.Http;


/// <summary>
/// 定义 HTTP 头部行
/// </summary>
public readonly struct HttpHeaderLine
{
  private readonly int delimiterIndex;

  /// <summary>
  /// 头部名
  /// </summary>
  public ReadOnlySpan<char> Name => RawText.AsSpan( ..delimiterIndex );

  /// <summary>
  /// 头部值
  /// </summary>
  public ReadOnlySpan<char> Value => RawText.AsSpan( (delimiterIndex + 1).. );

  /// <summary>
  /// 原始字符串
  /// </summary>
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
public readonly struct HttpRequestLine
{

  public string RawText { get; }
  private readonly int delimiter1;
  private readonly int delimiter2;


  /// <summary>
  /// 创建 <see cref="HttpRequestLine"/> 对象实例
  /// </summary>
  /// <param name="requestLine">请求行文本</param>
  /// <exception cref="ArgumentNullException"><paramref name="requestLine"/> 为 <see langword="null"/></exception>
  public HttpRequestLine( string requestLine )
  {
    RawText = requestLine ?? throw new ArgumentNullException( nameof( requestLine ) );

    delimiter1 = requestLine.IndexOf( ' ' );
    delimiter2 = requestLine.IndexOf( ' ', delimiter1 + 1 );
  }

  /// <summary>
  /// 请求行为
  /// </summary>
  public ReadOnlySpan<char> Method => RawText.AsSpan( ..delimiter1 );

  /// <summary>
  /// 请求路径
  /// </summary>
  public ReadOnlySpan<char> PathAndQuery => RawText.AsSpan( (delimiter1 + 1)..delimiter2 );

  /// <summary>
  /// 请求版本
  /// </summary>
  public ReadOnlySpan<char> Version => RawText.AsSpan( (delimiter2 + 1).. );


  public bool IsMethod( HttpMethod method ) => string.Equals( new string( Method ), method.ToString(), StringComparison.OrdinalIgnoreCase );
}


public class HttpHeaderLineCollection( IReadOnlyList<HttpHeaderLine> collection ) : IReadOnlyList<HttpHeaderLine>
{
  public HttpHeaderLine this[int index] => collection[index];

  public int Count => collection.Count;

  public IEnumerator<HttpHeaderLine> GetEnumerator() => collection.GetEnumerator();

  IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}



