using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using suporteEngenhariaUI.Interfaces;
using suporteEngenhariaUI.Services;
using System;
using System.IO;
using System.Net.Http.Headers;
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
                    // Configurar serviços
                    ConfigureServices(services, context.Configuration);
                });
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // Lê as configurações (BaseUrl, Timeout) do IConfiguration
            string baseUrl = configuration.GetValue<string>("ApiSettings:BaseUrl") ?? "http://ERRO_URL_NAO_CONFIG/";
            int timeoutSeconds = configuration.GetValue<int?>("ApiSettings:TimeoutSeconds") ?? 30;

            // REGISTRA o serviço E CONFIGURA seu HttpClient
            services.AddHttpClient<IWhatsAppApiService, WhatsAppApiService>(client =>
            {
                try
                {
                    client.BaseAddress = new Uri(baseUrl);
                    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    // Adicionar outros headers/autenticação aqui, se necessário
                }
                catch (Exception ex)
                {
                    // É crucial tratar erros de configuração aqui para diagnóstico
                    MessageBox.Show($"Erro configurar HttpClient: {ex.Message}\nURL Base: '{baseUrl}'", "Erro Crítico de Configuração", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    throw new InvalidOperationException($"Falha configurar HttpClient. Verifique a URL: {baseUrl}", ex);
                }
            });
            // Registrar configuração
            services.AddSingleton(configuration);

            // Registrar formulários
            services.AddTransient<FormPrincipal>();

            // Registrar serviços
            services.AddTransient<IWhatsAppApiService, WhatsAppApiService>();

            // Registrar outros serviços conforme necessário
            // services.AddTransient<IOutroServiço, ImplementaçãoDoServiço>();
        }
    }
}