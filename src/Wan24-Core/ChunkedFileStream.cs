namespace wan24.Core
{
    /// <summary>
    /// Chunked file stream helper
    /// </summary>
    public static class ChunkedFileStream
    {
        /// <summary>
        /// Create a chunked file stream
        /// </summary>
        /// <param name="fileName">Filename template (needs to use variable <c>chunk</c> (the numeric chunk index) for parsing a chunk filename using 
        /// <see cref="StringExtensions.Parse(string, in Dictionary{string, string}, int?, in bool, System.Text.RegularExpressions.Regex?, int?)"/></param>
        /// <param name="chunkSize">Chunk size in bytes</param>
        /// <param name="createMode">File create mode</param>
        /// <param name="openMode">Open existing file mode</param>
        /// <param name="access">File access</param>
        /// <param name="share">File share</param>
        /// <param name="options">File options</param>
        /// <returns>Chunked stream</returns>
        public static ChunkedStream Create(
            string fileName,
            in long chunkSize,
            FileMode openMode = FileMode.Open,
            FileAccess access = FileAccess.ReadWrite,
            FileShare share = FileShare.None,
            FileMode createMode = FileMode.CreateNew,
            FileOptions options = FileOptions.SequentialScan | FileOptions.Asynchronous
            )
        {
            if (chunkSize < 1) throw new ArgumentOutOfRangeException(nameof(chunkSize));
            string parseFileName(int chunk)
            {
                Dictionary<string, string> data = new()
                {
                    {"chunk", chunk.ToString() }
                };
                return fileName.Parse(data);
            }
            Stream chunkStream(ChunkedStream stream, int chunk) => chunk >= stream.CurrentNumberOfChunks
                ? new FileStream(parseFileName(chunk), createMode, access, share, bufferSize: 4096, options)
                : new FileStream(parseFileName(chunk), openMode, access, share, bufferSize: 4096, options);
            void deleteChunk(ChunkedStream stream, int chunk)
            {
                string fn = parseFileName(chunk);
                if (File.Exists(fn)) File.Delete(fn);
            }
            int numberOfChunks = 0;
            for (; File.Exists(parseFileName(numberOfChunks)); numberOfChunks++) ;
            return new ChunkedStream(
                access != FileAccess.Read,
                chunkSize,
                chunkStream,
                deleteChunk,
                numberOfChunks: numberOfChunks,
                lastChunkLength: numberOfChunks == 0 ? 0 : new FileInfo(parseFileName(numberOfChunks - 1)).Length
                );
        }
    }
}
