## Test assignment for Veeam

### Assignment

Implement a command line tool using C# for block-by-block compressing and decompressing of files using
class System.IO.Compression.GzipStream.

During compression source file should be split by blocks of the same size, for example, block of 1MB. Each
block then should be compressed and written to the output file independently of others blocks. Application
should effectively parallel and synchronize blocks processing in multicore environment and should be able
to process files, that are larger than available RAM size.

Program code must be safe and robust in terms of exceptions. In case of exceptional situations user should
be informed by user friendly message, that allows user to fix occurred issue, for example, in case of OS
limitations.

Only basic classes and synchronization objects should be used for multithreading (Thread,
Manual/AutoResetEvent, Monitor, Semaphore, Mutex), it is not allowed to use async/await, ThreadPool,
BackgroundWorker, TPL.

Source code should satisfy OOP and OOD principles (readability, classes separation and so on).

Use the following command line arguments:
 *  compressing: GZipTest.exe compress [original file name] [archive file name]
 *  decompressing: GZipTest.exe decompress [archive file name] [decompressing file name]
 
On success program should return 0, otherwise 1.

**Note:** format of the archive is up to solution author, and does not affects final score, for example there is
no requirement for archive file to be compatible with GZIP file format.
Please send us solution source files and Visual Studio project. Briefly describe architecture and algorithms
used.

### Solution

Solution is splitted in two projects, one handling console UI, second `GZipTest.Core` handles functionality.  
`GZipTest.Core` contains two visible items:
 *  `Comprimator` - wraps and handles comprimation and decomprimation
 *  `ILogger` - Interface for providing UI output from comprimation process, like progress, messages and errors.

Comprimation (and decomprimation) is handled by "Processor" classes:
 *  `CompressionProcessor` - for comprimation
 *  `DecompressionProcessor` - for decomprimation  
Both are derivated from `ProcessorBase`, where magic is done. In children, there are only overrides for specific functionality which differs between comprimation and decomprimation.

#### Threads

`ProcessorBase` uses 4 different functionalities (types of thread functions):
 1.  Reader thread - reads input file, block by block to memory and fills two BlockingCollections (hope they are allowed):  
      *  One for processing
      *  Second for writing
 1.  Processing threads - number of threads is based on count of cores in PC  
Processing threads taking blocks from 1st blocking collection, using GZipStream (de)compress data from source MemoryStream to target memory stream, and marks `Block` as ready for write
 1.  Writer - writing of blocks to target file is processed in main thread.  
Write functionality takes blocks from second blocking collection in order how they were readed. To ensure, that block is already processes, write function waits until is "marked" for write, using `ManualResetEvent`.
 1.  Printer thread - handles progress propagation to UI through `ILogger`

#### File format

Compressed file is in GZip standard, containg custom EXTRA field with ID "Ch". This field contains length of compressed block, for easier reading of blocks by Decompressor reader
Decompressor can only decompress GZip files maded by this program.

