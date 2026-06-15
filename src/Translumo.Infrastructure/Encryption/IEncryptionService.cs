using System.IO;

namespace Translumo.Infrastructure.Encryption
{
    public interface IEncryptionService
    {
        byte[] Encrypt(Stream toEncrypt, string password);

        string Decrypt(Stream encryptedData, string password);
    }
}
