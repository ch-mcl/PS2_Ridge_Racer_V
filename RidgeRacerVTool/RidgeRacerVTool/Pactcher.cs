using System;
using System.Collections.Generic;
using System.IO;
using RidgeRacerVTool.RR5_Lib;
using RidgeRacerVTool.TOC;

namespace RidgeRacerVTool
{
    class Pactcher
    {
        private string _elfPath;
        private string _inputPath;
        private string _destPath;
        private int _paddingSize;

        private TOCInformation _toc;

        public Pactcher(string elfPath, string inputPath, string outputPath, int paddingSize)
        {
            _elfPath = elfPath;
            _inputPath = inputPath;
            _destPath = outputPath;
            _paddingSize = paddingSize;

            string elfName = Path.GetFileName(_elfPath);

            if (!TableOfContents.TOCInfos.TryGetValue(elfName, out TOCInformation toc))
            {
                throw new ArgumentException("Invalid or non-supported elf of Ridge Racer V provided.");
            }

            _toc = toc;
        }

        public void Patch()
        {
            Console.WriteLine("Starting to patch...");

            List<string> fileSortList = new List<string>(Directory.GetFiles(_inputPath, "*", SearchOption.AllDirectories));
            if (fileSortList.Count != _toc.FileCount)
            {
                Console.WriteLine($"Faild: Unmaching region and file counts. Detected: {fileSortList.Count} files. Except: {_toc.FileCount} files.");
                return;
            }

            string elfPathMod = $@"{_destPath}\{Path.GetFileName(_elfPath)}";
            string arcPathMod = $@"{_destPath}\{_toc.ArcName}";

            Directory.CreateDirectory(_destPath);

            // duplicate elf file
            using (FileStream elfFileStream = new FileStream(_elfPath, FileMode.Open, FileAccess.Read))
            using (FileStream elfFileStreamMod = new FileStream(elfPathMod, FileMode.Create, FileAccess.Write))
            {
                elfFileStream.CopyTo(elfFileStreamMod);
            }

            // mod elf file
            using (FileStream elfFileStreamMod = new FileStream(elfPathMod, FileMode.Open, FileAccess.ReadWrite))
            // mod arc file
            using (FileStream arcFileStreamMod = new FileStream(arcPathMod, FileMode.Create, FileAccess.Write))
            {
                byte[] bytes = new byte[1];

                // Goto TOC Address
                elfFileStreamMod.Seek(_toc.TocAddress, SeekOrigin.Begin);
                foreach (string srcFilePath in fileSortList)
                {
                    FileDescriptor descriptor = new FileDescriptor();
                    using (FileStream srcFileStream = new FileStream(srcFilePath, FileMode.Open, FileAccess.Read))
                    {
                        if (arcFileStreamMod.Position > 1)
                        {
                            descriptor.blockOffset = (int)arcFileStreamMod.Position / 0x800;
                        }

                        // Write TOC
                        descriptor.Pack(srcFileStream, elfFileStreamMod);

                        bytes = new byte[descriptor.compressedSize];
                        srcFileStream.Read(bytes, 0x00, descriptor.compressedSize);
                    }
                    arcFileStreamMod.Write(bytes, 0x00, descriptor.compressedSize);

                    // padding
                    bytes = new byte[1];
                    bytes[0] = 0x00;
                    int padding = (descriptor.blockSize * 0x800) - (descriptor.compressedSize);
                    for (int i = 0; i < padding; i++)
                    {
                        arcFileStreamMod.Write(bytes, 0x00, bytes.Length);
                    }
                }

                // check teminator of TOC
                bytes = new byte[4];
                elfFileStreamMod.Read(bytes, 0x00, bytes.Length);
                int terminator = BitConverter.ToInt32(bytes, 0x00);
                if (terminator != 0xCC0000)
                {
                    int expectEndAdr = _toc.TocAddress + _toc.FileCount * 4;
                    Console.WriteLine(String.Format("Faild: Patch. unexpected terminator. FilePosition: {0:X8} files. Except: {1:D8}.", elfFileStreamMod.Position, expectEndAdr));
                    return;
                }

                //padding
                bytes = new byte[1];
                bytes[0] = 0x00;
                for (int i = 0; i < _paddingSize; i++)
                {
                    arcFileStreamMod.Write(bytes, 0x00, bytes.Length);
                }

                Console.WriteLine("Done.");
                return;
            }

        }
    }
}
