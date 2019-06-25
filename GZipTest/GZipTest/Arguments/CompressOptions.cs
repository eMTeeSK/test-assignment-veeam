using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GZipTest.Arguments
{
    [Verb("compress", HelpText = "Comprimate specfied file")]
    class CompressOptions : IValidatableOption
    {
        private const string OrigFilePretty = "Original file name";
        private const string ArchiveFilePretty = "Archive file name";

        [Value(0, HelpText = OrigFilePretty, Required = true, MetaName = "Source")]
        public string OriginalFileName { get; set; }

        [Value(1, HelpText = ArchiveFilePretty, Required = true, MetaName = "Target")]
        public string ArchiveFileName { get; set; }

        [Option('b', "block-size", Default = 1, HelpText = "Size of blocks in which the archive will be splitted in megabyte (MB).")]
        public int BlockSize { get; set; }



        [Usage(ApplicationAlias = "gziptest")]
        public static IEnumerable<Example> Examples => new List<Example>() {
                new Example(
                    "Compress source file to gzip archive",
                    new UnParserSettings
                    {
                        PreferShortName = true
                    },
                    new CompressOptions { OriginalFileName = "file.bin", ArchiveFileName = "file.bin.gz", BlockSize = 1 })
        };

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
