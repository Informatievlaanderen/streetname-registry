namespace StreetNameRegistry.Tests.BackOffice
{
    using System;
    using System.Threading.Tasks;
    using Consumer;
    using Consumer.Municipality;
    using global::AutoFixture;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Projections.Syndication;
    using Projections.Syndication.Municipality;

    public sealed class TestSyndicationContext : SyndicationContext
    {
        private readonly bool _dontDispose;

        // This needs to be here to please EF
        public TestSyndicationContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public TestSyndicationContext(DbContextOptions<SyndicationContext> options, bool dontDispose = false)
            : base(options)
        {
            _dontDispose = dontDispose;
        }

        public MunicipalityLatestItem AddMunicipalityLatestItemFixture()
        {
            var municipalityLatestItem = new Fixture().Create<MunicipalityLatestItem>();
            MunicipalityLatestItems.Add(municipalityLatestItem);
            SaveChanges();
            return municipalityLatestItem;
        }

        public MunicipalityLatestItem AddMunicipalityLatestItemFixtureWithNisCode(string nisCode)
        {
            var municipalityLatestItem = new Fixture().Create<MunicipalityLatestItem>();
            municipalityLatestItem.NisCode = nisCode;
            MunicipalityLatestItems.Add(municipalityLatestItem);
            SaveChanges();
            return municipalityLatestItem;
        }

        public MunicipalityLatestItem AddMunicipalityLatestItemFixtureWithMunicipalityIdAndNisCode(Guid municipalityId, string nisCode)
        {
            var municipalityLatestItem = new Fixture().Create<MunicipalityLatestItem>();
            municipalityLatestItem.MunicipalityId = municipalityId;
            municipalityLatestItem.NisCode = nisCode;
            MunicipalityLatestItems.Add(municipalityLatestItem);
            SaveChanges();
            return municipalityLatestItem;
        }

        public override ValueTask DisposeAsync()
        {
            if (_dontDispose)
            {
                return new ValueTask(Task.CompletedTask);
            }

            return base.DisposeAsync();
        }
    }

    public sealed class FakeSyndicationContextFactory : IDesignTimeDbContextFactory<TestSyndicationContext>
    {
        private readonly bool _dontDispose;

        public FakeSyndicationContextFactory(bool dontDispose = false)
        {
            _dontDispose = dontDispose;
        }

        public TestSyndicationContext CreateDbContext(params string[] args)
        {
            var builder = new DbContextOptionsBuilder<SyndicationContext>().UseInMemoryDatabase(Guid.NewGuid().ToString());
            return new TestSyndicationContext(builder.Options, _dontDispose);
        }
    }
}
