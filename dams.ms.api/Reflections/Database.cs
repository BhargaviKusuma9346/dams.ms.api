using System;
using System.Collections.Generic;
using System.Reflection;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using Microsoft.Extensions.Configuration;

namespace Dams.ms.auth.Reflections
{
	public class Database<T, O> where T : BaseEntity, new()
	{
		// private SqlConnection _connection;

		/// Default constructor which uses the "DefaultConnection" connectionString
		/// </summary>
		private static string Connectionstring;
		private readonly IConfigurationRoot ObjConfiguration;
		// Database<User, User> database;
		public Database(IConfigurationRoot Configuration)
		{
			ObjConfiguration = Configuration;
			Connectionstring = ObjConfiguration.GetConnectionString("DefaultConnection");
			//     database = new Database<User, User>(ObjConfiguration);

		}

		public List<T> ExecuteWithParms(string storedProcedureName, O obj, out int RowCount)
		{
			SqlConnection connection = null;
			RowCount = 0;
			try
			{
				connection = new SqlConnection(Connectionstring);

				// Create Command
				SqlCommand command = connection.CreateCommand();
				command.CommandType = CommandType.StoredProcedure;
				command.CommandText = storedProcedureName;

				// Open Connection
				connection.Open();

				// Discover Parameters for Stored Procedure
				// Populate command.Parameters Collection.
				// Causes Rountrip to Database.
				SqlCommandBuilder.DeriveParameters(command);

				// Initialize Index of parameterValues Array
				//int index = 0;

				// Populate the Input Parameters With Values Provided        
				foreach (SqlParameter parameter in command.Parameters)
				{
					if (parameter.Direction == ParameterDirection.Input ||
						parameter.Direction == ParameterDirection.Output ||
						parameter.Direction == ParameterDirection.InputOutput)
					{
						FindPropertiesansSet(obj, parameter);
					}
				}

				command.ExecuteNonQuery();
				if (command.Parameters["@row_count"].Value != DBNull.Value)
					RowCount = Convert.ToInt32(command.Parameters["@row_count"].Value);
				IDataReader idataReader = command.ExecuteReader(CommandBehavior.CloseConnection);
				return FillListwithProperties(idataReader);
			}
			catch
			{
				throw;
			}
			finally
			{
				if (connection != null && connection.State == ConnectionState.Open)
					connection.Close();
			}
		}

		public List<T> ExecuteWithParms(string storedProcedureName, O obj, out int RowCount, string connectionString)
		{
			SqlConnection connection = null;
			RowCount = 0;
			try
			{
				connection = new SqlConnection(connectionString);

				// Create Command
				SqlCommand command = connection.CreateCommand();
				command.CommandType = CommandType.StoredProcedure;
				command.CommandText = storedProcedureName;

				// Open Connection
				connection.Open();

				// Discover Parameters for Stored Procedure
				// Populate command.Parameters Collection.
				// Causes Rountrip to Database.
				SqlCommandBuilder.DeriveParameters(command);

				// Initialize Index of parameterValues Array
				//int index = 0;

				// Populate the Input Parameters With Values Provided        
				foreach (SqlParameter parameter in command.Parameters)
				{
					if (parameter.Direction == ParameterDirection.Input ||
						parameter.Direction == ParameterDirection.Output ||
						parameter.Direction == ParameterDirection.InputOutput)
					{
						FindPropertiesansSet(obj, parameter);
					}
				}

				command.ExecuteNonQuery();
				if (command.Parameters["@row_count"].Value != DBNull.Value)
					RowCount = Convert.ToInt32(command.Parameters["@row_count"].Value);
				IDataReader idataReader = command.ExecuteReader(CommandBehavior.CloseConnection);
				return FillListwithProperties(idataReader);
			}
			catch
			{
				throw;
			}
			finally
			{
				if (connection != null && connection.State == ConnectionState.Open)
					connection.Close();
			}
		}

		public List<T> Execute(string storedProcedureName, O obj)
		{
			SqlConnection connection = null;
			try
			{
				connection = new SqlConnection(Connectionstring);

				// Create Command
				SqlCommand command = connection.CreateCommand();
				command.CommandType = CommandType.StoredProcedure;
				command.CommandText = storedProcedureName;
				command.CommandTimeout = 60000;

				// Open Connection
				connection.Open();

				// Discover Parameters for Stored Procedure
				// Populate command.Parameters Collection.
				// Causes Rountrip to Database.
				SqlCommandBuilder.DeriveParameters(command);

				// Initialize Index of parameterValues Array
				//int index = 0;

				// Populate the Input Parameters With Values Provided        
				foreach (SqlParameter parameter in command.Parameters)
				{
					if (parameter.Direction == ParameterDirection.Input ||
						parameter.Direction == ParameterDirection.Output ||
						parameter.Direction == ParameterDirection.InputOutput)
					{
						FindPropertiesansSet(obj, parameter);
					}
				}

				IDataReader idataReader = command.ExecuteReader(CommandBehavior.
										  CloseConnection);
				return FillListwithProperties(idataReader);
			}
			catch
			{
				throw;
			}
			finally
			{
				if (connection != null && connection.State == ConnectionState.Open)
					connection.Close();
			}
		}

		public List<T> Execute(string storedProcedureName, O obj, string connectionString)
		{
			SqlConnection connection = null;
			try
			{
				connection = new SqlConnection(connectionString);

				// Create Command
				SqlCommand command = connection.CreateCommand();
				command.CommandType = CommandType.StoredProcedure;
				command.CommandText = storedProcedureName;

				// Open Connection
				connection.Open();

				// Discover Parameters for Stored Procedure
				// Populate command.Parameters Collection.
				// Causes Rountrip to Database.
				SqlCommandBuilder.DeriveParameters(command);

				// Initialize Index of parameterValues Array
				//int index = 0;

				// Populate the Input Parameters With Values Provided        
				foreach (SqlParameter parameter in command.Parameters)
				{
					if (parameter.Direction == ParameterDirection.Input ||
						parameter.Direction == ParameterDirection.Output ||
						parameter.Direction == ParameterDirection.InputOutput)
					{
						FindPropertiesansSet(obj, parameter);
					}
				}

				IDataReader idataReader = command.ExecuteReader(CommandBehavior.
										  CloseConnection);
				return FillListwithProperties(idataReader);
			}
			catch
			{
				throw;
			}
			finally
			{
				if (connection != null && connection.State == ConnectionState.Open)
					connection.Close();
			}
		}

		private int Contains(string name, IDataReader idr)
		{
			for (int i = 0; i < idr.FieldCount; i++)
			{
				if (idr.GetName(i).Replace("_", "").ToLower() == name.Replace("_", "").ToLower())
				{
					return i;
				}
			}
			return -1;
		}

		//Field Info
		private void FindandSet(O obj, SqlParameter parameter)
		{
			FieldInfo[] fields = Reflector<O>.GetFieldInfos();

			foreach (FieldInfo field in fields)
			{
				if (parameter.ParameterName.Replace("_", "").Replace("@", "").ToLower() == field.Name.ToLower())
				{
					parameter.Value = field.GetValue(obj);
				}
			}
		}

		//Property Info
		private void FindPropertiesansSet(O obj, SqlParameter parameter)
		{
			PropertyInfo[] properties = Reflector<O>.GetPropertyInfos();
			foreach (PropertyInfo property in properties)
			{
				if (parameter.ParameterName.Replace("_", "").Replace("@", "").ToLower() == property.Name.ToLower())
				{
					parameter.Value = property.GetValue(obj, null);
				}
			}
		}

		//FieldInfo
		private List<T> FillList(IDataReader idataReader)
		{
			FieldInfo[] fields = Reflector<T>.GetFieldInfos();
			List<T> objs = new List<T>();
			while (idataReader.Read())
			{
				T obj = new T();
				objs.Add(AssigningofData(idataReader, fields, obj));
			}
			return objs;
		}

		//PropertyInfo
		private List<T> FillListwithProperties(IDataReader idataReader)
		{
			PropertyInfo[] properties = Reflector<T>.GetPropertyInfos();
			List<T> objs = new List<T>();
			while (idataReader.Read())
			{
				T obj = new T();
				objs.Add(AssigningofData(idataReader, properties, obj));
			}
			return objs;
		}

		//FieldInfo
		private T AssigningofData(IDataReader idataReader, FieldInfo[] fields, T obj)
		{

			foreach (FieldInfo field in fields)
			{
				string name = field.Name; // Get string name
				object temp = field.GetValue(obj); // Get value

				int fieldColumn = Contains(name, idataReader);
				if (fieldColumn == -1)
				{
					continue;
				}

				object value = idataReader[fieldColumn];

				if (value == null)
				{
					//  the value is null and the field type is not nullable,
					//  default to the default value of the field
					value = Activator.CreateInstance(field.FieldType);
				}
				else
				{
					////value = Convert.ChangeType(value, pair.Key.FieldType);
					value = ChangeType(value, field.FieldType);
				}

				field.SetValue(obj, value);
			}

			return obj;
		}

		//PropertyInfo
		private T AssigningofData(IDataReader idataReader, PropertyInfo[] properties, T obj)
		{

			foreach (PropertyInfo property in properties)
			{
				string name = property.Name; // Get string name
				if (name != "Errors" && name != "Error" && name != "Item")
				{
					object temp = property.GetValue(obj, null); // Get value
					int propertyColumn = Contains(name, idataReader);
					if (propertyColumn == -1)
					{
						continue;
					}

					object value = null;

					if (idataReader[propertyColumn] == DBNull.Value)
					{
						continue;
					}

					value = idataReader[propertyColumn];

					if (value == null)
					{
						//  the value is null and the field type is not nullable,
						//  default to the default value of the field
						value = Activator.CreateInstance(property.PropertyType);
					}
					else
					{
						////value = Convert.ChangeType(value, pair.Key.FieldType);
						value = ChangeType(value, property.PropertyType);
					}

					property.SetValue(obj, value, null);
				}
			}

			return obj;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <param name="conversionType"></param>
		/// <returns></returns>
		private static object ChangeType(object value, Type conversionType)
		{
			// Note: This if block was taken from Convert.ChangeType as is, and is needed here since we're
			// checking properties on conversionType below.
			if (conversionType == null)
			{
				throw new ArgumentNullException("conversionType");
			}

			// If it's not a nullable type, just pass through the parameters to Convert.ChangeType

			if (conversionType.IsGenericType && conversionType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
			{
				// It's a nullable type, so instead of calling Convert.ChangeType directly which would throw a
				// InvalidCastException (per http://weblogs.asp.net/pjohnson/archive/2006/02/07/437631.aspx),
				// determine what the underlying type is
				// If it's null, it won't convert to the underlying type, but that's fine since nulls don't really
				// have a type--so just return null
				// Note: We only do this check if we're converting to a nullable type, since doing it outside
				// would diverge from Convert.ChangeType's behavior, which throws an InvalidCastException if
				// value is null and conversionType is a value type.
				if (value == null)
				{
					return null;
				}

				// It's a nullable type, and not null, so that means it can be converted to its underlying type,
				// so overwrite the passed-in conversion type with this underlying type
				System.ComponentModel.NullableConverter nullableConverter = new System.ComponentModel.NullableConverter(conversionType);
				conversionType = nullableConverter.UnderlyingType;
			}

			// Now that we've guaranteed conversionType is something Convert.ChangeType can handle (i.e. not a
			// nullable type), pass the call on to Convert.ChangeType
			return Convert.ChangeType(value, conversionType);
		}

		public void ExecuteWriter(string storedProcedureName, O obj)
		{
			SqlConnection connection = null;

			try
			{
				connection = new SqlConnection(Connectionstring);

				// Create Command
				SqlCommand command = connection.CreateCommand();
				command.CommandType = CommandType.StoredProcedure;
				command.CommandText = storedProcedureName;

				// Open Connection
				connection.Open();

				// Discover Parameters for Stored Procedure
				// Populate command.Parameters Collection.
				// Causes Rountrip to Database.
				SqlCommandBuilder.DeriveParameters(command);

				// Initialize Index of parameterValues Array
				//int index = 0;

				// Populate the Input Parameters With Values Provided        
				foreach (SqlParameter parameter in command.Parameters)
				{
					if (parameter.Direction == ParameterDirection.Input ||
						parameter.Direction == ParameterDirection.
											   InputOutput)
					{

						FindPropertiesansSet(obj, parameter);
					}
				}
				command.ExecuteNonQuery();
				//IDataReader idataReader = command.ExecuteReader(CommandBehavior.
				//                                CloseConnection);
				//return FillList(idataReader);
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				if (connection != null && connection.State
									== ConnectionState.Open)
					connection.Close();
			}
		}
	}
}
