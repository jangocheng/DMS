﻿using DMS.BaseFramework.Common.DI;
using DMS.Consul;
using DMS.Consul.Entity;
using DMS.Exceptionless.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace DMS.WebAPITest
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            #region 日志文件加载一：在此配置也是可以；加载二：在Program加载也是可以的
            //var currentDir = Directory.GetCurrentDirectory();
            //var log4netPath = $@"{currentDir}\Config\log4net.config";
            //configuration.ConfigureLog4net(log4netPath);//指定log4net配置文件

            //var nlogPath = $@"{currentDir}\Config\nlog.config";
            //configuration.ConfigureNLog(nlogPath);
            #endregion
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options =>
            {
                //添加全局异常捕获
                options.Filters.Add<GlobalExceptionFilter>();
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_1);


            #region 注入HttpContext
            //services.AddSingleton<Microsoft.AspNetCore.Http.IHttpContextAccessor, Microsoft.AspNetCore.Http.HttpContextAccessor>();
            services.AddHttpContextAccessor();
            #endregion

        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime lifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            app.UseStaticFiles();


            #region 使用HttpContext
            app.UseStaticHttpContext();
            #endregion

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
  
            #region register this service
            ConsulService consulService = new ConsulService()
            {
                IP = Configuration["Consul:IP"],
                Port = Convert.ToInt32(Configuration["Consul:Port"])
            };
            HealthService healthService = new HealthService()
            {
                IP = Configuration["Service:IP"],
                Port = Convert.ToInt32(Configuration["Service:Port"]),
                Name = Configuration["Service:Name"],
            };
            //app.RegisterConsul(lifetime, healthService, consulService);
            #endregion
        }
    }
}
