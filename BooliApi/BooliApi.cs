using Common;
using Common.Models.NativeBooli;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
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

        public async Task<List<Sold>> GetNativeSoldObjectsInArea(string area, int yearSpan, int offset)
        {
            var resultJson = await GetSoldObjectsJson(area, yearSpan, MaxLimit, offset* MaxLimit);
            return ParseSoldObjectsFromJson(resultJson);
        }

        public async Task<int> GetNumberOfPages(string area, int yearSpan)
        {
            var resultJson = await GetSoldObjectsJson(area, yearSpan);
            var totalNumber = GetTotalNumberOfSoldObjectsFromJson(resultJson);
            var numberOfPages = GetNumberOfPages(totalNumber, MaxLimit);

            return numberOfPages;
        }

        private int GetNumberOfPages(int totalNumberOfObjects, int limit)
        {
            return (int)Math.Ceiling((double)totalNumberOfObjects / (double)limit);
        }

        private async Task<string> GetSoldObjectsJson(string area, int sinceYears, int limit, int offset)
        {
            var requiredParameters = GetRequiredParameterString();
            var sinceYear = GetYear(sinceYears);

            return await _httpClient.GetStringAsync($"sold?q={area}&minPublished={sinceYear}&limit={limit}&offset={offset}&" + requiredParameters);
        }

        private async Task<string> GetSoldObjectsJson(string area, int sinceYears)
        {
            var requiredParameters = GetRequiredParameterString();
            var sinceYear = GetYear(sinceYears);

            return await _httpClient.GetStringAsync($"sold?q={area}&minPublished={sinceYear}&" + requiredParameters);
        }

        private List<Sold> ParseSoldObjectsFromJson(string json)
        {
            var jObject = JObject.Parse(json);
            var soldJsonArray = (JArray)jObject["sold"];

            return soldJsonArray.ToObject<List<Sold>>();
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
            var hash = Hash.Get(_callerId, unixTime, _privateKey, unique);

            return $"callerId={_callerId}&time={unixTime}&unique={unique}&hash={hash}";
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
    }
}
