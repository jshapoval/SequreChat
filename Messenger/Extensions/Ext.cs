using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Claims;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;

namespace Messenger.Extensions
{
    public static class Ext
    {
        public static string GetId(this ClaimsIdentity claimsIdentity)
        {
            return claimsIdentity.FindFirst(JwtRegisteredClaimNames.Sub).Value;
        }
        public static string GetEmail(this ClaimsIdentity claimsIdentity)
        {
            return claimsIdentity.FindFirst(JwtRegisteredClaimNames.Email).Value;
        }
        public static string GetJti(this ClaimsIdentity claimsIdentity)
        {
            return claimsIdentity.FindFirst(JwtRegisteredClaimNames.Jti).Value;
        }
        public static string GetIat(this ClaimsIdentity claimsIdentity)
        {
            return claimsIdentity.FindFirst(JwtRegisteredClaimNames.Iat).Value;
        }
        public static byte[] ToBytes(this Image<Rgba32> image, IImageFormat format)
        {
            using (var ms = new MemoryStream())
            {
                image.Save(ms, format);
                return ms.ToArray();
            }
        }
    }
}
