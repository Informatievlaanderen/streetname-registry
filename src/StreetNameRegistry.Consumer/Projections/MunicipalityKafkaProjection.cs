namespace StreetNameRegistry.Consumer.Projections
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.MunicipalityRegistry;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Microsoft.EntityFrameworkCore;
    using Municipality;
    using NodaTime.Text;
    using StreetNameRegistry.Municipality;
    using StreetNameRegistry.Municipality.Commands;
    using Contracts = Be.Vlaanderen.Basisregisters.GrAr.Contracts.Common;
    using Provenance = Be.Vlaanderen.Basisregisters.GrAr.Provenance.Provenance;

    public class MunicipalityKafkaProjection : ConnectedProjection<CommandHandler>
    {
        public MunicipalityKafkaProjection(IDbContextFactory<ConsumerContext> consumerContextFactory)
        {
            When<MunicipalityWasRegistered>(async (commandHandler, message, ct) =>
            {
                var command = GetCommand(message);
                await commandHandler.Handle(command, ct);
            });

            When<MunicipalityNisCodeWasDefined>(async (commandHandler, message, ct) =>
            {
                var command = GetCommand(message);
                await commandHandler.Handle(command, ct);
            });

            When<MunicipalityNisCodeWasCorrected>(async (commandHandler, message, ct) =>
            {
                var command = GetCommand(message);
                await commandHandler.Handle(command, ct);
            });

            When<MunicipalityWasNamed>(async (commandHandler, message, ct) =>
            {
                var command = GetCommand(message);
                await commandHandler.Handle(command, ct);
            });

            When<MunicipalityNameWasCorrected>(async (commandHandler, message, ct) =>
            {
                var command = GetCommand(message);
                await commandHandler.Handle(command, ct);
            });

            When<MunicipalityNameWasCorrectedToCleared>(async (commandHandler, message, ct) =>
            {
                var command = GetCommand(message);
                await commandHandler.Handle(command, ct);
            });

            When<MunicipalityOfficialLanguageWasAdded>(async (commandHandler, message, ct) =>
            {
                var command = GetCommand(message);
                await commandHandler.Handle(command, ct);
            });

            When<MunicipalityOfficialLanguageWasRemoved>(async (commandHandler, message, ct) =>
            {
                var command = GetCommand(message);
                await commandHandler.Handle(command, ct);
            });

            When<MunicipalityFacilityLanguageWasAdded>(async (commandHandler, message, ct) =>
            {
                var command = GetCommand(message);
                await commandHandler.Handle(command, ct);
            });

            When<MunicipalityFacilityLanguageWasRemoved>(async (commandHandler, message, ct) =>
            {
                var command = GetCommand(message);
                await commandHandler.Handle(command, ct);
            });

            When<MunicipalityBecameCurrent>(async (commandHandler, message, ct) =>
            {
                var command = GetCommand(message);
                await commandHandler.Handle(command, ct);
            });

            When<MunicipalityWasCorrectedToCurrent>(async (commandHandler, message, ct) =>
            {
                var command = GetCommand(message);
                await commandHandler.Handle(command, ct);
            });

            When<MunicipalityWasRetired>(async (commandHandler, message, ct) =>
            {
                var command = GetCommand(message);
                await commandHandler.Handle(command, ct);
            });

            When<MunicipalityWasCorrectedToRetired>(async (commandHandler, message, ct) =>
            {
                var command = GetCommand(message);
                await commandHandler.Handle(command, ct);
            });

            When<MunicipalityWasRemoved>(async (commandHandler, message, ct) =>
            {
                var command = GetCommand(message);
                await commandHandler.Handle(command, ct);
            });

            When<MunicipalityWasMerged>(async (commandHandler, message, ct) =>
            {
                await using var context = await consumerContextFactory.CreateDbContextAsync(ct);

                var municipalityId = new MunicipalityId(Guid.Parse(message.MunicipalityId));
                var newMunicipalityId = new MunicipalityId(Guid.Parse(message.NewMunicipalityId));

                var retireCommand = new RetireMunicipalityForMunicipalityMerger(
                    municipalityId,
                    newMunicipalityId,
                    FromProvenance(message.Provenance)
                );
                await commandHandler.Handle(retireCommand, ct);

                await context.MunicipalityMergerItems.AddAsync(new MunicipalityMergerItem
                {
                    MunicipalityId = municipalityId,
                    IsRetired = true
                });
                await context.SaveChangesAsync(ct);

                var allMunicipalitiesAreRetired = await AllMunicipalitiesAreRetiredForMunicipalityMerger(
                    message.MunicipalityIdsToMergeWith.Select(Guid.Parse),
                    context,
                    ct);
                if (allMunicipalitiesAreRetired)
                {
                    var approveCommand = new ApproveStreetNamesForMunicipalityMerger(
                        newMunicipalityId,
                        FromProvenance(message.Provenance)
                    );
                    await commandHandler.Handle(approveCommand, ct);
                }
            });
        }

        private static async Task<bool> AllMunicipalitiesAreRetiredForMunicipalityMerger(
            IEnumerable<Guid> municipalityIds,
            ConsumerContext context,
            CancellationToken ct)
        {
            foreach (var municipalityId in municipalityIds)
            {
                var municipalityMerge = await context.MunicipalityMergerItems.FindAsync(municipalityId, ct);
                if (municipalityMerge is null || !municipalityMerge.IsRetired)
                {
                    return false;
                }
            }

            return true;
        }

        public static IHasCommandProvenance GetCommand(IQueueMessage message)
        {
            var type = message.GetType();

            if (type == typeof(MunicipalityWasRegistered))
            {
                var msg = (MunicipalityWasRegistered)message;
                return new ImportMunicipality(
                    new MunicipalityId(MunicipalityId.CreateFor(msg.MunicipalityId)),
                    new NisCode(msg.NisCode),
                    FromProvenance(msg.Provenance, Modification.Insert)
                );
            }

            if (type == typeof(MunicipalityNisCodeWasDefined))
            {
                var msg = (MunicipalityNisCodeWasDefined)message;
                return new DefineMunicipalityNisCode(
                    new MunicipalityId(MunicipalityId.CreateFor(msg.MunicipalityId)),
                    new NisCode(msg.NisCode),
                    FromProvenance(msg.Provenance)
                );
            }

            if (type == typeof(MunicipalityNisCodeWasCorrected))
            {
                var msg = (MunicipalityNisCodeWasCorrected)message;
                return new CorrectMunicipalityNisCode(
                    new MunicipalityId(MunicipalityId.CreateFor(msg.MunicipalityId)),
                    new NisCode(msg.NisCode),
                    FromProvenance(msg.Provenance)
                );
            }

            if (type == typeof(MunicipalityWasNamed))
            {
                var msg = (MunicipalityWasNamed)message;
                return new NameMunicipality(
                    new MunicipalityId(MunicipalityId.CreateFor(msg.MunicipalityId)),
                    new MunicipalityName(msg.Name, Enum.Parse<Language>(msg.Language)),
                    FromProvenance(msg.Provenance));
            }

            if (type == typeof(MunicipalityNameWasCorrected))
            {
                var msg = (MunicipalityNameWasCorrected)message;
                return new CorrectMunicipalityName(
                    MunicipalityId.CreateFor(msg.MunicipalityId),
                    new MunicipalityName(msg.Name, Enum.Parse<Language>(msg.Language)),
                    FromProvenance(msg.Provenance));
            }

            if (type == typeof(MunicipalityNameWasCorrectedToCleared))
            {
                var msg = (MunicipalityNameWasCorrectedToCleared)message;
                return new CorrectToClearedMunicipalityName(
                    MunicipalityId.CreateFor(msg.MunicipalityId),
                    Enum.Parse<Language>(msg.Language),
                    FromProvenance(msg.Provenance));
            }

            if (type == typeof(MunicipalityOfficialLanguageWasAdded))
            {
                var msg = (MunicipalityOfficialLanguageWasAdded)message;
                return new AddOfficialLanguageToMunicipality(
                    MunicipalityId.CreateFor(msg.MunicipalityId),
                    Enum.Parse<Language>(msg.Language),
                    FromProvenance(msg.Provenance));
            }

            if (type == typeof(MunicipalityOfficialLanguageWasRemoved))
            {
                var msg = (MunicipalityOfficialLanguageWasRemoved)message;
                return new RemoveOfficialLanguageFromMunicipality(
                    MunicipalityId.CreateFor(msg.MunicipalityId),
                    Enum.Parse<Language>(msg.Language),
                    FromProvenance(msg.Provenance));
            }

            if (type == typeof(MunicipalityFacilityLanguageWasAdded))
            {
                var msg = (MunicipalityFacilityLanguageWasAdded)message;
                return new AddFacilityLanguageToMunicipality(
                    MunicipalityId.CreateFor(msg.MunicipalityId),
                    Enum.Parse<Language>(msg.Language),
                    FromProvenance(msg.Provenance));
            }

            if (type == typeof(MunicipalityFacilityLanguageWasRemoved))
            {
                var msg = (MunicipalityFacilityLanguageWasRemoved)message;
                return new RemoveFacilityLanguageFromMunicipality(
                    MunicipalityId.CreateFor(msg.MunicipalityId),
                    Enum.Parse<Language>(msg.Language),
                    FromProvenance(msg.Provenance));
            }

            if (type == typeof(MunicipalityBecameCurrent))
            {
                var msg = (MunicipalityBecameCurrent)message;
                return new SetMunicipalityToCurrent(
                    MunicipalityId.CreateFor(msg.MunicipalityId),
                    FromProvenance(msg.Provenance));
            }

            if (type == typeof(MunicipalityWasCorrectedToCurrent))
            {
                var msg = (MunicipalityWasCorrectedToCurrent)message;
                return new CorrectToCurrentMunicipality(
                    MunicipalityId.CreateFor(msg.MunicipalityId),
                    FromProvenance(msg.Provenance));
            }

            if (type == typeof(MunicipalityWasRetired))
            {
                var msg = (MunicipalityWasRetired)message;
                return new RetireMunicipality(
                    MunicipalityId.CreateFor(msg.MunicipalityId),
                    new RetirementDate(InstantPattern.General.Parse(msg.RetirementDate).GetValueOrThrow()),
                    FromProvenance(msg.Provenance));
            }

            if (type == typeof(MunicipalityWasCorrectedToRetired))
            {
                var msg = (MunicipalityWasCorrectedToRetired)message;
                return new CorrectToRetiredMunicipality(
                    MunicipalityId.CreateFor(msg.MunicipalityId),
                    new RetirementDate(InstantPattern.General.Parse(msg.RetirementDate).GetValueOrThrow()),
                    FromProvenance(msg.Provenance));
            }

            if (type == typeof(MunicipalityWasRemoved))
            {
                var msg = (MunicipalityWasRemoved)message;
                return new RemoveMunicipality(
                    MunicipalityId.CreateFor(msg.MunicipalityId),
                    FromProvenance(msg.Provenance));
            }

            throw new InvalidOperationException($"No command found for {type.FullName}");
        }

        private static Provenance FromProvenance(Contracts.Provenance provenance, Modification modification = Modification.Update) =>
            new Provenance(
                InstantPattern.General.Parse(provenance.Timestamp).Value,
                Enum.Parse<Application>(provenance.Application),
                new Reason(provenance.Reason),
                new Operator(string.Empty),
                modification,
                Enum.Parse<Organisation>(provenance.Organisation));
    }
}
