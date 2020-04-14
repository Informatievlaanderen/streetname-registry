namespace StreetNameRegistry.Importer.Console
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing.Api;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing.Api.Messages;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing.Json;
    using Newtonsoft.Json;
    using Microsoft.Extensions.Logging;

    public class FileBasedProxyFactory : IApiProxyFactory
    {
        public static IApiProxyFactory BuildFileBasedProxyFactory(ILogger _) => new FileBasedProxyFactory();

        public IApiProxy Create() => new FileBasedProxy();
    }

    public class FileBasedProxy : IApiProxy
    {
        private static readonly DateTime FromInit = DateTime.MinValue;
        private static readonly DateTime UntilInit = new DateTime(2018, 11, 30, 0, 0, 0);
        private static readonly string ImportFolder = $"{FromInit:yyyy-MM-dd}-{UntilInit:yyyy-MM-dd}";
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings().ConfigureForCrabImports();
        private static readonly JsonSerializer Serializer = JsonSerializer.CreateDefault(SerializerSettings);

        public void ImportBatch<TKey>(IEnumerable<KeyImport<TKey>> imports)
        {
            foreach (var import in imports)
            {
                File.WriteAllText(
                    Path.Combine(ImportFolder, $"{import.Key:D9}.json"),
                    Serializer.Serialize(import.Commands));
            }
        }

        public ICommandProcessorOptions<TKey> GetImportOptions<TKey>(ImportOptions options,
            ICommandProcessorBatchConfiguration<TKey> configuration)
        {
            var batchStatus = new BatchStatus
            {
                From = FromInit,
                Until = UntilInit,
                Completed = false
            };

            if (!Directory.Exists(ImportFolder))
                Directory.CreateDirectory(ImportFolder);

            return options.CreateProcessorOptions(batchStatus, configuration);
        }

        public void InitializeImport<TKey>(ICommandProcessorOptions<TKey> options)
        { }

        public void FinalizeImport<TKey>(ICommandProcessorOptions<TKey> options) { }
    }
}
