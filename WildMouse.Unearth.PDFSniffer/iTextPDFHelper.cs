using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Text;

namespace WildMouse.Unearth.PDFSniffer
{
    public static class iTextPDFHelper
    {
        private static ImageConverter _imageConverter = new ImageConverter();

        public static PDFFileInfo GetPDFMetaData(Stream pdfStream, string filePath, bool deepinfo)
        {
            var fileInfo = new PDFFileInfo();
            fileInfo.FilePath = filePath;
            fileInfo.FileName = System.IO.Path.GetFileName(filePath);
            if (pdfStream.Length == 0)
            {
                fileInfo.ErrorMessages.Add("Zero length file");
                return fileInfo;
            }

            try
            {
                pdfStream.Position = 0; // Ensure that we are at the start
            }
            catch (NotSupportedException) { }

            try
            {
                // Note: PdfReader Dispose closes the stream...
                using (PdfReader reader = new PdfReader(pdfStream))
                {
                    fileInfo.PDFVersion = reader.PdfVersion;
                    fileInfo.PageCount = reader.NumberOfPages;
                    fileInfo.FileSize = reader.FileLength;
                    if (reader.Info != null)
                    {
                        try
                        {
                            if (reader.Info.ContainsKey("CreationDate"))
                                fileInfo.CreationDate = PdfDate.Decode(reader.Info["CreationDate"]);
                            if (reader.Info.ContainsKey("ModDate"))
                                fileInfo.ModDate = PdfDate.Decode(reader.Info["ModDate"]);
                        }
                        catch(Exception ex)
                        {
                            fileInfo.ErrorMessages.Add($"PdfDate Decode {ex.Message.Replace(',',' ')}");
                        }
                        if (reader.Info.ContainsKey("Creator"))
                            fileInfo.Creator = reader.Info["Creator"];
                        if (reader.Info.ContainsKey("Producer"))
                            fileInfo.Producer = reader.Info["Producer"];
                    }

                    if (deepinfo)
                    {
                        var parser = new PdfReaderContentParser(reader);
                        ImageRenderListener listener = null;

                        for (var i = 1; i <= reader.NumberOfPages; i++)
                        {
                            var page = new PDFPageInfo();
                            try
                            {
                                parser.ProcessContent(i, (listener = new ImageRenderListener(fileInfo.ErrorMessages)));
                            }
                            catch (Exception ex)
                            {
                                fileInfo.ErrorMessages.Add($"Page {i} Image Processing Exception: {ex.Message.Replace(',', ' ')}");
                            }

                            try
                            {
                                var pageText = PdfTextExtractor.GetTextFromPage(reader, i, new SimpleTextExtractionStrategy());
                                page.TextCharacters += pageText.Length;
                            }
                            catch (System.ArgumentException ex)
                            {
                                fileInfo.ErrorMessages.Add($"Page {i} Text Extraction Exception {ex.Message.Replace(',', ' ')}");
                            }

                            page.PageNum = i;
                            page.ImageCount = listener.Images.Count;
                            page.Images = listener.Images;
                            for (int j = 0; j < page.ImageCount; j++)
                            {
                                page.Images[j].ImageNum = j + 1;
                                page.ImageBytes += page.Images[j].ImageBytes;
                            }

                            fileInfo.Pages.Add(page);
                            fileInfo.ImageCount += page.ImageCount;
                            fileInfo.ImageBytes += page.ImageBytes;
                            fileInfo.TextCharacters += page.TextCharacters;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                fileInfo.ErrorMessages.Add(ex.Message);
            }

            return fileInfo;
        }

        internal class ImageRenderListener : IRenderListener
        {
            private List<string> _log;

            public List<PDFImageInfo> Images { get; set; }

            public ImageRenderListener(List<string> log)
            {
                _log = log;
                Images = new List<PDFImageInfo>();
            }

            public void RenderImage(ImageRenderInfo info)
            {
                PdfImageObject image = info.GetImage();
                var fileType = image.GetFileType();
                var imgBytes = image.GetImageAsBytes();
                var imgDict = image.GetDictionary();
                var imgInfo = "Unknown";
                var filter = image.Get(PdfName.FILTER);
                if (filter != null)
                    imgInfo = filter.ToString().Replace(',', ' ');

                var ctm = info.GetImageCTM();
                var ctmWidth = ctm[Matrix.I11];
                var ctmHeight = ctm[Matrix.I22];

                int imgWidth = -1;
                int imgHeight = -1;
                int imgResolution = -1;
                PixelFormat imgFormat = PixelFormat.Undefined;
                if (imgInfo != "/JBIG2Decode" && imgInfo != "/JPXDecode")
                {
                    var img = image.GetDrawingImage();
                    imgWidth = img.Width;
                    imgHeight = img.Height;
                    imgFormat = img.PixelFormat;
                    imgResolution = Convert.ToInt32(img.VerticalResolution);
                    img.Dispose();
                }

                Images.Add(new PDFImageInfo() { ImageBytes = imgBytes.Length, ImageFormat = imgFormat.ToString(),
                    ImageHeight = imgHeight, ImageWidth = imgWidth, ImageResolution = imgResolution,
                    ImageInfo = imgInfo, ImageType = fileType });
            }

            public void BeginTextBlock() { }
            public void EndTextBlock() { }
            public void RenderText(TextRenderInfo renderInfo) { }

        }
    }
}
