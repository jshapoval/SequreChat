using System.Security.Cryptography;
using System.IO;
using Newtonsoft.Json;

namespace Messenger.Common.DTOs
{
    public class DialogInfo
    {
        public int Id { get; set; }
        public string MyPublicKeyString { get; set; }
        public string HisPublicKeyString { get; set; }
        public string PrivateKeyString { get; set; }

        [JsonIgnore]
        public bool IsCompleted
        {
            get { return MyPublicKeyString != null && HisPublicKeyString != null && PrivateKeyString != null; }
        }

        [JsonIgnore]
        public RSAParameters MyPublicKey
        {
            get
            {
                using (var sr = new StringReader(MyPublicKeyString))
                {
                    var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
                    return (RSAParameters) xs.Deserialize(sr);
                }
            }
        }

        [JsonIgnore]
        public RSAParameters HisPublicKey
        {
            get
            {
                using (var sr = new StringReader(HisPublicKeyString))
                {
                    var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
                    return (RSAParameters) xs.Deserialize(sr);
                }
            }
        }

        [JsonIgnore]
        public RSAParameters PrivateKey
        {
            get
            {
                using (var sr = new StringReader(PrivateKeyString))
                {
                    var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
                    return (RSAParameters) xs.Deserialize(sr);
                }
            }
        }

        public DialogInfo()
        {

        }

        public DialogInfo(int id, string publicKey, string privateKey)
        {
            Id = id;
            MyPublicKeyString = publicKey;
            PrivateKeyString = privateKey;
        }
    }
}
