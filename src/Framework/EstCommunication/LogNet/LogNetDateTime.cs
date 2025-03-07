﻿// Decompiled with JetBrains decompiler
// Type: EstCommunication.LogNet.LogNetDateTime
// Assembly: EstCommunication, Version=9.7.0.0, Culture=neutral, PublicKeyToken=cdb2261fa039ed67
// MVID: 5E8BF708-20B8-4DD6-9DCA-9D9885AC7B2C
// Assembly location: F:\开发\工具开发\hls2\EstCommunication.dll

using System;
using System.Globalization;
using System.IO;

namespace EstCommunication.LogNet
{
  /// <summary>
  /// 一个日志组件，可以根据时间来区分不同的文件存储<br />
  /// A log component that can distinguish different file storages based on time
  /// </summary>
  /// <remarks>
  /// 此日志实例将根据日期时间来进行分类，支持的时间分类如下：
  /// <list type="number">
  /// <item>分钟</item>
  /// <item>小时</item>
  /// <item>天</item>
  /// <item>周</item>
  /// <item>月份</item>
  /// <item>季度</item>
  /// <item>年份</item>
  /// </list>
  /// 当然你也可以指定允许存在多少个日志文件，比如我允许存在最多10个文件，如果你的日志是根据天来分文件的，那就是10天的数据。
  /// 同理，如果你的日志是根据年来分文件的，那就是10年的日志文件。
  /// </remarks>
  /// <example>
  /// <code lang="cs" source="EstCommunication_Net45.Test\Documentation\Samples\LogNet\LogNetSingle.cs" region="Example3" title="日期存储实例化" />
  /// <code lang="cs" source="EstCommunication_Net45.Test\Documentation\Samples\LogNet\LogNetSingle.cs" region="Example4" title="基本的使用" />
  /// <code lang="cs" source="EstCommunication_Net45.Test\Documentation\Samples\LogNet\LogNetSingle.cs" region="Example5" title="所有日志不存储" />
  /// <code lang="cs" source="EstCommunication_Net45.Test\Documentation\Samples\LogNet\LogNetSingle.cs" region="Example6" title="仅存储ERROR等级" />
  /// <code lang="cs" source="EstCommunication_Net45.Test\Documentation\Samples\LogNet\LogNetSingle.cs" region="Example7" title="不指定路径" />
  /// </example>
  public class LogNetDateTime : LogPathBase, ILogNet, IDisposable
  {
    private GenerateMode generateMode = GenerateMode.ByEveryYear;

    /// <summary>
    /// 实例化一个根据时间存储的日志组件，需要指定每个文件的存储时间范围<br />
    /// Instantiate a log component based on time, you need to specify the storage time range for each file
    /// </summary>
    /// <param name="filePath">文件存储的路径</param>
    /// <param name="generateMode">存储文件的间隔</param>
    /// <param name="fileQuantity">指定当前的日志文件数量上限，如果小于0，则不限制，文件一直增加，如果设置为10，就限制最多10个文件，会删除最近时间的10个文件之外的文件。</param>
    public LogNetDateTime(string filePath, GenerateMode generateMode = GenerateMode.ByEveryYear, int fileQuantity = -1)
    {
      this.filePath = filePath;
      this.generateMode = generateMode;
      this.LogSaveMode = LogSaveMode.Time;
      this.controlFileQuantity = fileQuantity;
      if (string.IsNullOrEmpty(filePath) || Directory.Exists(filePath))
        return;
      Directory.CreateDirectory(filePath);
    }

    /// <inheritdoc />
    protected override string GetFileSaveName()
    {
      if (string.IsNullOrEmpty(this.filePath))
        return string.Empty;
      switch (this.generateMode)
      {
        case GenerateMode.ByEveryMinute:
          return Path.Combine(this.filePath, "Logs_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".txt");
        case GenerateMode.ByEveryHour:
          return Path.Combine(this.filePath, "Logs_" + DateTime.Now.ToString("yyyyMMdd_HH") + ".txt");
        case GenerateMode.ByEveryDay:
          return Path.Combine(this.filePath, "Logs_" + DateTime.Now.ToString("yyyyMMdd") + ".txt");
        case GenerateMode.ByEveryWeek:
          int weekOfYear = new GregorianCalendar().GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
          return Path.Combine(this.filePath, "Logs_" + DateTime.Now.Year.ToString() + "_W" + weekOfYear.ToString() + ".txt");
        case GenerateMode.ByEveryMonth:
          return Path.Combine(this.filePath, "Logs_" + DateTime.Now.ToString("yyyy_MM") + ".txt");
        case GenerateMode.ByEverySeason:
          string filePath = this.filePath;
          string[] strArray = new string[5]
          {
            "Logs_",
            null,
            null,
            null,
            null
          };
          DateTime now = DateTime.Now;
          strArray[1] = now.Year.ToString();
          strArray[2] = "_Q";
          now = DateTime.Now;
          strArray[3] = (now.Month / 3 + 1).ToString();
          strArray[4] = ".txt";
          string path2 = string.Concat(strArray);
          return Path.Combine(filePath, path2);
        case GenerateMode.ByEveryYear:
          return Path.Combine(this.filePath, "Logs_" + DateTime.Now.Year.ToString() + ".txt");
        default:
          return string.Empty;
      }
    }

    /// <inheritdoc />
    public override string ToString() => string.Format("LogNetDateTime[{0}]", (object) this.generateMode);
  }
}
