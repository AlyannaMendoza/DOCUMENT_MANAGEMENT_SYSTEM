using System;
using System.Diagnostics;
using System.IO;

namespace OCR_TRIAL.Service
{
    public interface IFileService
    {
        byte[] GhostscriptPdfCompression(string filePath);
        //byte[] ImageSharpCompression(Stream imagestream, int maxWidth, int maxHeight, int quality);
    }

    public class FileService : IFileService
    {
        //Pdf compression using Ghostscript
        public byte[] GhostscriptPdfCompression(string filePath)
        {
            //Path of Ghostscript
            string ghostscriptPath = Path.Combine(Environment.CurrentDirectory, "gs10.03.1", "bin", "gswin64c.exe");

            string tempFilePath = Path.GetTempFileName();

            //Ghostscript command arguments
            string arguments = $"-q -dNOPAUSE -dBATCH -dSAFER -dSAFER -sDEVICE=pdfwrite" +
                            $"-dPDFSETTINGS=/screen" +
                            $"-dEmbedAllFonts=true -dSubsetFonts=true" +
                            $"-dColorImageDownsampleType=/Bicubic -dColorImageResolution=120" +     //Change numbers here to change resolution
                            $"-dGrayImageDownsampleType=/Bicubic -dGrayImageResolution=120" +        //Change numbers here to change resolution
                            $"-dMonoImageDownsampleType=/Bicubic -dMonoImageResolution=120" +    //Change numbers here to change resolution
                            $"-sOutputFile=\"{tempFilePath}\"\"{filePath}\"";

            //Start info process for Ghostscript
            var processStartInfo = new ProcessStartInfo
            {
                FileName = ghostscriptPath,
                Arguments = arguments,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try
            {
                //Start process
                using (var process = new Process { StartInfo = processStartInfo })
                {
                    process.Start();

                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    process.WaitForExit();

                    if (process.ExitCode != 0)
                    {
                        throw new Exception($"Ghostscript failed with error: {error}");
                    }

                    //Read the compressed PDF into a byte array
                    using (var fileStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read))
                    using (var memoryStream = new MemoryStream())
                    {
                        fileStream.CopyTo(memoryStream);
                        return memoryStream.ToArray();
                    }
                }
            }
            finally
            {
                // Clean up temporary files
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }
        }

        //Image Compression
        //public async Task<byte[]> ImageSharpCompressionAsync(Stream imageStream, int maxWidth, int maxHeight, int quality)
        //{
        //    return await Task.Run(() =>
        //    {
        //        imageStream.Position = 0;

        //        using (var image = Image.Load(imageStream))
        //        {
        //             Resize image
        //            image.Mutate(x => x.Resize(maxWidth, maxHeight));

        //             Compress and save to memory stream
        //            using (var outputStream = new MemoryStream())
        //            {
        //                var encoder = new JpegEncoder
        //                {
        //                    Quality = quality
        //                };
        //                image.Save(outputStream, encoder);

        //                return outputStream.ToArray();
        //            }
        //        }
        //    });
        //}
    }
}