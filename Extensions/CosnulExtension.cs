using Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MicroService.Extensions
{
    public static class CosnulExtension
    {
        public static IApplicationBuilder UseConsul(this IApplicationBuilder app, IHostApplicationLifetime lifetime, IConfiguration configuration)
        {
            var client = new ConsulClient(cc =>
            {
                cc.Address = new Uri(configuration["RegistryCenter"]);
            });

            var serviceId = Guid.NewGuid().ToString();
            var ip = configuration["ip"];
            var port = configuration["port"];

            var registration = new AgentServiceRegistration
            {
                ID = serviceId,
                Name = configuration["ServiceName"],
                Address = ip,
                Port = Convert.ToInt32(port),
                Check = new AgentServiceCheck
                {
                    Interval = TimeSpan.FromSeconds(10),
                    HTTP = $"http://{ip}:{port}/micro/health",
                    Timeout = TimeSpan.FromSeconds(5),
                    DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(5)
                }
            };

            client.Agent.ServiceRegister(registration);
            lifetime.ApplicationStopping.Register(() => {
                client.Agent.ServiceDeregister(serviceId);
            });

            return app;
        }
    }
}
