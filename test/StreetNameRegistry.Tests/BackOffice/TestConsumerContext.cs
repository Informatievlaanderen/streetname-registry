namespace StreetNameRegistry.Tests.BackOffice
{
    using System;
    using System.Threading.Tasks;
    using Consumer;
    using Consumer.Municipality;
    using global::AutoFixture;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;

    public sealed class TestConsumerContext : ConsumerContext
    {
        private readonly bool _dontDispose;

        // This needs to be here to please EF
        public TestConsumerContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public TestConsumerContext(DbContextOptions<ConsumerContext> options, bool dontDispose = false)
            : base(options)
        {
            _dontDispose = dontDispose;
        }

        public MunicipalityConsumerItem AddMunicipalityLatestItemFixture()
        {
            var municipalityLatestItem = new Fixture().Create<MunicipalityConsumerItem>();
            MunicipalityConsumerItems.Add(municipalityLatestItem);
            SaveChanges();
            return municipalityLatestItem;
        }

        public MunicipalityConsumerItem AddMunicipalityLatestItemFixtureWithNisCode(string nisCode)
        {
            var municipalityLatestItem = new Fixture().Create<MunicipalityConsumerItem>();
            municipalityLatestItem.NisCode = nisCode;
            MunicipalityConsumerItems.Add(municipalityLatestItem);
            SaveChanges();
            return municipalityLatestItem;
        }

        public MunicipalityConsumerItem AddMunicipalityLatestItemFixtureWithMunicipalityIdAndNisCode(Guid municipalityId, string nisCode)
        {
            var municipalityLatestItem = new Fixture().Create<MunicipalityConsumerItem>();
            municipalityLatestItem.MunicipalityId = municipalityId;
            municipalityLatestItem.NisCode = nisCode;
            MunicipalityConsumerItems.Add(municipalityLatestItem);
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

    public sealed class FakeConsumerContextFactory : IDesignTimeDbContextFactory<TestConsumerContext>
    {
        private readonly bool _dontDispose;

        public FakeConsumerContextFactory(bool dontDispose = false)
        {
            _dontDispose = dontDispose;
        }

        public TestConsumerContext CreateDbContext(params string[] args)
        {
            var builder = new DbContextOptionsBuilder<ConsumerContext>().UseInMemoryDatabase(Guid.NewGuid().ToString());
            return new TestConsumerContext(builder.Options, _dontDispose);
        }
    }
}
