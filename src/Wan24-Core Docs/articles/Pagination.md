# Pagination

There are multiple types which helps witrh enumerable pagination:

| Type | Description |
| ---- | ----------- |
| `EnumerablePagination<T>` | Pagination for an `IEnumerable<T>` (forward only, if the enumerator can't be reset) |
| `CachedEnumerablePagination<T>` | Pagination for an `IEnumerable<T>`, which stores items in a cache to avoid double enumeration |
| `AsyncEnumerablePagination<T>` | Pagination for an `IAsyncEnumerable<T>` (forward only) |
| `CachedAsyncEnumerablePagination<T>` | Pagination for an `IAsyncEnumerable<T>`, which stores items in a cache to avoid double enumeration and enables backward paging also |

Example usage:

```cs
using EnumerablePagination<AnyType> pagination = new(enumerable, itemsPerPage: 10);
IEnumerable<AnyType> currentPage = pagination.NextPage();
// currentPage enumerates all items of the first page
currentPage = pagination.NextPage();
// currentPage enumerates all items of the second page
currentPage = pagination.NextPage(page: pagination.CurrentPage - 1);
// currentPage enumerates all items of the first page
```

Paging backwards is only possible if the enumerator implemented the `Reset` method. To use backward paging with enumerators that don't, you can use the cached enumerable pagination instead.

Calling `NextPage` does cancel the enumeration of any previously returned page enumerable and points the base enumerator to the desired page offset.
