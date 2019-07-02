using ApplicationCore.Chat.Commands;
using ApplicationCore.Interfaces;
using ApplicationCore.RequestExtensions;
using ApplicationCore.Users.Domain;
using FluentValidation;
using Infrastructure.Bot;
using Infrastructure.Data;
using Infrastructure.Messaging;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using Web.Filters;
using Web.Hubs;

[assembly: ApiConventionType(typeof(DefaultApiConventions))]
namespace Web
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
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            // Db Context
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("AppDatabase"));
            services.AddScoped<IAppDbContext, ApplicationDbContext>();
            services.AddDefaultIdentity<ApplicationUser>()
                .AddDefaultUI(UIFramework.Bootstrap4)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            // Register MediaR Commands and Queries
            services.AddMediatR(typeof(SendMessageCommand).GetTypeInfo().Assembly);
            // Register All Commands and Queries Validators
            typeof(SendMessageCommand).Assembly.GetTypes()
                .Where(type => typeof(IValidator).IsAssignableFrom(type))
                .ToList()
                .ForEach(type => services.AddTransient(typeof(IValidator), type));
            // Add pipeline to validate all messages before they are processed
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>));

            // Bot Service
            services.AddSingleton((serviceProvider) =>
                new BotConfiguration
                {
                    BaseUrl = Configuration["Bot:BaseUrl"]
                });
            services.AddHttpClient(BotService.ClientName, (serviceProvider, client) =>
            {
                var config = serviceProvider.GetService<BotConfiguration>();
                client.BaseAddress = new Uri(config.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(5);
            });
            services.AddSingleton<IBotService, BotService>();

            // Messaging
            services.AddSingleton((serviceProvider) =>
                new RabbitMQConfig
                {
                    HostName = Configuration["RabbitMQ:HostName"],
                    Port = int.Parse(Configuration["RabbitMQ:Port"]),
                    User = Configuration["RabbitMQ:User"],
                    Password = Configuration["RabbitMQ:Pass"]
                });
            services.AddSingleton<IMessagingService, RabbitMQMessagingService>();

            // Broadcast
            services.AddScoped<IBroadcastService, BroadcastService>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddMvc(options =>
                {
                    options.Filters.Add(typeof(GlobalErrorFilter));
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddSignalR();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            IServiceProvider serviceProvider,
            IApplicationLifetime applicationLifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseSignalR(routes =>
            {
                routes.MapHub<ChatHub>("/chatHub");
            });

            app.UseMvc();

            // Setting up rabbitmq consumer
            // This is a workaround implemented due to time constraints
            var messagingService = serviceProvider.GetService<IMessagingService>();
            ServiceProviderReference.ServiceProvider = app.ApplicationServices;
            messagingService.RegisterHandler<HandleQueuedMessageCommand>(
                ApplicationCore.Chat.Domain.Constants.MessagingQueue,
                msg =>
                {
                    var sp = ServiceProviderReference.ServiceProvider;
                    using(var scope = sp.CreateScope())
                    {
                        var handler = scope.ServiceProvider
                            .GetService<IRequestHandler<HandleQueuedMessageCommand, HandleQueuedMessageCommandResult>>();
                        var t = handler.Handle(msg, new CancellationToken()).Result;
                    }
                });
            applicationLifetime.ApplicationStopping.Register(() => OnShutDown(serviceProvider));
        }

        public class ServiceProviderReference
        {
            public static IServiceProvider ServiceProvider { get; set; }
        }

        private void OnShutDown(IServiceProvider sp)
        {
            var messaging = (RabbitMQMessagingService)sp.GetService<IMessagingService>();
            messaging.Dispose();
        }
    }
}
