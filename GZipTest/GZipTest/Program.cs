using CommandLine;
using GZipTest.Arguments;
using GZipTest.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GZipTest
{
    class Program
    {
        private static readonly Logger logger = new Logger();
        static int Main(string[] args)
        {
            return Run(args);
        }

        private static int Run(string[] args)
        {
            var h = Parser.Default.Settings.AutoHelp;
            var result = Parser.Default.ParseArguments<CompressOptions, DecompressOptions>(args)
                .MapResult(
                    (CompressOptions o) => Compress(o),
                    (DecompressOptions o) => Decompress(o),
                    errs => HandleErrors(errs));
            return result;
        }

        private static int HandleErrors(IEnumerable<Error> errs)
        {
            if (errs.Any( err => err.Tag == ErrorType.HelpRequestedError || err.Tag == ErrorType.HelpVerbRequestedError ))
            {
                return 0;
            }
            return 1;
        }

        private static int Compress(CompressOptions options)
        {
            if (!options.Validate())
                return 1;

            try
            {
                Comprimator compressor = new Comprimator(logger, options.BlockSize);
                if (compressor.CompressFile(options.OriginalFileName, options.ArchiveFileName))
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to compress file {options.OriginalFileName}: {ex.Message}");
            }
            return 1;
        }

        private static int Decompress(DecompressOptions options)
        {
            if (!options.Validate())
                return 1;

            try
            {
                Comprimator compressor = new Comprimator(logger);
                {
                    if (compressor.DecompressFile(options.ArchiveFileName, options.DestinationFileName))
                    {
                        return 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to decompress archive {options.ArchiveFileName}: {ex.Message}");
            }
            return 1;
        }
    }
}
