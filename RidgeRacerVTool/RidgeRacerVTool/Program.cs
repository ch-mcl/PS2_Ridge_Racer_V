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
            Console.WriteLine("Ridge Racer V Tool - by chmcl95");
            Console.WriteLine();

            Parser.Default.ParseArguments<UnpackVerbs, PatchVerbs>(args)
                .WithParsed<UnpackVerbs>(Unpack)
                .WithParsed<PatchVerbs>(Patch);
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

            Unpacker unpacker = new Unpacker(options.ElfPath, options.InputPath, options.OutputPath, options.GenerateHash);
            unpacker.Unpack();

            return;
        }

        public static void Patch(PatchVerbs options)
        {
            if (!File.Exists(options.ElfPath))
            {
                Console.WriteLine($"Provided ELF file '{options.ElfPath}' does not exist.");
                return;
            }

            if (!Directory.Exists(options.InputPath))
            {
                Console.WriteLine($"Provided R5.ALL or RRV_1 file '{options.InputPath}' does not exist.");
                return;
            }

            string fileDirectory = Path.GetDirectoryName(options.ElfPath);
            string outputPath = $@"{fileDirectory}\patched";
            int paddingSize = 0x00;
            if(!string.IsNullOrEmpty(options.PaddingSize) && !int.TryParse(options.PaddingSize, out paddingSize))
            {
                Console.WriteLine($"Invalid pad option {options.PaddingSize}.");
                return;
            }

            Pactcher patcher = new Pactcher(options.ElfPath, options.InputPath, outputPath, paddingSize);
            patcher.Patch();

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

        // is this needs ???
        [Option('o', "output", Required = true, HelpText = "Output directory for the extracted files.")]
        public string OutputPath { get; set; }

        [Option('h', "hash", Required = false, HelpText = "Generate list of Hash value files.")]
        public bool GenerateHash { get; set; }

    }

    [Verb("patch", HelpText = "Packs R5.ALL(PS2) / RRV1_A(SYSTEM246). Also patching elf file. Those files are generats in \"patched\" folder.")]
    public class PatchVerbs
    {
        [Option('i', "input", Required = true, HelpText = "Input Directry. Need extracted R5.ALL files.")]
        public string InputPath { get; set; }

        [Option('e', "elf-path", Required = true, HelpText = "Input elf file. Example: SLUS_200.02.")]
        public string ElfPath { get; set; }

        [Option("pad", Required = false, HelpText = "Padding for R5.All file.")]
        public string PaddingSize { get; set; }

    }

}
