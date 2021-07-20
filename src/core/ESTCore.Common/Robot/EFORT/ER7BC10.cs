﻿// Decompiled with JetBrains decompiler
// Type: ESTCore.Common.Robot.EFORT.ER7BC10
// Assembly: ESTCore.Common, Version=9.7.0.0, Culture=neutral, PublicKeyToken=cdb2261fa039ed67
// MVID: 5E8BF708-20B8-4DD6-9DCA-9D9885AC7B2C
// Assembly location: F:\开发\工具开发\hls2\ESTCore.Common.dll

using ESTCore.Common.BasicFramework;
using ESTCore.Common.Core;
using ESTCore.Common.Core.IMessage;
using ESTCore.Common.Core.Net;
using ESTCore.Common.Reflection;

using Newtonsoft.Json;

using System;
using System.Text;
using System.Threading.Tasks;

namespace ESTCore.Common.Robot.EFORT
{
    /// <summary>
    /// 埃夫特机器人对应型号为ER7B-C10，此协议为定制版，新版报文对齐<br />
    /// The corresponding model of efort robot is er7b-c10. This protocol is the customized version, and the new version is the message alignment
    /// </summary>
    /// <remarks>针对的控制器是KEBA控制器</remarks>
    public class ER7BC10 : NetworkDoubleBase, IRobotNet
    {
        private SoftIncrementCount softIncrementCount;

        /// <summary>
        /// 实例化一个默认的对象，并指定IP地址和端口号，端口号通常为8008<br />
        /// Instantiate a default object and specify the IP address and port number, usually 8008
        /// </summary>
        /// <param name="ipAddress">Ip地址</param>
        /// <param name="port">端口号</param>
        public ER7BC10(string ipAddress, int port)
        {
            this.IpAddress = ipAddress;
            this.Port = port;
            this.ByteTransform = (IByteTransform)new RegularByteTransform();
            this.softIncrementCount = new SoftIncrementCount((long)ushort.MaxValue);
        }

        /// <inheritdoc />
        protected override INetMessage GetNewNetMessage() => (INetMessage)new EFORTMessage();

        /// <summary>
        /// 获取发送的消息的命令<br />
        /// Gets the command to send the message
        /// </summary>
        /// <returns>字节数组命令</returns>
        public byte[] GetReadCommand()
        {
            byte[] numArray = new byte[38];
            Encoding.ASCII.GetBytes("MessageHead").CopyTo((Array)numArray, 0);
            BitConverter.GetBytes((ushort)numArray.Length).CopyTo((Array)numArray, 16);
            BitConverter.GetBytes((ushort)1001).CopyTo((Array)numArray, 18);
            BitConverter.GetBytes((ushort)this.softIncrementCount.GetCurrentValue()).CopyTo((Array)numArray, 20);
            Encoding.ASCII.GetBytes("MessageTail").CopyTo((Array)numArray, 22);
            return numArray;
        }

        /// <inheritdoc cref="M:ESTCore.Common.Core.Net.IRobotNet.Read(System.String)" />
        [EstMqttApi(ApiTopic = "ReadRobotByte", Description = "Read the robot's original byte data information according to the address")]
        public OperateResult<byte[]> Read(string address) => this.ReadFromCoreServer(this.GetReadCommand());

        /// <inheritdoc cref="M:ESTCore.Common.Core.Net.IRobotNet.ReadString(System.String)" />
        [EstMqttApi(ApiTopic = "ReadRobotString", Description = "Read the string data information of the robot based on the address")]
        public OperateResult<string> ReadString(string address)
        {
            OperateResult<EfortData> operateResult = this.ReadEfortData();
            return !operateResult.IsSuccess ? OperateResult.CreateFailedResult<string>((OperateResult)operateResult) : OperateResult.CreateSuccessResult<string>(JsonConvert.SerializeObject((object)operateResult.Content, Formatting.Indented));
        }

        /// <summary>
        /// 本机器人不支持该方法操作，将永远返回失败，无效的操作<br />
        /// This robot does not support this method operation, will always return failed, invalid operation
        /// </summary>
        /// <param name="address">指定的地址信息，有些机器人可能不支持</param>
        /// <param name="value">原始的字节数据信息</param>
        /// <returns>是否成功的写入</returns>
        [EstMqttApi(ApiTopic = "WriteRobotByte", Description = "This robot does not support this method operation, will always return failed, invalid operation")]
        public OperateResult Write(string address, byte[] value) => new OperateResult(StringResources.Language.NotSupportedFunction);

        /// <summary>
        /// 本机器人不支持该方法操作，将永远返回失败，无效的操作<br />
        /// This robot does not support this method operation, will always return failed, invalid operation
        /// </summary>
        /// <param name="address">指定的地址信息，有些机器人可能不支持</param>
        /// <param name="value">字符串的数据信息</param>
        /// <returns>是否成功的写入</returns>
        [EstMqttApi(ApiTopic = "WriteRobotString", Description = "This robot does not support this method operation, will always return failed, invalid operation")]
        public OperateResult Write(string address, string value) => new OperateResult(StringResources.Language.NotSupportedFunction);

        /// <summary>
        /// 读取机器人的详细信息，返回解析后的数据类型<br />
        /// Read the details of the robot and return the resolved data type
        /// </summary>
        /// <returns>结果数据信息</returns>
        [EstMqttApi(Description = "Read the details of the robot and return the resolved data type")]
        public OperateResult<EfortData> ReadEfortData()
        {
            OperateResult<byte[]> operateResult = this.Read("");
            return !operateResult.IsSuccess ? OperateResult.CreateFailedResult<EfortData>((OperateResult)operateResult) : EfortData.PraseFrom(operateResult.Content);
        }

        /// <inheritdoc cref="M:ESTCore.Common.Core.Net.IRobotNet.ReadAsync(System.String)" />
        public async Task<OperateResult<byte[]>> ReadAsync(string address)
        {
            OperateResult<byte[]> operateResult = await this.ReadFromCoreServerAsync(this.GetReadCommand());
            return operateResult;
        }

        /// <inheritdoc cref="M:ESTCore.Common.Core.Net.IRobotNet.ReadStringAsync(System.String)" />
        public async Task<OperateResult<string>> ReadStringAsync(string address)
        {
            OperateResult<EfortData> read = await this.ReadEfortDataAsync();
            OperateResult<string> operateResult = read.IsSuccess ? OperateResult.CreateSuccessResult<string>(JsonConvert.SerializeObject((object)read.Content, Formatting.Indented)) : OperateResult.CreateFailedResult<string>((OperateResult)read);
            read = (OperateResult<EfortData>)null;
            return operateResult;
        }

        /// <inheritdoc cref="M:ESTCore.Common.Robot.EFORT.ER7BC10.Write(System.String,System.Byte[])" />
        public async Task<OperateResult> WriteAsync(string address, byte[] value) => new OperateResult(StringResources.Language.NotSupportedFunction);

        /// <inheritdoc cref="M:ESTCore.Common.Robot.EFORT.ER7BC10.Write(System.String,System.String)" />
        public async Task<OperateResult> WriteAsync(string address, string value) => new OperateResult(StringResources.Language.NotSupportedFunction);

        /// <inheritdoc cref="M:ESTCore.Common.Robot.EFORT.ER7BC10.ReadEfortData" />
        public async Task<OperateResult<EfortData>> ReadEfortDataAsync()
        {
            OperateResult<byte[]> read = await this.ReadAsync("");
            OperateResult<EfortData> operateResult = read.IsSuccess ? EfortData.PraseFrom(read.Content) : OperateResult.CreateFailedResult<EfortData>((OperateResult)read);
            read = (OperateResult<byte[]>)null;
            return operateResult;
        }

        /// <inheritdoc />
        public override string ToString() => string.Format("ER7BC10 Robot[{0}:{1}]", (object)this.IpAddress, (object)this.Port);
    }
}
