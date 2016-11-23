using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace FSM
{
    struct CityData
    {
        public string rus_name;
        public string eng_name;
    }

    class Parser
    {
        public Parser()
        {
            cities = new List<CityData>();                 
            File.WriteAllText("khg.csv", "Название; КХГ\n");
            

            GetCities();
            Console.WriteLine("Total cities: {0} ", cities.Count);
            float KHG = 0;
            foreach( CityData city in cities)
            {
                Console.WriteLine("Now processing {0}({1})...", city.rus_name, city.eng_name);
                if(  (KHG = GetKHG(city) ) != -1)
                {
                    Console.WriteLine(" {0} - {1} ", city.rus_name, KHG);
                    File.AppendAllText("khg.csv",city.rus_name + ";" + KHG+"\n");

                }else
                {
                    Console.WriteLine("Err!");
                }
            }
            Console.WriteLine("Finished!");

        }

        private float GetKHG(CityData city)
        {
            string adr = "https://www.avito.ru/" + city.eng_name + "/kvartiry";
            int sales = -1;
            int rents = -1;
            WebClient cli = new WebClient();
            try
            {
                using (Stream stream = cli.OpenRead(adr))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string line = "";
                        string sale_pattern = ">Продам</a>";
                        string rent_pattern = ">Сдам</a>";
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (Regex.IsMatch(line, sale_pattern) || Regex.IsMatch(line, rent_pattern)) {
                                int start_sub= line.IndexOf("\"> ") + 2;
                                int len_sub = 9;
                                string count_str = Regex.Replace(line.Substring(start_sub, len_sub), "[\\s\\D]?", String.Empty);
                                if(Regex.IsMatch(line, sale_pattern))
                                {
                                    sales = Int32.Parse(count_str);
                                }
                                if(Regex.IsMatch(line, rent_pattern))
                                {
                                    rents = Int32.Parse(count_str);
                                }
                            }
                        }
                    }
                }
            }catch (System.Net.WebException e)
            {
                return -1;
            }
            if( rents > 0  && sales > 0)
            {
                float KHG = (float)rents / (float)sales;
                return KHG;
            }


            return -1;
        }

        private void GetCities()
        {
            WebClient client = new WebClient();
            using (Stream stream = client.OpenRead("http://glisa.ru/cities/"))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string pattern = "\\s*<a href.*Отслеживание новых объявлений";
                    string line = "";
                    CityData city = new CityData();
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (Regex.IsMatch(line, pattern))
                        {
                            city.eng_name = line.Substring(line.IndexOf("http") + 7, line.IndexOf(".glisa.ru") - 20);
                            int sub_len = line.IndexOf("'>") - line.IndexOf("Авито") - 6;
                            int sub_start = line.IndexOf("Авито") + 6;
                            city.rus_name = line.Substring(sub_start, sub_len);
                            cities.Add(city);
                        }
                    }
                }
            }
        }
        List<CityData> cities;

    }
}
