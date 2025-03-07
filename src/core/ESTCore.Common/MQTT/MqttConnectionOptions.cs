﻿// Decompiled with JetBrains decompiler
// Type: ESTCore.Common.MQTT.MqttConnectionOptions
// Assembly: ESTCore.Common, Version=9.7.0.0, Culture=neutral, PublicKeyToken=cdb2261fa039ed67
// MVID: 5E8BF708-20B8-4DD6-9DCA-9D9885AC7B2C
// Assembly location: F:\开发\工具开发\hls2\ESTCore.Common.dll

using ESTCore.Common.Core;

using System;

namespace ESTCore.Common.MQTT
{
    /// <summary>
    /// 连接MQTT服务器的一些参数信息，适用<see cref="T:ESTCore.Common.MQTT.MqttClient" />消息发布订阅客户端以及<see cref="T:ESTCore.Common.MQTT.MqttSyncClient" />同步请求客户端。<br />
    /// Some parameter information for connecting to the MQTT server is applicable to the <see cref="T:ESTCore.Common.MQTT.MqttClient" /> message publishing and subscription client and the <see cref="T:ESTCore.Common.MQTT.MqttSyncClient" /> synchronization request client.
    /// </summary>
    public class MqttConnectionOptions
    {
        private string ipAddress = "127.0.0.1";

        /// <summary>
        /// 实例化一个默认的对象<br />
        /// Instantiate a default object
        /// </summary>
        public MqttConnectionOptions()
        {
            this.ClientId = string.Empty;
            this.IpAddress = "127.0.0.1";
            this.Port = 1883;
            this.KeepAlivePeriod = TimeSpan.FromSeconds(100.0);
            this.KeepAliveSendInterval = TimeSpan.FromSeconds(30.0);
            this.CleanSession = true;
            this.ConnectTimeout = 5000;
        }

        /// <summary>
        /// Mqtt服务器的ip地址<br />
        /// IP address of Mqtt server
        /// </summary>
        public string IpAddress
        {
            get => this.ipAddress;
            set => this.ipAddress = EstHelper.GetIpAddressFromInput(value);
        }

        /// <summary>
        /// 端口号。默认1883<br />
        /// The port number. Default 1883
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 客户端的id的标识<br />
        /// ID of the client
        /// </summary>
        /// <remarks>实际在传输的时候，采用的是UTF8编码的方式来实现。</remarks>
        public string ClientId { get; set; }

        /// <summary>
        /// 连接到服务器的超时时间，默认是5秒，单位是毫秒<br />
        /// The timeout period for connecting to the server, the default is 5 seconds, the unit is milliseconds
        /// </summary>
        public int ConnectTimeout { get; set; }

        /// <summary>
        /// 登录服务器的凭证，包含用户名和密码，可以为空<br />
        /// The credentials for logging in to the server, including the username and password, can be null
        /// </summary>
        public MqttCredential Credentials { get; set; }

        /// <summary>
        /// 设置的参数，最小单位为1s，当超过设置的时间间隔没有发送数据的时候，必须发送PINGREQ报文，否则服务器认定为掉线。<br />
        /// The minimum unit of the set parameter is 1s. When no data is sent beyond the set time interval, the PINGREQ message must be sent, otherwise the server considers it to be offline.
        /// </summary>
        /// <remarks>
        /// 保持连接（Keep Alive）是一个以秒为单位的时间间隔，表示为一个16位的字，它是指在客户端传输完成一个控制报文的时刻到发送下一个报文的时刻，
        /// 两者之间允许空闲的最大时间间隔。客户端负责保证控制报文发送的时间间隔不超过保持连接的值。如果没有任何其它的控制报文可以发送，
        /// 客户端必须发送一个PINGREQ报文，详细参见 [MQTT-3.1.2-23]
        /// </remarks>
        public TimeSpan KeepAlivePeriod { get; set; }

        /// <summary>
        /// 获取或是设置心跳时间的发送间隔。默认30秒钟<br />
        /// Get or set the heartbeat time interval. 30 seconds by default
        /// </summary>
        public TimeSpan KeepAliveSendInterval { get; set; }

        /// <summary>
        /// 是否清理会话，如果清理会话（CleanSession）标志被设置为1，客户端和服务端必须丢弃之前的任何会话并开始一个新的会话。
        /// 会话仅持续和网络连接同样长的时间。与这个会话关联的状态数据不能被任何之后的会话重用 [MQTT-3.1.2-6]。默认为清理会话。<br />
        /// Whether to clean the session. If the CleanSession flag is set to 1, the client and server must discard any previous session and start a new session.
        /// The session only lasts as long as the network connection. The state data associated with this session cannot be reused by any subsequent sessions [MQTT-3.1.2-6].
        /// The default is to clean up the session.
        /// </summary>
        public bool CleanSession { get; set; }
    }
}
