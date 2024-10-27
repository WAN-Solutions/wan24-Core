# ACID stream

The `AcidStream` type allows ACID stream manipulating operations, which allow to rollback all changes at any time. The stream does store original data in a backup stream, which will be deleted, if the changes have been committed. Since the backup is being written before the target stream is being changed, the original data is safe in case of any error. Using static methods you may investigate a backup stream, if a rollback failed also. An automatic rollback can be repeated at any time later.

For working with an ACID stream you'll require the target stream and a backup stream. If you work with a file, you could use the `AcidFileStream`:

```cs
AcidStream<FileStream> acidStream = await AcidFileStream.CreateAsync(File.OpenWrite("/path/to/file.ext"));
await using(acidStream)
{
	// Apply changes here
	await acidStream.CommitAsync();
}
```

The `AcidFileStream` does in this example use the file `/path/to/.acid.file.ext` as backup. The `Create(Async)` method will also try to perform a previously failed rollback, if an existing backup file was found. Have a look at this methods source code, if you'd like to know how the ACID stream is being handled for that in detail.

## Automatic rollback of a backup at a later time

If a rollback failed, you can run it at any time later:

```cs
await AcidStream<Stream>.PerformRollback(acidStream);
```

This requires the backup not to be corrupted in any way.

## Manual backup investigation (corrupted backup handling)

An ACID backup stream has a binary structure which stores

- the backup record type
- the time when the record was written
- the position in the target stream where the data was written
- the length of the overwritten or deleted data
- the original data

and the previous stream length, if `SetLength` was called. The initial target stream length is the first information, which is being written to the backup.

Records are being written in a style that allows to investigate a backup stream forward and backward easily. In case a backup stream is corrupted, you may need to investigate it manual using these static `AcidStream<T>` methods:

| Method | Usage |
| ------ | ----- |
| `ReadLengthFromBackup(Async)` | Read the initial target stream length from the beginning of the backup (offset zero) |
| `InvestigateBackup(Async)` | Validate the backup and get the number of backup records (from offset zero) |
| `ReadBackupRecordForward(Async)` | Read the next record from the backup (the stream position must be at the beginning of a record) |
| `ReadBackupRecordBackward(Async)` | Read the previous record from the backup (the stream position must be at the beginning of the next record, or at the end of the backup stream) |

Manual backup investigation allows to find errors in the backup, which may be useful for rescueing data. When restoring data, the backup should be processed backwards (from the last valid record). These are the possible backup record types, which may be red from a backup:

| Type | Description |
| ---- | ----------- |
| `AcidStream<T>.BackupWriteRecord` | Contains information about overwritten data |
| `AcidStream<T>.BackupLengthRecord` | Contains information about deleted data after calling `SetLength` |

Using these records you can access overwritten or deleted data easily.

Of course even the ACID stream can't guarantee 100% safety, because also the backup may become corrupted. For more safety you may consider to store the temporary backup stream on a different filesystem.
