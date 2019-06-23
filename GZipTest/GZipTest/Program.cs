using CommandLine;
using GZipTest.Arguments;
using GZipTest.Core;
using System;
using System.Collections.Generic;

namespace GZipTest
{
    class Program
    {
        private static readonly Logger logger = new Logger();
        static int Main(string[] args)
        {
            return TestRun(args);
        }

        private static int Run(string[] args)
        {
            var result = Parser.Default.ParseArguments<CompressOptions, DecompressOptions>(args)
                .MapResult(
                    (CompressOptions o) => Compress(o),
                    (DecompressOptions o) => Decompress(o),
                    HandleErrors);
            return result;
        }

        private static int TestRun(string[] args)
        {
            while (true)
            {
                Console.WriteLine($"Choose:\r\n1 compress\r\n2 decompress\r\nOtherwise exit");
                var input = Console.ReadKey();
                int result;
                if (input.KeyChar == '1')
                {
                    args = new[]
                    {
                        "compress",
                         "d:\\TempVeeam\\Fantom.avi",
                         $"d:\\TempVeeam\\Fantom{DateTime.Now:HH}.avi.gz"
                     };
                    Console.Clear();
                    result = Run(args);

                }
                else if (input.KeyChar == '2')
                {
                    args = new[]
                    {
                        "decompress",
                        $"d:\\TempVeeam\\Fantom{DateTime.Now:HH}.avi.gz",
                        $"d:\\TempVeeam\\Fantom{DateTime.Now:HH}.avi"
                     };
                    Console.Clear();
                    result = Run(args);

                }
                else
                {
                    return 0;
                }
                Console.WriteLine($"Retuned {result}\r\nPress enter to continue ...");
                Console.ReadLine();
                Console.Clear();
            }
        }

        private static int Compress(CompressOptions options)
        {
            if (!options.Validate())
                return 1;

            try
            {
                Compressor compressor = new Compressor(logger, options.BlockSize);
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
                Compressor compressor = new Compressor(logger);
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

        private static int HandleErrors(IEnumerable<Error> errs)
        {
            return 1;
        }
    }
}
