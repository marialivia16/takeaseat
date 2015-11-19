using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web.Configuration;

namespace TakeASeatApp.Data
{
	public class RepositoryRow : Dictionary<string, object>
	{

	}

	public class Repository : IDisposable
	{
		public static readonly string ConnectionStringName = "DefaultConnection";
		public static readonly string ConnectionString = WebConfigurationManager.ConnectionStrings[ConnectionStringName].ToString();
		public SqlConnection Connection;
		public SqlCommand Command;
		public SqlParameterCollection Params { get { return this.Command.Parameters; } }

		public Repository()
		{
			this.Connection = new SqlConnection(ConnectionString);
		    this.Command = new SqlCommand
		    {
		        Connection = this.Connection,
		        CommandType = CommandType.StoredProcedure
		    };
		}

		public SqlParameter AddParam(SqlParameter parameter)
		{
			return this.Command.Parameters.Add(parameter);
		}
		public SqlParameter AddParam(string parameterName, SqlDbType parameterType)
		{
			return this.Command.Parameters.Add(parameterName, parameterType);
		}
		public SqlParameter AddParam(string parameterName, SqlDbType parameterType, int parameterSize)
		{
			return this.Command.Parameters.Add(parameterName, parameterType, parameterSize);
		}

		/// <summary>
		/// 	<para>Gets the result set of a stored procedure as a DataTable. Then deletes the Params collection from the SqlCommand.</para>
		/// </summary>
		/// <param name="storedProcedureName">The params for the stored procedure must be added before calling this method, using AddParam().</param>
		/// <returns></returns>
		public DataTable GetDataTable(string storedProcedureName)
		{
			return this.GetDataTable(storedProcedureName, true);
		}
		/// <summary>
		/// 	<para>Gets the result set of a stored procedure as a DataTable. Then, if ClearParamsAfterCompletion is true, deletes the Params collection from the SqlCommand.</para>
		/// </summary>
		/// <param name="storedProcedureName">The params for the stored procedure must be added before calling this method, using AddParam().</param>
		/// <param name="clearParamsAfterCompletion"></param>
		/// <returns></returns>
		public DataTable GetDataTable(string storedProcedureName, bool clearParamsAfterCompletion)
		{
			DataTable resultTable = new DataTable();
			if (this.Connection.State != ConnectionState.Open) this.Connection.Open();
		    this.Command.CommandText = storedProcedureName;
			SqlDataReader rdrReader = this.Command.ExecuteReader();
			resultTable.Load(rdrReader);
			rdrReader.Close();
			rdrReader.Dispose();
			if (clearParamsAfterCompletion) this.Command.Parameters.Clear();
			this.Connection.Close();
			return resultTable;
		}
		/// <summary>
		/// 	<para>Gets the result set of a text command as a DataTable. Then, if ClearParamsAfterCompletion is true, deletes the Params collection from the SqlCommand.</para>
		/// </summary>
		/// <param name="commandText">The params for the command must be added before calling this method, using AddParam().</param>
		/// <param name="clearParamsAfterCompletion"></param>
		/// <returns></returns>
		public DataTable GetDataTableFromTextCommand(string commandText, bool clearParamsAfterCompletion)
		{
			DataTable resultTable = new DataTable();
			if (this.Connection.State != ConnectionState.Open) this.Connection.Open();
		    this.Command.CommandType = CommandType.Text;
			this.Command.CommandText = commandText;
			SqlDataReader rdrReader = this.Command.ExecuteReader();
			resultTable.Load(rdrReader);
			rdrReader.Close();
			rdrReader.Dispose();
			this.Command.CommandType = CommandType.StoredProcedure;
			if (clearParamsAfterCompletion) this.Command.Parameters.Clear();
			this.Connection.Close();
			return resultTable;
		}

		/// <summary>
		/// 	<para>Retrieves a row from a database table. The field types are .NET native: int, string, DateTime, long, byte, bool etc.</para>
		/// </summary>
		/// <param name="storedProcedureName">The params for the stored procedure must be added before calling this method, using AddParam().</param>
		/// <returns></returns>
		/// 
		public RepositoryRow GetDataRow(string storedProcedureName)
		{
			return this.GetDataRow(storedProcedureName, true);
		}
		/// <summary>
		/// 	<para>Retrieves a row from a database table. The field types are .NET native: int, string, DateTime, long, byte, bool etc.</para>
		/// </summary>
		/// <param name="storedProcedureName">The params for the stored procedure must be added before calling this method, using AddParam().</param>
		/// <returns></returns>
		public RepositoryRow GetDataRow(string storedProcedureName, bool clearParamsAfterCompletion)
		{
			RepositoryRow row = null;
			if (this.Connection.State != ConnectionState.Open) this.Connection.Open();
		    this.Command.CommandText = storedProcedureName;
			SqlDataReader rdrReader = this.Command.ExecuteReader();
			if (rdrReader.HasRows)
			{
				row = new RepositoryRow();
				rdrReader.Read();
				for (int i = 0; i < rdrReader.FieldCount; i++)
				{
					switch (rdrReader.GetDataTypeName(i))
					{
						case "uniqueidentifier": if (rdrReader.IsDBNull(i)) row.Add(rdrReader.GetName(i), null); else row.Add(rdrReader.GetName(i), rdrReader.GetSqlGuid(i).Value); break;

						case "bit": if (rdrReader.IsDBNull(i)) row.Add(rdrReader.GetName(i), null); else row.Add(rdrReader.GetName(i), rdrReader.GetSqlBoolean(i).Value); break;
						case "tinyint": if (rdrReader.IsDBNull(i)) row.Add(rdrReader.GetName(i), null); else row.Add(rdrReader.GetName(i), rdrReader.GetSqlByte(i).Value); break;
						case "smallint": if (rdrReader.IsDBNull(i)) row.Add(rdrReader.GetName(i), null); else row.Add(rdrReader.GetName(i), rdrReader.GetSqlInt16(i).Value); break;
						case "int": if (rdrReader.IsDBNull(i)) row.Add(rdrReader.GetName(i), null); else row.Add(rdrReader.GetName(i), rdrReader.GetSqlInt32(i).Value); break;
						case "bigint": if (rdrReader.IsDBNull(i)) row.Add(rdrReader.GetName(i), null); else row.Add(rdrReader.GetName(i), rdrReader.GetSqlInt64(i).Value); break;
						case "decimal": if (rdrReader.IsDBNull(i)) row.Add(rdrReader.GetName(i), null); else row.Add(rdrReader.GetName(i), rdrReader.GetSqlDecimal(i).Value); break;
						case "money": if (rdrReader.IsDBNull(i)) row.Add(rdrReader.GetName(i), null); else row.Add(rdrReader.GetName(i), rdrReader.GetSqlMoney(i).Value); break;
						case "smallmoney": if (rdrReader.IsDBNull(i)) row.Add(rdrReader.GetName(i), null); else row.Add(rdrReader.GetName(i), rdrReader.GetSqlMoney(i).Value); break;

						case "real": if (rdrReader.IsDBNull(i)) row.Add(rdrReader.GetName(i), null); else row.Add(rdrReader.GetName(i), rdrReader.GetSqlSingle(i).Value); break;
						case "float": if (rdrReader.IsDBNull(i)) row.Add(rdrReader.GetName(i), null); else row.Add(rdrReader.GetName(i), rdrReader.GetSqlDouble(i).Value); break;

						case "date": if (rdrReader.IsDBNull(i)) row.Add(rdrReader.GetName(i), null); else row.Add(rdrReader.GetName(i), rdrReader.GetSqlDateTime(i).Value); break;
						case "datetime": if (rdrReader.IsDBNull(i)) row.Add(rdrReader.GetName(i), null); else row.Add(rdrReader.GetName(i), rdrReader.GetSqlDateTime(i).Value); break;
						case "smalldatetime": if (rdrReader.IsDBNull(i)) row.Add(rdrReader.GetName(i), null); else row.Add(rdrReader.GetName(i), rdrReader.GetSqlDateTime(i).Value); break;
						case "time": if (rdrReader.IsDBNull(i)) row.Add(rdrReader.GetName(i), null); else row.Add(rdrReader.GetName(i), rdrReader.GetSqlDateTime(i).Value); break;

						case "char": if (rdrReader.IsDBNull(i)) row.Add(rdrReader.GetName(i), null); else row.Add(rdrReader.GetName(i), rdrReader.GetSqlChars(i).Value); break;
						case "nchar": if (rdrReader.IsDBNull(i)) row.Add(rdrReader.GetName(i), null); else row.Add(rdrReader.GetName(i), rdrReader.GetSqlString(i).Value); break;

						case "varchar": if (rdrReader.IsDBNull(i)) row.Add(rdrReader.GetName(i), null); else row.Add(rdrReader.GetName(i), rdrReader.GetSqlString(i).Value); break;
						case "text": if (rdrReader.IsDBNull(i)) row.Add(rdrReader.GetName(i), null); else row.Add(rdrReader.GetName(i), rdrReader.GetSqlString(i).Value); break;
						case "nvarchar": if (rdrReader.IsDBNull(i)) row.Add(rdrReader.GetName(i), null); else row.Add(rdrReader.GetName(i), rdrReader.GetSqlString(i).Value); break;
						case "ntext": if (rdrReader.IsDBNull(i)) row.Add(rdrReader.GetName(i), null); else row.Add(rdrReader.GetName(i), rdrReader.GetSqlString(i).Value); break;

						case "binary": if (rdrReader.IsDBNull(i)) row.Add(rdrReader.GetName(i), null); else row.Add(rdrReader.GetName(i), rdrReader.GetSqlBinary(i).Value); break;
						case "varbinary": if (rdrReader.IsDBNull(i)) row.Add(rdrReader.GetName(i), null); else row.Add(rdrReader.GetName(i), rdrReader.GetSqlBinary(i).Value); break;
						case "image": if (rdrReader.IsDBNull(i)) row.Add(rdrReader.GetName(i), null); else row.Add(rdrReader.GetName(i), rdrReader.GetSqlBinary(i).Value); break;

						case "xml": if (rdrReader.IsDBNull(i)) row.Add(rdrReader.GetName(i), null); else row.Add(rdrReader.GetName(i), rdrReader.GetSqlXml(i).Value); break;

						default: if (rdrReader.IsDBNull(i)) row.Add(rdrReader.GetName(i), null); else row.Add(rdrReader.GetName(i), rdrReader.GetSqlValue(i)); break;
					}
				}
			}
			rdrReader.Close();
			rdrReader.Dispose();
			if (clearParamsAfterCompletion) this.Command.Parameters.Clear();
			this.Connection.Close();
			return row;
		}
		/// <summary>
		/// 	<para>Retrieves a row from a database table as a result from a text command. The field types are .NET native: int, string, DateTime, long, byte, bool etc.</para>
		/// </summary>
		/// <param name="commandText">The params for command must be added before calling this method, using AddParam().</param>
		/// <param name="clearParamsAfterCompletion"></param>
		/// <returns></returns>
		public RepositoryRow GetDataRowFromTextCommand(string commandText, bool clearParamsAfterCompletion)
		{
			RepositoryRow row = null;
		    if (this.Connection.State != ConnectionState.Open) this.Connection.Open();
			this.Command.CommandType = CommandType.Text;
			this.Command.CommandText = commandText;
			SqlDataReader rdrReader = this.Command.ExecuteReader();
			if (rdrReader.HasRows)
			{
				row = new RepositoryRow();
				rdrReader.Read();
				for (int i = 0; i < rdrReader.FieldCount; i++)
				{
					switch (rdrReader.GetDataTypeName(i))
					{
						case "uniqueidentifier": if (rdrReader.IsDBNull(i)) row.Add(rdrReader.GetName(i), null); else row.Add(rdrReader.GetName(i), rdrReader.GetSqlGuid(i).Value); break;

						case "bit": if (rdrReader.IsDBNull(i)) row.Add(rdrReader.GetName(i), null); else row.Add(rdrReader.GetName(i), rdrReader.GetSqlBoolean(i).Value); break;
						case "tinyint": if (rdrReader.IsDBNull(i)) row.Add(rdrReader.GetName(i), null); else row.Add(rdrReader.GetName(i), rdrReader.GetSqlByte(i).Value); break;
						case "smallint": if (rdrReader.IsDBNull(i)) row.Add(rdrReader.GetName(i), null); else row.Add(rdrReader.GetName(i), rdrReader.GetSqlInt16(i).Value); break;
						case "int": if (rdrReader.IsDBNull(i)) row.Add(rdrReader.GetName(i), null); else row.Add(rdrReader.GetName(i), rdrReader.GetSqlInt32(i).Value); break;
						case "bigint": if (rdrReader.IsDBNull(i)) row.Add(rdrReader.GetName(i), null); else row.Add(rdrReader.GetName(i), rdrReader.GetSqlInt64(i).Value); break;
						case "decimal": if (rdrReader.IsDBNull(i)) row.Add(rdrReader.GetName(i), null); else row.Add(rdrReader.GetName(i), rdrReader.GetSqlDecimal(i).Value); break;
						case "money": if (rdrReader.IsDBNull(i)) row.Add(rdrReader.GetName(i), null); else row.Add(rdrReader.GetName(i), rdrReader.GetSqlMoney(i).Value); break;
						case "smallmoney": if (rdrReader.IsDBNull(i)) row.Add(rdrReader.GetName(i), null); else row.Add(rdrReader.GetName(i), rdrReader.GetSqlMoney(i).Value); break;

						case "real": if (rdrReader.IsDBNull(i)) row.Add(rdrReader.GetName(i), null); else row.Add(rdrReader.GetName(i), rdrReader.GetSqlSingle(i).Value); break;
						case "float": if (rdrReader.IsDBNull(i)) row.Add(rdrReader.GetName(i), null); else row.Add(rdrReader.GetName(i), rdrReader.GetSqlDouble(i).Value); break;

						case "date": if (rdrReader.IsDBNull(i)) row.Add(rdrReader.GetName(i), null); else row.Add(rdrReader.GetName(i), rdrReader.GetSqlDateTime(i).Value); break;
						case "datetime": if (rdrReader.IsDBNull(i)) row.Add(rdrReader.GetName(i), null); else row.Add(rdrReader.GetName(i), rdrReader.GetSqlDateTime(i).Value); break;
						case "smalldatetime": if (rdrReader.IsDBNull(i)) row.Add(rdrReader.GetName(i), null); else row.Add(rdrReader.GetName(i), rdrReader.GetSqlDateTime(i).Value); break;
						case "time": if (rdrReader.IsDBNull(i)) row.Add(rdrReader.GetName(i), null); else row.Add(rdrReader.GetName(i), rdrReader.GetSqlDateTime(i).Value); break;

						case "char": if (rdrReader.IsDBNull(i)) row.Add(rdrReader.GetName(i), null); else row.Add(rdrReader.GetName(i), rdrReader.GetSqlChars(i).Value); break;
						case "nchar": if (rdrReader.IsDBNull(i)) row.Add(rdrReader.GetName(i), null); else row.Add(rdrReader.GetName(i), rdrReader.GetSqlString(i).Value); break;

						case "varchar": if (rdrReader.IsDBNull(i)) row.Add(rdrReader.GetName(i), null); else row.Add(rdrReader.GetName(i), rdrReader.GetSqlString(i).Value); break;
						case "text": if (rdrReader.IsDBNull(i)) row.Add(rdrReader.GetName(i), null); else row.Add(rdrReader.GetName(i), rdrReader.GetSqlString(i).Value); break;
						case "nvarchar": if (rdrReader.IsDBNull(i)) row.Add(rdrReader.GetName(i), null); else row.Add(rdrReader.GetName(i), rdrReader.GetSqlString(i).Value); break;
						case "ntext": if (rdrReader.IsDBNull(i)) row.Add(rdrReader.GetName(i), null); else row.Add(rdrReader.GetName(i), rdrReader.GetSqlString(i).Value); break;

						case "binary": if (rdrReader.IsDBNull(i)) row.Add(rdrReader.GetName(i), null); else row.Add(rdrReader.GetName(i), rdrReader.GetSqlBinary(i).Value); break;
						case "varbinary": if (rdrReader.IsDBNull(i)) row.Add(rdrReader.GetName(i), null); else row.Add(rdrReader.GetName(i), rdrReader.GetSqlBinary(i).Value); break;
						case "image": if (rdrReader.IsDBNull(i)) row.Add(rdrReader.GetName(i), null); else row.Add(rdrReader.GetName(i), rdrReader.GetSqlBinary(i).Value); break;

						case "xml": if (rdrReader.IsDBNull(i)) row.Add(rdrReader.GetName(i), null); else row.Add(rdrReader.GetName(i), rdrReader.GetSqlXml(i).Value); break;

						default: if (rdrReader.IsDBNull(i)) row.Add(rdrReader.GetName(i), null); else row.Add(rdrReader.GetName(i), rdrReader.GetSqlValue(i)); break;
					}
				}
			}
			rdrReader.Close();
			rdrReader.Dispose();
			this.Command.CommandType = CommandType.StoredProcedure;
			if (clearParamsAfterCompletion) this.Command.Parameters.Clear();
			this.Connection.Close();
			return row;
		}

		/// <summary>
		/// 	<para>Gets the result set of a stored procedure as a SqlDataReader. Then deletes the Params collection from the SqlCommand.</para>
		/// </summary>
		/// <param name="storedProcedureName">The params for the stored procedure must be added before calling this method, using AddParam().</param>
		/// <returns></returns>
		public SqlDataReader GetSqlDataReader(string storedProcedureName)
		{
			return this.GetSqlDataReader(storedProcedureName, true);
		}
		/// <summary>
		/// 	<para>Gets the result set of a stored procedure as a SqlDataReader. Then, if ClearParamsAfterCompletion is true, deletes the Params collection from the SqlCommand.</para>
		/// </summary>
		/// <param name="storedProcedureName">The params for the stored procedure must be added before calling this method, using AddParam().</param>
		/// <param name="clearParamsAfterCompletion"></param>
		/// <returns></returns>
		public SqlDataReader GetSqlDataReader(string storedProcedureName, bool clearParamsAfterCompletion)
		{
		    if (this.Connection.State != ConnectionState.Open) this.Connection.Open();
			this.Command.CommandText = storedProcedureName;
			SqlDataReader rdrReader = this.Command.ExecuteReader();
			if (clearParamsAfterCompletion) this.Command.Parameters.Clear();
			return rdrReader;
		}
		/// <summary>
		/// 	<para>Gets the result set of a text command as a SqlDataReader. Then, if ClearParamsAfterCompletion is true, deletes the Params collection from the SqlCommand.</para>
		/// </summary>
		/// <param name="commandText">The params for the command must be added before calling this method, using AddParam().</param>
		/// <param name="clearParamsAfterCompletion"></param>
		/// <returns></returns>
		public SqlDataReader GetSqlDataReaderFromTextCommand(string commandText, bool clearParamsAfterCompletion)
		{
		    if (this.Connection.State != ConnectionState.Open) this.Connection.Open();
			this.Command.CommandType = CommandType.Text;
			this.Command.CommandText = commandText;
			SqlDataReader rdrReader = this.Command.ExecuteReader();
			this.Command.CommandType = CommandType.StoredProcedure;
			if (clearParamsAfterCompletion) this.Command.Parameters.Clear();
			return rdrReader;
		}

		/// <summary>
		/// 	<para>Retrieves a row from a database table. The field types are those found in System.Data.SqlClient: SqlInt32, SqlString, SqlDateTime etc.</para>
		/// </summary>
		/// <param name="storedProcedureName">The params for the stored procedure must be added before calling this method, using AddParam().</param>
		/// <returns></returns>
		public RepositoryRow GetSqlDataRecord(string storedProcedureName)
		{
			return this.GetSqlDataRecord(storedProcedureName, true);
		}
		/// <summary>
		/// 	<para>Retrieves a row from a database table. The field types are those found in System.Data.SqlClient: SqlInt32, SqlString, SqlDateTime etc.</para>
		/// </summary>
		/// <param name="storedProcedureName">The params for the stored procedure must be added before calling this method, using AddParam().</param>
		/// <returns></returns>
		public RepositoryRow GetSqlDataRecord(string storedProcedureName, bool clearParamsAfterCompletion)
		{
			RepositoryRow row = null;
		    if (this.Connection.State != ConnectionState.Open) this.Connection.Open();
			this.Command.CommandText = storedProcedureName;
			SqlDataReader rdrReader = this.Command.ExecuteReader();
			if (rdrReader.HasRows)
			{
				row = new RepositoryRow();
				rdrReader.Read();
				for (int i = 0; i < rdrReader.FieldCount; i++)
				{
					row.Add(rdrReader.GetName(i), rdrReader.GetSqlValue(i));
				}
			}
			rdrReader.Close();
			rdrReader.Dispose();
			if (clearParamsAfterCompletion) this.Command.Parameters.Clear();
			return row;
		}
		/// <summary>
		///		<para>Retrieves a row from a database table as a result from a text command. The field types are those found in System.Data.SqlClient: SqlInt32, SqlString, SqlDateTime etc.</para>
		/// </summary>
		/// <param name="commandText">The params for the command must be added before calling this method, using AddParam().</param>
		/// <param name="clearParamsAfterCompletion"></param>
		/// <returns></returns>
		public RepositoryRow GetSqlDataRecordFromTextCommand(string commandText, bool clearParamsAfterCompletion)
		{
			RepositoryRow row = null;
		    if (this.Connection.State != ConnectionState.Open) this.Connection.Open();
			this.Command.CommandType = CommandType.Text;
			this.Command.CommandText = commandText;
			SqlDataReader rdrReader = this.Command.ExecuteReader();
			if (rdrReader.HasRows)
			{
				row = new RepositoryRow();
				rdrReader.Read();
				for (int i = 0; i < rdrReader.FieldCount; i++)
				{
					row.Add(rdrReader.GetName(i), rdrReader.GetSqlValue(i));
				}
			}
			rdrReader.Close();
			rdrReader.Dispose();
			this.Command.CommandType = CommandType.StoredProcedure;
			if (clearParamsAfterCompletion) this.Command.Parameters.Clear();
			this.Connection.Close();
			return row;
		}

		/// <summary>
		///		<para>Gets the result of a SQL scalar function. Then deletes the Params collection from the SqlCommand.</para>
		/// </summary>
		/// <param name="functionName">The params for the database function must be added before calling this method, using AddParam().</param>
		/// <returns></returns>
		public object GetScalar(string functionName)
		{
			return this.GetScalar(functionName, true);
		}
		/// <summary>
		///		<para>Gets the result of a SQL scalar function. Then, if ClearParamsAfterCompletion is true, deletes the Params collection from the SqlCommand.</para>
		/// </summary>
		/// <param name="functionName">The params for the database function must be added before calling this method, using AddParam().</param>
		/// <param name="clearParamsAfterCompletion"></param>
		/// <returns></returns>
		public object GetScalar(string functionName, bool clearParamsAfterCompletion)
		{
			object retVal = null;
			if (this.Connection.State != ConnectionState.Open) this.Connection.Open();
			StringBuilder stb = new StringBuilder();
			stb.Append("SELECT ");
			stb.Append(functionName.Contains('.') ? functionName : "dbo." + functionName);
			if (this.Command.Parameters.Count > 0)
			{
				stb.Append("(");
				foreach (SqlParameter param in this.Command.Parameters) stb.AppendFormat("{0}, ", param.ParameterName);
				stb.Remove(stb.Length - 2, 2);
				stb.Append(")");
			}
			this.Command.CommandType = CommandType.Text;
			this.Command.CommandText = stb.ToString();
			retVal = this.Command.ExecuteScalar();
			this.Command.CommandType = CommandType.StoredProcedure;
			if (clearParamsAfterCompletion) this.Command.Parameters.Clear();
			this.Connection.Close();
			return (retVal == DBNull.Value) ? null : retVal;
		}
		/// <summary>
		///		<para>Gets the scalar result of a text command. Then, if ClearParamsAfterCompletion is true, deletes the Params collection from the SqlCommand.</para>
		/// </summary>
		/// <param name="commandText">The params for the command must be added before calling this method, using AddParam().</param>
		/// <param name="clearParamsAfterCompletion"></param>
		/// <returns></returns>
		public object GetScalarFromTextCommand(string commandText, bool clearParamsAfterCompletion)
		{
			if (this.Connection.State != ConnectionState.Open) this.Connection.Open();
			this.Command.CommandType = CommandType.Text;
			this.Command.CommandText = commandText;
			object retVal = this.Command.ExecuteScalar();
			this.Command.CommandType = CommandType.StoredProcedure;
			if (clearParamsAfterCompletion) this.Command.Parameters.Clear();
			this.Connection.Close();
			return retVal;
		}

		public int ExecuteProcedure(string storedProcedureName)
		{
			return this.ExecuteProcedure(storedProcedureName, true);
		}
		public int ExecuteProcedure(string storedProcedureName, bool clearParamsAfterCompletion)
		{
			int numberOfRecordsAffected = 0;
			if (this.Connection.State != ConnectionState.Open) this.Connection.Open();
			this.Command.CommandText = storedProcedureName;
			numberOfRecordsAffected = this.Command.ExecuteNonQuery();
			if (clearParamsAfterCompletion)
			{
				this.Command.Parameters.Clear();
			}
			this.Connection.Close();
			return numberOfRecordsAffected;
		}
		public int ExecuteTextCommand(string transactSqlCommandText)
		{
			return this.ExecuteTextCommand(transactSqlCommandText, true);
		}
		public int ExecuteTextCommand(string transactSqlCommandText, bool clearParamsAfterCompletion)
		{
			int numberOfRecordsAffected = 0;
			if (this.Connection.State != ConnectionState.Open) this.Connection.Open();
			this.Command.CommandType = CommandType.Text;
			this.Command.CommandText = transactSqlCommandText;
			numberOfRecordsAffected = this.Command.ExecuteNonQuery();
			if (clearParamsAfterCompletion)
			{
				this.Command.Parameters.Clear();
			}
			this.Command.CommandType = CommandType.StoredProcedure;
			this.Connection.Close();
			return numberOfRecordsAffected;
		}

		public int SaveRecord(string storedProcedureName)
		{
			return this.SaveRecord(storedProcedureName, true);
		}
		public int SaveRecord(string storedProcedureName, bool clearParamsAfterCompletion)
		{
			int numberOfRecordsAffected = 0;
			if (this.Connection.State != ConnectionState.Open) this.Connection.Open();
			this.Command.CommandText = storedProcedureName;
			numberOfRecordsAffected = this.Command.ExecuteNonQuery();
			if (clearParamsAfterCompletion)
			{
				this.Command.Parameters.Clear();
			}
			this.Connection.Close();
			return numberOfRecordsAffected;
		}
		public int DeleteRecord(string storedProcedureName)
		{
			return this.DeleteRecord(storedProcedureName, true);
		}
		public int DeleteRecord(string storedProcedureName, bool clearParamsAfterCompletion)
		{
			int numberOfRecordsAffected = 0;
			if (this.Connection.State != ConnectionState.Open) this.Connection.Open();
			this.Command.CommandText = storedProcedureName;
			numberOfRecordsAffected = this.Command.ExecuteNonQuery();
			if (clearParamsAfterCompletion) this.Command.Parameters.Clear();
			this.Connection.Close();
			return numberOfRecordsAffected;
		}

		public void LogAction(string actionOrEvent, string category = null, string contextUserAccount = null)
		{

			if (string.IsNullOrEmpty(category)) category = ". Generic event .";
			if (string.IsNullOrEmpty(contextUserAccount)) contextUserAccount = "Application Level Account";
			if (string.IsNullOrEmpty(actionOrEvent)) throw new Exception("The application cannot log an event with an empty message. Populate EventMessage parameter of the ApplicationLogging.LogApplicationEvent() method.");

			this.AddParam("@RecordID", SqlDbType.Int).Direction = ParameterDirection.Output;
			this.AddParam("@LoggingTime", SqlDbType.SmallDateTime).Value = DateTime.Now;
			this.AddParam("@CategoryName", SqlDbType.NVarChar, 64).Value = category;
			this.AddParam("@LoggingEvent", SqlDbType.NVarChar, 256).Value = actionOrEvent;
			this.AddParam("@Who", SqlDbType.NVarChar, 256).Value = contextUserAccount;

			this.SaveRecord("Application_Events_Log_Ins");
		}

		public void Dispose()
		{
			this.Params.Clear();
			if (this.Connection.State == ConnectionState.Open)
			{
				this.Connection.Close();
			}
			this.Command.Dispose();
			this.Connection.Dispose();
		}
	}
}