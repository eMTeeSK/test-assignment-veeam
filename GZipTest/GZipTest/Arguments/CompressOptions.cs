using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GZipTest.Arguments
{
    [Verb("compress", HelpText = "Compress specfied file")]
    class CompressOptions : IValidatableOption
    {
        private const string OrigFilePretty = "Original file name";
        private const string ArchiveFilePretty = "Archive file name";

        [Value(0, HelpText = OrigFilePretty, Required = true)]
        public string OriginalFileName { get; set; }

        [Value(1, HelpText = ArchiveFilePretty, Required = true)]
        public string ArchiveFileName { get; set; }

        [Option('b', "block-size", Default = 1, HelpText = "Size of blocks in which the archive will be splitted in megabite (MB).")]
        public int BlockSize { get; set; }

        public bool Validate()
        {
            if (string.IsNullOrEmpty(OriginalFileName?.Trim()))
            {
                Console.WriteLine($"{OrigFilePretty} must be provided");
                return false;
            }

            if (string.IsNullOrEmpty(ArchiveFileName?.Trim()))
            {
                Console.WriteLine($"{ArchiveFilePretty} must be provided");
                return false;
            }

            if (!File.Exists(this.OriginalFileName))
            {
                Console.WriteLine($"{OrigFilePretty} with path {this.OriginalFileName} was not found.");
                return false;
            }

            if (BlockSize <= 0)
            {
                Console.WriteLine($"Block size hat to be greater than 0");
                return false;
            }
            return true;
        }
    }
}
