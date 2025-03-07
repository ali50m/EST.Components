﻿// Decompiled with JetBrains decompiler
// Type: EstCommunication.Profinet.Beckhoff.BeckhoffCommandId
// Assembly: EstCommunication, Version=9.7.0.0, Culture=neutral, PublicKeyToken=cdb2261fa039ed67
// MVID: 5E8BF708-20B8-4DD6-9DCA-9D9885AC7B2C
// Assembly location: F:\开发\工具开发\hls2\EstCommunication.dll

namespace EstCommunication.Profinet.Beckhoff
{
  /// <summary>
  /// 倍福PLC的命令码<br />
  /// Command Id: https://infosys.beckhoff.com/english.php?content=../content/1033/tc3_ads_intro/index.html
  /// </summary>
  public class BeckhoffCommandId
  {
    /// <summary>
    /// 读取PLC的名字和版本号等信息<br />
    /// Reads the name and the version number of the ADS device.
    /// </summary>
    public const ushort ReadDeviceInfo = 1;
    /// <summary>
    /// 可以读ADS的设备读取数据<br />
    /// With ADS Read data can be read from an ADS device
    /// </summary>
    public const ushort Read = 2;
    /// <summary>
    /// 将ADS数据写入到ADS的设备里去<br />
    /// With ADS Write data can be written to an ADS device
    /// </summary>
    public const ushort Write = 3;
    /// <summary>
    /// 读取ADS设备里的设备状态信息和ADS状态<br />
    /// Reads the ADS status and the device status of an ADS device.
    /// </summary>
    public const ushort ReadState = 4;
    /// <summary>
    /// 更改ADS设备的设备状态信息和ADS状态<br />
    /// Changes the ADS status and the device status of an ADS device.
    /// </summary>
    public const ushort WriteControl = 5;
    /// <summary>
    /// 在ADS设备里面创建一个通知对象<br />
    /// A notification is created in an ADS device
    /// </summary>
    public const ushort AddDeviceNotification = 6;
    /// <summary>
    /// 删除ADS设备里的一个通知对象<br />
    /// One before defined notification is deleted in an ADS device.
    /// </summary>
    public const ushort DeleteDeviceNotification = 7;
    /// <summary>
    /// 从ADS设备订阅一个数据的通知，将会发送到客户端<br />
    /// Data will carry forward independently from an ADS device to a Client
    /// </summary>
    public const ushort DeviceNotification = 8;
    /// <summary>
    /// 在写入的时候进行同时的读取<br />
    /// With ADS ReadWrite data will be written to an ADS device. Additionally, data can be read from the ADS device.
    /// </summary>
    public const ushort ReadWrite = 9;
  }
}
