﻿using System;
using DevExpress.AspNetCore.Reporting;
using DevExpress.Blazor.Reporting;
using DevExpress.ExpressApp.Blazor.Services;
using DevExpress.ExpressApp.ReportsV2.Blazor;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.Base;
using DevExpress.XtraReports.Web.WebDocumentViewer;
using MainDemo.Blazor.ServerSide.Services;
using MainDemo.Module.Blazor;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MainDemo.Blazor.ServerSide {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services) {
            //services.AddSingleton(typeof(HubConnectionHandler<>), typeof(ProxyHubConnectionHandler<>));

            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddHttpContextAccessor();
            services.AddSingleton<XpoDataStoreProviderAccessor>();
            services.AddScoped<CircuitHandler, CircuitHandlerProxy>();
            services.AddXaf<MainDemoBlazorApplication>(Configuration);
            services.AddXafReporting();

            services.AddXafSecurity(options => {
                options.RoleType = typeof(DevExpress.Persistent.BaseImpl.PermissionPolicy.PermissionPolicyRole);
                options.UserType = typeof(DevExpress.Persistent.BaseImpl.PermissionPolicy.PermissionPolicyUser);
                options.Events.OnSecurityStrategyCreated = securityStrategy => ((SecurityStrategy)securityStrategy).RegisterXPOAdapterProviders();
                options.SupportNavigationPermissionsForTypes = false;
            }).AddExternalAuthentication<HttpContextPrincipalProvider>()
            .AddAuthenticationStandard(options => {
                options.IsSupportChangePassword = true;
            });

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options => {
                options.LoginPath = "/LoginPage";
            });

            DefaultWebDocumentViewerContainer.Register<DocumentOperationService, CustomDocumentOperationService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if(env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }
            else {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseRequestLocalization();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseXaf();
            app.UseDevExpressBlazorReporting();
            app.UseEndpoints(endpoints => {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
                endpoints.MapControllers();
            });
        }
    }
}
