using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RestAPIConsumer
{
    class Program
    {
        

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            string city = "Seattle";

            int pageNum = 1;
            int total_pages = 0;
            List<Datum> foodOutlets = new List<Datum>();
            while (pageNum <= total_pages || pageNum==1)
            {
                var root = RunAsync(city, pageNum).GetAwaiter().GetResult();
                total_pages = root.total_pages;
                foodOutlets.AddRange(root.data);
                pageNum = pageNum + 1;
            }

            
            double maxrating = foodOutlets.Max(fo => fo.user_rating.average_rating);
            List<string> outletNames = new List<string>();

            foodOutlets = foodOutlets.FindAll(fo => fo.user_rating.average_rating == maxrating);
            
            outletNames = foodOutlets.Select(fo => fo.name).ToList<string>();

            Console.WriteLine("Below are the food outlets with top rating in "+city);
            foreach(var foodOutlet in outletNames)
               Console.WriteLine(foodOutlet);
        }

        public static double findMaxRating(List<Datum> foodOutlets)
        {
            double max = 0;
            foreach (var foodOutlet in foodOutlets)
            {
                var rating = foodOutlet.user_rating.average_rating;
                if (rating > max)
                {
                    max = rating;
                }
            }
            return max;
        }


        [DataContract]
        public class UserRating
        {
            [DataMember]
            public double average_rating { get; set; }
            [DataMember]
            public int votes { get; set; }
        }
        [DataContract]
        public class Datum
        {
            [DataMember]
            public string city { get; set; }
            [DataMember]
            public string name { get; set; }
            [DataMember]
            public int estimated_cost { get; set; }
            [DataMember]
            public UserRating user_rating { get; set; }
            [DataMember]
            public int id { get; set; }
        }
        [DataContract]
        public class Root
        {
            [DataMember]
            public int page { get; set; }
            [DataMember]
            public int per_page { get; set; }
            [DataMember]
            public int total { get; set; }
            [DataMember]
            public int total_pages { get; set; }
            [DataMember]
            public List<Datum> data { get; set; }
        }



        static HttpClient client = new HttpClient();

        static async Task<Root> RunAsync(string city, int pagenum)
        {
           

            try
            {
                List<Datum> foodoutlets = new List<Datum>();
                
                var uri =new  Uri("https://jsonmock.hackerrank.com/api/food_outlets?city=" + city + "&page=" + pagenum);

                
                client = new HttpClient();
                client.BaseAddress = uri;
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync(uri);
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();

                    Root root =
                        JsonSerializer.Deserialize<Root>(jsonString);

                    var deserializedRoot = new Root();
                    var ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
                    var ser = new DataContractJsonSerializer(deserializedRoot.GetType());
                    deserializedRoot = ser.ReadObject(ms) as Root;
                    ms.Close();

                    
                    return deserializedRoot;
                }

                return null;

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return null;
        }
    }
}
