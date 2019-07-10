namespace StreetNameRegistry.Tests.AggregateTests
{
    using Assert;
    using Generate;
    using StreetName;
    using StreetName.Events;
    using Testing;
    using Xunit;
    using Xunit.Abstractions;

    public class AssignPersistentLocalIdFromCrabTests : TestBase
    {
        public AssignPersistentLocalIdFromCrabTests(ITestOutputHelper output) : base(output)
        {
        }

        protected StreetName RegisterWithId(StreetNameId streetNameId)
        {
            return StreetName.Register(streetNameId, new MunicipalityId(Arrange(Generate.CrabMunicipalityId).CreateDeterministicId()), Arrange(Generate.NisCode));
        }

        [Fact]
        public void AppliesPersistentLocalIdWasAssigned()
        {
            //Arrange
            var streetNameId = Arrange(Generate.StreetNameId);
            var sut = RegisterWithId(streetNameId);
            var date = Arrange(Produce.Instant());
            var persistentLocalId = Arrange(Produce.Integer(10000, 15000));

            //Act
            sut.AssignPersistentLocalId(new PersistentLocalId(persistentLocalId), new PersistentLocalIdAssignmentDate(date));

            //Assert
            sut.Should().HaveChanges();
            sut.Should().HaveSingleChange<StreetNamePersistentLocalIdWasAssigned>()
                .Which.Should().HaveStreetNameId(streetNameId)
                .And.HavePersistentLocalId(persistentLocalId)
                .And.HaveAssignmentDate(date);
        }

        [Fact]
        public void AppliesPersistentLocalIdWhenAlreadyAssignedDoesNothing()
        {
            //Arrange
            var streetNameId = Arrange(Generate.StreetNameId);
            var sut = RegisterWithId(streetNameId);
            var persistentLocalId = Arrange(Produce.Integer(10000, 15000));
            var date = Arrange(Produce.Instant());

            sut.AssignPersistentLocalId(new PersistentLocalId(persistentLocalId), new PersistentLocalIdAssignmentDate(date));
            sut.ClearChanges();

            //Act
            sut.AssignPersistentLocalId(new PersistentLocalId(Arrange(Produce.Integer(10000, 15000))), new PersistentLocalIdAssignmentDate(date));

            //Assert
            sut.Should().HaveCountOfChanges<object>(0);
        }
    }
}
