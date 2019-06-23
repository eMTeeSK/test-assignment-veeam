using CommandLine;
using System;
using System.IO;

namespace GZipTest.Arguments
{
    [Verb("decompress", HelpText = "Decompress specfied archive")]
    class DecompressOptions : IValidatableOption
    {
        private const string DestFilePretty = "Decompressed file name";
        private const string ArchiveFilePretty = "Archive file name";

        [Value(1, HelpText = DestFilePretty, Required = true)]
        public string DestinationFileName { get; set; }

        [Value(0, HelpText = ArchiveFilePretty, Required = true)]
        public string ArchiveFileName { get; set; }


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
