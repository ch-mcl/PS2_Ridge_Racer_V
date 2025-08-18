using System;
using System.IO;
using RidgeRacerVTool.RR5_Lib;
using RidgeRacerVTool.TOC;

namespace RidgeRacerVTool
{
    class Unpacker
    {
        private string _elfPath;
        private string _inputPath;
        private string _destPath;

        private TOCInformation _toc;

        public Unpacker(string elfPath, string inputPath, string outputPath)
        {
            _elfPath = elfPath;
            _inputPath = inputPath;
            _destPath = outputPath;

            string elfName = Path.GetFileName(_elfPath);

            if (!TableOfContents.TOCInfos.TryGetValue(elfName, out TOCInformation toc))
            {
                throw new ArgumentException("Invalid or non-supported elf of Ridge Racer V provided.");
            }

            _toc = toc;
        }

        public void Unpack() {
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
        }
    }
}
