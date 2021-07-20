﻿// Decompiled with JetBrains decompiler
// Type: ESTCore.Common.Core.IMessage.EstMessage
// Assembly: ESTCore.Common, Version=9.7.0.0, Culture=neutral, PublicKeyToken=cdb2261fa039ed67
// MVID: 5E8BF708-20B8-4DD6-9DCA-9D9885AC7B2C
// Assembly location: F:\开发\工具开发\hls2\ESTCore.Common.dll

using ESTCore.Common.BasicFramework;

using System;

namespace ESTCore.Common.Core.IMessage
{
    /// <summary>本组件系统使用的默认的消息规则，说明解析和反解析规则的</summary>
    public class EstMessage : INetMessage
    {
        /// <inheritdoc cref="P:ESTCore.Common.Core.IMessage.INetMessage.ProtocolHeadBytesLength" />
        public int ProtocolHeadBytesLength => 32;

        /// <inheritdoc cref="P:ESTCore.Common.Core.IMessage.INetMessage.HeadBytes" />
        public byte[] HeadBytes { get; set; }

        /// <inheritdoc cref="P:ESTCore.Common.Core.IMessage.INetMessage.ContentBytes" />
        public byte[] ContentBytes { get; set; }

        /// <inheritdoc cref="M:ESTCore.Common.Core.IMessage.INetMessage.CheckHeadBytesLegal(System.Byte[])" />
        public bool CheckHeadBytesLegal(byte[] token)
        {
            if (this.HeadBytes == null)
                return false;
            byte[] headBytes = this.HeadBytes;
            return headBytes != null && headBytes.Length >= 32 && SoftBasic.IsTwoBytesEquel(this.HeadBytes, 12, token, 0, 16);
        }

        /// <inheritdoc cref="M:ESTCore.Common.Core.IMessage.INetMessage.GetContentLengthByHeadBytes" />
        public int GetContentLengthByHeadBytes()
        {
            byte[] headBytes = this.HeadBytes;
            return headBytes != null && headBytes.Length >= 32 ? BitConverter.ToInt32(this.HeadBytes, 28) : 0;
        }

        /// <inheritdoc cref="M:ESTCore.Common.Core.IMessage.INetMessage.GetHeadBytesIdentity" />
        public int GetHeadBytesIdentity()
        {
            byte[] headBytes = this.HeadBytes;
            return headBytes != null && headBytes.Length >= 32 ? BitConverter.ToInt32(this.HeadBytes, 4) : 0;
        }

        /// <inheritdoc cref="P:ESTCore.Common.Core.IMessage.INetMessage.SendBytes" />
        public byte[] SendBytes { get; set; }
    }
}
