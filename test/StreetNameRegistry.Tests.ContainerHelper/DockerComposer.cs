namespace StreetNameRegistry.Tests.ContainerHelper
{
    using System.IO;
    using Ductus.FluentDocker.Builders;
    using Ductus.FluentDocker.Services;

    public static class DockerComposer
    {
        public static ICompositeService Compose(string fileName)
        {
            fileName = Path.Combine(Directory.GetCurrentDirectory(), fileName);

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
