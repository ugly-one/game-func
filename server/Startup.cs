using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace server
{
    public class Startup{

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSignalR();
        }

        public void Configure(IApplicationBuilder app){
            
            app.UseRouting();
            app.UseEndpoints(routeBuilder => routeBuilder.MapHub<SomeHub>(HubConfiguration.Name));
            app.Run(context => {
                return context.Response.WriteAsync("Hello world");
            });
            
        }
    }
}