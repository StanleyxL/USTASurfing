using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using System;
using System.IO;
using System.Threading;

namespace ISS.Common.GoogleSheet
{
    public class GoogleSheet
    {
        public static bool test()
        {
            bool Rc = false;

            UserCredential credential;
            using(var stream =new FileStream("GS_Client.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    new[] { SheetsService.Scope.Spreadsheets },
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }

            // Create Google Sheets API service.
            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "USTASurfing1",
            });
            try
            {

                // Define request parameters.
                String spreadsheetId =// "Brendon's Tennis Schedule";//
                                      "1oYbcK9hpuJSVUpiWiOCe6B6mqf9VBWFLJ2mypElH4p4";//Brendon's Tennis Schedule
                String range = "Schedule!A1:H13";
                SpreadsheetsResource.ValuesResource.GetRequest request =
                        service.Spreadsheets.Values.Get(spreadsheetId, range);
                // Prints the names and majors of students in a sample spreadsheet:
                ValueRange response = request.Execute();
                IList<IList<Object>> values = response.Values;
                if (values != null && values.Count > 0)
                {
                    Console.WriteLine("Name, Major");
                    foreach (var row in values)
                    {
                        // Print columns A and E, which correspond to indices 0 and 4.
                        Console.WriteLine("{0}, {1}", row[0], row[4]);
                    }
                }
                else
                {
                    /*
                     var ss = SpreadsheetApp.getActiveSpreadsheet();
  var sheet = ss.getSheets()[0];
  var targetCell = sheet.getRange("A1");
  var sourceCell = sheet.getRange("B1");

  var noteText = sourceCell.getValue();

  targetCell.setNote(noteText);
                     */
                    Console.WriteLine("No data found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return Rc;

        }
    }
}
