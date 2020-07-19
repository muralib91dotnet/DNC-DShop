﻿using System;
using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Consul;
using DShop.Common;
using DShop.Common.Consul;
using DShop.Common.Dispatchers;
using DShop.Common.Jaeger;
using DShop.Common.Mongo;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DShop.Common.Mvc;
using DShop.Common.RabbitMq;
using DShop.Common.RestEase;
using DShop.Services.Discounts.Domain;
using DShop.Services.Discounts.Messages.Commands;
using DShop.Services.Discounts.Messages.Events;
using DShop.Services.Discounts.Metrics;
using DShop.Services.Discounts.Services;
using OpenTracing;

namespace DShop.Services.Discounts
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IContainer Container { get; private set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            //NOTE: AddCustomMvc is custom Extension method which injects the necessary dependencies, instead of adding bulk of references using single Aspnet.Core.App package
            services.AddCustomMvc();
            //Seeddata extension
            services.AddInitializers(typeof(IMongoDbInitializer));
            services.AddConsul();
            services.AddJaeger();
            services.RegisterServiceForwarder<IOrdersService>("orders-service");
            services.AddTransient<IMetricsRegistry, MetricsRegistry>();

            var builder = new ContainerBuilder();
            //Scans & registers all the interfaces defined in the current assembly with the concrete types implementing.
            //Eg: Here it registers ICustomerRepository, IDiscountsRepository.
            //NOTE: Since this registers all interfaces in current assembly, it might register unwanted interfaces also
            builder.RegisterAssemblyTypes(typeof(Startup).Assembly)
                .AsImplementedInterfaces();
            builder.Populate(services);

            //For sending commands
            builder.AddDispatchers();
            builder.AddMongo();
            builder.AddMongoRepository<Customer>("Customers");
            builder.AddMongoRepository<Discount>("Discounts");
            builder.AddRabbitMq();

            Container = builder.Build();

            return new AutofacServiceProvider(Container);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env,
            IApplicationLifetime applicationLifetime, IStartupInitializer initializer,
            IConsulClient consulClient)
        {
            if (env.IsDevelopment() || env.EnvironmentName == "local")
            {
                app.UseDeveloperExceptionPage();
            }

            initializer.InitializeAsync();
            app.UseMvc();
            app.UseRabbitMq()
                .SubscribeCommand<CreateDiscount>(onError: (cmd, ex)
                    => new CreateDiscountRejected(cmd.CustomerId, ex.Message, "customer_not_found"))
                .SubscribeEvent<CustomerCreated>(@namespace: "customers")
                .SubscribeEvent<OrderCompleted>(@namespace: "orders");
            var serviceId = app.UseConsul();

            //NOTE: Container getting disposed on STOP of application & closes all Connections like Sql, File-reader & any other memory resources used etc
            applicationLifetime.ApplicationStopped.Register(() =>
            {
                consulClient.Agent.ServiceDeregister(serviceId);
                Container.Dispose();
            });
        }
    }
}
