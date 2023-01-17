namespace StreetNameRegistry.Tests.ContainerHelper
{
    using System.IO;
    using Ductus.FluentDocker.Builders;
    using Ductus.FluentDocker.Services;

    public static class BackOfficeApiContainers
    {
        public static ICompositeService Compose()
        {
            var fileName = Path.Combine(Directory.GetCurrentDirectory(), "backofficeapidependencies.yml");

            return new Builder()
                .UseContainer()
                .UseCompose()
                .FromFile(fileName)
                .RemoveOrphans()
                .Build()
                .Start();
        }
    }
}
