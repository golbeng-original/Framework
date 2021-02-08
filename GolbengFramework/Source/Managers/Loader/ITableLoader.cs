using CommonPackage.Tables;

using Mono.Data.SqliteClient;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Golbeng.Framework.Loader
{
	public abstract class ITableLoder
	{
		protected string AssetsPath { get; private set; }

		public ITableLoder(string assestRootPath)
		{
			AssetsPath = assestRootPath;
		}

		protected async Task<HashSet<TblBase>> LoadSqliteDB<T>(string path) where T : new()
		{
			var tableMeta = TableUtils.GetTableMeta<T>();
			if (tableMeta == null)
				throw new Exception("Type is not TableData");

			string connectionPath = $"URI=file:{path}";
			var sqliteConnection = new SqliteConnection(connectionPath);

			try
			{
				await sqliteConnection.OpenAsync();

				var contaner = new HashSet<TblBase>();
				using (var cmd = sqliteConnection.CreateCommand())
				{
					cmd.CommandText = $"SELECT * FROM {tableMeta.TableName}";

					using (var reader = await cmd.ExecuteReaderAsync())
					{
						Type newTblType = typeof(T);
						var properties = newTblType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

						while (reader.Read())
						{
							TblBase newTbl = new T() as TblBase;
							if (newTbl == null)
								break;

							foreach (var property in properties)
							{
								int ordinal = -1;
								try
								{
									ordinal = reader.GetOrdinal(property.Name);
								}
								catch { }

								if (ordinal == -1)
									continue;

								if (property.PropertyType == typeof(int))
								{
									int value = reader.GetInt32(ordinal);
									property.SetValue(newTbl, value);
								}
								else if (property.PropertyType == typeof(uint))
								{
									uint value = (uint)reader.GetInt64(ordinal);
									property.SetValue(newTbl, value);
								}
								else if (property.PropertyType == typeof(float))
								{
									float value = reader.GetFloat(ordinal);
									property.SetValue(newTbl, value);
								}
								else if (property.PropertyType == typeof(string))
								{
									string value = reader.GetString(ordinal);
									property.SetValue(newTbl, value);
								}
								else if (property.PropertyType == typeof(bool))
								{
									bool value = reader.GetBoolean(ordinal);
									property.SetValue(newTbl, value);
								}
							}

							contaner.Add(newTbl);
						}
					}
				}

				return contaner;
			}
			catch(Exception e)
			{
				throw e;
			}
			finally
			{
				sqliteConnection.Close();
			}
		}

		public abstract HashSet<TblBase> LoadTable<T>() where T : class, new();
	}

}
