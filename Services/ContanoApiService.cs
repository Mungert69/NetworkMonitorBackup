using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetworkMonitorBackup.Models;


namespace NetworkMonitorBackup.Services
{
    public class ContaboService : IContaboService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ContaboService> _logger;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _apiUser;
        private readonly string _apiPassword;
        private string? _accessToken;

        public ContaboService(HttpClient httpClient, ILogger<ContaboService> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;

            var contaboConfig = configuration.GetSection("Contabo");
            _clientId = contaboConfig["ClientId"];
            _clientSecret = contaboConfig["ClientSecret"];
            _apiUser = contaboConfig["ApiUser"];
            _apiPassword = contaboConfig["ApiPassword"];
            // Set the BaseAddress for the HttpClient
            _httpClient.BaseAddress = new Uri("https://api.contabo.com");
        }

        public async Task<ResultObj> AuthenticateAsync()
        {
            try
            {
                _logger.LogInformation("Starting authentication with Contabo API.");

                var tokenRequest = new Dictionary<string, string>
                {
                    { "client_id", _clientId },
                    { "client_secret", _clientSecret },
                    { "username", _apiUser },
                    { "password", _apiPassword },
                    { "grant_type", "password" }
                };

                var requestPayload = new FormUrlEncodedContent(tokenRequest);
                var response = await _httpClient.PostAsync(
                    "https://auth.contabo.com/auth/realms/contabo/protocol/openid-connect/token",
                    requestPayload
                );

                _logger.LogInformation("Received authentication response with status code: {StatusCode}", response.StatusCode);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var json = JsonConvert.DeserializeObject<dynamic>(content);
                    _accessToken = json?.access_token;

                    _logger.LogInformation("Authentication successful. Access token retrieved.");
                    return new ResultObj("Authentication successful.", true);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Authentication failed. StatusCode: {StatusCode}, Response: {ErrorContent}",
                        response.StatusCode, errorContent);

                    return new ResultObj($"Authentication failed: {errorContent}", false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during authentication.");
                return new ResultObj($"An unexpected error occurred: {ex.Message}", false);
            }
        }



        public async Task<ResultObj> ListInstancesAsync()
        {
            if (!await EnsureAuthenticatedAsync())
                return new ResultObj("Authentication failed. Unable to list instances.", false);

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "/v1/compute/instances");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
                request.Headers.Add("x-request-id", Guid.NewGuid().ToString());

                var response = await _httpClient.SendAsync(request);
                _logger.LogInformation("Received instances response with status code: {StatusCode}", response.StatusCode);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var instances = JsonConvert.DeserializeObject<InstanceResponse>(content);

                    return new ResultObj("Instances retrieved successfully.", true) { Data = instances };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to list instances. StatusCode: {StatusCode}, Response: {ErrorContent}",
                        response.StatusCode, errorContent);

                    return new ResultObj($"Failed to retrieve instances: {errorContent}", false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while listing instances.");
                return new ResultObj($"Failed to retrieve instances: {ex.Message}", false);
            }
        }

        private async Task<bool> EnsureAuthenticatedAsync()
        {
            if (!string.IsNullOrEmpty(_accessToken))
            {
                return true;
            }

            var authResult = await AuthenticateAsync();
            return authResult.Success;
        }

        public async Task<ResultObj> ListSnapshotsAsync(long instanceId)
        {
            if (!await EnsureAuthenticatedAsync())
            {
                return new ResultObj("Authentication failed. Unable to list snapshots.", false);
            }

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"/v1/compute/instances/{instanceId}/snapshots");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
                request.Headers.Add("x-request-id", Guid.NewGuid().ToString());

                var response = await _httpClient.SendAsync(request);
                _logger.LogDebug("Received snapshots response for InstanceId: {InstanceId} with StatusCode: {StatusCode}", instanceId, response.StatusCode);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    // Deserialize into SnapshotListResponse
                    var snapshotListResponse = JsonConvert.DeserializeObject<SnapshotListResponse>(content);

                    return new ResultObj("Snapshots retrieved successfully.", true) { Data = snapshotListResponse };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to list snapshots for InstanceId: {InstanceId}. StatusCode: {StatusCode}, Response: {ErrorContent}",
                        instanceId, response.StatusCode, errorContent);

                    return new ResultObj($"Failed to retrieve snapshots: {errorContent}", false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while listing snapshots for InstanceId: {InstanceId}", instanceId);
                return new ResultObj($"Failed to retrieve snapshots: {ex.Message}", false);
            }
        }





        public async Task<ResultObj> CreateSnapshotAsync(long instanceId, SnapshotRequest request)
        {
            if (!await EnsureAuthenticatedAsync())
            {
                return new ResultObj("Authentication failed. Unable to create snapshot.", false);
            }

            try
            {
                // Log the request details for debugging
                _logger.LogInformation("Creating snapshot for Instance ID: {InstanceId} with Name: {Name}, Description: {Description}", instanceId, request.Name, request.Description);

                // Serialize the request body
                var requestBody = JsonConvert.SerializeObject(new
                {
                    name = request.Name,
                    description = request.Description
                });

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/v1/compute/instances/{instanceId}/snapshots")
                {
                    Content = new StringContent(requestBody, Encoding.UTF8, "application/json")
                };

                // Add required headers
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
                httpRequest.Headers.Add("x-request-id", Guid.NewGuid().ToString());

                var response = await _httpClient.SendAsync(httpRequest);

                // Log the response status
                _logger.LogInformation("Received response with status code: {StatusCode}", response.StatusCode);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var snapshotApiResponse = JsonConvert.DeserializeObject<SnapshotApiResponse>(content);

                    if (snapshotApiResponse?.Data == null || snapshotApiResponse.Data.Count == 0)
                    {
                        var errorMessage = $"API response did not contain a valid snapshot for Instance ID: {instanceId}.";
                        _logger.LogError(errorMessage);
                        return new ResultObj(errorMessage, false);
                    }

                    // Retrieve the first snapshot
                    var snapshot = snapshotApiResponse.Data[0];
                    _logger.LogInformation("Snapshot created successfully: {SnapshotId}", snapshot.SnapshotId);
                    return new ResultObj("Snapshot created successfully.", true) { Data = snapshot };
                }
                else
                {
                    // Log the error response content
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to create snapshot. StatusCode: {StatusCode}, Response: {ErrorContent}",
                        response.StatusCode, errorContent);

                    return new ResultObj($"Failed to create snapshot: {errorContent}", false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while creating snapshot for Instance ID: {InstanceId}", instanceId);
                return new ResultObj($"Failed to create snapshot: {ex.Message}", false);
            }
        }
        public async Task<ResultObj> DeleteSnapshotAsync(long instanceId, string snapshotId)
        {
            if (!await EnsureAuthenticatedAsync())
            {
                return new ResultObj("Authentication failed. Unable to delete snapshot.", false);
            }

            try
            {
                _logger.LogInformation("Deleting snapshot with ID: {SnapshotId} for Instance ID: {InstanceId}", snapshotId, instanceId);

                var request = new HttpRequestMessage(HttpMethod.Delete, $"/v1/compute/instances/{instanceId}/snapshots/{snapshotId}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
                request.Headers.Add("x-request-id", Guid.NewGuid().ToString());

                var response = await _httpClient.SendAsync(request);

                _logger.LogInformation("Received response for deleting snapshot with status code: {StatusCode}", response.StatusCode);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Snapshot deleted successfully: {SnapshotId}", snapshotId);
                    return new ResultObj("Snapshot deleted successfully.", true);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to delete snapshot. StatusCode: {StatusCode}, Response: {ErrorContent}",
                        response.StatusCode, errorContent);

                    return new ResultObj($"Failed to delete snapshot: {errorContent}", false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while deleting snapshot with ID: {SnapshotId} for Instance ID: {InstanceId}", snapshotId, instanceId);
                return new ResultObj($"Failed to delete snapshot: {ex.Message}", false);
            }
        }


    }
}
