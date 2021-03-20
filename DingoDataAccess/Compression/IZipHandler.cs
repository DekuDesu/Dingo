namespace DingoDataAccess.Compression
{
    public interface IZipHandler
    {
        string Unzip(byte[] bytes);
        byte[] Zip(string stringToZip);
    }
}