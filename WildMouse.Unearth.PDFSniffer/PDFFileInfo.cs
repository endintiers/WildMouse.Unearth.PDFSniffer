using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WildMouse.Unearth.PDFSniffer
{
    public class PDFFileInfo
    {
        public PDFFileInfo()
        {
            ErrorMessages = new List<string>();
            Pages = new List<PDFPageInfo>();
        }
        public List<string> ErrorMessages { get; set; }
        public char PDFVersion { get; set; }
        public DateTime CreationDate { get; set; }
        public string Creator { get; set; }
        public DateTime ModDate { get; set; }
        public string Producer { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public int PageCount { get; set; }
        public int TextCharacters { get; set; }
        public int ImageCount { get; set; }
        public long ImageBytes { get; set; }
        public List<PDFPageInfo> Pages { get; set; }

    }

    public class PDFPageInfo
    {
        public PDFPageInfo()
        {
            Images = new List<PDFImageInfo>();
        }
        public int PageNum { get; set; }
        public int TextCharacters { get; set; }
        public int ImageCount { get; set; }
        public long ImageBytes { get; set; }
        public List<PDFImageInfo> Images { get; set; }
    }

    public class PDFImageInfo
    {
        public int ImageNum { get; set; }
        public long ImageBytes { get; set; }
        public string ImageFormat { get; set; }
        public string ImageType { get; set; }
        public string ImageInfo { get; set; }
        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }
        public int ImageResolution { get; set; }
    }
}
