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
using System.Drawing;
using System.Drawing.Imaging;

namespace HttpClientNASA
{

    public class Program
    {
        // API for retrieving images from NASA
        private const string url = "https://api.nasa.gov/mars-photos/api/v1/rovers/curiosity/photos?earth_date=earthDate&api_key=apiKey";
       
        // API Key generated
        private const string apiKey = "DEMO_KEY";

        // function in getting user input
        public static string getDateInput ()
        {
            // Get user input
            Console.WriteLine("Enter date (MM/DD/YYYY): ");

            // return the date inputted by user
            return Console.ReadLine();
        }

        public static void Main(string[] args)
        {

           // Process start goto
            ProcessStart:

            // Call get date input function
            string getDate = getDateInput();
           
            DateTime output;

            // Check if date inputted is a valid date
            if (DateTime.TryParseExact(getDate, "MM/dd/yyyy", System.Globalization.DateTimeFormatInfo.InvariantInfo,
                                          System.Globalization.DateTimeStyles.AdjustToUniversal, out output))
            {

                // get file directory where images will be saved
                string fileDirectory = CreateDesktopFolder(output.ToString("yyyy-MM-dd"));

                // image counter.
                // Default value to 1 in case user already downloaded the images before
                int count = 1;


                // variable where image filepath will be saved
                List<string> image_url_local = new List<string>();

                // Check if file directory already existed.
                // This will determine if user already downloaded the images
                // Prevent duplicate
                // If not
                if (!(fileDirectory.Split('\\').Last()).Equals("exist"))
                {
                    // counter revert to 0 since it is user's first time to download the images
                    count = 0;

                    // Call function and pass API URL as parameter. Replace API Parameters with corresponding values.
                    string details = CallRestMethod(url.Replace("earthDate", output.ToString("yyyy-MM-dd")).Replace("apiKey", apiKey));

                    // Deserialize HTTP response as JSON object
                    dynamic result = JsonConvert.DeserializeObject<dynamic>(details);

                    // Get all images inside the return response
                    foreach (var item in result.photos)
                    {
                        // Image source stored in a variable string
                        string url = item.img_src.ToString();

                        // get image file name from the image source
                        string filename = url.Split('/').Last();

                        // configure filepath with file name
                        string filepath = fileDirectory + filename;

                        // print out filepath of each downloaded images (for verification only. can be removed)
                        Console.WriteLine(filepath);

                        // Add filepath on the list
                        image_url_local.Add(filepath);

                        // Call function to download image and save to the created directory
                        SaveImage((item.img_src).ToString(), filepath);

                        // increment counter for each saved image
                        count++;

                    }

                    //Call functionn  to create html file
                    CreateHTMLFile(image_url_local, output.ToString("yyyy-MM-dd"));
                } 
                // if directory already exist, images already downloaded
                else
                {
                    // Remove exist from file directory path
                    // This is to prepare if user wants to view the images in browser
                    fileDirectory = fileDirectory.Replace("exist", "");
                }

                // Check if there is retrieved image
                if (count > 0)
                {

                    // goto point
                    viewImage:

                    // Prompt user for viewing of images
                    Console.WriteLine("Do you want to view the images? (Y or N)");
                    string answer = Console.ReadLine();

                    // If user wants to view image
                    if (string.Equals(answer, "Y", StringComparison.OrdinalIgnoreCase) || string.Equals(answer, "y", StringComparison.OrdinalIgnoreCase))
                    {
                        // show all downloaded images on browser
                         OpenHTMLFile(fileDirectory+output.ToString("yyyy-MM-dd")+".htm");
                    }

                    // if user answered N, end process
                    else if (string.Equals(answer, "N", StringComparison.OrdinalIgnoreCase) || string.Equals(answer, "n", StringComparison.OrdinalIgnoreCase))
                    {
                        // End process
                        System.Environment.Exit(0);
                    }
                    else
                    {

                        // Catch wrong user input
                        Console.WriteLine("Wrong Input!");
                        // ask user again
                        goto viewImage;
                    }
                } 
                else
                {

                    // Prompt user that there is no existing image on the specified date
                    Console.WriteLine("No image on specific date!");
                }
            }
            else
            {
                //Print Error Message
                Console.WriteLine("Not A Valid Date!!");

                // Ask user to input valid date again
                goto ProcessStart;
            }
          
        }

        // Creation of necessary directory
        public static string CreateDesktopFolder (string folderDate)
        {
            // get desktop path. adding the default folder name
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\NASA";

            // Check if path directory already exist on user desktop
            // If not
            if (!Directory.Exists(path))
            {
                // Create the given directory
                DirectoryInfo di = Directory.CreateDirectory(path);
            }

            // add the user input date to the end of the path for creation of date folder where images will be save
            // this will also separate each images by date it was taken
            path += "\\" + folderDate;

            // check if directory already exist
            // If not
            if (!Directory.Exists(path))
            {
                // Create necessary directory
                DirectoryInfo di = Directory.CreateDirectory(path);

                // add "\" at the end of the path to complete the file directory
                path += "\\";
            }
            else
            {
                // add exist if directory existed
                path += "\\exist";
            }

            // return path address
            return path;
        }
        // Function to call REST API
        public static string CallRestMethod(string url)
        {
            // create a webrequest on the given url
            HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(url);

            // "GET" for getting data using the API
            webrequest.Method = "GET";

            //Content is set to JSON 
            webrequest.ContentType = "application/json";

            // get the response
            HttpWebResponse webresponse = (HttpWebResponse)webrequest.GetResponse();
            Encoding enc = System.Text.Encoding.GetEncoding("utf-8");

            //gets the body of the response
            StreamReader responseStream = new StreamReader(webresponse.GetResponseStream(), enc);
            string result = string.Empty;

            // Insert the response to the result string
            result = responseStream.ReadToEnd();
            webresponse.Close();
            return result;
        }

        // Function to download image
        public static void SaveImage(string imageUrl, string filename)
        {
            // define webclient
            using (WebClient client = new WebClient())
            {
                // download file from give url and save to thr
                client.DownloadFile(new Uri(imageUrl), filename);
            }
        }

        // Function in creating HTML file
        public static void CreateHTMLFile(List<string> image_url, string fileDate)
        {
            // define filename of html
            string filepath = "C:\\Users\\jayed\\OneDrive\\Desktop\\NASA\\" + fileDate + "\\" + fileDate + ".htm";

            // create html
            string html = "<html> <head> <title>MARS ROVER PHOTO</title> </ head> ";

            html += "<body> <h1> NASA MARS PHOTO </h1> ";

            // add the date of the photos taken
            html += "<p><h2>" + fileDate + "</h2></p>";

            // get each images on the list
            foreach (string url in image_url)
            {
                // insert image path to the html
                html += "<p><img src=\"" + url + "\" width=150px></p>";
            }

            html += "</body>";
            
            // create html file and save on the designated directory
            File.WriteAllText(filepath, html);

            
        }

        // Function in opening the html file that contains all images
        public static void OpenHTMLFile(string filepath)
        {
            // define new process
            var p = new Process();

            // use to open html
            p.StartInfo = new ProcessStartInfo(filepath)
            {
                UseShellExecute = true
            };
            p.Start();
        }

    }
}




