﻿// Decompiled with JetBrains decompiler
// Type: ESTCore.Common.Core.IMessage.AlienMessage
// Assembly: ESTCore.Common, Version=9.7.0.0, Culture=neutral, PublicKeyToken=cdb2261fa039ed67
// MVID: 5E8BF708-20B8-4DD6-9DCA-9D9885AC7B2C
// Assembly location: F:\开发\工具开发\hls2\ESTCore.Common.dll

namespace ESTCore.Common.Core.IMessage
{
    /// <summary>异形消息对象，用于异形客户端的注册包接收以及验证使用</summary>
    public class AlienMessage : INetMessage
    {
        /// <inheritdoc cref="P:ESTCore.Common.Core.IMessage.INetMessage.ProtocolHeadBytesLength" />
        public int ProtocolHeadBytesLength => 5;

        /// <inheritdoc cref="P:ESTCore.Common.Core.IMessage.INetMessage.HeadBytes" />
        public byte[] HeadBytes { get; set; }

        /// <inheritdoc cref="P:ESTCore.Common.Core.IMessage.INetMessage.ContentBytes" />
        public byte[] ContentBytes { get; set; }

        /// <inheritdoc cref="M:ESTCore.Common.Core.IMessage.INetMessage.CheckHeadBytesLegal(System.Byte[])" />
        public bool CheckHeadBytesLegal(byte[] token) => this.HeadBytes != null && (this.HeadBytes[0] == (byte)72 && this.HeadBytes[1] == (byte)115 && this.HeadBytes[2] == (byte)110);

        /// <inheritdoc cref="M:ESTCore.Common.Core.IMessage.INetMessage.GetContentLengthByHeadBytes" />
        public int GetContentLengthByHeadBytes() => (int)this.HeadBytes[4];

        /// <inheritdoc cref="M:ESTCore.Common.Core.IMessage.INetMessage.GetHeadBytesIdentity" />
        public int GetHeadBytesIdentity() => 0;

        /// <inheritdoc cref="P:ESTCore.Common.Core.IMessage.INetMessage.SendBytes" />
        public byte[] SendBytes { get; set; }
    }
}
