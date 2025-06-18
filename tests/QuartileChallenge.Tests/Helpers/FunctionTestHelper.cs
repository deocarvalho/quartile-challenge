using Microsoft.Azure.Functions.Worker.Http;
using Moq;
using System.Net;

namespace QuartileChallenge.Tests.Helpers;

public static class FunctionTestHelper
{
    public static Mock<HttpRequestData> CreateHttpRequestMock(string? body = null)
    {
        var mockRequest = new Mock<HttpRequestData>();
        var mockResponse = new Mock<HttpResponseData>(mockRequest.Object);
        
        // Setup response properties
        mockResponse.Setup(r => r.StatusCode).Returns(HttpStatusCode.OK);
        
        // Setup async methods with their specific return types
        mockResponse.Setup(r => r.WriteAsJsonAsync(It.IsAny<object>(), default))
            .Returns(ValueTask.CompletedTask);
        mockResponse.Setup(r => r.WriteStringAsync(It.IsAny<string>(), default))
            .Returns(Task.CompletedTask);

        // Setup request response creation
        mockRequest.Setup(r => r.CreateResponse())
            .Returns(mockResponse.Object);

        // Setup request body if provided
        if (body != null)
        {
            var stream = new MemoryStream();
            using var writer = new StreamWriter(stream, leaveOpen: true);
            writer.Write(body);
            writer.Flush();
            stream.Position = 0;
            
            mockRequest.Setup(r => r.Body)
                .Returns(stream);
        }

        return mockRequest;
    }
}