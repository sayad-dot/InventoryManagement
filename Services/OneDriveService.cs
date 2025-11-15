using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace InventoryManagement.Services
{
    public interface IOneDriveService
    {
        Task<string> UploadFileAsync(string fileName, string jsonContent);
    }

    public class OneDriveService : IOneDriveService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<OneDriveService> _logger;
        private readonly HttpClient _httpClient;

        public OneDriveService(
            IConfiguration configuration,
            ILogger<OneDriveService> logger,
            HttpClient httpClient)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<string> UploadFileAsync(string fileName, string jsonContent)
        {
            try
            {
                // Get Dropbox access token
                var accessToken = await GetAccessTokenAsync();

                // Upload file to Dropbox
                var uploadUrl = "https://content.dropboxapi.com/2/files/upload";

                var request = new HttpRequestMessage(HttpMethod.Post, uploadUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                
                // Dropbox API requires path in header
                var dropboxPath = $"/SupportTickets/{fileName}";
                var dropboxApiArg = new
                {
                    path = dropboxPath,
                    mode = "add",
                    autorename = true,
                    mute = false
                };
                request.Headers.Add("Dropbox-API-Arg", JsonSerializer.Serialize(dropboxApiArg));
                
                // Use ByteArrayContent to avoid charset being added to Content-Type
                var jsonBytes = Encoding.UTF8.GetBytes(jsonContent);
                request.Content = new ByteArrayContent(jsonBytes);
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var jsonDoc = JsonDocument.Parse(responseContent);
                    var fileId = jsonDoc.RootElement.GetProperty("id").GetString();
                    
                    _logger.LogInformation($"Successfully uploaded file {fileName} to Dropbox. File ID: {fileId}");
                    return fileId ?? string.Empty;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to upload file to Dropbox. Status: {response.StatusCode}, Error: {errorContent}");
                    throw new Exception($"Failed to upload file to Dropbox: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error uploading file {fileName} to Dropbox");
                throw;
            }
        }

        private async Task<string> GetAccessTokenAsync()
        {
            try
            {
                var appKey = _configuration["Dropbox:AppKey"];
                var appSecret = _configuration["Dropbox:AppSecret"];
                var refreshToken = _configuration["Dropbox:RefreshToken"];

                // If we have a refresh token, use it to get access token
                if (!string.IsNullOrEmpty(refreshToken))
                {
                    var tokenUrl = "https://api.dropboxapi.com/oauth2/token";

                    var requestBody = new Dictionary<string, string>
                    {
                        { "grant_type", "refresh_token" },
                        { "refresh_token", refreshToken },
                        { "client_id", appKey ?? "" },
                        { "client_secret", appSecret ?? "" }
                    };

                    var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl)
                    {
                        Content = new FormUrlEncodedContent(requestBody)
                    };

                    var response = await _httpClient.SendAsync(request);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonDoc = JsonDocument.Parse(responseContent);
                        var accessToken = jsonDoc.RootElement.GetProperty("access_token").GetString();
                        return accessToken ?? throw new Exception("Access token is null");
                    }
                    else
                    {
                        _logger.LogError($"Failed to get access token. Status: {response.StatusCode}, Error: {responseContent}");
                        throw new Exception($"Failed to get access token: {response.StatusCode}");
                    }
                }
                else
                {
                    // Use long-lived access token directly (generated from Dropbox console)
                    var accessToken = _configuration["Dropbox:AccessToken"];
                    if (string.IsNullOrEmpty(accessToken))
                    {
                        throw new Exception("No Dropbox access token or refresh token configured");
                    }
                    return accessToken;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Dropbox access token");
                throw;
            }
        }
    }
}
