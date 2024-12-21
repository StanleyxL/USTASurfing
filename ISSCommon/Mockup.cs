using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISS.Common
{
    public class Mockup
    {
        public static string getMockupFile(string filename)
        {
            string fullfilename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Mockup", filename);
            return getStringFromFile(fullfilename);
        }
        public static string getStringFromFile(string filePath)
        {
            string Rc = "Error";
                try
                {
                    using (StreamReader reader = new StreamReader(filePath))
                    {
                        return reader.ReadToEnd();                     
                    }                     
                }
                catch (Exception ex)
                {
                    Logger.Log(ex.Message);
                }

                return Rc;
            
        }
    }
}
