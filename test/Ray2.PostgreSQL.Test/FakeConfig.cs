﻿using Microsoft.Extensions.DependencyInjection;
using Orleans.Runtime;
using Ray2.Configuration;
using Ray2.Serialization;
using Ray2.Storage;
using System;
using Moq;

namespace Ray2.PostgreSQL.Test
{
    public static class FakeConfig
    {
        public const string ConnectionString = "Server=localhost;Port=5432;Database=ray2;User Id=postgres; Password=sapass;Pooling=true;MaxPoolSize=50;Timeout=10;";


        public const string ProviderName = "Default";

        public static PostgreSqlOptions Options = new PostgreSqlOptions()
        {
            ConnectionString = FakeConfig.ConnectionString
        };

        public static IServiceProvider BuildServiceProvider()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddOptions().Configure<PostgreSqlOptions>(ProviderName, opt =>
            {
                opt.ConnectionString = Options.ConnectionString;
            });
            services.AddLogging();
            Mock<IInternalConfiguration> internalConfiguration = new Mock<IInternalConfiguration>();

            var type = typeof(TestEvent);
            string name = type.FullName;
            internalConfiguration.Setup(f => f.GetEvenType(name, out type)).Returns(true);
            services.AddSingleton<IInternalConfiguration>(internalConfiguration.Object);
            services.AddSingleton(typeof(IKeyedServiceCollection<,>), typeof(KeyedServiceCollection<,>));
            services.AddSingletonNamedService<ISerializer, JsonSerializer>(SerializationType.JsonUTF8);
            services.AddSingletonNamedService(ProviderName, (Func<IServiceProvider, string, IStateStorage>)((sp, n) =>
            {
                return new PostgreSQL.StateStorage(sp, n);
            }));
            services.AddSingletonNamedService<IEventStorage>(ProviderName, (sp, n) =>
            {
                return new EventStorage(sp, n);
            });
            services.AddSingletonNamedService<IPostgreSqlTableStorage>(ProviderName, (sp, n) =>
            {
                return new PostgreSqlTableStorage(sp, n);
            });
            return services.BuildServiceProvider();
        }
    }
}
