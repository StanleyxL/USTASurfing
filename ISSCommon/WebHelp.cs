 

namespace ISS.Common.WebHelp
{
    public class USTASearchFilter
    {
        public int Distance=100;
        public string Gender { get; set; }// ="Boy";
        public List<string> Age { get; set; }// =["16U"];
    public List<string> Level { get; set; }//=["3","4","5"];
        public List<string> TournamentType { get; set; }// = ["Single"];//, "Double"],
        //public string StartDate = "Today";
        public DateTime StartDate { get; set; }// = DateTime.Now;
        public int DateRange { get; set; }// ={ get; set; }// 30;
        public string getSearchURL()
        {
           Level.ForEach(delegate (string level)
            {
                level = "00000000-0000-0000-0000-00000000000" + level;
            });
            string url = "https://playtennis.usta.com/tournaments?level-category=junior&location=08502,%20NJ"
                + "&event-wtn-level[]=1&event-wtn-level[]=40"
                + "&event-division-age-range[]=5&event-division-age-range[]=99"
                + "&tournament-level[,]=" + string.Join(",", Level)
                + "&event-division-age-category[,]=" + string.Join(",", Age)
                + "&event-division-event-type[,]=singles";
                //+ Distance + "&gender;

                return url;
                }
    }
}
