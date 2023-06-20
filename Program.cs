// See https://aka.ms/new-console-template for more information
using System;
using System.IO;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;

namespace AutochaveamentoACCA
{
    class Program
    {
        static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };

        static readonly string ApplicationName = "Autochaveamento";

        static readonly string SpreadsheetID = "1sqYEUa3gYFn5Bo2lbp9t83mDFv5BlfPEt-8WdP_hvjY";

        static readonly string sheet = "Atletas";

        static SheetsService? service;

        static void Main(string[] args)
        {
            GoogleCredential credential;
            using (var stream = new FileStream("autochaveamento-acca-d575b21e390c.json", FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream).CreateScoped(Scopes);
            }

            service = new SheetsService(new Google.Apis.Services.BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            ReadEntries();
        }

        static void ReadEntries()
        {
            var range = "B1:H200";
            var request = service.Spreadsheets.Values.Get(SpreadsheetID, range);

            var response = request.Execute();
            var values = response.Values;
            if (values != null && values.Count > 0)
            {
                foreach (var row in values)
                {
                    Console.WriteLine("{0}, {1}, {2}, {3}, {4}, {5}, {6}", row[0], row[1], row[2], row[3], row[4], row[5], row[6]);
                }
            }
            else
            {
                Console.WriteLine("No data found.");
            }
        }
    }
}