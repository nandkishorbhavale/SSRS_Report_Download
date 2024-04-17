using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SSRSReportDownloader
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string reportServerUrl = "http://nandkishor/ReportServer?/ProductionReport&rs:Command=Render";
            string folderPath = "C:\\D Drive\\SSRS Reports\\ReportDownload\\DownloadedReports";

            // Delete all files in the directory before downloading the report
            DirectoryInfo di = new DirectoryInfo(folderPath);
            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            Console.WriteLine($"All files in {folderPath} have been deleted.");

            DateTime currentDate = DateTime.Now;
            string shift = GetShift(currentDate);

            byte[] reportBytes = await DownloadReport(reportServerUrl, currentDate, shift);

            if (reportBytes != null && reportBytes.Length > 0)
            {
                string filePath = Path.Combine(folderPath, $"Report_{currentDate:yyyyMMdd}_{shift}.pdf");
                File.WriteAllBytes(filePath, reportBytes);
                Console.WriteLine($"Report downloaded and saved to: {filePath}");
            }
            else
            {
                Console.WriteLine("Failed to download the report.");
            }
        }

        static async Task<byte[]> DownloadReport(string reportServerUrl, DateTime currentDate, string shift)
        {
            using (HttpClientHandler handler = new HttpClientHandler())
            {
                handler.UseDefaultCredentials = true;

                using (HttpClient client = new HttpClient(handler))
                {
                    string reportUrl = $"{reportServerUrl}&rs:Format=PDF&ReportDate={currentDate:yyyy-MM-dd}&Shift={shift}";

                    try
                    {
                        HttpResponseMessage response = await client.GetAsync(reportUrl);
                        if (response.IsSuccessStatusCode)
                        {
                            if (response.Content.Headers.ContentType.MediaType != "application/pdf")
                            {
                                Console.WriteLine("The response content is not a PDF file.");
                                return Array.Empty<byte>();
                            }
                            return await response.Content.ReadAsByteArrayAsync();
                        }
                        else
                        {
                            Console.WriteLine($"Failed to download report. Status code: {response.StatusCode}");
                            return Array.Empty<byte>(); // Return an empty byte array in case of failure
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error downloading report: {ex.Message}");
                        return Array.Empty<byte>(); // Return an empty byte array on exception
                    }
                }
            }
        }

        static string GetShift(DateTime currentTime)
        {
            if (currentTime.TimeOfDay >= new TimeSpan(6, 30, 0) && currentTime.TimeOfDay < new TimeSpan(15, 45, 0))
            {
                return "SHIFT A";
            }
            else if (currentTime.TimeOfDay >= new TimeSpan(15, 45, 0) && currentTime.TimeOfDay < new TimeSpan(23, 59, 0))
            {
                return "SHIFT B";
            }
            else if (currentTime.TimeOfDay >= new TimeSpan(00, 00, 0) && currentTime.TimeOfDay < new TimeSpan(00, 45, 0))
            {
                return "SHIFT B";
            }
            else
            {
                return "SHIFT C";
            }
        }
    }
}
