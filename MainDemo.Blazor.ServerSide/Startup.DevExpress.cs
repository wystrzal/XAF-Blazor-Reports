//------------------------------------------------------------------------------
//<auto-generated>
// Generated by the DevExpress.Blazor package.
// To prevent this operation, add the DxExtendStartupHost property to the project and set this property to False.
//
// MainDemo.Blazor.ServerSide.csproj:
//
// <Project Sdk="Microsoft.NET.Sdk.Web">
//  <PropertyGroup>
//    <TargetFramework>netcoreapp3.1</TargetFramework>
//    <DxExtendStartupHost>False</DxExtendStartupHost>
//  </PropertyGroup>
//</auto-generated>
//------------------------------------------------------------------------------
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(MainDemo.Blazor.ServerSide.DevExpressHostingStartup))]

namespace MainDemo.Blazor.ServerSide {
    public partial class DevExpressHostingStartup : IHostingStartup {
        void IHostingStartup.Configure(IWebHostBuilder builder) {
            builder.ConfigureServices((serviceCollection) => {
                serviceCollection.AddDevExpressBlazor();
            });
        }
    }
}
