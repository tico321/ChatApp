using ApplicationCore.Bot.Commands;
using ApplicationCore.Interfaces;
using ApplicationCore.RequestExtensions;
using FluentValidation;
using Infrastructure.Messaging;
using Infrastructure.Stock;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace Bot
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Register MediaR Commands and Queries
            services.AddMediatR(typeof(ProcessCommandMessageCommand).GetTypeInfo().Assembly);
            // Register All Commands and Queries Validators
            typeof(ProcessCommandMessageCommand).Assembly.GetTypes()
                .Where(type => typeof(IValidator).IsAssignableFrom(type))
                .ToList()
                .ForEach(type => services.AddTransient(typeof(IValidator), type));
            // Add pipeline to validate all messages before they are processed
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>));

            // Stock
            services.AddSingleton((serviceProvider) =>
                new StockConfiguration
                {
                    BaseUrl = Configuration["Stock:BaseUrl"]
                });
            services.AddScoped<IStockService, StockService>();
            services.AddHttpClient(StockService.ClientName, (serviceProvider, client) =>
            {
                var config = serviceProvider.GetService<StockConfiguration>();
                client.BaseAddress = new Uri(config.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(5);
            });

            // Messaging
            services.AddSingleton((serviceProvider) =>
                new RabbitMQConfig
                {
                    HostName = Configuration["RabbitMQ:HostName"],
                    Port = int.Parse(Configuration["RabbitMQ:Port"]),
                    User = Configuration["RabbitMQ:User"],
                    Password = Configuration["RabbitMQ:Pass"]
                });
            services.AddScoped<IMessagingService, RabbitMQMessagingService>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
