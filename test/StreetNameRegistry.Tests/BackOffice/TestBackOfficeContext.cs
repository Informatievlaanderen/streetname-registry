namespace StreetNameRegistry.Tests.BackOffice
{
    using System;
    using System.Threading.Tasks;
    using global::AutoFixture;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using StreetNameRegistry.Api.BackOffice.Abstractions;

    public sealed class TestBackOfficeContext : BackOfficeContext
    {
        private bool _dontDispose = false;

        // This needs to be here to please EF
        public TestBackOfficeContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public TestBackOfficeContext(DbContextOptions<BackOfficeContext> options, bool dontDispose = false)
            : base(options)
        {
            _dontDispose = dontDispose;
        }

        public MunicipalityIdByPersistentLocalId AddMunicipalityIdByPersistentLocalIdToFixture(int? persistentLocalId = null, Guid? municipalityId = null)
        {
            var item = new Fixture().Create<MunicipalityIdByPersistentLocalId>();
            if (persistentLocalId is not null)
            {
                item.PersistentLocalId = persistentLocalId.Value;
            }

            if (municipalityId is not null)
            {
                item.MunicipalityId = municipalityId.Value;
            }
            MunicipalityIdByPersistentLocalId.Add(item);
            SaveChanges();
            return item;
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

    public sealed class FakeBackOfficeContextFactory : IDesignTimeDbContextFactory<TestBackOfficeContext>
    {
        private bool _dontDispose;

        public FakeBackOfficeContextFactory(bool dontDispose = false)
        {
            _dontDispose = dontDispose;
        }

        public TestBackOfficeContext CreateDbContext(params string[] args)
        {
            var builder = new DbContextOptionsBuilder<BackOfficeContext>().UseInMemoryDatabase(Guid.NewGuid().ToString());
            return new TestBackOfficeContext(builder.Options, _dontDispose);
        }
    }
}
