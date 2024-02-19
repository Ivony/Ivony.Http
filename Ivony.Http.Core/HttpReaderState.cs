using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ivony.Http;
public enum HttpReaderState
{
  /// <summary>尚未开始读取任何内容</summary>
  NotStarted,
  /// <summary>已经开始读取到头部</summary>
  Headers,
  /// <summary>已经开始读取到主体</summary>
  Body,
}
