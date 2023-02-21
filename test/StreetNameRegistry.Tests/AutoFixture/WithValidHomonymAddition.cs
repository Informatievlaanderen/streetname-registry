namespace StreetNameRegistry.Tests.AutoFixture
{
    using global::AutoFixture;
    using global::AutoFixture.Kernel;
    using Municipality;
    using System.Linq;
    using System;

    public sealed class WithValidHomonymAddition : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Register(() => new StreetNameHomonymAddition(GenerateRandomString(10), fixture.Create<Language>()));
        }

        public static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
