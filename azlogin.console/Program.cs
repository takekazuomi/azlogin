using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace azlogin.console
{
    /// <summary>
    /// device flow azure login console 
    /// </summary>
    class Program
    {
        private const string OAuth2Uri = "https://login.microsoftonline.com/common/oauth2";
        private const string LoginUri = OAuth2Uri + "/devicecode?api-version=1.0";
        private const string TokenUri = OAuth2Uri + "/token";
        private const string ResourceUri = "https://management.core.windows.net/";

        // used by powershell cmdlet
        // https://github.com/Azure/azure-powershell/blob/master/src/Common/Commands.Common.Authentication/Authentication/AdalConfiguration.cs#L31
        // private const string ClientId =  "1950a258-227b-4e31-a9cf-717495945fc2";

        // used by az and xplat tools 
        // private const string ClientId = "04b07795-8ddb-461a-bbee-02f9e1bf7b46";

        // used by devcode sample code
        // https://github.com/Azure-Samples/active-directory-dotnet-deviceprofile/blob/master/DirSearcherClient/Program.cs#L18
        private const string ClientId = "b78054de-7478-45a6-be1c-09f696a91d64";

        private const string MediaType = "application/x-www-form-urlencoded";

        private static bool Verbose { get; set; }

        private async Task RunAsync()
        {
            using (var client = new HttpClient())
            {
                var sc = ToStringContent(new { client_id = ClientId, resource = ResourceUri }, Encoding.UTF8, MediaType);

                var longin = await client.PostAsync(new Uri(LoginUri), sc);
                var result = await longin.Content.ReadAsStringAsync();
                if ((int)longin.StatusCode != 200)
                {
                    Console.Error.WriteLine("{0}\n{1}", longin.StatusCode, result);
                    return;
                }
                if (Verbose)
                    Console.Error.WriteLine(JsonFormatter(result));
                dynamic data = JsonConvert.DeserializeObject(result);

                Console.Error.WriteLine(data.message);

                var wait = TimeSpan.FromSeconds(int.Parse(data.interval.ToString()));
                for (;;)
                {
                    var vc = ToStringContent(new
                    {
                        grant_type = "device_code",
                        client_id = ClientId,
                        resource = ResourceUri,
                        code = data.device_code.ToString()
                    }, Encoding.UTF8, MediaType);

                    var token = await client.PostAsync(new Uri(TokenUri), vc);
                    if ((int)token.StatusCode == 200)
                    {
                        var content = await token.Content.ReadAsStringAsync();
                        Console.WriteLine(JsonFormatter(content));
                        break;
                    }
                    await Task.Delay(wait);
                    Console.Error.Write(".");
                }
            }
            Console.Error.WriteLine();
        }

        static StringContent ToStringContent(object data, Encoding encoding, string mediaType)
        {
            var s = data.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty)
                .Select(info =>
                    string.Format("{0}={1}",
                        info.Name,
                        Uri.EscapeDataString((string)info.GetValue(data)).Replace("%20", "+")))
                .ToArray();

            return new StringContent(string.Join("&", s), encoding, mediaType);
        }

        static string JsonFormatter(string data)
        {
            var obj = JsonConvert.DeserializeObject(data);
            return JsonConvert.SerializeObject(obj, Formatting.Indented);
        }

        static int Main(string[] args)
        {
            if (args.Length > 0 && args[0].Equals("-v", StringComparison.InvariantCulture))
                Verbose = true;

            try
            {
                var p = new Program();
                p.RunAsync().Wait();
                return 0;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
                return 1;
            }
        }
    }
}
