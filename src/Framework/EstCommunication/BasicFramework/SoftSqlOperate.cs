﻿// Decompiled with JetBrains decompiler
// Type: EstCommunication.BasicFramework.SoftSqlOperate
// Assembly: EstCommunication, Version=9.7.0.0, Culture=neutral, PublicKeyToken=cdb2261fa039ed67
// MVID: 5E8BF708-20B8-4DD6-9DCA-9D9885AC7B2C
// Assembly location: F:\开发\工具开发\hls2\EstCommunication.dll

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace EstCommunication.BasicFramework
{
  /// <summary>数据库操作的相关类，包含了常用的方法，避免大量的重复代码</summary>
  public static class SoftSqlOperate
  {
    /// <summary>普通的执行SQL语句，并返回影响行数，该方法应该放到try-catch代码块中</summary>
    /// <param name="conStr">数据库的连接字符串</param>
    /// <param name="cmdStr">sql语句，适合插入，更新，删除</param>
    /// <returns>返回受影响的行数</returns>
    /// <exception cref="T:System.Data.SqlClient.SqlException"></exception>
    public static int ExecuteSql(string conStr, string cmdStr)
    {
      using (SqlConnection conn = new SqlConnection(conStr))
      {
        conn.Open();
        return SoftSqlOperate.ExecuteSql(conn, cmdStr);
      }
    }

    /// <summary>普通的执行SQL语句，并返回影响行数，该方法应该放到try-catch代码块中</summary>
    /// <param name="conn">数据库的连接对象</param>
    /// <param name="cmdStr">sql语句，适合插入，更新，删除</param>
    /// <returns>返回受影响的行数</returns>
    /// <exception cref="T:System.Data.SqlClient.SqlException"></exception>
    public static int ExecuteSql(SqlConnection conn, string cmdStr)
    {
      using (SqlCommand sqlCommand = new SqlCommand(cmdStr, conn))
        return sqlCommand.ExecuteNonQuery();
    }

    /// <summary>选择数据表的执行SQL语句，并返回最终数据表，该方法应该放到try-catch代码块中</summary>
    /// <param name="conStr">数据库的连接字符串</param>
    /// <param name="cmdStr">sql语句，选择数据表的语句</param>
    /// <returns>结果数据表</returns>
    /// <exception cref="T:System.Data.SqlClient.SqlException"></exception>
    /// <exception cref="T:System.InvalidOperationException"></exception>
    public static DataTable ExecuteSelectTable(string conStr, string cmdStr)
    {
      using (SqlConnection conn = new SqlConnection(conStr))
      {
        conn.Open();
        return SoftSqlOperate.ExecuteSelectTable(conn, cmdStr);
      }
    }

    /// <summary>选择数据表的执行SQL语句，并返回最终数据表，该方法应该放到try-catch代码块中</summary>
    /// <param name="conn">数据库连接对象</param>
    /// <param name="cmdStr">sql语句，选择数据表的语句</param>
    /// <returns>结果数据表</returns>
    /// <exception cref="T:System.Data.SqlClient.SqlException"></exception>
    public static DataTable ExecuteSelectTable(SqlConnection conn, string cmdStr)
    {
      using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(cmdStr, conn))
      {
        using (DataSet dataSet = new DataSet())
        {
          sqlDataAdapter.Fill(dataSet);
          return dataSet.Tables[0];
        }
      }
    }

    /// <summary>选择指定类型数据集合执行SQL语句，并返回指定类型的数据集合，该方法应该放到try-catch代码块中</summary>
    /// <param name="conStr">数据库的连接字符串</param>
    /// <param name="cmdStr">sql语句，选择数据表的语句</param>
    /// <returns>结果数据集合</returns>
    /// <exception cref="T:System.Data.SqlClient.SqlException"></exception>
    /// <exception cref="T:System.InvalidOperationException"></exception>
    public static List<T> ExecuteSelectEnumerable<T>(string conStr, string cmdStr) where T : ISqlDataType, new()
    {
      using (SqlConnection conn = new SqlConnection(conStr))
      {
        conn.Open();
        return SoftSqlOperate.ExecuteSelectEnumerable<T>(conn, cmdStr);
      }
    }

    /// <summary>选择指定类型数据集合执行SQL语句，并返回指定类型的数据集合，该方法应该放到try-catch代码块中</summary>
    /// <param name="conn">数据库的连接对象</param>
    /// <param name="cmdStr">sql语句，选择数据表的语句</param>
    /// <returns>结果数据集合</returns>
    /// <exception cref="T:System.Data.SqlClient.SqlException"></exception>
    /// <exception cref="T:System.InvalidOperationException"></exception>
    public static List<T> ExecuteSelectEnumerable<T>(SqlConnection conn, string cmdStr) where T : ISqlDataType, new()
    {
      using (SqlCommand sqlCommand = new SqlCommand(cmdStr, conn))
      {
        using (SqlDataReader sdr = sqlCommand.ExecuteReader())
        {
          List<T> objList = new List<T>();
          while (sdr.Read())
          {
            T obj = new T();
            obj.LoadBySqlDataReader(sdr);
            objList.Add(obj);
          }
          return objList;
        }
      }
    }

    /// <summary>更新指定类型数据执行SQL语句，并返回指定类型的数据集合，该方法应该放到try-catch代码块中</summary>
    /// <param name="conStr">数据库的连接字符串</param>
    /// <param name="cmdStr">sql语句，选择数据表的语句</param>
    /// <returns>结果数据</returns>
    /// <exception cref="T:System.Data.SqlClient.SqlException"></exception>
    /// <exception cref="T:System.InvalidOperationException"></exception>
    public static T ExecuteSelectObject<T>(string conStr, string cmdStr) where T : ISqlDataType, new()
    {
      using (SqlConnection conn = new SqlConnection(conStr))
      {
        conn.Open();
        return SoftSqlOperate.ExecuteSelectObject<T>(conn, cmdStr);
      }
    }

    /// <summary>更新指定类型数据执行SQL语句，并返回指定类型的数据集合，该方法应该放到try-catch代码块中</summary>
    /// <param name="conn">数据库的连接对象</param>
    /// <param name="cmdStr">sql语句，选择数据表的语句</param>
    /// <returns>结果数据</returns>
    /// <exception cref="T:System.Data.SqlClient.SqlException"></exception>
    /// <exception cref="T:System.InvalidOperationException"></exception>
    public static T ExecuteSelectObject<T>(SqlConnection conn, string cmdStr) where T : ISqlDataType, new()
    {
      using (SqlCommand sqlCommand = new SqlCommand(cmdStr, conn))
      {
        using (SqlDataReader sdr = sqlCommand.ExecuteReader())
        {
          if (!sdr.Read())
            return default (T);
          T obj = new T();
          obj.LoadBySqlDataReader(sdr);
          return obj;
        }
      }
    }

    /// <summary>用于选择聚合函数值的方法，例如Count，Average，Max，Min，Sum等最终只有一个结果值的对象</summary>
    /// <param name="conStr">数据库的连接字符串</param>
    /// <param name="cmdStr">sql语句，选择数据表的语句</param>
    /// <returns>返回的int数据</returns>
    public static int ExecuteSelectCount(string conStr, string cmdStr)
    {
      using (SqlConnection conn = new SqlConnection(conStr))
      {
        conn.Open();
        return SoftSqlOperate.ExecuteSelectCount(conn, cmdStr);
      }
    }

    /// <summary>用于选择聚合函数值的方法，例如Count，Average，Max，Min，Sum等最终只有一个结果值的对象</summary>
    /// <param name="conn">数据库的连接对象</param>
    /// <param name="cmdStr">sql语句，选择数据表的语句</param>
    /// <returns>返回的int数据</returns>
    public static int ExecuteSelectCount(SqlConnection conn, string cmdStr)
    {
      using (SqlCommand sqlCommand = new SqlCommand(cmdStr, conn))
      {
        int num = 0;
        SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
        if (sqlDataReader.Read())
          num = Convert.ToInt32(sqlDataReader[0]);
        sqlDataReader.Close();
        return num;
      }
    }
  }
}
