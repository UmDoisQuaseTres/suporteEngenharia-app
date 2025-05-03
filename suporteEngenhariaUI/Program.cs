using Microsoft.Extensions.DependencyInjection;
using System;
using System.Configuration; // Para App.config
using System.Net.Http.Headers;
using System.Windows.Forms;
using suporteEngenhariaUI.Interfaces;
using suporteEngenhariaUI.Services;

namespace suporteEngenhariaUI // Namespace raiz ou da pasta onde Program.cs est�
{
    internal static class Program
    {
        public static IServiceProvider? ServiceProvider { get; private set; }

        [STAThread]
        static void Main()
        {
            ConfigureServices(); // Configura DI PRIMEIRO
            ApplicationConfiguration.Initialize(); // Config padr�o depois

            if (ServiceProvider != null)
            {
                var mainForm = ServiceProvider.GetRequiredService<FormPrincipal>();
                Application.Run(mainForm);
            }
            else { MessageBox.Show("Falha cr�tica ao configurar servi�os.", "Erro Fatal", MessageBoxButtons.OK, MessageBoxIcon.Stop); }
        }

        static void ConfigureServices()
        {
            var services = new ServiceCollection();
            string baseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"] ?? "http://INDEFINIDO/";
            string timeoutSetting = ConfigurationManager.AppSettings["ApiTimeoutSeconds"] ?? "30";
            if (!int.TryParse(timeoutSetting, out int timeoutSeconds)) timeoutSeconds = 30;

            services.AddHttpClient<IWhatsAppApiService, WhatsAppApiService>(client =>
            {
                try
                {
                    client.BaseAddress = new Uri(baseUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
                    // Configurar Autentica��o aqui se necess�rio
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro configurar HttpClient: {ex.Message}\nURL Base: '{baseUrl}'", "Erro Cr�tico", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    throw new InvalidOperationException($"Falha configurar HttpClient. URL: {baseUrl}", ex);
                }
            });
            // .AddTransientHttpErrorPolicy(...) // Opcional: Polly

            services.AddTransient<FormPrincipal>(); // Registra o formul�rio

            try { ServiceProvider = services.BuildServiceProvider(); }
            catch (Exception ex) { MessageBox.Show($"Erro construir DI: {ex.Message}", "Erro Fatal DI", MessageBoxButtons.OK, MessageBoxIcon.Stop); ServiceProvider = null; }
        }
    }
}