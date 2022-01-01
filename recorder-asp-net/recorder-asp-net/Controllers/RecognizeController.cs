using Google.Cloud.Speech.V1;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System.Text;
using System.Web;

namespace recorder_asp_net.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RecognizeController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private static readonly string[] Summaries = new[]
        {
           "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<RecognizeController> _logger;

        public RecognizeController(ILogger<RecognizeController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }


        [HttpPost("audio")]
        public TranslatedWord Post([FromForm] AudioModel audiomodel )
        {
            string audiofile = audiomodel.audiofile;
            string lang = audiomodel.lang;

            TranslatedWord transcript = SendAudio(AudioXMpeg3Base64ToBytes(audiofile), lang);
            return transcript;
        }

      //  data:audio/x-mpeg-3;base64,
        private byte [] AudioXMpeg3Base64ToBytes(string data)
        {
            byte[] newBytes = Convert.FromBase64String( data.Replace("data:audio/x-mpeg-3;base64,", String.Empty)); //TODO
            return newBytes;
        }
        private TranslatedWord SendAudio(byte[] dataBytes, string lang)
        {
            string code = String.Empty;
            string translate = String.Empty;
            if (lang == "ru") { code = "ru-RU" ; translate = "en"; }
            if (lang == "en") { code = "en-US"; translate = "ru"; }
            var speech = SpeechClient.Create();
            var config = new RecognitionConfig
            {
                Encoding = RecognitionConfig.Types.AudioEncoding.WebmOpus,
                SampleRateHertz = 48000,
                LanguageCode = code,
                EnableWordTimeOffsets = true
            };



            var audio = RecognitionAudio.FromBytes(dataBytes);

            var response = speech.Recognize(config, audio);

            string Transcript = response.Results[0].Alternatives[0].Transcript;


            return GetTranslate(Transcript, translate);

        }
        public TranslatedWord GetTranslate  (string text, string translate)
        {
            var baseAddress = new Uri("https://translate.googleapis.com");
            using (var handler = new HttpClientHandler { UseCookies = false })
            using (var client = new HttpClient(handler) { BaseAddress = baseAddress })
            {
                //client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
                //client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
                //client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("br"));

                var message = new HttpRequestMessage(HttpMethod.Post, "/translate_a/single");
                message.Headers.Add("Accept", "*/*");
                message.Headers.Add("Origin", "chrome-extension://gfgpkepllngchpmcippidfhmbhlljhoo");
                message.Headers.Add("X-Client-Data", "CLG1yQEIkLbJAQiitskBCMS2yQEIqZ3KAQie+csBCOaEzAEItoXMAQjLicwBCNKPzAEYjp7LAQ==");

                message.Headers.UserAgent.TryParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.110 Safari/537.36");
                message.Headers.Add("Sec-Fetch-Site", "none");
                message.Headers.Add("Sec-Fetch-Mode", "cors");
                message.Headers.Add("Sec-Fetch-Dest", "empty");
    
                message.Content = new StringContent($"client=gtx&source=input&dj=1&q={HttpUtility.UrlEncode(text)}&sl=auto&tl={translate}&hl={translate}&dt=t&dt=bd&dt=rm&dt=rw&dt=qca", Encoding.UTF8, "application/x-www-form-urlencoded");

                HttpResponseMessage response = client.Send(message);
                response.EnsureSuccessStatusCode();
                string body = response.Content.ReadAsStringAsync().Result;
                dynamic deserializeObj = JsonConvert.DeserializeObject<dynamic>(body);

                string trans = deserializeObj.sentences[0].trans;
                string orig = deserializeObj.sentences[0].orig;

                return new TranslatedWord { orig = orig, trans = trans};
            }
        }
    }
}