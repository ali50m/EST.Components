﻿// Decompiled with JetBrains decompiler
// Type: ESTCore.Common.Core.Net.FileStateObject
// Assembly: ESTCore.Common, Version=9.7.0.0, Culture=neutral, PublicKeyToken=cdb2261fa039ed67
// MVID: 5E8BF708-20B8-4DD6-9DCA-9D9885AC7B2C
// Assembly location: F:\开发\工具开发\hls2\ESTCore.Common.dll

using System.IO;

namespace ESTCore.Common.Core.Net
{
    /// <summary>文件传送的异步对象</summary>
    internal class FileStateObject : StateOneBase
    {
        /// <summary>操作的流</summary>
        public Stream Stream { get; set; }
    }
}
