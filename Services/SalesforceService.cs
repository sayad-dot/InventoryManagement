using System.Text;
using System.Text.Json;

namespace InventoryManagement.Services;

public class SalesforceService : ISalesforceService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly ILogger<SalesforceService> _logger;

    public SalesforceService(IConfiguration configuration, HttpClient httpClient, ILogger<SalesforceService> logger)
    {
        _configuration = configuration;
        _httpClient = httpClient;
        _logger = logger;
    }

    public string GetAuthorizationUrl(string redirectUri, string state, string codeChallenge)
    {
        var clientId = _configuration["Salesforce:ClientId"];
        var loginUrl = _configuration["Salesforce:LoginUrl"] ?? "https://login.salesforce.com";
        
        return $"{loginUrl}/services/oauth2/authorize?" +
               $"response_type=code&" +
               $"client_id={Uri.EscapeDataString(clientId!)}&" +
               $"redirect_uri={Uri.EscapeDataString(redirectUri)}&" +
               $"state={Uri.EscapeDataString(state)}&" +
               $"code_challenge={Uri.EscapeDataString(codeChallenge)}&" +
               $"code_challenge_method=S256";
    }

    public async Task<string> ExchangeCodeForTokenAsync(string code, string redirectUri, string codeVerifier)
    {
        var clientId = _configuration["Salesforce:ClientId"];
        var clientSecret = _configuration["Salesforce:ClientSecret"];
        var loginUrl = _configuration["Salesforce:LoginUrl"] ?? "https://login.salesforce.com";

        var tokenUrl = $"{loginUrl}/services/oauth2/token";
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "code", code },
            { "client_id", clientId! },
            { "client_secret", clientSecret! },
            { "redirect_uri", redirectUri },
            { "code_verifier", codeVerifier }
        });

        try
        {
            var response = await _httpClient.PostAsync(tokenUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Token exchange failed: {StatusCode} - {Content}", response.StatusCode, responseContent);
                throw new Exception($"Failed to exchange code for token: {responseContent}");
            }

            var tokenResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
            var accessToken = tokenResponse.GetProperty("access_token").GetString();
            
            return accessToken!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exchanging code for token");
            throw;
        }
    }

    public async Task<string> CreateAccountAsync(string accessToken, string companyName, string phone, string industry)
    {
        var instanceUrl = await GetInstanceUrlAsync(accessToken);
        var apiVersion = "v58.0";
        var url = $"{instanceUrl}/services/data/{apiVersion}/sobjects/Account";

        var accountData = new
        {
            Name = companyName,
            Phone = phone,
            Industry = industry
        };

        var json = JsonSerializer.Serialize(accountData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

        try
        {
            var response = await _httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Account creation failed: {StatusCode} - {Content}", response.StatusCode, responseContent);
                throw new Exception($"Failed to create Account: {responseContent}");
            }

            var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
            var accountId = result.GetProperty("id").GetString();
            
            _logger.LogInformation("Created Salesforce Account: {AccountId}", accountId);
            return accountId!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Salesforce Account");
            throw;
        }
    }

    public async Task<string> CreateContactAsync(string accessToken, string accountId, string firstName, string lastName, string email, string title)
    {
        var instanceUrl = await GetInstanceUrlAsync(accessToken);
        var apiVersion = "v58.0";
        var url = $"{instanceUrl}/services/data/{apiVersion}/sobjects/Contact";

        var contactData = new
        {
            AccountId = accountId,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Title = title
        };

        var json = JsonSerializer.Serialize(contactData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

        try
        {
            var response = await _httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Contact creation failed: {StatusCode} - {Content}", response.StatusCode, responseContent);
                throw new Exception($"Failed to create Contact: {responseContent}");
            }

            var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
            var contactId = result.GetProperty("id").GetString();
            
            _logger.LogInformation("Created Salesforce Contact: {ContactId}", contactId);
            return contactId!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Salesforce Contact");
            throw;
        }
    }

    private async Task<string> GetInstanceUrlAsync(string accessToken)
    {
        // For simplicity, we'll extract it from the token or use a default
        // In production, you'd store this from the OAuth response
        var loginUrl = _configuration["Salesforce:LoginUrl"] ?? "https://login.salesforce.com";
        
        // Call userinfo endpoint to get instance URL
        var userInfoUrl = $"{loginUrl}/services/oauth2/userinfo";
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

        try
        {
            var response = await _httpClient.GetAsync(userInfoUrl);
            var content = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                var userInfo = JsonSerializer.Deserialize<JsonElement>(content);
                if (userInfo.TryGetProperty("urls", out var urls))
                {
                    // Extract base instance URL from any of the URLs
                    var metadataUrl = urls.GetProperty("metadata").GetString();
                    var uri = new Uri(metadataUrl!);
                    return $"{uri.Scheme}://{uri.Host}";
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not get instance URL, using default");
        }

        // Fallback to common instance URL pattern
        return "https://orgfarm-2d4842-dev-ed.develop.my.salesforce.com";
    }
}
