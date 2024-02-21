using System.IO.Pipelines;

internal class HttpResponseWriter
{
  private PipeWriter output;

  public HttpResponseWriter( PipeWriter output )
  {
    this.output = output;
  }
}