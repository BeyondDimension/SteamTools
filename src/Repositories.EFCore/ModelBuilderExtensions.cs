using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Application.Columns;
using System.Application.Entities;
using System.Application.Models;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace System
{
    public static class ModelBuilderExtensions
    {
        const string TAG = "ModelBuilderEx";

        /// <summary>
        /// 软删除的查询过滤表达式
        /// <para>https://docs.microsoft.com/zh-cn/ef/core/querying/filters#example</para>
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        static LambdaExpression SoftDeletedQueryFilter(Type type)
        {
            var parameter = Expression.Parameter(type);
            var left = Expression.PropertyOrField(parameter, nameof(ISoftDeleted.SoftDeleted));
            var body = Expression.Not(left);
            return Expression.Lambda(body, parameter);
        }

        /// <summary>
        /// 根据实体模型继承的接口，生成列的 索引/默认值
        /// 在 <see cref="DbContext.OnModelCreating(ModelBuilder)"/> 中调用此函数，仅支持 SqlServer
        /// </summary>
        /// <param name="modelBuilder"></param>
        /// <param name="settings"></param>
        public static IEnumerable<IMutableEntityType> BuildEntities(
            this ModelBuilder modelBuilder,
            IDatabaseSettings settings)
        {
            var hasTablePrefix = !string.IsNullOrWhiteSpace(settings.TablePrefix);
            var entityTypes = modelBuilder.Model.GetEntityTypes();
            if (entityTypes == null) throw new NullReferenceException(nameof(entityTypes));
            foreach (var entityType in entityTypes)
            {
                var type = entityType.ClrType;
                if (type == sharedType) continue;

                if (hasTablePrefix)
                {
                    var oldTableName = entityType.GetTableName();
                    if (!oldTableName.StartsWith(settings.TablePrefix))
                    {
                        var tableName = settings.TablePrefix + oldTableName;
                        modelBuilder.Entity(type, b => b.ToTable(tableName));
                        Log.Info(TAG,
                            "TablePrefix type: {0}, tableName: {1}, oldTableName: {2}",
                            type.FullName,
                            tableName,
                            oldTableName);
                    }
                }

                #region 继承自 排序(IOrder) 接口的要设置索引

                if (pOrder.IsAssignableFrom(type))
                {
                    modelBuilder.Entity(type, p => p.HasIndex(nameof(IOrder.Order)));
                    //Log.Info(TAG, "IOrder type: {0}", type.FullName);
                }

                #endregion

                #region 继承自 软删除(IsSoftDeleted) 接口的要设置索引

                if (pSoftDeleted.IsAssignableFrom(type))
                {
                    // https://docs.microsoft.com/zh-cn/ef/core/querying/filters
                    modelBuilder.Entity(type, p => p.HasIndex(nameof(ISoftDeleted.SoftDeleted)));
                    modelBuilder.Entity(type).HasQueryFilter(SoftDeletedQueryFilter(type));
                    softDeleted.Add(type);
                    //Log.Info(TAG, "IsSoftDeleted type: {0}", type.FullName);
                }

                #endregion

                #region 继承自 创建时间(ICreationTime) 接口的要设置默认值使用数据库当前时间

                if (pCreationTime.IsAssignableFrom(type))
                {
                    modelBuilder.Entity(type, p => p.Property(nameof(ICreationTime.CreationTime)).HasDefaultValueSql(SQLStrings.SYSDATETIMEOFFSET).IsRequired());
                    //Log.Info(TAG, "ICreationTime type: {0}", type.FullName);
                }

                #endregion

                #region 继承自 更新时间(IUpdateTime) 接口的要设置默认值使用数据库当前时间与更新时间

                if (pUpdateTime.IsAssignableFrom(type))
                {
                    modelBuilder.Entity(type, p => p.Property(nameof(IUpdateTime.UpdateTime)).HasDefaultValueSql(SQLStrings.SYSDATETIMEOFFSET).IsRequired());
                    modelBuilder.Entity(type, p => p.Property(nameof(IUpdateTime.UpdateTime)).ValueGeneratedOnAddOrUpdate()
                    .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Save));
                    //Log.Info(TAG, "IUpdateTime type: {0}", type.FullName);
                }

                if (pPhoneNumber.IsAssignableFrom(type))
                {
                    modelBuilder.Entity(type, p => p.Property(nameof(IPhoneNumber.PhoneNumber)).HasMaxLength(IPhoneNumber.Db_MaxLength_PhoneNumber));
                }

                #endregion

                #region  继承 主键为GUID(INEWSEQUENTIALID) 接口的要设置默认值使用 NEWSEQUENTIALID

                if (pkGuid.IsAssignableFrom(type))
                {
                    modelBuilder.Entity(type, b => b.Property(nameof(IEntity<Guid>.Id)).HasDefaultValueSql(SQLStrings.NEWSEQUENTIALID));
                    //Log.Info(TAG, "INEWSEQUENTIALID type: {0}", type.FullName);
                }

                #endregion
            }
            return entityTypes;
        }

        public static readonly Type pOrder = typeof(IOrder);
        public static readonly Type pSoftDeleted = typeof(ISoftDeleted);
        public static readonly Type pCreationTime = typeof(ICreationTime);
        public static readonly Type pUpdateTime = typeof(IUpdateTime);
        public static readonly Type pkGuid = typeof(INEWSEQUENTIALID);
        public static readonly Type pPhoneNumber = typeof(IPhoneNumber);
        /// <summary>
        /// https://docs.microsoft.com/zh-cn/ef/core/modeling/shadow-properties#property-bag-entity-types
        /// </summary>
        public static readonly Type sharedType = typeof(Dictionary<string, object>);
        static readonly HashSet<Type> softDeleted = new();

        public static bool IsSoftDeleted(Type type) => softDeleted.Contains(type);

    }
}