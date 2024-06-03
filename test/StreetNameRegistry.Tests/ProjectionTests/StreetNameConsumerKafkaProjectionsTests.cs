namespace StreetNameRegistry.Tests.ProjectionTests
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.MunicipalityRegistry;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Consumer.Projections;
    using global::AutoFixture;
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;
    using Municipality;
    using Municipality.Commands;
    using NodaTime;
    using Xunit;
    using Xunit.Abstractions;
    using Provenance = Be.Vlaanderen.Basisregisters.GrAr.Contracts.Common.Provenance;

    public sealed class StreetNameConsumerKafkaProjectionsTests : StreetNameConsumerKafkaProjectionTest<CommandHandler, MunicipalityKafkaProjection>
    {
        private readonly Mock<FakeCommandHandler> _mockCommandHandler;

        public StreetNameConsumerKafkaProjectionsTests(ITestOutputHelper output)
            : base(output)
        {
            _mockCommandHandler = new Mock<FakeCommandHandler>();
        }

        private class MunicipalityEventsGenerator : IEnumerable<object[]>
        {
            private readonly Fixture? _fixture;

            public MunicipalityEventsGenerator()
            {
                _fixture = new Fixture();
                _fixture.Customize(new InfrastructureCustomization());
                _fixture.Customize(new WithFixedMunicipalityId());
            }

            public IEnumerator<object[]> GetEnumerator()
            {
                var id = _fixture.Create<MunicipalityId>().ToString();
                var nisCode = _fixture.Create<NisCode>();
                var name = _fixture.Create<string>();
                var language = Language.Dutch.ToString();
                var retirementDate = _fixture.Create<Instant>().ToString();
                var provenance = new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.StreetNameRegistry.ToString(),
                    Modification.Unknown.ToString(),
                    Organisation.DigitaalVlaanderen.ToString(),
                    new Reason("")
                    );

                var result = new List<object[]>
                {
                    new object[] { new MunicipalityWasRegistered(id, nisCode, provenance) },
                    new object[] { new MunicipalityNisCodeWasDefined(id, nisCode, provenance) },
                    new object[] { new MunicipalityNisCodeWasCorrected(id, nisCode, provenance) },
                    new object[] { new MunicipalityWasNamed(id, name, language, provenance) },
                    new object[] { new MunicipalityNameWasCorrected(id, name, language, provenance) },
                    new object[] { new MunicipalityNameWasCorrectedToCleared(id, language, provenance) },
                    new object[] { new MunicipalityOfficialLanguageWasAdded(id, language, provenance) },
                    new object[] { new MunicipalityOfficialLanguageWasRemoved(id, language, provenance) },
                    new object[] { new MunicipalityFacilityLanguageWasAdded(id, language, provenance) },
                    new object[] { new MunicipalityFacilityLanguageWasRemoved(id, language, provenance) },
                    new object[] { new MunicipalityBecameCurrent(id, provenance) },
                    new object[] { new MunicipalityWasCorrectedToCurrent(id, provenance) },
                    new object[] { new MunicipalityWasRetired(id, retirementDate, provenance) },
                    new object[] { new MunicipalityWasCorrectedToRetired(id, retirementDate, provenance) } // test fails on date format
                };
                return result.GetEnumerator();
            }
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        [Theory]
        [ClassData(typeof(MunicipalityEventsGenerator))]
        public async Task HandleMessage(object obj)
        {
            if (obj is not IQueueMessage queueMessage)
            {
                throw new InvalidOperationException("Parameter is not an IQueueMessage");
            }

            var command = MunicipalityKafkaProjection.GetCommand(queueMessage);
            _mockCommandHandler.Setup(commandHandler => commandHandler.Handle(command, default)).Returns(Task.CompletedTask);

            Given(command);
            await Then(ct =>
                {
                    _mockCommandHandler.Verify(commandHandler => commandHandler.Handle(It.IsAny<IHasCommandProvenance>(), default), Times.AtMostOnce());
                    return Task.CompletedTask;
                });
        }

        [Fact]
        public async Task GivenMunicipalityWasRegistered_ThenImportMunicipality()
        {
            var @event = new MunicipalityWasRegistered(
                Fixture.Create<Guid>().ToString(),
                Fixture.Create<string>(),
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.StreetNameRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(
                    x => x.Handle(
                        It.Is<IHasCommandProvenance>(y =>
                            y is ImportMunicipality
                            && y.Provenance.Timestamp.ToString() == @event.Provenance.Timestamp),
                        CancellationToken.None),
                    Times.Once);
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task GivenMunicipalityNisCodeWasDefined_ThenDefineMunicipalityNisCode()
        {
            var @event = new MunicipalityNisCodeWasDefined(
                Fixture.Create<Guid>().ToString(),
                Fixture.Create<string>(),
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.StreetNameRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(
                    x => x.Handle(It.Is<IHasCommandProvenance>(x => x is DefineMunicipalityNisCode), CancellationToken.None),
                    Times.Once);
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task GivenMunicipalityNisCodeWasCorrected_ThenCorrectMunicipalityNisCode()
        {
            var @event = new MunicipalityNisCodeWasCorrected(
                Fixture.Create<Guid>().ToString(),
                Fixture.Create<string>(),
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.StreetNameRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(
                    x => x.Handle(It.Is<IHasCommandProvenance>(x => x is CorrectMunicipalityNisCode), CancellationToken.None),
                    Times.Once);
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task GivenMunicipalityWasNamed_ThenNameMunicipality()
        {
            var @event = new MunicipalityWasNamed(
                Fixture.Create<Guid>().ToString(),
                Fixture.Create<string>(),
                Fixture.Create<Language>().ToString(),
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.StreetNameRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(
                    x => x.Handle(It.Is<IHasCommandProvenance>(x => x is NameMunicipality), CancellationToken.None),
                    Times.Once);
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task GivenMunicipalityNameWasCorrected_ThenCorrectMunicipalityName()
        {
            var @event = new MunicipalityNameWasCorrected(
                Fixture.Create<Guid>().ToString(),
                Fixture.Create<string>(),
                Fixture.Create<Language>().ToString(),
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.StreetNameRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(
                    x => x.Handle(It.Is<IHasCommandProvenance>(x => x is CorrectMunicipalityName), CancellationToken.None),
                    Times.Once);
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task GivenMunicipalityNameWasCorrectedToCleared_ThenCorrectToClearedMunicipalityName()
        {
            var @event = new MunicipalityNameWasCorrectedToCleared(
                Fixture.Create<Guid>().ToString(),
                Fixture.Create<Language>().ToString(),
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.StreetNameRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(
                    x => x.Handle(It.Is<IHasCommandProvenance>(x => x is CorrectToClearedMunicipalityName), CancellationToken.None),
                    Times.Once);
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task GivenMunicipalityOfficialLanguageWasAdded_ThenAddOfficialLanguageToMunicipality()
        {
            var @event = new MunicipalityOfficialLanguageWasAdded(
                Fixture.Create<Guid>().ToString(),
                Fixture.Create<Language>().ToString(),
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.StreetNameRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(
                    x => x.Handle(It.Is<IHasCommandProvenance>(x => x is AddOfficialLanguageToMunicipality), CancellationToken.None),
                    Times.Once);
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task GivenMunicipalityOfficialLanguageWasRemoved_ThenRemoveOfficialLanguageFromMunicipality()
        {
            var @event = new MunicipalityOfficialLanguageWasRemoved(
                Fixture.Create<Guid>().ToString(),
                Fixture.Create<Language>().ToString(),
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.StreetNameRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(
                    x => x.Handle(It.Is<IHasCommandProvenance>(x => x is RemoveOfficialLanguageFromMunicipality), CancellationToken.None),
                    Times.Once);
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task GivenMunicipalityFacilityLanguageWasAdded_ThenAddFacilityLanguageToMunicipality()
        {
            var @event = new MunicipalityFacilityLanguageWasAdded(
                Fixture.Create<Guid>().ToString(),
                Fixture.Create<Language>().ToString(),
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.StreetNameRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(
                    x => x.Handle(It.Is<IHasCommandProvenance>(x => x is AddFacilityLanguageToMunicipality), CancellationToken.None),
                    Times.Once);
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task GivenMunicipalityFacilityLanguageWasRemoved_ThenRemoveFacilityLanguageFromMunicipality()
        {
            var @event = new MunicipalityFacilityLanguageWasRemoved(
                Fixture.Create<Guid>().ToString(),
                Fixture.Create<Language>().ToString(),
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.StreetNameRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(
                    x => x.Handle(It.Is<IHasCommandProvenance>(x => x is RemoveFacilityLanguageFromMunicipality), CancellationToken.None),
                    Times.Once);
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task GivenMunicipalityBecameCurrent_ThenSetMunicipalityToCurrent()
        {
            var @event = new MunicipalityBecameCurrent(
                Fixture.Create<Guid>().ToString(),
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.StreetNameRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(
                    x => x.Handle(It.Is<IHasCommandProvenance>(x => x is SetMunicipalityToCurrent), CancellationToken.None),
                    Times.Once);
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task GivenMunicipalityWasCorrectedToCurrent_ThenCorrectToCurrentMunicipality()
        {
            var @event = new MunicipalityWasCorrectedToCurrent(
                Fixture.Create<Guid>().ToString(),
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.StreetNameRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(
                    x => x.Handle(It.Is<IHasCommandProvenance>(x => x is CorrectToCurrentMunicipality), CancellationToken.None),
                    Times.Once);
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task GivenMunicipalityWasRetired_ThenCorrectToCurrentMunicipality()
        {
            var @event = new MunicipalityWasRetired(
                Fixture.Create<Guid>().ToString(),
                Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.StreetNameRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(
                    x => x.Handle(It.Is<IHasCommandProvenance>(x => x is RetireMunicipality), CancellationToken.None),
                    Times.Once);
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task GivenMunicipalityWasCorrectedToRetired_ThenCorrectToRetiredMunicipality()
        {
            var @event = new MunicipalityWasCorrectedToRetired(
                Fixture.Create<Guid>().ToString(),
                Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.StreetNameRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(
                    x => x.Handle(It.Is<IHasCommandProvenance>(x => x is CorrectToRetiredMunicipality), CancellationToken.None),
                    Times.Once);
                await Task.CompletedTask;
            });
        }

        protected override CommandHandler CreateContext()
        {
            return _mockCommandHandler.Object;
        }

        protected override MunicipalityKafkaProjection CreateProjection()
        {
            return new MunicipalityKafkaProjection();
        }
    }

    public class FakeCommandHandler : CommandHandler
    {
        public FakeCommandHandler() : base(null, new NullLoggerFactory())
        { }
    }
}
