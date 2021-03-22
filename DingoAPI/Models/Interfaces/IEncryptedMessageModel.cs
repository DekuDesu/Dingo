using DingoAuthentication.Encryption;

namespace DingoAPI
{
    public interface IEncryptedMessageModel
    {
        string Id { get; set; }
        EncryptedDataModel EncryptedData { get; set; }
    }
}