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
