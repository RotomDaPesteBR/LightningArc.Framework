using System.Net;
using LightningArc.Results;
using LightningArc.Results.AspNetCore.Localization;

namespace LightningArc.Results.AspNetCore;

public static partial class ErrorMappingServiceExtensions
{
    /// <summary>
    /// Adds error mappings related to the network.
    /// </summary>
    /// <remarks>
    /// Includes errors such as <see cref="Error.Network.ConnectionFailedError"/>,
    /// <see cref="Error.Network.RequestTimeoutError"/>, among others.
    /// </remarks>
    /// <param name="service">The error mapping service instance.</param>
    /// <returns>The service instance for call chaining.</returns>
    public static ErrorMappingService MapNetwork(this ErrorMappingService service)
    {
        // NETWORK module
        service.Map<Error.Network.ConnectionFailedError>(
            HttpStatusCode.ServiceUnavailable,
            LocalizationManager.GetErrorTitle("Network_ConnectionFailed"),
            "urn:api-errors:net-connection-failed"
        );
        service.Map<Error.Network.RequestTimeoutError>(
            HttpStatusCode.GatewayTimeout,
            LocalizationManager.GetErrorTitle("Network_RequestTimeout"),
            "urn:api-errors:net-request-timeout"
        );
        service.Map<Error.Network.ServiceUnavailableError>(
            HttpStatusCode.ServiceUnavailable,
            LocalizationManager.GetErrorTitle("Network_ServiceUnavailable"),
            "urn:api-errors:net-service-unavailable"
        );
        service.Map<Error.Network.DnsFailureError>(
            HttpStatusCode.ServiceUnavailable,
            LocalizationManager.GetErrorTitle("Network_DnsFailure"),
            "urn:api-errors:net-dns-failure"
        );
        service.Map<Error.Network.SslHandshakeFailedError>(
            HttpStatusCode.ServiceUnavailable,
            LocalizationManager.GetErrorTitle("Network_SslHandshakeFailed"),
            "urn:api-errors:net-ssl-handshake-failed"
        );
        service.Map<Error.Network.ProxyFailureError>(
            HttpStatusCode.BadGateway,
            LocalizationManager.GetErrorTitle("Network_ProxyFailure"),
            "urn:api-errors:net-proxy-failure"
        );

        return service;
    }
}
