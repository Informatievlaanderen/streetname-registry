namespace StreetNameRegistry.Tests.Assert
{
    using System;
    using FluentAssertions;
    using NodaTime;
    using StreetName.Events;

    public static class StreetNamePersistentLocalIdWasAssignedAssertionsProvider
    {
        public static StreetNamePersistentLocalIdWasAssignedAssertions Should(this StreetNamePersistentLocalIdWasAssigned subject)
        {
            return new StreetNamePersistentLocalIdWasAssignedAssertions(subject);
        }
    }

    public class StreetNamePersistentLocalIdWasAssignedAssertions :
        HasStreetNameIdAssertions<StreetNamePersistentLocalIdWasAssigned, StreetNamePersistentLocalIdWasAssignedAssertions>
    {
        public StreetNamePersistentLocalIdWasAssignedAssertions(StreetNamePersistentLocalIdWasAssigned subject) : base(subject)
        {
        }

        public AndConstraint<StreetNamePersistentLocalIdWasAssignedAssertions> HavePersistentLocalId(int persistentLocalId)
        {
            AssertingThat($"has persistentLocalId {persistentLocalId}");

            Subject.PersistentLocalId.Should().Be(persistentLocalId);

            return And();
        }

        public AndConstraint<StreetNamePersistentLocalIdWasAssignedAssertions> HaveAssignmentDate(Instant date)
        {
            AssertingThat($"has assignment date is {date}");

            Subject.AssignmentDate.Should().Be(date);

            return And();
        }
    }
}
