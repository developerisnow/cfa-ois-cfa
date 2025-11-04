using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace OIS.FabricGateway.Services;

/// <summary>
/// Service for interacting with Hyperledger Fabric chaincode
/// </summary>
public class FabricGatewayService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<FabricGatewayService> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;
    private readonly string _peerEndpoint;
    private readonly string _channelName;
    private readonly string _mspId;
    private readonly string _tlsCertPath;
    private readonly string _tlsKeyPath;
    private readonly string _tlsRootCertPath;

    public FabricGatewayService(
        HttpClient httpClient,
        ILogger<FabricGatewayService> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        
        _peerEndpoint = configuration["Fabric:PeerEndpoint"] ?? "http://localhost:7051";
        _channelName = configuration["Fabric:ChannelName"] ?? "cfa-main";
        _mspId = configuration["Fabric:MspId"] ?? "OisDevMSP";
        _tlsCertPath = configuration["Fabric:TlsCertPath"] ?? "";
        _tlsKeyPath = configuration["Fabric:TlsKeyPath"] ?? "";
        _tlsRootCertPath = configuration["Fabric:TlsRootCertPath"] ?? "";

        _httpClient.BaseAddress = new Uri(_peerEndpoint);
        _httpClient.Timeout = TimeSpan.FromSeconds(30);

        // Retry policy with exponential backoff
        _retryPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(
                        "Retry {RetryCount} after {Delay}ms for {Operation}",
                        retryCount, timeSpan.TotalMilliseconds, context.OperationKey);
                });
    }

    /// <summary>
    /// Invoke chaincode function
    /// </summary>
    public async Task<string> InvokeChaincodeAsync(
        string chaincodeName,
        string functionName,
        string[] args,
        CancellationToken ct = default)
    {
        var operationKey = $"{chaincodeName}:{functionName}";
        
        return await _retryPolicy.ExecuteAsync(async (context) =>
        {
            try
            {
                var payload = new
                {
                    channel = _channelName,
                    chaincode = chaincodeName,
                    function = functionName,
                    args = args
                };

                _logger.LogDebug(
                    "Invoking chaincode {Chaincode}:{Function} with args {Args}",
                    chaincodeName, functionName, string.Join(", ", args));

                // Note: This is a simplified HTTP adapter
                // In production, use Fabric Gateway SDK (gRPC) or Fabric SDK
                var response = await _httpClient.PostAsJsonAsync("/chaincode/invoke", payload, ct);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(ct);
                    _logger.LogError(
                        "Chaincode invoke failed: {StatusCode} {Error}",
                        response.StatusCode, errorContent);
                    throw new HttpRequestException(
                        $"Chaincode invoke failed: {response.StatusCode} - {errorContent}");
                }

                var result = await response.Content.ReadFromJsonAsync<ChaincodeResponse>(cancellationToken: ct);
                
                if (result?.TransactionHash == null)
                {
                    throw new InvalidOperationException("Failed to get transaction hash from chaincode response");
                }

                _logger.LogInformation(
                    "Chaincode {Chaincode}:{Function} invoked successfully, txHash {TxHash}",
                    chaincodeName, functionName, result.TransactionHash);

                return result.TransactionHash;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error invoking chaincode {Chaincode}:{Function}",
                    chaincodeName, functionName);
                throw;
            }
        }, new Context(operationKey));
    }

    /// <summary>
    /// Query chaincode function
    /// </summary>
    public async Task<T?> QueryChaincodeAsync<T>(
        string chaincodeName,
        string functionName,
        string[] args,
        CancellationToken ct = default)
    {
        var operationKey = $"{chaincodeName}:{functionName}:query";
        
        return await _retryPolicy.ExecuteAsync(async (context) =>
        {
            try
            {
                var payload = new
                {
                    channel = _channelName,
                    chaincode = chaincodeName,
                    function = functionName,
                    args = args
                };

                _logger.LogDebug(
                    "Querying chaincode {Chaincode}:{Function} with args {Args}",
                    chaincodeName, functionName, string.Join(", ", args));

                var response = await _httpClient.PostAsJsonAsync("/chaincode/query", payload, ct);
                
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return default(T);
                }

                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<T>(cancellationToken: ct);
                
                _logger.LogDebug(
                    "Chaincode {Chaincode}:{Function} queried successfully",
                    chaincodeName, functionName);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error querying chaincode {Chaincode}:{Function}",
                    chaincodeName, functionName);
                throw;
            }
        }, new Context(operationKey));
    }
}

public class ChaincodeResponse
{
    public string? TransactionHash { get; set; }
    public string? Payload { get; set; }
    public string? Error { get; set; }
}

