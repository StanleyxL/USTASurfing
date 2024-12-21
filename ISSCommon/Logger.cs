
using ISS.Common.WebHelp;
using System.Text;
using System.Text.RegularExpressions;
 

namespace ISS.Common
{
    public static class Logger
    {
        public static void Log(string message)
        {
            string logDirectory = "Logs"; // Directory to store log files
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            string logFileName = $"{DateTime.Now:yyyy-MM-dd}.log";
            string logFilePath = Path.Combine(logDirectory, logFileName);

            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";

            File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
        }
        public static void LogPlayersToFile(Dictionary<string, int> participants, string filePath)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    var sortedlist = participants.OrderByDescending(p => p.Value).ToList();
                    //foreach (var participant in sortedlist)
                    for (int i = 0; i < sortedlist.Count(); i++)
                    {
                        writer.WriteLine($"{i}|{sortedlist[i].Key.Trim()} |  {sortedlist[i].Value}");

                        //writer.WriteLine($"Name: {participant.Key}|  Points: {participant.Value}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
            }
        }

        public static Dictionary<string, int> ReadPlayersFromFile(string filePath)
        {
            var participantsDictionary = new Dictionary<string, int>();
            var players = new Dictionary<string, int>();
            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        // Assuming each line is in the format: "Name: John Doe, Event: Boys' 16 & under Singles, City: New York, NY, Gender: M"
                        //$"{i}|{sortedlist[i].Key} |  {sortedlist[i].Value}"
                        var parts = line.Split(new[] { "|" }, StringSplitOptions.None);
                        if (parts.Length != 3)
                        {
                            Log(string.Format("the line is not good format {0}", line));
                        }
                        var rank = parts[0];
                        var name = parts[1].Trim();//.Split(new[] { "Name: " }, StringSplitOptions.None)[1];
                        var points = parts[2];//.Split(new[] { "Points: " }, StringSplitOptions.None)[1];
                        //var city = parts[2].Split(new[] { "City: " }, StringSplitOptions.None)[1];
                        //var gender = parts[3].Split(new[] { "Gender: " }, StringSplitOptions.None)[1];

                        // Using name as the key and a concatenation of other details as the value
                        participantsDictionary[name] = int.Parse(points);// $"Points: {points}";

                    }
                }

                var sortedlist = participantsDictionary.OrderByDescending(p => p.Value);
                foreach (var item in sortedlist)
                {
                    players[item.Key] = item.Value;
                    //Console.WriteLine($"Name: {item.Key}, Points: {item.Value}");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
            }

            return players;
        }
    }
}
