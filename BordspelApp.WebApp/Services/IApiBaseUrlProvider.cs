namespace BordspelApp.WebApp.Services
{
    public interface IApiBaseUrlProvider
    {
        string GetBaseUrl();
        bool IsProduction { get; }
    }
}
