using System;
using System.CommandLine;
using System.IO;
using System.Linq;
using ImageMagick;

class Program
{
    static int Main(string[] args)
    {
        var input = new Option<string>(
                "--input",
                description: "Input HEIC file or directory");
        var output = new Option<string>(
                           "--output",
                description: "Output JPG file or directory");
        var quality = new Option<int>(
                "--quality",
                getDefaultValue: () => 100,
                description: "JPEG quality (1-100)");

        var rootCommand = new RootCommand();
        rootCommand.AddOption(input);
        rootCommand.AddOption(output);
        rootCommand.AddOption(quality);


        rootCommand.SetHandler((inputFolder, outputFolder, qualityOfJpg) =>
        {
            ConvertHeicToJpg(inputFolder, outputFolder, qualityOfJpg);
        }, input, output, quality);
            
        return rootCommand.InvokeAsync(args).Result;
    }

    static void ConvertHeicToJpg(string input, string output, int quality)
    {
        if (string.IsNullOrEmpty(input))
        {
            Console.WriteLine("Error: Input path is required");
            return;
        }

        if (string.IsNullOrEmpty(output))
        {
            output = input;
        }

        if (quality < 1 || quality > 100)
        {
            Console.WriteLine("Error: Quality must be between 1 and 100");
            return;
        }

        try
        {
            if (Directory.Exists(input))
            {
                ProcessDirectory(input, output, quality);
            }
            else if (File.Exists(input))
            {
                ProcessFile(input, output, quality);
            }
            else
            {
                Console.WriteLine($"Error: Input path '{input}' does not exist");
                return;
            }

            Console.WriteLine("Conversion completed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    static void ProcessDirectory(string inputDir, string outputDir, int quality)
    {
        foreach (var file in Directory.EnumerateFiles(inputDir, "*.*", SearchOption.AllDirectories))
        {
            if (Path.GetExtension(file).ToLower() == ".heic")
            {

                string relativePath = GetRelativePath(inputDir, file);
                string outputPath = Path.Combine(outputDir, Path.ChangeExtension(relativePath, ".jpg"));
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
                ConvertFile(file, outputPath, quality);
            }
        }
    }

    public static string GetRelativePath(string relativeTo, string path)
    {
        var relativeToParts = relativeTo.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var pathParts = path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        int commonIndex = 0;
        while (commonIndex < relativeToParts.Length && commonIndex < pathParts.Length &&
               string.Equals(relativeToParts[commonIndex], pathParts[commonIndex], StringComparison.OrdinalIgnoreCase))
        {
            commonIndex++;
        }

        if (commonIndex == 0)
        {
            return path; // No common path, return the full path
        }

        var result = Enumerable.Repeat("..", relativeToParts.Length - commonIndex)
            .Concat(pathParts.Skip(commonIndex))
            .ToArray();

        return string.Join(Path.DirectorySeparatorChar.ToString(), result);
    }

    static void ProcessFile(string inputFile, string outputFile, int quality)
    {
        if (Directory.Exists(outputFile))
        {
            outputFile = Path.Combine(outputFile, Path.ChangeExtension(Path.GetFileName(inputFile), ".jpg"));
        }
        else if (string.IsNullOrEmpty(Path.GetExtension(outputFile)))
        {
            outputFile = Path.ChangeExtension(outputFile, ".jpg");
        }

        Directory.CreateDirectory(Path.GetDirectoryName(outputFile));
        ConvertFile(inputFile, outputFile, quality);
    }

    static void ConvertFile(string inputPath, string outputPath, int quality)
    {
        try
        {
            using (var image = new MagickImage(inputPath))
            {
                image.Quality = quality;
                image.Write(outputPath);
            }
            Console.WriteLine($"Converted {inputPath} to {outputPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error converting {inputPath}: {ex.Message}");
        }
    }
}
