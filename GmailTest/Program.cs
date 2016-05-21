
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GmailTest
{
    class Program
    {
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/gmail-dotnet-quickstart.json
        static string[] Scopes = { GmailService.Scope.GmailReadonly };
        static string ApplicationName = "Gmail API .NET Quickstart";

        static void Main(string[] args)
        {
            UserCredential credential;

            using (var stream =
                new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = System.Environment.GetFolderPath(
                    System.Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials/gmail-dotnet-quickstart.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Gmail API service.
            var service = new GmailService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });

            UsersResource.LabelsResource.ListRequest request = service.Users.Labels.List("me");
            var res = service.Users.Messages;
            ListLabels(request);
            ListMessages(service, "me", "in after:2016/05/15");
        }

        public static void ListLabels(UsersResource.LabelsResource.ListRequest request)
        {
            IList<Label> labels = request.Execute().Labels;
            Console.WriteLine("Labels:");
            if (labels != null && labels.Count > 0)
            {
                foreach (var labelItem in labels)
                {
                    Console.WriteLine("{0}", labelItem.Name);
                }
            }
            else
            {
                Console.WriteLine("No labels found.");
            }
        }
        
        public static List<Message> ListMessages(GmailService service, String userId, String query)
        {
            List<Message> result = new List<Message>();
            UsersResource.MessagesResource.ListRequest request = service.Users.Messages.List(userId);
            request.Q = query;

            do
            {
                try
                {
                    ListMessagesResponse response = request.Execute();
                    result.AddRange(response.Messages);
                    foreach (var item in response.Messages)
                    {
                        var msg = GetMessage(service, userId, item.Id);
                        var headers = msg.Payload.Headers;
                        Console.WriteLine(GetHeader(headers, "From"));
                       // Console.WriteLine(GetHeader(headers, "To"));
                        Console.WriteLine(GetHeader(headers, "Date"));
                        Console.WriteLine(GetHeader(headers, "Subject"));

                        if (msg.Payload.MimeType == "text/plain")
                        {
                            byte[] data = Convert.FromBase64String(msg.Payload.Body.Data);
                            string decodedString = Encoding.UTF8.GetString(data);
                            Console.WriteLine(decodedString);
                        }
                    }
                    request.PageToken = response.NextPageToken;
                }
                catch (Exception e)
                {
                    Console.WriteLine("An error occurred: " + e.Message);
                }
            } while (!String.IsNullOrEmpty(request.PageToken));

            return result;
        }

        public static string GetHeader(IList<MessagePartHeader> headers,string search)
        {
            var data = "";
            var hdr = headers.Where(x => x.Name == search).FirstOrDefault();
            if (hdr != null)
            {
                data = hdr.Value;
            }
            return data;
        }

        public static Message GetMessage(GmailService service, String userId, String messageId)
        {
            try
            {
                return service.Users.Messages.Get(userId, messageId).Execute();
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
            }

            return null;
        }
    }
}

// UsersResource.MessagesResource.ListRequest emailreq = service.Users.Messages.List("me");

//emailreq.MaxResults = 100;

//IList<Message> curMsgs = emailreq.Execute().Messages;
//foreach (Message m in curMsgs)
//{
//    Console.WriteLine(m.Id);
//    //Console.WriteLine(m.InternalDate);
//    //if (m.Payload.MimeType == "text/plain")
//    //{
//    //    byte[] data = Convert.FromBase64String(m.Payload.Body.Data);
//    //    string decodedString = Encoding.UTF8.GetString(data);
//    //    Console.WriteLine(decodedString);
//    //}
//    //else
//    //{
//    //    IList<MessagePart> parts = m.Payload.Parts;
//    //    if (parts != null && parts.Count > 0)
//    //    {
//    //        foreach (MessagePart part in parts)
//    //        {
//    //            if (part.MimeType == "text/html")
//    //            {
//    //                byte[] data = Convert.FromBase64String(part.Body.Data);
//    //                string decodedString = Encoding.UTF8.GetString(data);
//    //                Console.WriteLine(decodedString);
//    //            }
//    //        }
//    //    }
//    //}
//}