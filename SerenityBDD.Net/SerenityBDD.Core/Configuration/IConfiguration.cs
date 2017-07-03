namespace SerenityBDD.Core.Configuration
{
    public interface IConfiguration
    {
        string getBaseUrl();

        string OutputDirectory { get;  }
    }
}
