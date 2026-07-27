using System;
using System.IO;
using EnkaDotNet.Caching;
using EnkaDotNet.Caching.Providers;
using Microsoft.Data.Sqlite;
using Xunit;

namespace EnkaDotNet.Tests.Caching
{
    public class SQLiteConnectionStringTests
    {
        /// <summary>
        /// ';' is a legal path character, so interpolating the path into "Data Source={path}"
        /// let a crafted path append extra connection options. The builder quotes it instead.
        /// </summary>
        [Fact]
        public async System.Threading.Tasks.Task DatabasePathContainingSemicolon_IsTreatedAsAPathNotAnOption()
        {
            string directory = Path.Combine(Path.GetTempPath(), "enka-sqlite-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            string databasePath = Path.Combine(directory, "cache.db;Mode=Memory");

            try
            {
                var provider = new SQLiteCacheProvider(new SQLiteCacheOptions
                {
                    DatabasePath = databasePath,
                    EnableAutoCleanup = false
                });

                await provider.SetAsync("key", "value", TimeSpan.FromMinutes(5));
                provider.Dispose();

                // Had the ";Mode=Memory" fragment been parsed as an option, nothing would be on disk.
                SqliteConnection.ClearAllPools();
                Assert.True(File.Exists(databasePath));
            }
            finally
            {
                SqliteConnection.ClearAllPools();
                try { Directory.Delete(directory, recursive: true); } catch (IOException) { }
            }
        }
    }
}
