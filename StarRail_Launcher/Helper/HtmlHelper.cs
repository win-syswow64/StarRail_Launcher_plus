using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace StarRail_Launcher.Core
{
    public class HtmlHelper
    {
        private const string Url = "https://api.xingdream.top/API/starrail.php";

        public static async Task<JsonElement> GetAPIData()
        {
            try
            {
                using HttpClient client = new();
                HttpResponseMessage response = await client.GetAsync(Url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                // Parse the JSON document and return the root element
                JsonDocument doc = JsonDocument.Parse(responseBody);
                return doc.RootElement.Clone(); // Clone the root element to avoid accessing a disposed object
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Request error: {e.Message}");
                throw;
            }
            catch (JsonException e)
            {
                Console.WriteLine($"JSON parsing error: {e.Message}");
                throw;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unexpected error: {e.Message}");
                throw;
            }
        }

        public static async Task<string> GetInfoFromHtmlAsync(string tag)
        {
            try
            {
                JsonElement data = await GetAPIData();
                return data.GetProperty(tag).ToString();
            }
            catch (KeyNotFoundException e)
            {
                Console.WriteLine($"Tag not found: {e.Message}");
                return string.Empty;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unexpected error: {e.Message}");
                return string.Empty;
            }
        }
    }
}
