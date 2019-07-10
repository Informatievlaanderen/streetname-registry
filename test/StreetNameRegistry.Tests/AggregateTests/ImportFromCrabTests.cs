namespace StreetNameRegistry.Tests.AggregateTests
{
    using System;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Assert;
    using FluentAssertions;
    using StreetName.Events;
    using Testing;
    using Generate;
    using Xunit;
    using Xunit.Abstractions;

    public class ImportFromCrabTests : ImportFromCrabTest
    {
        public ImportFromCrabTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void AppliesPersistentLocalIdWasAssigned()
        {
            var sut = RegisterWithId(Arrange(Generate.StreetNameId));
            sut = Act(sut, Arrange(Generate.CrabLifetime), Arrange(Generate.CrabModificationNullableExceptDeleteCorrection));

            sut.Should().HaveSingleChange<StreetNamePersistentLocalIdWasAssigned>();
        }

        [Fact]
        public void AppliesStreetNameStatusWasCorrectedToPreviousStatus()
        {
            //Arrange
            var lifetime = Arrange(Generate.CrabLifetime);
            var streetNameId = Arrange(Generate.StreetNameId);
            var sut = RegisterWithId(streetNameId);

            var givenLifetime = Arrange(Generate.CrabLifetimeWithEndDate(null));

            sut = Act(sut,
                lifetime,
                Arrange(Generate.CrabModificationNullableExceptDeleteCorrection));

            sut = Act(sut,
                givenLifetime,
                CrabModification.Correction);

            sut.Should().HaveChanges();
            sut.Should().HaveSingleChange<StreetNameStatusWasCorrectedToRemoved>()
                .Which.Should().HaveStreetNameId(streetNameId);
        }

        [Fact]
        public void AppliesStreetNameStatusWasCorrectedToPreviousStatusWithNonRemovedStatus()
        {
            //Arrange
            var lifetime = Arrange(Generate.CrabLifetime);
            var streetNameId = Arrange(Generate.StreetNameId);
            var sut = StreetName.StreetName.Factory();
            var givenLifetime = Arrange(Generate.CrabLifetimeWithEndDate(null));

            sut = RegisterWithId(streetNameId);

            sut = Act(sut, CrabStreetNameStatus.InUse, givenLifetime, CrabModification.Insert);

            sut = Act(sut,
                lifetime,
                Arrange(Generate.CrabModificationNullableExceptDeleteCorrection));

            sut = Act(sut,
                givenLifetime,
                CrabModification.Correction);

            sut.Should().HaveChanges();
            sut.Should().HaveSingleChange<StreetNameWasCorrectedToCurrent>()
                .Which.Should().HaveStreetNameId(streetNameId);
        }

        [Fact]
        public void AppliesStreetNameStatusWasChangedToPreviousStatus()
        {
            //Arrange
            var lifetime = Arrange(Generate.CrabLifetime);
            var streetNameId = Arrange(Generate.StreetNameId);
            var sut = RegisterWithId(streetNameId);

            var givenLifetime = Arrange(Generate.CrabLifetimeWithEndDate(null));

            sut = Act(sut,
                lifetime,
                Arrange(Generate.CrabModificationNullableExceptDeleteCorrection));

            sut = Act(sut,
                givenLifetime,
                Arrange(Generate.CrabModificationNullableExceptDeleteCorrection));

            sut.Should().HaveChanges();
            sut.Should().HaveSingleChange<StreetNameStatusWasRemoved>()
                .Which.Should().HaveStreetNameId(streetNameId);
        }

        [Fact]
        public void AppliesStreetNameStatusWasChangedToPreviousStatusWithNonRemovedStatus()
        {
            //Arrange
            var lifetime = Arrange(Generate.CrabLifetime);
            var streetNameId = Arrange(Generate.StreetNameId);
            var sut = RegisterWithId(streetNameId);

            var givenLifetime = Arrange(Generate.CrabLifetimeWithEndDate(null));

            sut = Act(sut, CrabStreetNameStatus.InUse, givenLifetime, CrabModification.Insert);

            sut.ClearChanges();

            sut = Act(sut,
                lifetime,
                Arrange(Generate.CrabModificationNullableExceptDeleteCorrection));

            sut = Act(sut,
                givenLifetime,
                Arrange(Generate.CrabModificationNullableExceptDeleteCorrection));

            sut.Should().HaveChanges();
            sut.Should().HaveSingleChange<StreetNameBecameCurrent>()
                .Which.Should().HaveStreetNameId(streetNameId);
        }

        [Fact]
        public void AppliesStreetNameStatusWasCorrectedToRetired()
        {
            //Arrange
            var givenLifetime = Arrange(Generate.CrabLifetime);

            var streetNameId = Arrange(Generate.StreetNameId);
            var sut = RegisterWithId(streetNameId);

            var lifetime = Arrange(Generate.CrabLifetimeWithEndDate(null));

            //Act
            sut = Act(sut,
                lifetime,
                Arrange(Generate.CrabModificationNullableExceptDeleteCorrection)
            );

            sut = Act(sut,
                givenLifetime,
                CrabModification.Correction);

            sut.Should().HaveChanges();
            sut.Should().HaveSingleChange<StreetNameWasCorrectedToRetired>()
                .Which.Should().HaveStreetNameId(streetNameId);
        }

        [Fact]
        public void AppliesStreetNameWasRetired()
        {
            //Arrange
            var lifetime = Arrange(Generate.CrabLifetime);

            var streetNameId = Arrange(Generate.StreetNameId);
            var sut = RegisterWithId(streetNameId);

            //Act
            sut = Act(sut, lifetime, Arrange(Generate.CrabModificationNullableExceptDeleteCorrection));

            sut.Should().HaveChanges();
            sut.Should().HaveSingleChange<StreetNameWasRetired>()
                .Which.Should().HaveStreetNameId(streetNameId);
        }

        [Fact]
        public void AppliesStreetNameWasRemovedWhenModificationIsDeleted()
        {
            //Arrange
            var givenModification = CrabModification.Delete;
            var streetNameId = Arrange(Generate.StreetNameId);
            var sut = RegisterWithId(streetNameId);

            //Act
            sut = Act(sut, givenModification);

            //Assert
            sut.Should().HaveChanges();
            sut.Should().HaveSingleChange<StreetNameWasRemoved>();
        }

        [Fact]
        public void ThrowsWhenStreetNameAlreadyRemoved()
        {
            //Arrange
            var givenModification = CrabModification.Delete;
            var streetNameId = Arrange(Generate.StreetNameId);
            var sut = RegisterWithId(streetNameId);
            sut = Act(sut, givenModification);

            //Act
            Action act = () => sut = Act(sut, givenModification);

            //Assert
            act.Should().Throw<InvalidOperationException>();
        }
    }
}
