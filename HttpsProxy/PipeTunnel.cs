using System.IO.Pipelines;

internal class PipeTunnel( IDuplexPipe upStream, Stream downStream )
{

  public Task TransportAsync( CancellationToken cancellationToken = default )
  {

    var upload = UploadAsync( cancellationToken );
    var download = DownloadAsync( cancellationToken );

    return Task.WhenAll( upload, download );
  }

  private Task DownloadAsync( CancellationToken cancellationToken )
  {
    return PipeToStream( upStream, downStream, cancellationToken );
  }

  public static async Task PipeToStream( IDuplexPipe upStream, Stream downStream, CancellationToken cancellationToken )
  {
    while ( true )
    {
      var result = await upStream.Input.ReadAsync( cancellationToken );
      foreach ( var memory in result.Buffer )
        await downStream.WriteAsync( memory, cancellationToken );

      upStream.Input.AdvanceTo( result.Buffer.End );

      Console.WriteLine( $"[DOWN] {result.Buffer.Length}bytes" );

      if ( result.IsCompleted )
        return;
    }
  }

  private Task UploadAsync( CancellationToken cancellationToken )
  {
    return StreamToPipe( upStream, downStream, cancellationToken );
  }

  private static async Task StreamToPipe( IDuplexPipe upStream, Stream downStream, CancellationToken cancellationToken )
  {
    while ( true )
    {

      var buffer = upStream.Output.GetMemory();
      var count = await downStream.ReadAsync( buffer, cancellationToken );

      Console.WriteLine( $"[UP] {count}bytes" );

      if ( count == 0 )//is completed.
        break;

      upStream.Output.Advance( count );
      await upStream.Output.FlushAsync();
    }

    upStream.Output.Complete();
  }
}