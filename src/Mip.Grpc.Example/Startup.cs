﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Mip.Grpc.Example
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc();

            // add handlers implementing IHandler<TRequest, TResponse>
            // these are the implementations the grpc requests will be forwarded to.
            services.AddDispatchedGrpcHandlers(new[] { typeof(Startup).Assembly },
                builder => 
                {
                    // override the default SayNothingHandler
                    builder.Add<AlternativeSayNothingHandler>(nameof(Greeter.GreeterBase.SayNothing));
                });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                // usually you would have to implement a class deriving from Greeter.GreeterBase
                // override its methods and then register it like this:
                //   endpoints.MapGrpcService<GreeterService>();

                // but instead, you write
                endpoints.CompileAndMapGrpcServiceDispatcher(app.ApplicationServices, typeof(Greeter.GreeterBase));
                // this will compile a class deriving from Greeter.GreeterBase
                // overriding its methods so that they get forwarded to implementations of IHandler<TRequest, TResponse>

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });
        }
    }
}
