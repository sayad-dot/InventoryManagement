namespace InventoryManagement.Services;

public interface ISalesforceService
{
    string GetAuthorizationUrl(string redirectUri, string state, string codeChallenge);
    Task<string> ExchangeCodeForTokenAsync(string code, string redirectUri, string codeVerifier);
    Task<string> CreateAccountAsync(string accessToken, string companyName, string phone, string industry);
    Task<string> CreateContactAsync(string accessToken, string accountId, string firstName, string lastName, string email, string title);
}
