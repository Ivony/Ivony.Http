using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Text;

namespace Ivony.Http;


/// <summary>
/// 定义一个 HTTP 流读取器
/// </summary>
/// <param name="HttpPipeReader">获取读取 Http 请求流的 PipeReader 对象</param>
/// <param name="AutoAdvance">指示该读取器是否会在每读取一行之后自动提交偏移量给 <see cref="HttpPipeReader"/> ，若此属性为 <see langword="false"/> ，则需要自行调用 <see cref="Commit"/> 方法。</param>
public class HttpReader( PipeReader HttpPipeReader, bool AutoAdvance = false )
{


  private SequencePosition offset;



  /// <summary>
  /// 指示当前读取器的状态
  /// </summary>
  public HttpReaderState ReaderState { get; private set; }


  /// <summary>
  /// 读取 HTTP 请求行
  /// </summary>
  /// <param name="cancellationToken">取消标志</param>
  /// <returns>HTTP 请求行</returns>
  /// <exception cref="InvalidOperationException">当前位置不是合法的 HTTP 请求行的位置</exception>
  /// <exception cref="FormatException">HTTP 请求行内容格式不正确</exception>
  public async ValueTask<HttpRequestLine> ReadHttpRequestLine( CancellationToken cancellationToken = default )
  {


    if ( ReaderState != HttpReaderState.NotStarted )
      throw new InvalidOperationException();

    var line = await ReadLine( cancellationToken );
    if ( line == null )
      throw new FormatException();

    ReaderState = HttpReaderState.Headers;
    return new HttpRequestLine( line );
  }

  /// <summary>
  /// 读取 HTTP 头部行
  /// </summary>
  /// <param name="cancellationToken">取消标志</param>
  /// <returns>HTTP 头部行，如果已经读取到头部末尾，则返回 null。</returns>
  /// <exception cref="InvalidOperationException">当前位置不是合法的 HTTP 头部行的位置</exception>
  /// <exception cref="FormatException">HTTP 内容格式不正确</exception>
  public async ValueTask<HttpHeaderLine?> ReadHttpHeaderLine( CancellationToken cancellationToken = default )
  {
    if ( ReaderState != HttpReaderState.Headers )
      throw new InvalidOperationException();

    var line = await ReadLine( cancellationToken );
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
      var header = await ReadHttpHeaderLine( cancellationToken );
      if ( header == null )
        break;
      else
        list.Add( header.Value );
    }

    return list.AsReadOnly();
  }

  /// <summary>
  /// 尝试从 HTTP 请求中读取一行数据
  /// </summary>
  /// <param name="cancellationToken">取消标志</param>
  /// <returns>读取到的行数据，如果已经读取到末尾，则返回 null。</returns>
  public async ValueTask<string?> ReadLine( CancellationToken cancellationToken = default )
  {
    while ( true )
    {
      var result = await HttpPipeReader.ReadAsync( cancellationToken );
      var buffer = result.Buffer;

      if ( TryReadLine( buffer.Slice( offset ), out var line, out offset ) )
      {
        if ( AutoAdvance )
          Commit();
        return line;
      }

      HttpPipeReader.AdvanceTo( buffer.Start, buffer.End );

      if ( result.IsCompleted )
        return null;
    }
  }



  /// <summary>
  /// 将当前读取偏移量提交给 <see cref="HttpPipeReader"/> 。
  /// </summary>
  public void Commit()
  {
    HttpPipeReader.AdvanceTo( offset );
  }



  /// <summary>
  /// 尝试从缓冲中读取一行数据
  /// </summary>
  /// <param name="buffer">要读取的缓冲区</param>
  /// <param name="line">读取到的数据行</param>
  /// <returns>是否读取成功</returns>
  public static bool TryReadLine( ReadOnlySequence<byte> buffer, [NotNullWhen( true )] out string? line, out SequencePosition offset )
  {
    line = null;
    offset = buffer.Start;


    var reader = new SequenceReader<byte>( buffer );
    if ( reader.TryReadTo( out ReadOnlySequence<byte> result, HttpProtocalConstant.NewlineBytes.AsSpan(), true ) == false )
      return false;

    line = Encoding.ASCII.GetString( result );
    offset = reader.Position;
    return true;
  }


  /// <summary>
  /// 快进到请求的主体位置，
  /// </summary>
  /// <returns></returns>
  public async ValueTask ForwardToBody( CancellationToken cancellationToken = default )
  {
    if ( ReaderState == HttpReaderState.NotStarted )
      await ReadHttpRequestLine( cancellationToken );

    if ( ReaderState == HttpReaderState.Headers )
      while ( await ReadHttpHeaderLine( cancellationToken ) != null ) ;
  }
}

public enum HttpReaderState
{
  /// <summary>尚未开始读取任何内容</summary>
  NotStarted,
  /// <summary>已经开始读取到头部</summary>
  Headers,
  /// <summary>已经开始读取到主体</summary>
  Body,
}
