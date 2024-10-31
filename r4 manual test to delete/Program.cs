using MimeKit;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using MailKit;
using MailKit.Net.Smtp;

namespace r4_manual_test_to_delete
{
    internal class Program
    {
        const string mapsLink = "https://www.google.com/maps/place/";
        static async Task Main(string[] args)
        {
            InternetAddressList list = new InternetAddressList();
            list.Add(new MailboxAddress("LukaszZ", "zajaclukasz64@gmail.com"));


            // The URL to fetch the XML data
            string url = "https://mapy.geoportal.gov.pl/wss/service/sdi/LPR/get?SERVICE=WMS&request=GetFeatureInfo&version=1.1.1&layers=Bazy_HEMS&styles=&srs=EPSG:2180&bbox=25711.851198035758,148320.20997375305,1041713.8832020999,431954.11057488766&width=1920&height=536&format=image/png&transparent=true&query_layers=Bazy_HEMS&x=900&y=318&INFO_FORMAT=text/xml";

            // this needs to be changed later on. It's like this so nobody gets an email once the application starts running.
            string compareStatus = "1 - zespól w gotowosci";

            // all the strings in one line for readability
            string airport = "", latitude = "", longitude = "", latitudeVia = "", longitudeVia = "", statusColor = "", status = "";


            while (true)
            {
                using (HttpClient client = new HttpClient())
                {
                    try
                    {
                        // a GET request from teh URL
                        HttpResponseMessage response = await client.GetAsync(url);

                        // self explanatory
                        response.EnsureSuccessStatusCode();

                        // read an XML file
                        string xmlContent = await response.Content.ReadAsStringAsync();

                        // the xml file is read as a document
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml(xmlContent);

                        // namespaces for an XML
                        XmlNamespaceManager namespaceManager = new XmlNamespaceManager(xmlDoc.NameTable);

                        namespaceManager.AddNamespace("gml", "http://www.opengis.net/gml");
                        namespaceManager.AddNamespace("default", "http://www.intergraph.com/geomedia/gml");

                        // define XPath queries to extract the required attributes
                        string airportXPath = "//default:Attribute[@Name='AIRPORT']";
                        string latitudeXPath = "//default:Attribute[@Name='DESTINATIONLAT']";
                        string longitudeXPath = "//default:Attribute[@Name='DESTINATIONLON']";
                        string latitudeViaXPath = "//default:Attribute[@Name='DESTINATIONLATVIA']"; // these two are responsible for inter-hospital transport
                        string longitudeViaXPath = "//default:Attribute[@Name='DESTINATIONLONVIA']";

                        // responsible for picking up current status along with a corresponding color
                        string statusColorXPath = "//default:Attribute[@Name='STATUS']";     // gets statuses from 1-6, indicating their colour
                        string statusXPath = "//default:Attribute[@Name='HEMSSTATUSDESCRIPTION']";



                        // nodes from the namespace manager
                        XmlNode airportNode = xmlDoc.SelectSingleNode(airportXPath, namespaceManager);
                        XmlNode latitudeNode = xmlDoc.SelectSingleNode(latitudeXPath, namespaceManager);
                        XmlNode longitudeNode = xmlDoc.SelectSingleNode(longitudeXPath, namespaceManager);
                        XmlNode latitudeViaNode = xmlDoc.SelectSingleNode(latitudeViaXPath, namespaceManager);
                        XmlNode longitudeViaNode = xmlDoc.SelectSingleNode(longitudeViaXPath, namespaceManager);
                        XmlNode statusColorNode = xmlDoc.SelectSingleNode(statusColorXPath, namespaceManager);      //status 1 - 10, indicates colour
                        XmlNode statusNode = xmlDoc.SelectSingleNode(statusXPath, namespaceManager);
                        ;
                        // Get the values or default to "N/A" if not found
                        airport = airportNode?.InnerText ?? "N/A";
                        latitude = latitudeNode?.InnerText ?? "N/A";
                        longitude = longitudeNode?.InnerText ?? "N/A";
                        latitudeVia = latitudeViaNode?.InnerText ?? "N/A";
                        longitudeVia = longitudeViaNode?.InnerText ?? "N/A";
                        statusColor = statusColorNode?.InnerText ?? "N/A";
                        status = statusNode?.InnerText ?? "N/A";


                        // Display the data in the console
                        Console.WriteLine($"Airport: {airport}");
                        Console.WriteLine($"Coordinates: {latitude}, {longitude}");
                        Console.WriteLine($"Via: {latitudeVia}, {longitudeVia}");
                        Console.Write($"Status: ");

                        ChangeColor(statusColor, status);
                    }
                    catch (Exception ex)
                    {
                        // Handle any errors (e.g., network issues, invalid XML)
                        Console.WriteLine($"Error: {ex.Message}");
                        Console.ForegroundColor = ConsoleColor.Red;
                    }
                    SendEmail("zajaclukasz64@gmail.com", status, compareStatus, list, latitude, longitude, latitudeVia, longitudeVia);        // checks if compareStatus is equal to status
                    compareStatus = status; // 
                    RestartMessage(15); // after 15 seconds, it runs the code again. In this time, the status might change, thus, the SendEmail function checks it and assigns the value again. If the value is the same, the code continues. If it changes, it sends an email and THEN assigns the value again.

                    Console.Clear();
                    continue;
                }
            }

        }
        // changes color of the string. Make it a helper class
        public static void ChangeColor(string statusColor, string status)
        {
            switch (statusColor)
            {
                case "1":
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(status);
                    Console.ResetColor();
                    break;
                case "2":
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(status);
                    Console.ResetColor();
                    break;
                case "3":
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(status);
                    Console.ResetColor();
                    break;
                case "4":
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.Write(status);
                    Console.ResetColor();
                    break;
                case "5":
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(status);
                    Console.ResetColor();
                    break;
                case "6":
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(status);
                    Console.ResetColor();
                    break;
                case "7":
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.Write(status);
                    Console.ResetColor();
                    break;
                case "10":
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("dyżur zakończony");
                    Console.ResetColor();
                    break;

            }
        }
        // sets the restart message with a timer
        public static void RestartMessage(int seconds)
        {
            for (int i = seconds; i > 0; i--)
            {
                SetCursorPosition(0, 3);
                Console.WriteLine($"Restarting in {i} seconds!\n");
                Thread.Sleep(1000);
            }
        }
        // sets the cursor position so the timer displays in the same line instead of creating new ones
        public static void SetCursorPosition(int x, int y)
        {
            x = 0;  //left to right
            y = 5;  // top to bottom
            Console.SetCursorPosition(x, y);
        }

        public static void SendEmail(string receiver, string status, string compareStatus, InternetAddressList list, string latitude, string longitude, string latitudeVia, string longitudeVia)
        {
            var email = new MimeMessage();
            // holds a google maps constructed link for everything but an inter-hospital transport
            string link = $"{mapsLink}{latitude},{longitude}";
            string subject = status;
            string body = $@"<a href=""{link}"">Google Maps</a>";

            // if one of these is not zero, it means that this is an inter-hospital transport. Sadly, it's glitched on the official site and it doesn't have a proper status (1-10).
            // holds a link and body for inter-hospital transport
            if (latitudeVia != "0" || longitudeVia != "0")
            {
                subject = "Transport międzyszpitalny!";
                link = $"{mapsLink}{latitudeVia},{longitude}";
                body = $@"<a href=""{link}"">Google Maps</a>";
            }


            email.From.Add(new MailboxAddress("Ratownik 4", "ratownikwakcji@onet.pl"));
            email.To.AddRange(list);

            email.Subject = $"{subject}";

            // checks if the status changed

            if (compareStatus != status)        // compare status takes the last value. If the value changes with next iteration, it triggers the inequality, sends the email and changes the compareStatus to the same value as status.
            {
                email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                {
                    Text = body  // the body is the status. It is a string from above.
                };
                using (var smtp = new SmtpClient())
                {
                    smtp.Connect("smtp.poczta.onet.pl", 465, true);

                    // Note: only needed if the SMTP server requires authentication
                    smtp.Authenticate("ratownikwakcji@onet.pl", "my_password");

                    smtp.Send(email);
                    smtp.Disconnect(true);

                    Console.Write($"----- Email sent at ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write($"{DateTime.Now}");
                    Console.ResetColor();
                    Console.Write(" -----");
                }

            }

        }
    }
}
