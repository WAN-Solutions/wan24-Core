using System.Diagnostics.CodeAnalysis;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text;

namespace wan24.Core
{
    /// <summary>
    /// Physical ethernet MAC address (48 bit = 6 bytes)
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public readonly record struct MacAddress : ISerializeBinary<MacAddress>, ISerializeString<MacAddress>, IStringValueConverter<MacAddress>
    {
        /// <summary>
        /// Structure size in bytes
        /// </summary>
        public const int STRUCTURE_SIZE = sizeof(ulong);
        /// <summary>
        /// Serialized structure size in bytes
        /// </summary>
        public const int SERIALIZED_STRUCTURE_SIZE = 48 >> 3;// 48 bit = 6 bytes
        /// <summary>
        /// Maximum string size in characters
        /// </summary>
        public const int MAX_STRING_SIZE = (SERIALIZED_STRUCTURE_SIZE << 1) + 5;
        /// <summary>
        /// String byte delimiter
        /// </summary>
        public const char STRING_DELIMITER = ':';
        /// <summary>
        /// Broadcast MAC address
        /// </summary>
        public const ulong BROADCAST = 0x0000_ffffffffffffUL;
        /// <summary>
        /// I/G (individual/group flag; <c>1</c> = group)
        /// </summary>
        public const ulong INDIVIDUAL_GROUP_FLAG = 1UL << 40;
        /// <summary>
        /// U/L (universal/local flag; <c>1</c> = local)
        /// </summary>
        public const ulong UNIVERSAL_LOCAL_FLAG = 1UL << 41;

        /// <summary>
        /// Zero
        /// </summary>
        public static readonly MacAddress Zero = new();
        /// <summary>
        /// Broadcast MAC address
        /// </summary>
        public static readonly MacAddress Broadcast = new(BROADCAST);

        /// <summary>
        /// MAC address (LSB format)
        /// </summary>
        [FieldOffset(0)]
        public readonly ulong Address;

        /// <summary>
        /// Constructor
        /// </summary>
        public MacAddress() => Address = 0UL;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="address">MAC address (LSB format; will be normalized)</param>
        public MacAddress(in ulong address) => Address = address & BROADCAST;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buffer">Serialized data (LSB format)</param>
        public MacAddress(in ReadOnlySpan<byte> buffer)
        {
            if (buffer.Length < SERIALIZED_STRUCTURE_SIZE) throw new InvalidDataException("Buffer too small");
            using RentedMemoryRef<byte> rentedBuffer = new(len: sizeof(ulong));
            Span<byte> rentedSpan = rentedBuffer.Span;
            buffer[..SERIALIZED_STRUCTURE_SIZE].CopyTo(rentedSpan[2..]);
            rentedSpan.Reverse();
            Address = rentedSpan.ToULong();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pa"><see cref="PhysicalAddress"/></param>
        public MacAddress(in PhysicalAddress pa) : this(pa.GetAddressBytes()) { }

        /// <summary>
        /// Default string delimiter
        /// </summary>
        public static char DefaultStringDelimiter { get; set; } = STRING_DELIMITER;

        /// <inheritdoc/>
        public static int? MaxStructureSize => SERIALIZED_STRUCTURE_SIZE;

        /// <inheritdoc/>
        public static bool IsFixedStructureSize => true;

        /// <inheritdoc/>
        public static int? MaxStringSize => MAX_STRING_SIZE;

        /// <inheritdoc/>
        public static bool IsFixedStringSize => true;

        /// <summary>
        /// MSB MAC address (RFC 2469)
        /// </summary>
        public ulong MsbAddress
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => ~Address & BROADCAST;
        }

        /// <summary>
        /// If this is the broadcast MAC address
        /// </summary>
        public bool IsBroadcast
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Address == BROADCAST;
        }

        /// <summary>
        /// If this is an IPv4 multicast MAC address
        /// </summary>
        public bool IsIPv4Multicast
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => ((Address >> 24) & 0x0000_000000ffffffUL) == 0x0000_00000001005eUL && (Address & 0x0000_000000ffffffUL) <= 0x0000_0000007fffffUL;
        }

        /// <summary>
        /// If this is an IPv6 multicast MAC address
        /// </summary>
        public bool IsIPv6Multicast
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Address >> 32 == 0x0000_000000003333UL;
        }

        /// <summary>
        /// If this is a group MAC address
        /// </summary>
        public bool IsGroup
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => (Address & INDIVIDUAL_GROUP_FLAG) == INDIVIDUAL_GROUP_FLAG;
        }

        /// <summary>
        /// If this is an individual MAC address
        /// </summary>
        public bool IsIndividual
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => (Address & INDIVIDUAL_GROUP_FLAG) == 0;
        }

        /// <summary>
        /// If this is a local MAC address (LAA)
        /// </summary>
        public bool IsLocal
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => (Address & UNIVERSAL_LOCAL_FLAG) == UNIVERSAL_LOCAL_FLAG;
        }

        /// <summary>
        /// If this is an universal MAC address (UAA)
        /// </summary>
        public bool IsUniversal
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => (Address & UNIVERSAL_LOCAL_FLAG) == 0;
        }

        /// <summary>
        /// Organizationally Unique Identifier
        /// </summary>
        public uint OUI
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => (uint)((Address >> 24) & 0x0000_000000ffffffUL);
        }

        /// <summary>
        /// Vendor specific ID
        /// </summary>
        public uint VendorId
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => (uint)(Address & 0x0000_000000ffffffUL);
        }

        /// <summary>
        /// IPv4 multicast ID
        /// </summary>
        public uint IPv4MulticastId
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => (uint)(Address & 0x0000_0000007fffffUL);
        }

        /// <summary>
        /// IPv6 multicast ID
        /// </summary>
        public uint IPv6MulticastId
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => (uint)(Address & 0x0000_0000ffffffffUL);
        }

        /// <inheritdoc/>
        public int? StructureSize
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => SERIALIZED_STRUCTURE_SIZE;
        }

        /// <inheritdoc/>
        public int? StringSize
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => MAX_STRING_SIZE;
        }

        /// <inheritdoc/>
        public string DisplayString
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => ToString();
        }

        /// <inheritdoc/>
        public byte[] GetBytes()
        {
            byte[] res = new byte[SERIALIZED_STRUCTURE_SIZE];
            GetBytes(res);
            return res;
        }

        /// <inheritdoc/>
        public int GetBytes(in Span<byte> buffer)
        {
            if (buffer.Length < SERIALIZED_STRUCTURE_SIZE) throw new OutOfMemoryException();
            using RentedMemoryRef<byte> rentedBuffer = new(len: sizeof(ulong), clean: false);
            Span<byte> rentedSpan = rentedBuffer.Span;
            Address.GetBytes(rentedSpan);
            rentedSpan[..SERIALIZED_STRUCTURE_SIZE].CopyTo(buffer);
            buffer[..SERIALIZED_STRUCTURE_SIZE].Reverse();
            return SERIALIZED_STRUCTURE_SIZE;
        }

        /// <summary>
        /// Get RFC 2469 bytes (Token Ring MSB format)
        /// </summary>
        /// <returns>Bytes</returns>
        public byte[] GetRfc2469Bytes()
        {
            byte[] res = new byte[SERIALIZED_STRUCTURE_SIZE];
            GetRfc2469Bytes(res);
            return res;
        }

        /// <summary>
        /// Get RFC 2469 bytes (Token Ring MSB format)
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <returns>Number of bytes written to the buffer</returns>
        /// <exception cref="OutOfMemoryException">Buffer too small</exception>
        public int GetRfc2469Bytes(in Span<byte> buffer)
        {
            if (buffer.Length < SERIALIZED_STRUCTURE_SIZE) throw new OutOfMemoryException();
            using RentedMemoryRef<byte> rentedBuffer = new(len: sizeof(ulong), clean: false);
            Span<byte> rentedSpan = rentedBuffer.Span;
            MsbAddress.GetBytes(rentedSpan);
            rentedSpan[..SERIALIZED_STRUCTURE_SIZE].CopyTo(buffer);
            buffer[..SERIALIZED_STRUCTURE_SIZE].Reverse();
            return SERIALIZED_STRUCTURE_SIZE;
        }

        /// <summary>
        /// Get this MAC address as <see cref="PhysicalAddress"/>
        /// </summary>
        /// <returns><see cref="PhysicalAddress"/></returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public PhysicalAddress ToPhysicalAddress() => new(GetBytes());

        /// <summary>
        /// Get as string (canonical LSB format)
        /// </summary>
        /// <param name="delimiter">Delimiter</param>
        /// <returns>String</returns>
        public string ToString(in char? delimiter)
        {
            using RentedMemoryRef<byte> addressBuffer = new(len: SERIALIZED_STRUCTURE_SIZE, clean: false);
            Span<byte> addressSpan = addressBuffer.Span;
            GetBytes(addressSpan);
            if (!delimiter.HasValue) return Convert.ToHexString(addressSpan).ToLower();
            StringBuilder sb = new(MAX_STRING_SIZE);
            for (int i = 0; i < SERIALIZED_STRUCTURE_SIZE; i++)
            {
                sb.Append(addressSpan[i].ToString("x2"));
                if (i < SERIALIZED_STRUCTURE_SIZE - 1) sb.Append(delimiter);
            }
            return sb.ToString();
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public override string ToString() => ToString(DefaultStringDelimiter);

        /// <summary>
        /// Cast as <see cref="Address"/>
        /// </summary>
        /// <param name="mac">MAC address</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator ulong(in MacAddress mac) => mac.Address;

        /// <summary>
        /// Cast as serialized data
        /// </summary>
        /// <param name="mac">MAC address</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator byte[](in MacAddress mac) => mac.GetBytes();

        /// <summary>
        /// Cast as string
        /// </summary>
        /// <param name="mac">MAC address</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator string(in MacAddress mac) => mac.ToString();

        /// <summary>
        /// Cast from serialized data
        /// </summary>
        /// <param name="buffer">Serialized data</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator MacAddress(in byte[] buffer) => new(buffer);

        /// <summary>
        /// Cast from serialized data
        /// </summary>
        /// <param name="buffer">Serialized data</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator MacAddress(in Span<byte> buffer) => new(buffer);

        /// <summary>
        /// Cast from serialized data
        /// </summary>
        /// <param name="buffer">Serialized data</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator MacAddress(in ReadOnlySpan<byte> buffer) => new(buffer);

        /// <summary>
        /// Cast from serialized data
        /// </summary>
        /// <param name="buffer">Serialized data</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator MacAddress(in Memory<byte> buffer) => new(buffer.Span);

        /// <summary>
        /// Cast from serialized data
        /// </summary>
        /// <param name="buffer">Serialized data</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator MacAddress(in ReadOnlyMemory<byte> buffer) => new(buffer.Span);

        /// <summary>
        /// Cast from string
        /// </summary>
        /// <param name="str">String</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator MacAddress(in string str) => Parse(str);

        /// <summary>
        /// Cast from a MAC address
        /// </summary>
        /// <param name="mac">MAC address</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator MacAddress(in ulong mac) => new(mac);

        /// <summary>
        /// Cast from <see cref="PhysicalAddress"/>
        /// </summary>
        /// <param name="pa"><see cref="PhysicalAddress"/></param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator MacAddress(in PhysicalAddress pa) => new(pa);

        /// <summary>
        /// Cast as <see cref="PhysicalAddress"/>
        /// </summary>
        /// <param name="mac">MAC address</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator PhysicalAddress(in MacAddress mac) => mac.ToPhysicalAddress();

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static object DeserializeFrom(in ReadOnlySpan<byte> buffer) => new MacAddress(buffer);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static MacAddress DeserializeTypeFrom(in ReadOnlySpan<byte> buffer) => new(buffer);

        /// <inheritdoc/>
        public static bool TryDeserializeFrom(in ReadOnlySpan<byte> buffer, [NotNullWhen(true)] out object? result)
        {
            if (buffer.Length < SERIALIZED_STRUCTURE_SIZE)
            {
                result = null;
                return false;
            }
            result = new MacAddress(buffer);
            return true;
        }

        /// <inheritdoc/>
        public static bool TryDeserializeTypeFrom(in ReadOnlySpan<byte> buffer, [NotNullWhen(true)] out MacAddress result)
        {
            if (buffer.Length < SERIALIZED_STRUCTURE_SIZE)
            {
                result = default;
                return false;
            }
            result = new(buffer);
            return true;
        }

        /// <inheritdoc/>
        public static MacAddress Parse(in ReadOnlySpan<char> str)
        {
            if (str.Length < MAX_STRING_SIZE) throw new FormatException("Not enough data");
            using RentedMemoryRef<char> charBuffer = new(len: SERIALIZED_STRUCTURE_SIZE << 1, clean: false);
            Span<char> charSpan = charBuffer.Span;
            int j = 0,
                len2 = charSpan.Length;
            for (int i = 0, len = str.Length; i < len && j < len2; i++)
                switch (str[i])
                {
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                    case 'a':
                    case 'b':
                    case 'c':
                    case 'd':
                    case 'e':
                    case 'f':
                    case 'A':
                    case 'B':
                    case 'C':
                    case 'D':
                    case 'E':
                    case 'F':
                        charSpan[j] = str[i];
                        j++;
                        break;
                }
            if (j < len2) throw new FormatException("Not enough valid characters");
            return new(Convert.FromHexString(charSpan));
        }

        /// <inheritdoc/>
        public static bool TryParse(in ReadOnlySpan<char> str, [NotNullWhen(true)] out MacAddress result)
        {
            if (str.Length < MAX_STRING_SIZE)
            {
                result = default;
                return false;
            }
            using RentedMemoryRef<char> charBuffer = new(len: SERIALIZED_STRUCTURE_SIZE << 1, clean: false);
            Span<char> charSpan = charBuffer.Span;
            int j = 0,
                len2 = charSpan.Length;
            for (int i = 0, len = str.Length; i < len && j < len2; i++)
                switch (str[i])
                {
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                    case 'a':
                    case 'b':
                    case 'c':
                    case 'd':
                    case 'e':
                    case 'f':
                    case 'A':
                    case 'B':
                    case 'C':
                    case 'D':
                    case 'E':
                    case 'F':
                        charSpan[j] = str[i];
                        j++;
                        break;
                }
            if (j < len2)
            {
                result = default;
                return false;
            }
            result = new(Convert.FromHexString(charSpan));
            return true;
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static object ParseObject(in ReadOnlySpan<char> str) => Parse(str);

        /// <inheritdoc/>
        public static bool TryParseObject(in ReadOnlySpan<char> str, [NotNullWhen(true)] out object? result)
        {
            if (!TryParse(str, out MacAddress res))
            {
                result = null;
                return false;
            }
            result = res;
            return true;
        }

        /// <summary>
        /// Create from OUI and ID
        /// </summary>
        /// <param name="oui">OUI (24 bit)</param>
        /// <param name="id">ID (24 bit)</param>
        /// <returns>MAC address</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static MacAddress Create(in uint oui, in uint id) => new(((ulong)oui << 24) | (id & 0x00_000000ffffffUL));

        /// <summary>
        /// Create an IPv4 multicast MAC address
        /// </summary>
        /// <param name="id">ID (<c>0 - 0x00_7fffffu</c>)</param>
        /// <returns>MAC address</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static MacAddress CreateIPv4Multicast(in uint id)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(id, 0x00_7fffffu, nameof(id));
            return new(0x0000_01005e000000UL | id);
        }

        /// <summary>
        /// Create an IPv6 multicast MAC address
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns>MAC address</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static MacAddress CreateIPv6Multicast(in uint id) => new(0x0000_333300000000UL | id);
    }
}
