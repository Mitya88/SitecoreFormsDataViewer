namespace FormsViewer.Service.DI
{
    using FormsViewer.Service.Controllers;
    using FormsViewer.Service.Services;
    using Microsoft.Extensions.DependencyInjection;
    using Sitecore.DependencyInjection;

    /// <summary>
    /// Registering the DI container
    /// </summary>
    /// <seealso cref="Sitecore.DependencyInjection.IServicesConfigurator"/>
    public class RegisterContainer : IServicesConfigurator
    {
        /// <summary>
        /// Configures the specified service collection.
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        public void Configure(IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IExportService, ExportService>();
            serviceCollection.AddTransient<FormsViewerApiController>();
        }
    }
}