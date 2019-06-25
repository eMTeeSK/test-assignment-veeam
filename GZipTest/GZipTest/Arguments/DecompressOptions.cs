using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.IO;

namespace GZipTest.Arguments
{
    [Verb("decompress", HelpText = "Decompress specified archive created by this tool")]
    class DecompressOptions : IValidatableOption
    {
        private const string DestFilePretty = "Decompressed file name";
        private const string ArchiveFilePretty = "Archive file name";

        [Value(0, HelpText = ArchiveFilePretty, Required = true, MetaName = "Source")]
        public string ArchiveFileName { get; set; }

        [Value(1, HelpText = DestFilePretty, Required = true, MetaName = "Target")]
        public string DestinationFileName { get; set; }


        [Usage(ApplicationAlias = "gziptest")]
        public static IEnumerable<Example> Examples => new List<Example>() {
                new Example(
                    "Decompress source file to gzip archive",
                    new UnParserSettings
                    {
                        PreferShortName = true
                    },
                    new DecompressOptions { DestinationFileName = "file.bin", ArchiveFileName = "file.bin.gz" })
        };

        public bool Validate()
        {
            if (string.IsNullOrEmpty(DestFilePretty?.Trim()))
            {
                Console.WriteLine($"{DestFilePretty} must be provided");
                return false;
            }

            if (string.IsNullOrEmpty(ArchiveFileName?.Trim()))
            {
                Console.WriteLine($"{ArchiveFilePretty} must be provided");
                return false;
            }

            if (!File.Exists(this.ArchiveFileName))
            {
                Console.WriteLine($"{ArchiveFileName} with path {this.ArchiveFileName} was not found.");
                return false;
            }

            return true;
        }
    }
}
