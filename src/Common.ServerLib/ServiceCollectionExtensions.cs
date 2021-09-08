using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Application.Models;
using System.Security;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加由 Repository 实现的 <see cref="IStorage"/>
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection TryAddStorage<TDbContext>(this IServiceCollection services) where TDbContext : DbContext
        {
            services.TryAddScoped<IStorage, ServerStorage<TDbContext>>();
            return services;
        }

        /// <summary>
        /// 隐藏服务响应头
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection HideServerHeader(this IServiceCollection services)
        {
            services.Configure<KestrelServerOptions>(options =>
            {
                options.AddServerHeader = false;
            });
            return services;
        }

        const string MigrationsHistory = "__EFMigrationsHistory";

        /// <summary>
        /// 添加数据库上下文，并配置使用 SqlServer
        /// </summary>
        /// <typeparam name="TContext"></typeparam>
        /// <typeparam name="TAppSettings"></typeparam>
        /// <param name="services"></param>
        /// <param name="appSettings"></param>
        /// <param name="connectionString"></param>
        /// <param name="migrationsHistoryTableNameSuffix"></param>
        /// <returns></returns>
        public static TAppSettings AddDbContextWithAppSettings<TContext, TAppSettings>(
            this IServiceCollection services,
            TAppSettings appSettings,
            string connectionString,
            string? migrationsHistoryTableNameSuffix = null)
            where TContext : DbContext
            where TAppSettings : class, IDatabaseSettings
        {
            var tablePrefix = appSettings.TablePrefix;
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                var hasTablePrefix = !string.IsNullOrEmpty(tablePrefix);
                var hasSuffix = !string.IsNullOrEmpty(migrationsHistoryTableNameSuffix);
                void SqlServerOptionsAction(SqlServerDbContextOptionsBuilder b)
                {
                    var tableName = MigrationsHistory;
                    if (hasTablePrefix)
                    {
                        tableName = tablePrefix + tableName;
                    }
                    if (hasSuffix)
                    {
                        tableName = tableName + "_" + migrationsHistoryTableNameSuffix;
                    }
                    b.MigrationsHistoryTable(tableName);
                }
                Action<SqlServerDbContextOptionsBuilder>? sqlServerOptionsAction
                    = (hasTablePrefix || hasSuffix) ? SqlServerOptionsAction : null;
                services.AddDbContext<TContext>(options
                    => options.UseSqlServer(connectionString, sqlServerOptionsAction));
            }
            return appSettings;
        }

        /// <summary>
        /// 添加数据库上下文，并配置使用 SqlServer
        /// </summary>
        /// <typeparam name="TContext"></typeparam>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="cfgName"></param>
        /// <returns></returns>
        public static TAppSettings AddDbContextWithAppSettings<TContext, TAppSettings>(
            this IServiceCollection services,
            IConfiguration configuration,
            string cfgName = "DefaultConnection",
            string? migrationsHistoryTableNameSuffix = null)
            where TContext : DbContext
            where TAppSettings : class, IDatabaseSettings
        {
            var appSettings_ = configuration.GetSection("AppSettings");
            services.Configure<TAppSettings>(appSettings_);
            var appSettings = appSettings_.Get<TAppSettings>();
            var connectionString = configuration.GetConnectionString(cfgName);
            return services.AddDbContextWithAppSettings<TContext, TAppSettings>(appSettings, connectionString, migrationsHistoryTableNameSuffix);
        }
    }
}