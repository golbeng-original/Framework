using CommonPackage.Tables;
using Golbeng.Framework.Managers;
using Mono.Data.SqliteClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Golbeng.Framework.Loader
{
	public abstract class ITableLoder
	{
		protected CResourceManager ResourceManager { get; }

		protected string AssetsPath { get; }

		public ITableLoder(CResourceManager resourceManager, string targetTablePath)
		{
			ResourceManager = resourceManager;
			AssetsPath = targetTablePath;
		}

		protected async Task<HashSet<TblBase>> LoadSqliteDBAsync<T>(string path) where T : new()
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
						while (reader.Read())
						{
							TblBase newTbl = new T() as TblBase;
							if (newTbl == null)
								break;

							for (int i = 0; i < newTbl.PropertyCount; i++)
							{
								var propertyInfo = newTbl.GetPropertyInfo(i);
								if (propertyInfo == null)
									continue;

								int ordinal = -1;
								try
								{
									ordinal = reader.GetOrdinal(propertyInfo?.propertyName);
								}
								catch { }

								if (ordinal == -1)
									continue;

								if (propertyInfo?.type == typeof(int))
								{
									int value = reader.GetInt32(ordinal);
									newTbl.SetPropertyValue(i, value);
								}
								else if (propertyInfo?.type == typeof(uint))
								{
									uint value = (uint)reader.GetInt64(ordinal);
									newTbl.SetPropertyValue(i, value);
								}
								else if (propertyInfo?.type == typeof(float))
								{
									float value = reader.GetFloat(ordinal);
									newTbl.SetPropertyValue(i, value);
								}
								else if (propertyInfo?.type == typeof(string))
								{
									string value = reader.GetString(ordinal);
									newTbl.SetPropertyValue(i, value);
								}
								else if (propertyInfo?.type == typeof(bool))
								{
									bool value = reader.GetBoolean(ordinal);
									newTbl.SetPropertyValue(i, value);
								}
							}

							contaner.Add(newTbl);
						}
					}
				}

				return contaner;
			}
			catch (Exception e)
			{
				throw e;
			}
			finally
			{
				sqliteConnection.Close();
			}
		}

		protected HashSet<TblBase> LoadSqliteDB<T>(string path) where T : new()
		{
			var tableMeta = TableUtils.GetTableMeta<T>();
			if (tableMeta == null)
				throw new Exception("Type is not TableData");

			string connectionPath = $"URI=file:{path}";
			var sqliteConnection = new SqliteConnection(connectionPath);

			try
			{
				sqliteConnection.Open();

				var contaner = new HashSet<TblBase>();
				using (var cmd = sqliteConnection.CreateCommand())
				{
					cmd.CommandText = $"SELECT * FROM {tableMeta.TableName}";

					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							TblBase newTbl = new T() as TblBase;
							if (newTbl == null)
								break;

							for (int i = 0; i < newTbl.PropertyCount; i++)
							{
								var propertyInfo = newTbl.GetPropertyInfo(i);
								if (propertyInfo == null)
									continue;

								int ordinal = -1;
								try
								{
									ordinal = reader.GetOrdinal(propertyInfo?.propertyName);
								}
								catch { }

								if (ordinal == -1)
									continue;

								if (propertyInfo?.type == typeof(int))
								{
									int value = reader.GetInt32(ordinal);
									newTbl.SetPropertyValue(i, value);
								}
								else if (propertyInfo?.type == typeof(uint))
								{
									uint value = (uint)reader.GetInt64(ordinal);
									newTbl.SetPropertyValue(i, value);
								}
								else if (propertyInfo?.type == typeof(float))
								{
									float value = reader.GetFloat(ordinal);
									newTbl.SetPropertyValue(i, value);
								}
								else if (propertyInfo?.type == typeof(string))
								{
									string value = reader.GetString(ordinal);
									newTbl.SetPropertyValue(i, value);
								}
								else if (propertyInfo?.type == typeof(bool))
								{
									bool value = reader.GetBoolean(ordinal);
									newTbl.SetPropertyValue(i, value);
								}
								else if (propertyInfo?.type.IsEnum == true)
								{
									int value = reader.GetInt32(ordinal);
									newTbl.SetPropertyValue(i, value);
								}
							}

							contaner.Add(newTbl);
						}
					}
				}

				return contaner;
			}
			catch (Exception e)
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
