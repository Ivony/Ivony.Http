using System.Formats.Asn1;
using System.IO.Pipelines;
using System.Text;

namespace Ivony.Http;


public class HttpWriter( PipeWriter HttpPipeWriter )
{


  private static readonly Encoding Encoding = Encoding.ASCII;

  private static readonly ReadOnlyMemory<byte> newline = Encoding.GetBytes( HttpProtocalConstant.Newline ).AsMemory();



  /// <summary>
  /// 向 HTTP 响应中写入一个空行
  /// </summary>
  public void WriteLine()
  {
    var target = HttpPipeWriter.GetSpan( newline.Length );
    newline.Span.CopyTo( target );
    HttpPipeWriter.Advance( newline.Length );
  }

  /// <summary>
  /// 向 HTTP 响应流中写入状态行
  /// </summary>
  /// <param name="line"></param>
  /// <returns></returns>
  public void WriteLine( HttpStatusLine line )
  {
    var target = HttpPipeWriter.GetSpan( line.Length + newline.Length );
    foreach ( var memory in line )
    {
      var length = Encoding.GetBytes( memory.Span, target );
      target = target.Slice( length );
    }

    newline.Span.CopyTo( target );

    HttpPipeWriter.Advance( line.Length + newline.Length );
  }

  /// <summary>
  /// 向HTTP相应中写入一行文本数据
  /// </summary>
  /// <param name="line"></param>
  /// <returns></returns>
  public void WriteLine( ReadOnlySpan<char> line, Encoding? encoding = null )
  {
    encoding ??= Encoding.UTF8;

    var buffer = HttpPipeWriter.GetSpan( line.Length + newline.Length );
    var count = encoding.GetBytes( line, buffer );
    newline.Span.CopyTo( buffer.Slice( count ) );

    HttpPipeWriter.Advance( count + newline.Length );
  }

}