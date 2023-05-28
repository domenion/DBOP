using System.Collections;
using System.Data;
using System.Data.Common;
using System.Reflection;
using DBOP.Attributes;
using DBOP.Extensions;
using DBOP.Interfaces;
using DBOP.Models;

namespace DBOP.Queries;

public abstract class Query<TEntity, TConnection, TCommand, TTransaction>
    where TEntity : IEntityBase, new()
    where TConnection : DbConnection
    where TCommand : DbCommand
    where TTransaction : DbTransaction
{
    protected Query(Query<TEntity, TConnection, TCommand, TTransaction> query)
    {
        ConnectionString = query.ConnectionString;
        MainCommand = query.MainCommand;
        JoinCommand = query.JoinCommand;
        ConditionCommand = query.ConditionCommand;
        Entities = query.Entities;
    }

    protected Query(string connection)
    {
        ConnectionString = connection;
        MainCommand = "";
        Entities = new List<TEntity>();
    }

    protected Query(string connection, string command)
    {
        ConnectionString = connection;
        MainCommand = command;
        Entities = new List<TEntity>();
    }

    public virtual TConnection? DbConnection { get; set; }

    public virtual TCommand? DbCommand { get; set; }

    public virtual TTransaction? DbTransaction { get; set; }

    public string ConnectionString { get; }

    public string MainCommand { get; set; }

    public string? JoinCommand { get; set; }

    public string? ConditionCommand { get; set; }

    public string? ExtendedCommand { get; set; }

    public string CommandText
    {
        get
        {
            var cmd = MainCommand;
            if (!string.IsNullOrWhiteSpace(JoinCommand))
            {
                cmd += $" {JoinCommand}";
            }
            if (!string.IsNullOrWhiteSpace(ConditionCommand))
            {
                cmd += $" {ConditionCommand}";
            }
            if (!string.IsNullOrWhiteSpace(ExtendedCommand))
            {
                cmd += $" {ExtendedCommand}";
            }
            return cmd;
        }
    }

    public IEnumerable<Parameter> Parameters
    {
        get
        {
            IEnumerable<DbParameter> dbParameters = DbCommand?.Parameters.Cast<DbParameter>() ?? new List<DbParameter>();
            return dbParameters?.Select(p => new Parameter(p.ParameterName, p.Value!)) ?? new List<Parameter>();
        }
    }

    public string ParameterPrefix { get; set; } = "";

    public IEnumerable<TEntity> Entities { get; set; }

    public string TableName
    {
        get
        {
            return typeof(TEntity).GetTableName();
        }
    }

    public string Aliases { get; set; } = "";

    public string Database
    {
        get
        {
            var sIdx = ConnectionString.IndexOf("Database");
            if (sIdx < 0)
            {
                return "";
            }

            var eIdx = ConnectionString[sIdx..].IndexOf(";");
            if (eIdx < 0)
            {
                return "";
            }

            var dbKv = ConnectionString[sIdx..eIdx];
            if (string.IsNullOrWhiteSpace(dbKv) || !dbKv.Contains('='))
            {
                return "";
            }

            return dbKv.Split("=")?[1] ?? "";
        }
    }

    public string Operation => CommandText?.Split(' ')?[0] ?? "";

    public string AliasOperator { get; set; } = "AS";

    public int JoiningCounters { get; set; } = 0;

    public virtual bool TestConnection()
    {
        var isConnected = false;
        try
        {
            if (DbConnection == null)
            {
                return false;
            }

            DbConnection.Open();
            isConnected = true;
        }
        catch (ArgumentException aEx)
        {
            Console.WriteLine("Check the Connection String.");
            Console.WriteLine(aEx.Message);
            Console.WriteLine(aEx.ToString());
        }
        catch (DbException ex)
        {
            string sqlErrorMessage = $"Message: {ex.Message} \nSource: {ex.Source}";
            Console.WriteLine(sqlErrorMessage);
            isConnected = false;
        }
        finally
        {
            if (DbConnection?.State == ConnectionState.Open)
            {
                DbConnection.Close();
            }
        }

        return isConnected;
    }

    public Query<TEntity, TConnection, TCommand, TTransaction> SetCommand(string command)
    {
        MainCommand = command;
        JoinCommand = "";
        ConditionCommand = "";
        return this;
    }

    public Query<TEntity, TConnection, TCommand, TTransaction> Select(string name = "", string aliases = "")
    {
        return Select(new TEntity(), name, aliases);
    }

    public Query<TEntity, TConnection, TCommand, TTransaction> Select(TEntity entity, string name = "", string aliases = "")
    {
        Aliases = aliases;
        if (string.IsNullOrWhiteSpace(Aliases))
        {
            Aliases = "T0";
        }

        var items = "*";
        if (!string.IsNullOrWhiteSpace(name))
        {
            items = $"{Aliases}.{name}";
        }

        MainCommand = $"SELECT {items} FROM {TableName} {AliasOperator} {Aliases}";
        MainCommand = MainCommand.Replace("  ", " ");

        if (entity is not null)
        {
            foreach (var prop in GetVulnerableProperties(entity))
            {
                var value = prop.GetValue(entity);
                if (value == default)
                {
                    continue;
                }

                this.Where(prop.Name.ToUpper(), value);
            }
        }

        entity ??= new TEntity();
        Entities = new List<TEntity>() { entity };

        return this;
    }

    public Query<TEntity, TConnection, TCommand, TTransaction> Insert(TEntity entity)
    {
        GenerateParamsAndFields(entity, out string fieldList, out string paramList);
        MainCommand = $"INSERT INTO {TableName} ({fieldList}) VALUES ({paramList})";
        Entities = new List<TEntity>() { entity };
        return this;
    }

    public Query<TEntity, TConnection, TCommand, TTransaction> Update()
    {
        return Update(new TEntity());
    }

    public Query<TEntity, TConnection, TCommand, TTransaction> Update(TEntity entity)
    {
        entity ??= new TEntity();
        Entities = new List<TEntity>() { entity };

        MainCommand = $"UPDATE {TableName}";
        AddSETKeyword(entity);
        return this;
    }

    public Query<TEntity, TConnection, TCommand, TTransaction> Delete(TEntity entity)
    {
        entity ??= new TEntity();
        Entities = new List<TEntity>() { entity };

        MainCommand = $"DELETE FROM {TableName}";

        var pkProp = typeof(TEntity).GetPrimaryKeyProperty();
        var pkName = pkProp?.Name ?? "ID";
        var pkValue = (pkProp?.GetValue(entity) ?? default) ?? (object?)0;

        this.Where(pkName, pkValue);
        return this;
    }

    public Query<TEntity, TConnection, TCommand, TTransaction> AddParameter(string name, object value)
    {
        if (DbCommand == null)
        {
            return this;
        }

        var parameter = DbCommand.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        return AddParameter(parameter);
    }

    public Query<TEntity, TConnection, TCommand, TTransaction> AddParameter(DbParameter parameter)
    {
        if (DbCommand == null)
        {
            return this;
        }

        DbCommand.Parameters.Add(parameter);
        return this;
    }

    #region Utilities methods
    public string VerifyParameterName(string param)
    {
        var l = param.Length - 1;
        var n = Parameters?.Count(x => x.Name.StartsWith(param)) ?? 0;
        return $"{param}{n}";
    }

    private void GenerateParamsAndFields(TEntity entity, out string fieldList, out string paramList)
    {
        fieldList = string.Empty;
        paramList = string.Empty;
        foreach (var prop in GetEntityProperties(entity))
        {
            var value = prop.GetValue(entity, null);
            if (value == null)
            {
                continue;
            }

            var connector = string.IsNullOrWhiteSpace(fieldList) ? "" : ",";
            var field = prop.GetColumnName();
            var param = ParameterPrefix + field;
            fieldList = fieldList + connector + field;
            paramList = paramList + connector + param;
            AddParameter(param, value);
        }
    }

    private static IEnumerable<PropertyInfo> GetVulnerableProperties(TEntity entity)
    {
        return entity
            .GetType()
            .GetProperties()
            .Where(p => !Attribute.IsDefined(p, typeof(IgnoredColumnAttribute), true)
                && p.GetValue(entity) != default);
    }

    private static IEnumerable<PropertyInfo> GetEntityProperties(TEntity entity)
    {
        return entity
            .GetType()
            .GetProperties()
            .Where(p => !Attribute.IsDefined(p, typeof(IgnoredColumnAttribute), true)
                && !Attribute.IsDefined(p, typeof(PrimaryKeyAttribute), true)
                && !Attribute.IsDefined(p, typeof(RelationshipAttribute), true));
    }

    private void AddSETKeyword(TEntity entity)
    {
        foreach (var prop in GetEntityProperties(entity))
        {
            var value = prop.GetValue(entity, null);
            if (value == null)
            {
                continue;
            }

            this.Set(_ => prop.GetColumnName(), value);
        }
    }
    #endregion

    #region Executions
    public virtual async Task<int> ExecuteNonQueryAsync()
    {
        return await await ExecuteAsync(
            async (cmd) => await cmd.ExecuteNonQueryAsync());
    }

    public virtual int ExecuteNonQuery()
    {
        return Execute((cmd) => cmd.ExecuteNonQuery());
    }

    public virtual async Task<IEnumerable<TEntity>> ExecuteQueryAsync()
    {
        await Task.Yield();
        return await await ExecuteAsync(
            async (cmd) =>
            {
                IEnumerable<TEntity> entities = new List<TEntity>();
                using var dr = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);
                while (await dr.ReadAsync())
                {
                    var entity = new TEntity();
                    entity.GetEntityData(dr);
                    entities = UpdateDistinct(entity, entities);
                }

                return entities;
            });
    }

    public virtual IEnumerable<TEntity> ExecuteQuery()
    {
        Task.Yield();
        return Execute(
            (cmd) =>
            {
                IEnumerable<TEntity> entities = new List<TEntity>();
                using var dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                while (dr.Read())
                {
                    var entity = new TEntity();
                    entity.GetEntityData(dr);
                    entities = UpdateDistinct(entity, entities);
                }

                return entities;
            });
    }

    private List<T> UpdateDistinct<T>(T newEntity, IEnumerable<T> entities)
        where T : IEntityBase
    {
        var entityList = entities.ToList();
        var foundDup = entities.Any(e => e.CompareExcludeRelationship(newEntity));
        if (foundDup)
        {
            DeDuplicate(entityList, newEntity);
        }
        else
        {
            entityList.Add(newEntity);
        }

        return entityList;
    }

    private bool DeDuplicate<T>(List<T> entityList, T newEntity)
        where T : IEntityBase
    {
        return entityList
            .Where(e => e.CompareExcludeRelationship(newEntity))
            .Any(current =>
            {
                foreach (var cInfo in current.GetRelationshipProperties())
                {
                    var genericType = cInfo.PropertyType;
                    var nInfo = newEntity.GetType().GetProperty(cInfo.Name);
                    var nValue = nInfo?.GetValue(newEntity);
                    var cValue = cInfo.GetValue(current);
                    IList? nList = nValue as IList;
                    IList? cList = cValue as IList;
                    var value = nList?[0];
                    var valueList = cList;
                    UpdateDistinct(value as dynamic, valueList as dynamic);
                    cInfo.SetValue(current, cList);
                }
                return true;
            });
    }

    protected async Task<TResult> ExecuteAsync<TResult>(Func<TCommand, TResult> action)
    {
        try
        {
            if (DbConnection == null)
            {
                throw new Exception("DbConnection is null");
            }

            if (DbCommand == null)
            {
                throw new Exception("DbCommand is null");
            }

            await DbConnection.OpenAsync();
            DbCommand.Connection = DbConnection;
            DbCommand.CommandText = CommandText;
            await DbCommand.PrepareAsync();
            return action(DbCommand);
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            DbConnection?.CloseAsync();
        }
    }

    protected TResult Execute<TResult>(Func<TCommand, TResult> action)
    {
        try
        {
            if (DbConnection == null)
            {
                throw new Exception("DbConnection is null");
            }

            if (DbCommand == null)
            {
                throw new Exception("DbCommand is null");
            }

            DbConnection.Open();
            DbCommand.Connection = DbConnection;
            DbCommand.CommandText = CommandText;
            DbCommand.Prepare();
            return action(DbCommand);
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            DbConnection?.Close();
        }
    }
    #endregion
}