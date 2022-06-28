// See https://aka.ms/new-console-template for more information

using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace HttpClientNASA
{

    class Program
    {
        // API for retrieving images from NASA
        private const string url = "https://api.nasa.gov/mars-photos/api/v1/rovers/curiosity/photos?earth_date=earthDate&api_key=apiKey";
       
        // API Key generated
        private const string apiKey = "DEMO_KEY";

        public static void Main(string[] args)
        {

            // Get user input
            Console.WriteLine("Enter date (MM/DD/YYYY): ");
            string getDate = Console.ReadLine();
           
            DateTime output;

            // Check if date inputted is a valid date
            if (DateTime.TryParseExact(getDate, "MM/dd/yyyy", System.Globalization.DateTimeFormatInfo.InvariantInfo,
                                          System.Globalization.DateTimeStyles.AdjustToUniversal, out output))
            {
                // Call function and pass API URL as parameter. Replace API Parameters with corresponding values.
                string details = CallRestMethod(url.Replace("earthDate", output.ToString("yyyy-MM-dd")).Replace("apiKey", apiKey));

                // Deserialize HTTP response as JSON object
                dynamic result = JsonConvert.DeserializeObject<dynamic>(details);

                int count = 0;
                string[] image_url = new string[1000];
                // Get all images inside the return response
                foreach (var item in result.photos)
                {

                    image_url[count] = item.img_src;
                    count++;
                }

                if (count > 0)
                {
                    Console.WriteLine("Do you want to view the images? (Y or N)");
                    string answer = Console.ReadLine();

                    if (string.Equals(answer, "Y", StringComparison.OrdinalIgnoreCase) || string.Equals(answer, "y", StringComparison.OrdinalIgnoreCase))
                    {
                        int imageCounter = 0;

                        do
                        {
                            for(int i = imageCounter; i < 5; i++, imageCounter++)
                            {
                                OpenBrowser(image_url[i]);
                            }

                            if (count - imageCounter > 0)
                            {
                                Console.WriteLine("{0} images remaining. Do you want to view next images? (Y or N)", count - imageCounter);
                                answer = Console.ReadLine();   
                            }
                        } while (string.Equals(answer, "Y", StringComparison.OrdinalIgnoreCase) || string.Equals(answer, "y", StringComparison.OrdinalIgnoreCase));
                    } 
                } else
                {
                    Console.WriteLine("No image on specific date!");
                }

                
            }
            else
            {
                //Print Error Message
                Console.WriteLine("Not A Valid Date!!");
            }
          
        }

        // Function to call REST API
        public static string CallRestMethod(string url)
        {
            HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(url);
            webrequest.Method = "GET";
            webrequest.ContentType = "application/json";
            HttpWebResponse webresponse = (HttpWebResponse)webrequest.GetResponse();
            Encoding enc = System.Text.Encoding.GetEncoding("utf-8");
            StreamReader responseStream = new StreamReader(webresponse.GetResponseStream(), enc);
            string result = string.Empty;
            result = responseStream.ReadToEnd();
            webresponse.Close();
            return result;
        }

        public static void OpenBrowser(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}




