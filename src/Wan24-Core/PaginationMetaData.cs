using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace wan24.Core
{
    /// <summary>
    /// Pagination meta data
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public record struct PaginationMetaData : ISerializeBinary<PaginationMetaData>, ISerializeString<PaginationMetaData>
    {
        /// <summary>
        /// Structure size in bytes
        /// </summary>
        public const int STRUCTURE_SIZE = sizeof(int) * 3;
        /// <summary>
        /// <see cref="_Page"/> byte offset
        /// </summary>
        public const int PAGE_BYTE_OFFSET = 0;
        /// <summary>
        /// <see cref="Count"/> byte offset
        /// </summary>
        public const int COUNT_BYTE_OFFSET = PAGE_BYTE_OFFSET + sizeof(int);
        /// <summary>
        /// <see cref="ItemsPerPage"/> byte offset
        /// </summary>
        public const int ITEMS_PER_PAGE_BYTE_OFFSET = COUNT_BYTE_OFFSET + sizeof(int);

        /// <summary>
        /// Current page
        /// </summary>
        [FieldOffset(PAGE_BYTE_OFFSET)]
        private int _Page = 1;
        /// <summary>
        /// Total number of items
        /// </summary>
        [FieldOffset(COUNT_BYTE_OFFSET)]
        public readonly int Count;
        /// <summary>
        /// Number of items per page
        /// </summary>
        [FieldOffset(ITEMS_PER_PAGE_BYTE_OFFSET)]
        public readonly int ItemsPerPage;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="count">Total number of items</param>
        /// <param name="itemsPerPage">Number of items per page</param>
        /// <param name="page">Current page</param>
        public PaginationMetaData(in int count, in int itemsPerPage, in int? page = null)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(count, nameof(count));
            ArgumentOutOfRangeException.ThrowIfLessThan(itemsPerPage, other: 1, nameof(itemsPerPage));
            Count = count;
            ItemsPerPage = itemsPerPage;
            if (page.HasValue) Page = page.Value;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buffer">Serialized data</param>
        public PaginationMetaData(in ReadOnlySpan<byte> buffer)
        {
            if (buffer.Length < STRUCTURE_SIZE) throw new ArgumentOutOfRangeException(nameof(buffer));
            _Page = buffer.ToInt();
            if (_Page < 1) throw new InvalidDataException($"Invalid page number {_Page}");
            Count = buffer[COUNT_BYTE_OFFSET..].ToInt();
            if (Count < 0) throw new InvalidDataException($"Invalid item count {Count}");
            ItemsPerPage = buffer[ITEMS_PER_PAGE_BYTE_OFFSET..].ToInt();
            if (ItemsPerPage < 1) throw new InvalidDataException($"Invalid number of items {ItemsPerPage}");
            if (_Page > TotalPages) throw new InvalidDataException($"Invalid page number {_Page}");
        }

        /// <summary>
        /// Current page
        /// </summary>
        public int Page
        {
            readonly get => _Page;
            set
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(value, other: 1, nameof(value));
                ArgumentOutOfRangeException.ThrowIfGreaterThan(value, other: TotalPages, nameof(value));
                _Page = value;
            }
        }

        /// <inheritdoc/>
        static int? ISerializeBinary.MaxStructureSize => STRUCTURE_SIZE;

        /// <inheritdoc/>
        static bool ISerializeBinary.IsFixedStructureSize => true;

        /// <inheritdoc/>
        static int? ISerializeString.MaxStringSize => null;

        /// <inheritdoc/>
        static bool ISerializeString.IsFixedStringSize => false;

        /// <summary>
        /// Total number of pages
        /// </summary>
        public readonly int TotalPages => (int)Math.Ceiling((float)Count / ItemsPerPage);

        /// <summary>
        /// If there's a previous page
        /// </summary>
        public readonly bool HasPreviousPage => _Page != 1;

        /// <summary>
        /// If there's a next page
        /// </summary>
        public readonly bool HasNextPage => _Page != TotalPages;

        /// <summary>
        /// Index of the first item from the current page
        /// </summary>
        public readonly int FirstItemIndex => (_Page - 1) * ItemsPerPage;

        /// <summary>
        /// Index of the last item from the current page
        /// </summary>
        public readonly int LastItemIndex => Math.Min(_Page * ItemsPerPage, Count - 1);

        /// <summary>
        /// Number of items on the current page
        /// </summary>
        public readonly int ItemsOnPage => _Page >= TotalPages ? Count % ItemsPerPage : ItemsPerPage;

        /// <inheritdoc/>
        readonly int? ISerializeBinary.StructureSize => STRUCTURE_SIZE;

        /// <inheritdoc/>
        readonly int? ISerializeString.StringSize => null;

        /// <summary>
        /// Navigate to the previous page
        /// </summary>
        /// <returns>Current page</returns>
        /// <exception cref="InvalidOperationException">Is at the first page</exception>
        public int PreviousPage()
        {
            if (_Page <= 1) throw new InvalidOperationException();
            return --_Page;
        }

        /// <summary>
        /// Navigate to the next page
        /// </summary>
        /// <returns>Current page</returns>
        /// <exception cref="InvalidOperationException">Is at the last page</exception>
        public int NextPage()
        {
            if (_Page >= TotalPages) throw new InvalidOperationException();
            return ++_Page;
        }

        /// <summary>
        /// Set the first page
        /// </summary>
        /// <returns>Current page</returns>
        public int FirstPage() => _Page = 1;

        /// <summary>
        /// Set the last page
        /// </summary>
        /// <returns>Current page</returns>
        public int LastPage() => _Page = TotalPages;

        /// <inheritdoc/>
        public override readonly string ToString() => $"{_Page};{Count};{ItemsPerPage}";

        /// <inheritdoc/>
        public readonly byte[] GetBytes()
        {
            byte[] res = new byte[STRUCTURE_SIZE];
            GetBytes(res);
            return res;
        }

        /// <inheritdoc/>
        public readonly int GetBytes(in Span<byte> buffer)
        {
            if (buffer.Length < STRUCTURE_SIZE) throw new ArgumentOutOfRangeException(nameof(buffer));
            _Page.GetBytes(buffer);
            Count.GetBytes(buffer[COUNT_BYTE_OFFSET..]);
            ItemsPerPage.GetBytes(buffer[ITEMS_PER_PAGE_BYTE_OFFSET..]);
            return STRUCTURE_SIZE;
        }

        /// <summary>
        /// Cast as <see cref="Page"/>
        /// </summary>
        /// <param name="meta"><see cref="PaginationMetaData"/></param>
        public static implicit operator int(in PaginationMetaData meta) => meta._Page;

        /// <summary>
        /// Cast as serialized data
        /// </summary>
        /// <param name="meta"><see cref="PaginationMetaData"/></param>
        public static implicit operator byte[](in PaginationMetaData meta) => meta.GetBytes();

        /// <summary>
        /// Cast from serialized data
        /// </summary>
        /// <param name="data">Serialized data</param>
        public static implicit operator PaginationMetaData(in byte[] data) => new(data);

        /// <summary>
        /// Cast from serialized data
        /// </summary>
        /// <param name="data">Serialized data</param>
        public static implicit operator PaginationMetaData(in Span<byte> data) => new(data);

        /// <summary>
        /// Cast from serialized data
        /// </summary>
        /// <param name="data">Serialized data</param>
        public static implicit operator PaginationMetaData(in ReadOnlySpan<byte> data) => new(data);

        /// <summary>
        /// Cast from serialized data
        /// </summary>
        /// <param name="data">Serialized data</param>
        public static implicit operator PaginationMetaData(in Memory<byte> data) => new(data.Span);

        /// <summary>
        /// Cast from serialized data
        /// </summary>
        /// <param name="data">Serialized data</param>
        public static implicit operator PaginationMetaData(in ReadOnlyMemory<byte> data) => new(data.Span);

        /// <inheritdoc/>
        public static PaginationMetaData DeserializeTypeFrom(in ReadOnlySpan<byte> buffer) => new(buffer);

        /// <inheritdoc/>
        public static bool TryDeserializeTypeFrom(in ReadOnlySpan<byte> buffer, out PaginationMetaData result)
        {
            if (buffer.Length < 1)
            {
                result = default;
                return false;
            }
            int page = buffer.ToInt(),
                count = buffer[COUNT_BYTE_OFFSET..].ToInt(),
                itemsPerPage = buffer[ITEMS_PER_PAGE_BYTE_OFFSET..].ToInt();
            if (page < 1 || count < 0 || itemsPerPage < 1 || page > Math.Ceiling((float)count / itemsPerPage))
            {
                result = default;
                return false;
            }
            result = new(count, itemsPerPage, page);
            return true;
        }

        /// <inheritdoc/>
        public static object DeserializeFrom(in ReadOnlySpan<byte> buffer) => new PaginationMetaData(buffer);

        /// <inheritdoc/>
        public static bool TryDeserializeFrom(in ReadOnlySpan<byte> buffer, [NotNullWhen(returnValue: true)] out object? result)
        {
            bool res = TryDeserializeTypeFrom(buffer, out PaginationMetaData meta);
            result = meta;
            return res;
        }

        /// <inheritdoc/>
        public static PaginationMetaData Parse(in ReadOnlySpan<char> str)
        {
            if (str.Length < 5) throw new FormatException("Invalid pagination meta data format");
            int index = str.IndexOf(';');
            if (index < 0) throw new FormatException("Invalid pagination meta data format (missing first separator)");
            int page = int.Parse(str[..index]);
            if (page < 1) throw new InvalidDataException($"Invalid page {page}");
            index++;
            int index2 = str[index..].IndexOf(';');
            if (index2 < 0) throw new FormatException("Invalid pagination meta data format (missing second separator)");
            int count = int.Parse(str.Slice(index, index2));
            if (count < 0) throw new InvalidDataException($"Invalid item count {count}");
            int itemsPerPage = int.Parse(str[(index + index2 + 1)..]);
            if (itemsPerPage < 1) throw new InvalidDataException($"Invalid number of items per page {itemsPerPage}");
            return new(count, itemsPerPage, page);
        }

        /// <inheritdoc/>
        public static bool TryParse(in ReadOnlySpan<char> str, out PaginationMetaData result)
        {
            if (str.Length < 5)
            {
                result = default;
                return false;
            }
            int index = str.IndexOf(';');
            if (index < 0 || !int.TryParse(str[..index], out int page) || page < 1)
            {
                result = default;
                return false;
            }
            index++;
            int index2 = str[index..].IndexOf(';');
            if (index2 < 0)
            {
                result = default;
                return false;
            }
            if (index2 < 0 || !int.TryParse(str.Slice(index, index2), out int count) || count < 0 || !int.TryParse(str[(index + index2 + 1)..], out int itemsPerPage) || itemsPerPage < 1)
            {
                result = default;
                return false;
            }
            result = new(count, itemsPerPage, page);
            return true;
        }

        /// <inheritdoc/>
        public static object ParseObject(in ReadOnlySpan<char> str) => Parse(str);

        /// <inheritdoc/>
        public static bool TryParseObject(in ReadOnlySpan<char> str, [NotNullWhen(returnValue: true)] out object? result)
        {
            bool res = TryParse(str, out PaginationMetaData meta);
            result = meta;
            return res;
        }
    }
}
