namespace StreetNameRegistry.Projections.Elastic.IntegrationTests
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using FluentAssertions;
    using Municipality;
    using StreetNameList;
    using Xunit;

    [Collection("Elastic")]
    public class StreetNameListElasticClientTests : IClassFixture<ElasticsearchClientTestFixture>
    {
        private readonly ElasticsearchClientTestFixture _clientFixture;
        private readonly Fixture _fixture;

        public StreetNameListElasticClientTests(ElasticsearchClientTestFixture clientFixture)
        {
            _clientFixture = clientFixture;
            _fixture = new Fixture();
        }

        [Fact]
        public async Task CreateDocument()
        {
            var client = await BuildClient();

            var givenDocument = _fixture.Create<StreetNameListDocument>();
            await client.CreateDocument(givenDocument, CancellationToken.None);

            var actualDocument = (await client.GetDocuments([givenDocument.StreetNamePersistentLocalId], CancellationToken.None)).Single();
            actualDocument.Should().NotBeNull();
        }

        [Fact]
        public async Task UpdateDocument()
        {
            var client = await BuildClient();

            var givenDocument = _fixture.Create<StreetNameListDocument>();
            await client.CreateDocument(givenDocument, CancellationToken.None);

            var updateDocument = _fixture.Create<StreetNameListDocument>();
            updateDocument.StreetNamePersistentLocalId = givenDocument.StreetNamePersistentLocalId;
            await client.UpdateDocument(updateDocument, CancellationToken.None);

            var actualDocument = (await client.GetDocuments([updateDocument.StreetNamePersistentLocalId], CancellationToken.None)).Single();

            actualDocument.Should().BeEquivalentTo(updateDocument);
        }

        [Fact]
        public async Task GetDocuments()
        {
            var client = await BuildClient();

            var givenDocument = _fixture.Create<StreetNameListDocument>();
            await client.CreateDocument(givenDocument, CancellationToken.None);

            var actualDocument = (await client.GetDocuments([givenDocument.StreetNamePersistentLocalId], CancellationToken.None)).Single();

            actualDocument.Should().BeEquivalentTo(givenDocument);
        }

        [Fact]
        public async Task UpdateDocument_ShouldUpdateOnlyProvidedFields()
        {
            var client = await BuildClient();

            var givenDocument = _fixture.Create<StreetNameListDocument>();
            await client.CreateDocument(givenDocument, CancellationToken.None);

            var documentUpdate = new StreetNameListPartialDocument(DateTimeOffset.Now)
            {
                Status = _fixture.Create<StreetNameStatus>()
            };

            EnsureAllPropertiesAreNotNull(documentUpdate);

            await client.PartialUpdateDocument(
                givenDocument.StreetNamePersistentLocalId,
                documentUpdate,
                CancellationToken.None);

            var actualDocument = (await client.GetDocuments([givenDocument.StreetNamePersistentLocalId], CancellationToken.None)).Single();

            actualDocument.StreetNamePersistentLocalId.Should().Be(givenDocument.StreetNamePersistentLocalId);
            actualDocument.Status.Should().Be(documentUpdate.Status);
            actualDocument.VersionTimestamp.Should().Be(documentUpdate.VersionTimestamp);
        }

        [Fact]
        public async Task DeleteDocument()
        {
            var client = await BuildClient();

            var givenDocument = _fixture.Create<StreetNameListDocument>();
            await client.CreateDocument(givenDocument, CancellationToken.None);

            await client.DeleteDocument(givenDocument.StreetNamePersistentLocalId, CancellationToken.None);
            var actualDocument = (await client.GetDocuments([givenDocument.StreetNamePersistentLocalId], CancellationToken.None)).SingleOrDefault();

            actualDocument.Should().BeNull();
        }

        private async Task<IStreetNameListElasticClient> BuildClient()
        {
            var indexName = $"test-{Guid.NewGuid():N}";
            await _clientFixture.CreateIndex(indexName);
            return new StreetNameListElasticClient(_clientFixture.Client, indexName);
        }

        private void EnsureAllPropertiesAreNotNull(object value)
        {
            var properties = value.GetType().GetProperties();
            properties.Should().NotBeEmpty();
            foreach (var pi in properties)
            {
                pi.GetValue(value).Should().NotBeNull();
            }
        }
    }
}
