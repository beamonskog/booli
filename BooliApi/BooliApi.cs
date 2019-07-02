using Common.Models.NativeBooli;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Booli
{
    public class BooliApi
    {
        private readonly HttpClient _httpClient;
        private readonly string _callerId = "kristoffer.skog";
        private readonly string _privateKey = "jc1RtwXG2ykSKvbVZhzSREatZE8MAWHF99ahE4MF";

        private const int MaxLimit = 500;

        public BooliApi()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(@" https://api.booli.se/");
            //_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("jc1RtwXG2ykSKvbVZhzSREatZE8MAWHF99ahE4MF");
            //_httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.booli-v2+json"));
        }

        // Inte optimal. Egentligen borde man istället dra hem 500, synka, dra hem 500, synka osv. 
//        public async Task<List<Sold>> GetNativeSoldObjectsInArea(string area, int yearSpan)
//        {
//            var requiredParameters = GetRequiredParameterString();
//            var totalNumberOfObjects = -1;
//            var results = new List<Sold>();
//            var resultJson = await GetSoldObjectsJson(area, yearSpan, MaxLimit, 0);
//            var soldObjects = ParseSoldObjectsFromJson(resultJson);
//            results.AddRange(soldObjects);

//            totalNumberOfObjects = GetTotalNumberOfSoldObjectsFromJson(resultJson);
//            var numberOfPages = GetNumberOfPages(totalNumberOfObjects, MaxLimit);

//            for (int i = 1; i < numberOfPages; i++)
//            {
//                resultJson = await GetSoldObjectsJson(area, yearSpan, MaxLimit, i * MaxLimit);
//                soldObjects = ParseSoldObjectsFromJson(resultJson);
//                results.AddRange(soldObjects);
//#if DEBUG
//                if ((i + 1) % 10 == 0)
//                {
//                    Console.WriteLine($"{results.Count}/{totalNumberOfObjects} downloaded...");
//                }
//#endif
//            }

//            return results;
//        }

        //        public async Task<List<Sold>> GetAllNativeSoldObjectsInArea(string area, int yearSpan, int offset)
        //        {
        //            var requiredParameters = GetRequiredParameterString();
        //            var totalNumberOfObjects = -1;
        //            var results = new List<Sold>();
        //            var resultJson = await GetSoldObjectsJson(area, yearSpan, MaxLimit, offset);
        //            var soldObjects = ParseSoldObjectsFromJson(resultJson);
        //            results.AddRange(soldObjects);

        //            totalNumberOfObjects = GetTotalNumberOfSoldObjectsFromJson(resultJson);
        //            var numberOfPages = GetNumberOfPages(totalNumberOfObjects, MaxLimit);

        //            for (int i = 1; i < numberOfPages; i++)
        //            {
        //                resultJson = await GetSoldObjectsJson(area, yearSpan, MaxLimit, i);
        //                soldObjects = ParseSoldObjectsFromJson(resultJson);
        //                results.AddRange(soldObjects);
        //#if DEBUG
        //                if ((i + 1) % 10 == 0)
        //                {
        //                    Console.WriteLine($"{results.Count}/{totalNumberOfObjects} downloaded...");
        //                }
        //#endif
        //            }

        //            return results;
        //        }


        public async Task<List<Sold>> GetNativeSoldObjectsInArea(string area, int yearSpan, int offset)
        {
            var requiredParameters = GetRequiredParameterString();
            var results = new List<Sold>();
            var resultJson = await GetSoldObjectsJson(area, yearSpan, MaxLimit, offset* MaxLimit);
            var soldObjects = ParseSoldObjectsFromJson(resultJson);
            results.AddRange(soldObjects);

            return results;
        }

        public async Task<int> GetNumberOfPages(string area, int yearSpan)
        {
            var requiredParameters = GetRequiredParameterString();
            var resultJson = await GetSoldObjectsJson(area, yearSpan);
            var soldObjects = ParseSoldObjectsFromJson(resultJson);
            var totalNumber = GetTotalNumberOfSoldObjectsFromJson(resultJson);
            var numberOfPages = GetNumberOfPages(totalNumber, MaxLimit);

            return numberOfPages;
        }

        private int GetNumberOfPages(int totalNumberOfObjects, int limit)
        {
            return (int)Math.Ceiling((double)totalNumberOfObjects / (double)limit);
        }

        public async Task<string> GetSoldObjectsJson(string area, int sinceYears, int limit, int offset)
        {
            var requiredParameters = GetRequiredParameterString();
            var sinceYear = GetYear(sinceYears);

            var resultJson = await _httpClient.GetStringAsync($"sold?q={area}&minPublished={sinceYear}&limit={limit}&offset={offset}&" + requiredParameters);

            return resultJson;
        }

        public async Task<string> GetSoldObjectsJson(string area, int sinceYears)
        {
            var requiredParameters = GetRequiredParameterString();
            var sinceYear = GetYear(sinceYears);

            var resultJson = await _httpClient.GetStringAsync($"sold?q={area}&minPublished={sinceYear}&" + requiredParameters);

            return resultJson;
        }

        private List<Sold> ParseSoldObjectsFromJson(string json)
        {
            var jObject = JObject.Parse(json);
            var soldJsonArray = (JArray)jObject["sold"];
            var soldItemList = soldJsonArray.ToObject<List<Sold>>();
            return soldItemList;
        }

        private int GetTotalNumberOfSoldObjectsFromJson(string json)
        {
            var jObject = JObject.Parse(json);
            var jToken = (JToken)jObject["totalCount"];
            var totalNumberOfSoldObjects = jToken.ToObject<int>();
            return totalNumberOfSoldObjects;
        }

        private object GetYear(int sinceYears)
        {
            var thisYear = DateTime.Now.Year;
            var thatYear = thisYear - sinceYears;

            return thatYear.ToString() + "0101";
        }

        private string GetRequiredParameterString()
        {
            //https://api.booli.se/listings?q=nacka&limit=3&offset=0&callerId=[callerId]&time=1323793365&unique=3116053465361547264&hash=a053d19fcced8e180df1a40b3fc95b6560eee1af

            var unixTime = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds();
            var unique = GetUnique();
            var hash = GetHash(_callerId, unixTime, _privateKey, unique);

            var resultString = $"callerId={_callerId}&time={unixTime}&unique={unique}&hash={hash}";

            return resultString;
        }

        private string GetUnique()
        {
            var chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[16];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            var finalString = new String(stringChars);
            return finalString;
        }

        private string GetHash(string callerId, long unixTime, string privateKey, string unique)
        {
            var stringToHash = callerId + unixTime.ToString() + privateKey + unique;
            var stringToHashInBytes = Encoding.ASCII.GetBytes(stringToHash);
            var hashedString = Hash(stringToHashInBytes);

            return hashedString;
        }

        private string Hash(byte[] temp)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hashBytes = sha1.ComputeHash(temp);
                var hexString = BitConverter.ToString(hashBytes);
                hexString = hexString.Replace("-", "");
                return hexString;
            }
        }
    }
}
