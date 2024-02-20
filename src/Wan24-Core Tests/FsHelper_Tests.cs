using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class FsHelper_Tests
    {
        [TestMethod]
        public void General_Tests()
        {
            string folderName = Path.Combine(Path.GetFullPath("./"), Guid.NewGuid().ToString());
            try
            {
                // Ensure/create folder
                Assert.IsTrue(FsHelper.EnsureFolder(folderName));

                // Create file
                string fileName = Path.Combine(folderName, "test.dat");
                using (FileStream fs = FsHelper.CreateFileStream(fileName))
                    fs.WriteByte(0);
                Assert.IsTrue(File.Exists(fileName));
                Assert.AreEqual(1, new FileInfo(fileName).Length);

                // Overwrite existing files
                using (FileStream fs = FsHelper.CreateFileStream(fileName, FileMode.OpenOrCreate, overwrite: true))
                    Assert.AreEqual(0, fs.Length);

                // Find files
                string[] files = FsHelper.FindFiles(folderName, new($"{Path.GetFileName(fileName)}$")).ToArray();
                Assert.AreEqual(1, files.Length);
                Assert.AreEqual(fileName, files[0]);

                // Find folders
                string[] folders = FsHelper.FindFolders(Path.GetFullPath("./"), new($"{Path.GetFileName(folderName)}$")).ToArray();
                Assert.AreEqual(1, folders.Length);
                Assert.AreEqual(folderName, folders[0]);
            }
            finally
            {
                if (Directory.Exists(folderName)) Directory.Delete(folderName, recursive: true);
            }
        }
    }
}
