namespace StreetNameRegistry.Projections.Bosa.Infrastructure
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Amazon.S3;
    using Amazon.S3.Model;

    public interface IBosaFileStore
    {
        /// <summary>
        /// Creates or overwrites the file.
        /// </summary>
        Task SaveAsync(string name, Stream content, CancellationToken cancellationToken);
    }

    public sealed class S3BosaFileStore : IBosaFileStore
    {
        private readonly IAmazonS3 _s3;
        private readonly string _bucket;

        public S3BosaFileStore(IAmazonS3 s3, string bucket)
        {
            _s3 = s3;
            _bucket = bucket;
        }

        public async Task SaveAsync(string name, Stream content, CancellationToken cancellationToken)
        {
            await _s3.PutObjectAsync(new PutObjectRequest
            {
                BucketName = _bucket,
                Key = name,
                ContentType = "application/xml",
                InputStream = content,
                AutoCloseStream = false
            }, cancellationToken);
        }
    }

    public sealed class InMemoryBosaFileStore : IBosaFileStore
    {
        private readonly ConcurrentDictionary<string, byte[]> _files = new ConcurrentDictionary<string, byte[]>();

        public IReadOnlyDictionary<string, byte[]> Files => _files;

        public async Task SaveAsync(string name, Stream content, CancellationToken cancellationToken)
        {
            await using var copy = new MemoryStream();
            await content.CopyToAsync(copy, cancellationToken);
            _files[name] = copy.ToArray();
        }
    }
}
