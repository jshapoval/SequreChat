using Messenger.Common.DTOs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Messenger.Entities.DTOs;

namespace Messenger.Common
{
    public static class Common
    {
        public const string Host = "http://jsmessenger.gearhostpreview.com/";// "http://localhost:5000";
        public const string Hub = "/mainHub";
        public const string TokenEndpoint = "/token";
        public const string Issuer = "MessengerAuthServer";

        private const string aesSalt = "`bb_+D)F-=dfm0+_AFM(D";

        public async static Task<AuthObject> GetAuthObject(string email, string password)
        {
            var response = await PostJsonAsync(Host + TokenEndpoint, new LoginDTO(email, password), x => { });

            return JsonConvert.DeserializeObject<AuthObject>(response);
        }

        public static async Task<string> GetAsync(string uri, Action<HttpWebRequest> action)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            action(request);
            
            using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return await reader.ReadToEndAsync();
            }
        }

        public static async Task<string> PostAsync(string uri, Dictionary<string, string> paremeters, Action<HttpClient> action)
        {
            return await PostAsync(uri, new FormUrlEncodedContent(paremeters), action);
        }

        public static async Task<string> PostJsonAsync(string uri, object data, Action<HttpClient> action)
        {
            return await PostAsync(uri,
                new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json"), action);
        }

        private static async Task<string> PostAsync(string uri, ByteArrayContent content, Action<HttpClient> action)
        {
            var client = new HttpClient();

            action(client);

            var response = await client.PostAsync(uri, content);

            return await response.Content.ReadAsStringAsync();
        }

        public static string EncryptRSA(string plainTextData, RSAParameters parameters)
        {
            var csp = new RSACryptoServiceProvider();
            csp.ImportParameters(parameters);
            var bytesPlainTextData = System.Text.Encoding.Unicode.GetBytes(plainTextData);
            var bytesCypherText = csp.Encrypt(bytesPlainTextData, false);
            var cypherText = Convert.ToBase64String(bytesCypherText);
            return cypherText;
        }
        public static string DecryptRSA(string cypherText, RSAParameters parameters)
        {
            var csp = new RSACryptoServiceProvider();
            csp.ImportParameters(parameters);
            var bytesCypherText = Convert.FromBase64String(cypherText);
            var bytesPlainTextData = csp.Decrypt(bytesCypherText, false);
            return System.Text.Encoding.Unicode.GetString(bytesPlainTextData);
        }
        public static string EncryptAES(string clearText, string key)
        {
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(key, Encoding.UTF8.GetBytes(aesSalt));
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }
        public static string DecryptAES(string cipherText, string key)
        {
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(key, Encoding.UTF8.GetBytes(aesSalt));
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }

            return cipherText;
        }
        public static (string, string) CreateRSAKeys()
        {
            var csp = new RSACryptoServiceProvider(2048);

            var privKey = csp.ExportParameters(true);
            var pubKey = csp.ExportParameters(false);

            string pubKeyString = GetString(pubKey);
            string privKeyString = GetString(privKey);

            return (pubKeyString, privKeyString);
        }
        public static string CreateAesKey()
        {
            using (var provider = Aes.Create())
            {
                provider.GenerateKey();
                return Convert.ToBase64String(provider.Key);
            }
        }
        private static string GetString(RSAParameters data)
        {
            var sw = new System.IO.StringWriter();
            var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
            xs.Serialize(sw, data);
            return sw.ToString();
        }

        public static DialogInfo GetDialogInfo(int id, string keyFolder, AuthObject auth)
        {
            var path = Path.Combine(keyFolder, auth.User.Id.GetHashCode().ToString("X") + "_" + id.ToString());

            if (File.Exists(path))
            {
                return JsonConvert.DeserializeObject<DialogInfo>(File.ReadAllText(path));
            }

            return null;
        }

        public static void SaveDialogInfo(DialogInfo value, string keyFolder, AuthObject auth)
        {
            var path = Path.Combine(keyFolder, auth.User.Id.GetHashCode().ToString("X") + "_" + value.Id.ToString());

            Directory.CreateDirectory(keyFolder);

            File.WriteAllText(path, JsonConvert.SerializeObject(value));
        }
    }
}
