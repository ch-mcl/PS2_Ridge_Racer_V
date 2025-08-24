using System;
using System.IO;
using System.Security.Cryptography;
using RidgeRacerVTool.RR5_Lib;
using RidgeRacerVTool.TOC;

namespace RidgeRacerVTool
{
    class Unpacker
    {
        private string _elfPath;
        private string _inputPath;
        private string _destPath;
        private bool _isGenerateHashList = false;

        private TOCInformation _toc;

        public Unpacker(string elfPath, string inputPath, string outputPath, bool generateHashList = false)
        {
            _elfPath = elfPath;
            _inputPath = inputPath;
            _destPath = outputPath;
            _isGenerateHashList = generateHashList;

            string elfName = Path.GetFileName(_elfPath);

            if (!TableOfContents.TOCInfos.TryGetValue(elfName, out TOCInformation toc))
            {
                throw new ArgumentException("Invalid or non-supported elf of Ridge Racer V provided.");
            }

            _toc = toc;
        }

        public void Unpack()
        {
            Console.WriteLine("Starting to unpack...");

            if (_isGenerateHashList)
            {
                // Generate CSV
                UnpackWithHash();
                return;
            }

            Directory.CreateDirectory(_destPath);

            // elf file
            using (FileStream elfFileStream = new FileStream(_elfPath, FileMode.Open, FileAccess.Read))
            // arc file
            using (FileStream arcFileStream = new FileStream(_inputPath, FileMode.Open, FileAccess.Read))
            {
                elfFileStream.Seek((long)_toc.TocAddress, SeekOrigin.Begin);
                for (int i = 0; i < _toc.FileCount; i++)
                {
                    FileDescriptor descriptor = new FileDescriptor();
                    bool result = descriptor.Unpack(elfFileStream);
                    // get Terminator
                    if (result)
                    {
                        break;
                    }

                    string extention = Archive.EXT_RR5_RAW;
                    if (descriptor.compressedSize < descriptor.uncompressedSize)
                    {
                        extention = Archive.EXT_RR5_LZ; // Needs decompress by LZSS(RRV Format).
                    }

                    string destFileName = string.Format("{0:D8}.{1}", i, extention);
                    string fullpath = $@"{_destPath}\{destFileName}";
                    using (FileStream destFileStream = new FileStream(fullpath, FileMode.Create, FileAccess.Write))
                    {
                        byte[] destBytes = new byte[descriptor.compressedSize];
                        arcFileStream.Seek(descriptor.blockOffset * 0x800, SeekOrigin.Begin);
                        arcFileStream.Read(destBytes, 0x00, descriptor.compressedSize);
                        destFileStream.Write(destBytes, 0x00, descriptor.compressedSize);
                    }
                }
            }

            Console.WriteLine("Done.");
            return;
        }

        private void UnpackWithHash()
        {
            Directory.CreateDirectory(_destPath);

            // elf file
            using (FileStream elfFileStream = new FileStream(_elfPath, FileMode.Open, FileAccess.Read))
            // arc file
            using (FileStream arcFileStream = new FileStream(_inputPath, FileMode.Open, FileAccess.Read))
            // csv file
            using (StreamWriter streamWriterHashList = new StreamWriter($@"{_destPath}\hashList.csv"))
            {
                streamWriterHashList.WriteLine("Id, Id (Hex), Hash (SHA1), Compress");

                elfFileStream.Seek((long)_toc.TocAddress, SeekOrigin.Begin);
                for (int i = 0; i < _toc.FileCount; i++)
                {
                    FileDescriptor descriptor = new FileDescriptor();
                    bool result = descriptor.Unpack(elfFileStream);
                    // get Terminator
                    if (result)
                    {
                        break;
                    }

                    string extention = Archive.EXT_RR5_RAW;
                    if (descriptor.compressedSize < descriptor.uncompressedSize)
                    {
                        extention = Archive.EXT_RR5_LZ; // Needs decompress by LZSS(RRV Format).
                    }

                    string destFileName = string.Format("{0:D8}.{1}", i, extention);
                    string fullpath = $@"{_destPath}\{destFileName}";
                    using (FileStream destFileStream = new FileStream(fullpath, FileMode.Create, FileAccess.Write))
                    {
                        byte[] destBytes = new byte[descriptor.compressedSize];
                        arcFileStream.Seek(descriptor.blockOffset * 0x800, SeekOrigin.Begin);
                        arcFileStream.Read(destBytes, 0x00, descriptor.compressedSize);
                        destFileStream.Write(destBytes, 0x00, descriptor.compressedSize);

                        using (SHA1 sha1 = SHA1.Create())
                        {
                            byte[] hash = sha1.ComputeHash(destBytes);
                            string row = String.Format("{0:D8}, {0:X8}, {1}, {2}", i, BitConverter.ToString(hash).Replace("-", ""), (descriptor.compressedSize < descriptor.uncompressedSize ? "Yes" : "No"));
                            streamWriterHashList.WriteLine(row);
                        }
                    }
                }
            }

            Console.WriteLine("Done.");
            return;
        }

    }
}
