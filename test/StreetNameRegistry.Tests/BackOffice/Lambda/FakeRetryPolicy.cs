namespace StreetNameRegistry.Tests.BackOffice.Lambda
{
    using System;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;

    internal class FakeRetryPolicy : ICustomRetryPolicy
    {
        public Task Retry(Func<Task> functionToRetry)
        {
            return functionToRetry();
        }
    }
}
