using System.Diagnostics.Contracts;
using System.Security.Cryptography;

namespace wan24.Core
{
    /// <summary>
    /// Checksum hash algorithm
    /// </summary>
    public sealed class ChecksumTransform : HashAlgorithm
    {
        /// <summary>
        /// Algorithm name (being used when calling <see cref="Register"/>)
        /// </summary>
        public const string ALGORITHM_NAME = "Checksum";
        /// <summary>
        /// Default checksum length in bytes
        /// </summary>
        public const int HASH_LENGTH = 8;

        /// <summary>
        /// Constructor
        /// </summary>
        public ChecksumTransform() : this(HASH_LENGTH) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="len">Checksum length in bytes (must be greater than zero, a power of two and not larger than <c>256</c>)</param>
        public ChecksumTransform(in int len) : base()
        {
            Contract.Ensures(HashValue is not null);
            HashValue = Array.Empty<byte>().CreateChecksum(len);
            HashSizeValue = len;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="checksum">Checksum (length must be greater than zero, a power of two and not larger than <c>256</c>)</param>
        public ChecksumTransform(in byte[] checksum) : base()
        {
            Contract.Ensures(HashValue is not null);
            Array.Empty<byte>().UpdateChecksum(checksum);
            HashValue = checksum;
            HashSizeValue = checksum.Length;
        }

        /// <summary>
        /// Hash algorithm name
        /// </summary>
        public static HashAlgorithmName HashAlgorithmName { get; } = new(ALGORITHM_NAME);

        /// <inheritdoc/>
        public override bool CanReuseTransform => true;

        /// <inheritdoc/>
        public override bool CanTransformMultipleBlocks => true;

        /// <inheritdoc/>
        public override int InputBlockSize => 1;

        /// <inheritdoc/>
        public override int OutputBlockSize => 1;

        /// <summary>
        /// Transform a block
        /// </summary>
        /// <param name="input">Input</param>
        public void TransformBlock(in ReadOnlySpan<byte> input) => HashCore(input);

        /// <inheritdoc/>
        public override void Initialize() => HashValue.AsSpan().Clear();

        /// <inheritdoc/>
        protected override void HashCore(byte[] array, int ibStart, int cbSize)
            => array.AsSpan(ibStart, cbSize).UpdateChecksum(HashValue ?? throw new ObjectDisposedException(GetType().ToString()));

        /// <inheritdoc/>
        protected override void HashCore(ReadOnlySpan<byte> source) => source.UpdateChecksum(HashValue);

        /// <inheritdoc/>
        protected override byte[] HashFinal() => HashValue ?? throw new ObjectDisposedException(GetType().ToString());

        /// <summary>
        /// Register the algorithm to the <see cref="CryptoConfig"/>
        /// </summary>
        public static void Register() => CryptoConfig.AddAlgorithm(typeof(ChecksumTransform), ALGORITHM_NAME);

        /// <inheritdoc/>
        new public static ChecksumTransform Create() => new();

        /// <summary>
        /// Create a checksum from source data
        /// </summary>
        /// <param name="source">Source data</param>
        /// <returns>Checksum</returns>
        public static byte[] HashData(byte[] source) => source.CreateChecksum();

        /// <summary>
        /// Create a checksum from source data
        /// </summary>
        /// <param name="source">Source data</param>
        /// <returns>Checksum</returns>
        public static byte[] HashData(ReadOnlySpan<byte> source) => source.CreateChecksum();

        /// <summary>
        /// Create a checksum from source data
        /// </summary>
        /// <param name="source">Source data</param>
        /// <param name="destination">Destination (the checksum; length must be greater than zero, a power of two and not larger than <c>256</c>)</param>
        /// <returns>Checksum</returns>
        public static int HashData(ReadOnlySpan<byte> source, Span<byte> destination)
        {
            destination.Clear();
            source.UpdateChecksum(destination);
            return destination.Length;
        }

        /// <summary>
        /// Create a checksum from source data
        /// </summary>
        /// <param name="source">Source data</param>
        /// <param name="destination">Destination (the checksum; length should be greater than zero, a power of two and not larger than <c>256</c>)</param>
        /// <param name="bytesWritten">Number of bytes written to the destination (will be a power of two, if succeed)</param>
        /// <returns>Succeed?</returns>
        public static bool TryHashData(ReadOnlySpan<byte> source, Span<byte> destination, out int bytesWritten)
        {
            bytesWritten = destination.Length;
            if (bytesWritten == 0)
            {
                return false;
            }
            else if (bytesWritten > 256)
            {
                bytesWritten = 256;
            }
            else
            {
                for (bytesWritten = destination.Length; bytesWritten > 0 && (bytesWritten & (bytesWritten - 1)) != 0; bytesWritten--) ;
            }
            HashData(source, destination[..bytesWritten]);
            return true;
        }
    }
}
