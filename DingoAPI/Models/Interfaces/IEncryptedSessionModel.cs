namespace DingoAPI
{
    public interface IEncryptedSessionModel
    {
        byte[] AsymmetricKey { get; set; }
        byte[] X509IdentityKey { get; set; }
    }
}