using CommandLine;
using RidgeRacerVTool.TOC;
using System;
using System.IO;

namespace RidgeRacerVTool
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            Parser.Default.ParseArguments<UnpackVerbs>(args)
                .WithParsed<UnpackVerbs>(Unpack);
        }

        public static void Unpack(UnpackVerbs options)
        {
            if (!File.Exists(options.ElfPath))
            {
                Console.WriteLine($"Provided ELF file '{options.ElfPath}' does not exist.");
                return;
            }

            if (!File.Exists(options.InputPath))
            {
                Console.WriteLine($"Provided R5.ALL or RRV_1 file '{options.InputPath}' does not exist.");
                return;
            }

            string elfName = Path.GetFileName(options.ElfPath);

            var toc = new TableOfContents(elfName, options.ElfPath);
            //toc.

            return;
        }
    }

    [Verb("unpack", HelpText = "Unpacks R5.ALL(PS2) / RRV1_A(SYSTEM246)")]
    public class UnpackVerbs
    {
        [Option('i', "input", Required = true, HelpText = "Input .DAT file like R5.ALL.")]
        public string InputPath { get; set; }

        [Option('e', "elf-path", Required = true, HelpText = "Input elf file. Example: SLUS_200.02.")]
        public string ElfPath { get; set; }

        [Option('o', "output", Required = true, HelpText = "Output directory for the extracted files.")]
        public string OutputPath { get; set; }

    }

    //[Verb("pack", HelpText = "Packs R5.ALL(PS2) / RRV1_A(SYSTEM246). Also patching elf file.")]
    //public class PackArchiveVerbs
    //{
    //    [Option('i', "input", Required = true, HelpText = "Input .DAT file like RR7.DAT.")]
    //    public string InputPath { get; set; }

    //    [Option('e', "elf-path", Required = true, HelpText = "Input elf file. Example: SLUS_200.02.")]
    //    public string ElfPath { get; set; }

    //    [Option('o', "output", Required = true, HelpText = "Output directory for the packed files.")]
    //    public string OutputPath { get; set; }

    //}

}
