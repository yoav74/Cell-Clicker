public interface ISaveService
{
    int GetInt(string key, int fallback = 0);
    void SetInt(string key, int value);
}
