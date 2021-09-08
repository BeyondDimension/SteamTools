// https://github.com/MicrosoftTranslator/Text-Translation-API-V3-C-Sharp/blob/master/Translate.cs
using Newtonsoft.Json;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

#nullable disable
namespace System
{
    /// <summary>
    /// The C# classes that represents the JSON returned by the Translator Text API.
    /// </summary>
    public class TranslationResult
    {
        public DetectedLanguage DetectedLanguage { get; set; }
        public TextResult SourceText { get; set; }
        public Translation[] Translations { get; set; }
    }

    public class DetectedLanguage
    {
        public string Language { get; set; }
        public float Score { get; set; }
    }

    public class TextResult
    {
        public string Text { get; set; }
        public string Script { get; set; }
    }

    public class Translation
    {
        public string Text { get; set; }
        public TextResult Transliteration { get; set; }
        public string To { get; set; }
        public Alignment Alignment { get; set; }
        public SentenceLength SentLen { get; set; }
    }

    public class Alignment
    {
        public string Proj { get; set; }
    }

    public class SentenceLength
    {
        public int[] SrcSentLen { get; set; }
        public int[] TransSentLen { get; set; }
    }

    public class TranslatecsSettings
    {
        public string Key { get; set; }

        public string Endpoint { get; set; }

        public string Region { get; set; }
    }

    public class Translatecs
    {
        public static TranslatecsSettings Settings { internal get; set; }

        //const string region_var = "TRANSLATOR_SERVICE_REGION";
        static /*readonly*/ string region => Settings.Region;
        //= Environment.GetEnvironmentVariable(region_var);

        //const string key_var = "TRANSLATOR_TEXT_SUBSCRIPTION_KEY";
        static /*readonly*/ string subscriptionKey => Settings.Key;
        //= Environment.GetEnvironmentVariable(key_var);

        //const string endpoint_var = "TRANSLATOR_TEXT_ENDPOINT";
        static /*readonly*/ string endpoint => Settings.Endpoint;

        static readonly JsonSerializer jsonSerializer = new();

        //= Environment.GetEnvironmentVariable(endpoint_var);

        //static Translatecs()
        //{
        //    if (null == region)
        //    {
        //        throw new Exception("Please set/export the environment variable: " + region_var);
        //    }
        //    if (null == subscriptionKey)
        //    {
        //        throw new Exception("Please set/export the environment variable: " + key_var);
        //    }
        //    if (null == endpoint)
        //    {
        //        throw new Exception("Please set/export the environment variable: " + endpoint_var);
        //    }
        //}

        // Async call to the Translator Text API
        public static async Task<TranslationResult[]> TranslateTextAsync(/*string subscriptionKey, string endpoint,*/ string route, string inputText)
        {
            var body = new object[] { new { Text = inputText } };
            var requestBody = JsonConvert.SerializeObject(body);

            var f = DI.Get<IHttpClientFactory>();

            var client = f.CreateClient();
            var request = new HttpRequestMessage
            {
                // Build the request.
                Method = HttpMethod.Post,
                RequestUri = new Uri(/*endpoint +*/ route),
                Content = new StringContent(requestBody, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
            request.Headers.Add("Ocp-Apim-Subscription-Region", region);

            // Send the request and get response.
            var response = await client.SendAsync(request).ConfigureAwait(false);
            // Read response as a string.

            //using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            //using var reader = new StreamReader(stream, Encoding.UTF8);
            //using var json = new JsonTextReader(reader);
            //var deserializedOutput = jsonSerializer.Deserialize<TranslationResult>(json);

            string result = await response.Content.ReadAsStringAsync();
            var deserializedOutput = JsonConvert.DeserializeObject<TranslationResult[]>(result);

            return deserializedOutput;

            // Iterate over the deserialized results.
            //foreach (TranslationResult o in deserializedOutput)
            //{
            //    // Print the detected input languge and confidence score.
            //    Console.WriteLine("Detected input language: {0}\nConfidence score: {1}\n", o.DetectedLanguage.Language, o.DetectedLanguage.Score);
            //    // Iterate over the results and print each translation.
            //    foreach (Translation t in o.Translations)
            //    {
            //        Console.WriteLine("Translated to {0}: {1}", t.To, t.Text);
            //    }
            //}
        }

        //static async Task Main(string[] args)
        //{
        //    // This is our main function.
        //    // Output languages are defined in the route.
        //    // For a complete list of options, see API reference.
        //    // https://docs.microsoft.com/azure/cognitive-services/translator/reference/v3-0-translate
        //    string route = "/translate?api-version=3.0&to=de&to=it&to=ja&to=th";
        //    // Prompts you for text to translate. If you'd prefer, you can
        //    // provide a string as textToTranslate.
        //    Console.Write("Type the phrase you'd like to translate? ");
        //    string textToTranslate = Console.ReadLine();
        //    await TranslateTextRequest(subscriptionKey, endpoint, route, textToTranslate);
        //    Console.WriteLine("Press any key to continue.");
        //    Console.ReadKey();
        //}
    }
}
#nullable enable