using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using suporteEngenhariaUI.Interfaces;
using suporteEngenhariaUI.Services;
using System;
using System.IO;
using System.Windows.Forms;

namespace suporteEngenhariaUI
{
    internal static class Program
    {
        /// <summary>
        /// Ponto de entrada principal para o aplicativo.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var host = CreateHostBuilder().Build();
            ServiceProvider = host.Services;

            Application.Run(ServiceProvider.GetRequiredService<FormPrincipal>());
        }

        public static IServiceProvider ServiceProvider { get; private set; }

        static IHostBuilder CreateHostBuilder()
        {
            return Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    // Configurar servi�os
                    ConfigureServices(services, context.Configuration);
                });
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // Registrar configura��o
            services.AddSingleton(configuration);

            // Registrar formul�rios
            services.AddTransient<FormPrincipal>();

            // Registrar servi�os
            services.AddTransient<IWhatsAppApiService, WhatsAppApiService>();

            // Registrar outros servi�os conforme necess�rio
            // services.AddTransient<IOutroServi�o, Implementa��oDoServi�o>();
        }
    }
}