using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ivony.Http;
public static class HttpProtocalConstant
{
  public static readonly string Newline = "\r\n";
  public static readonly string WhiteSpace = " ";




  public static readonly byte[] NewlineBytes = Encoding.ASCII.GetBytes( Newline );
  public static readonly byte[] WhitespaceBytes = Encoding.ASCII.GetBytes( WhiteSpace );

}
