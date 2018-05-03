using NDesk.Options;
using NDesk.Options.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WildMouse.Unearth.PDFSniffer
{
    class Program
    {
        static void Main(string[] args)
        {
            var os = new OptionSet();
            var dir = os.AddVariable<string>("d|dir", "Directory to sniff, defaults to current");
            var csv = os.AddVariable<string>("c|csv", "FileSpec of csv output");
            var deepInfo = os.AddSwitch("i|inf", "Deep scan of pages and images. If csv specified outputs additional page and image files.");
            var showHelp = os.AddSwitch("h|help", "Show Help");
            os.Parse(args);

            if (showHelp.Enabled)
            {
                Console.WriteLine("Options:");
                os.WriteOptionDescriptions(Console.Out);
                return;
            }

            var directoryToScan = dir.Value ?? Directory.GetCurrentDirectory();
            if (!Directory.Exists(directoryToScan))
            {
                Console.WriteLine($"Error - Can't find directory {directoryToScan}");
                return;
            }

            string[] pdfFiles =
                Directory.GetFiles(directoryToScan, "*.pdf", SearchOption.AllDirectories);

            Console.WriteLine($"Found {pdfFiles.Length} PDF files");

            string csvFileDir = null;
            string csvFileName = null;
            string csvFileExt = null;

            string fileCsvFilePath = null;
            FileStream fileCsvFs = null;
            StreamWriter filesr = null;

            string piCsvFilePath = null;
            FileStream piCsvFs = null;
            StreamWriter pisr = null;

            string imCsvFilePath = null;
            FileStream imCsvFs = null;
            StreamWriter imsr = null;

            if (csv.Value != null)
            {
                csvFileDir = Path.GetDirectoryName(csv.Value);
                csvFileName = Path.GetFileNameWithoutExtension(csv.Value);
                csvFileExt = Path.GetExtension(csv.Value);

                fileCsvFilePath = csvFileDir + "\\" + csvFileName + csvFileExt;
                piCsvFilePath = csvFileDir + "\\" + csvFileName + "_pageinf" + csvFileExt;
                imCsvFilePath = csvFileDir + "\\" + csvFileName + "_imginf" + csvFileExt;

                fileCsvFs = new FileStream(fileCsvFilePath, FileMode.Create);
                filesr = new StreamWriter(fileCsvFs);
                filesr.WriteLine("FilePath, FileName, PDFVersion, CreationDate, Creator, ModDate, Producer, PageCount, FileSize, ImageCount, TextCharacters, ImageBytes, ErrorMessageCount, Errors");

                if (deepInfo.Enabled)
                {
                    piCsvFs = new FileStream(piCsvFilePath, FileMode.Create);
                    pisr = new StreamWriter(piCsvFs);
                    pisr.WriteLine("FilePath, PageNum, TextCharacters, ImageCount, ImageBytes ");
                    imCsvFs = new FileStream(imCsvFilePath, FileMode.Create);
                    imsr = new StreamWriter(imCsvFs);
                    imsr.WriteLine("FilePath, PageNum, ImageNum, ImageBytes, ImageFormat, ImageType, ImageInfo, ImageWidth, ImageHeight, ImageResolution");
                }
            }

            for (int i = 0; i < pdfFiles.Length; i++)
            {
                if (i > 0 && i % 100 == 0)
                {
                    Console.WriteLine($"Processed {i} Files");
                }

                var fileSpec = pdfFiles[i];
                using (var fs = File.OpenRead(fileSpec))
                {
                    var fileInfo = iTextPDFHelper.GetPDFMetaData(fs, fileSpec, deepInfo);
                    if (filesr != null)
                    {
                        var Errors = String.Join("|", fileInfo.ErrorMessages);
                        filesr.WriteLine($"\"{fileInfo.FilePath}\", {fileInfo.FileName.Replace(',',' ')}, {fileInfo.PDFVersion}, " +
                            $"{fileInfo.CreationDate}, {fileInfo.Creator}, {fileInfo.ModDate}, {fileInfo.Producer}, " +
                            $"{fileInfo.PageCount}, {fileInfo.FileSize}, {fileInfo.ImageCount}, " +
                            $"{fileInfo.TextCharacters}, {fileInfo.ImageBytes}, {fileInfo.ErrorMessages.Count}, {Errors}");

                        if (pisr != null)
                        {
                            foreach (var page in fileInfo.Pages)
                            {
                                pisr.WriteLine($"\"{fileInfo.FilePath}\", {page.PageNum}, {page.TextCharacters}, " +
                                    $"{page.ImageCount}, {page.ImageBytes}");
                            }
                        }
                        if (imsr != null)
                        {
                            foreach (var page in fileInfo.Pages)
                            {
                                foreach (var img in page.Images)
                                {
                                    imsr.WriteLine($"\"{fileInfo.FilePath}\", {page.PageNum}, {img.ImageNum}, " +
                                        $"{img.ImageBytes}, {img.ImageFormat}, {img.ImageType}, {img.ImageInfo}, " +
                                        $"{img.ImageWidth}, {img.ImageHeight}, {img.ImageResolution}");
                                }
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("---------------------------------------");
                        Console.WriteLine($"FilePath: {fileInfo.FilePath}");
                        Console.WriteLine($"FileName: {fileInfo.FileName}");
                        Console.WriteLine($"PDFVersion: {fileInfo.PDFVersion}");
                        Console.WriteLine($"CreationDate: {fileInfo.CreationDate}");
                        Console.WriteLine($"Creator: {fileInfo.Creator}");
                        Console.WriteLine($"ModDate: {fileInfo.ModDate}");
                        Console.WriteLine($"Producer: {fileInfo.Producer}");
                        Console.WriteLine($"PageCount: {fileInfo.PageCount}");
                        Console.WriteLine($"FileSize: {fileInfo.FileSize}");
                        Console.WriteLine($"ImageCount: {fileInfo.ImageCount}");
                        Console.WriteLine($"ImageBytes: {fileInfo.ImageBytes}");
                        Console.WriteLine($"TextCharacters: {fileInfo.TextCharacters}");
                        Console.WriteLine($"ErrorMessageCount: {fileInfo.ErrorMessages.Count}");
                        foreach(var msg in fileInfo.ErrorMessages)
                        {
                            Console.WriteLine("Error:" + msg);
                        }
                        Console.WriteLine();
                    }
                }
            }

            if (filesr != null)
                filesr.Close();
            if (fileCsvFs != null)
                fileCsvFs.Close();

            if (pisr != null)
                pisr.Close();
            if (piCsvFs != null)
                piCsvFs.Close();

            if (imsr != null)
                imsr.Close();
            if (imCsvFs != null)
                imCsvFs.Close();

            Console.WriteLine("Scan Completed");
        }
    }
}
