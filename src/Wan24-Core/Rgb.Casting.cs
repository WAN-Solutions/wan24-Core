namespace wan24.Core
{
    // Casting
    public readonly partial record struct Rgb
    {
        /// <summary>
        /// Cast as <see cref="string"/>
        /// </summary>
        /// <param name="rgb"><see cref="Rgb"/></param>
        public static implicit operator string(in Rgb rgb) => rgb.ToString();

        /// <summary>
        /// Cast as <see cref="int"/> (24 bit unsigned integer value)
        /// </summary>
        /// <param name="rgb"><see cref="Rgb"/></param>
        public static implicit operator int(in Rgb rgb) => rgb.ToInt24();

        /// <summary>
        /// Cast as byte array
        /// </summary>
        /// <param name="rgb"><see cref="Rgb"/></param>
        public static implicit operator byte[](in Rgb rgb) => rgb.GetBytes();

        /// <summary>
        /// Cast from <see cref="int"/> 24 bit unsigned integer value
        /// </summary>
        /// <param name="rgb">24 bit RGB unsigned integer value</param>
        public static implicit operator Rgb(in int rgb) => new(rgb);

        /// <summary>
        /// Cast from bytes
        /// </summary>
        /// <param name="rgb">RGB bytes</param>
        public static implicit operator Rgb(in byte[] rgb) => new(rgb);

        /// <summary>
        /// Cast from span
        /// </summary>
        /// <param name="rgb">RGB bytes</param>
        public static implicit operator Rgb(in Span<byte> rgb) => new(rgb);

        /// <summary>
        /// Cast from span
        /// </summary>
        /// <param name="rgb">RGB bytes</param>
        public static implicit operator Rgb(in ReadOnlySpan<byte> rgb) => new(rgb);

        /// <summary>
        /// Cast from memory
        /// </summary>
        /// <param name="rgb">RGB bytes</param>
        public static implicit operator Rgb(in Memory<byte> rgb) => new(rgb.Span);

        /// <summary>
        /// Cast from memory
        /// </summary>
        /// <param name="rgb">RGB bytes</param>
        public static implicit operator Rgb(in ReadOnlyMemory<byte> rgb) => new(rgb.Span);
    }
}
