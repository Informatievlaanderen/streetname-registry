namespace StreetNameRegistry.Tests.AutoFixture
{
    using System;
    using global::AutoFixture;
    using global::AutoFixture.Kernel;
    using Municipality;

    public sealed class WithFixedNisCode : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            var nisCode = fixture.Create<NisCode>();

            fixture.Register(() => nisCode);

            fixture.Customizations.Add(
                new FilteringSpecimenBuilder(
                    new FixedBuilder(nisCode),
                    new ParameterSpecification(
                        typeof(string),
                        "nisCode")));
        }
    }
}
