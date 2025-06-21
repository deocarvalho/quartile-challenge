using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.AspNetCore.Http.Features;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Collections.Immutable;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.Functions.Worker.Core;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace QuartileChallenge.Tests.Helpers;

public static class FunctionTestHelper
{
    public static HttpRequestData CreateHttpRequest(string? body = null, string method = "GET", string url = "http://localhost/api/test")
    {
        var context = new TestFunctionContext();
        var request = new TestHttpRequestData(context, method, url);
        
        if (body != null)
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(body));
            request.SetBody(stream);
        }
        
        return request;
    }
}

public class TestFunctionContext : FunctionContext
{
    private IServiceProvider _instanceServices;
    private IDictionary<object, object> _items = new Dictionary<object, object>();

    public TestFunctionContext()
    {
        var services = new ServiceCollection();
        services.Configure<JsonSerializerOptions>(options =>
        {
            options.PropertyNameCaseInsensitive = true;
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        });
        RegisterRealObjectSerializer(services);
        _instanceServices = services.BuildServiceProvider();
    }

    private void RegisterRealObjectSerializer(ServiceCollection services)
    {
        var workerAsm = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == "Microsoft.Azure.Functions.Worker");
        if (workerAsm == null) return;
        var objSerType = workerAsm.GetType("Microsoft.Azure.Functions.Worker.Converters.ObjectSerializer");
        var sysTextSerType = workerAsm.GetType("Microsoft.Azure.Functions.Worker.Converters.SystemTextJsonObjectSerializer");
        if (objSerType == null || sysTextSerType == null) return;
        var ctor = sysTextSerType.GetConstructor(new[] { typeof(JsonSerializerOptions) });
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var serializer = ctor != null ? ctor.Invoke(new object[] { options }) : Activator.CreateInstance(sysTextSerType);
        services.AddSingleton(objSerType, serializer!);
    }

    public override string InvocationId => Guid.NewGuid().ToString();
    public override string FunctionId => "test-function";
    public override TraceContext TraceContext => new TestTraceContext();
    public override BindingContext BindingContext => new TestBindingContext();
    public override RetryContext RetryContext => new TestRetryContext();
    public override IServiceProvider InstanceServices { get => _instanceServices; set => _instanceServices = value; }
    public override FunctionDefinition FunctionDefinition => new TestFunctionDefinition();
    public override IDictionary<object, object> Items { get => _items; set => _items = value; }
    public override IInvocationFeatures Features => new TestFeatureCollection();
}

public class TestServiceProvider : IServiceProvider
{
    private readonly IServiceProvider _inner;
    private static readonly Type? _objectSerializerType = Type.GetType("Microsoft.Azure.Functions.Worker.Converters.ObjectSerializer, Microsoft.Azure.Functions.Worker");

    public TestServiceProvider(IServiceProvider inner)
    {
        _inner = inner;
    }

    public object? GetService(Type serviceType)
    {
        if (_objectSerializerType != null && serviceType == _objectSerializerType)
        {
            // Return a custom serializer compatible with the expected interface
            return new TestObjectSerializer();
        }
        return _inner.GetService(serviceType);
    }
}

public class TestObjectSerializer // mimics ObjectSerializer
{
    public ValueTask SerializeAsync<T>(T value, Stream stream, CancellationToken cancellationToken = default)
    {
        return new ValueTask(JsonSerializer.SerializeAsync(stream, value, typeof(T), new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }, cancellationToken));
    }
}

public class TestFeatureCollection : IInvocationFeatures
{
    public object this[Type key] { get => null!; set { } }
    public bool IsReadOnly => false;
    public int Revision => 0;
    public TFeature? Get<TFeature>() => default;
    public void Set<TFeature>(TFeature instance) { }
    
    public IEnumerator<KeyValuePair<Type, object>> GetEnumerator()
    {
        return new List<KeyValuePair<Type, object>>().GetEnumerator();
    }
    
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public class TestFunctionDefinition : FunctionDefinition
{
    public override string PathToAssembly => "test-assembly";
    public override string EntryPoint => "test-entry-point";
    public override string Id => "test-id";
    public override string Name => "test-name";
    public override IImmutableDictionary<string, BindingMetadata> InputBindings => ImmutableDictionary<string, BindingMetadata>.Empty;
    public override IImmutableDictionary<string, BindingMetadata> OutputBindings => ImmutableDictionary<string, BindingMetadata>.Empty;
    public override ImmutableArray<FunctionParameter> Parameters => ImmutableArray<FunctionParameter>.Empty;
}

public class TestRetryContext : RetryContext
{
    public override int RetryCount => 0;
    public override int MaxRetryCount => 0;
}

public class TestTraceContext : TraceContext
{
    public override string TraceParent => "test-trace-parent";
    public override string TraceState => "test-trace-state";
}

public class TestBindingContext : BindingContext
{
    public override IReadOnlyDictionary<string, object?> BindingData => new Dictionary<string, object?>();
}

public class TestHttpRequestData : HttpRequestData
{
    private Stream _body;
    private readonly HttpHeadersCollection _headers;

    public TestHttpRequestData(FunctionContext functionContext, string method, string url) 
        : base(functionContext)
    {
        Method = method;
        Url = new Uri(url);
        _headers = new HttpHeadersCollection();
        _body = new MemoryStream();
    }

    public override string Method { get; }
    public override Uri Url { get; }
    public override HttpHeadersCollection Headers => _headers;
    public override Stream Body => _body;
    public override IReadOnlyCollection<IHttpCookie> Cookies => new List<IHttpCookie>();
    public override IEnumerable<ClaimsIdentity> Identities => new List<ClaimsIdentity>();

    public void SetBody(Stream body)
    {
        _body = body;
    }

    public override HttpResponseData CreateResponse()
    {
        return new TestHttpResponseData(FunctionContext);
    }
}

public class TestHttpResponseData : HttpResponseData
{
    private readonly JsonSerializerOptions _jsonOptions;

    public TestHttpResponseData(FunctionContext functionContext) : base(functionContext)
    {
        Headers = new HttpHeadersCollection();
        Body = new MemoryStream();
        StatusCode = HttpStatusCode.OK;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public override HttpStatusCode StatusCode { get; set; }
    public override HttpHeadersCollection Headers { get; set; }
    public override Stream Body { get; set; }
    public override HttpCookies Cookies => new TestHttpCookies();

    public async Task WriteAsJsonAsync<T>(T value)
    {
        // Reset body stream position
        Body.Position = 0;
        Body.SetLength(0);
        
        // Serialize directly to the body stream
        await JsonSerializer.SerializeAsync(Body, value, _jsonOptions);
        
        // Set content type header
        Headers.Add("Content-Type", "application/json");
        
        // Reset position for reading
        Body.Position = 0;
    }

    public async Task WriteStringAsync(string value)
    {
        // Reset body stream position
        Body.Position = 0;
        Body.SetLength(0);
        
        // Write string to body
        var bytes = Encoding.UTF8.GetBytes(value);
        await Body.WriteAsync(bytes, 0, bytes.Length);
        
        // Set content type header
        Headers.Add("Content-Type", "text/plain");
        
        // Reset position for reading
        Body.Position = 0;
    }
}

public class TestHttpCookies : HttpCookies
{
    private readonly List<IHttpCookie> _cookies = new();

    public override void Append(string name, string value)
    {
        _cookies.Add(new TestHttpCookie { Name = name, Value = value });
    }

    public override void Append(IHttpCookie cookie)
    {
        _cookies.Add(cookie);
    }

    public override IHttpCookie CreateNew()
    {
        return new TestHttpCookie();
    }
}

public class TestHttpCookie : IHttpCookie
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Domain { get; set; }
    public string? Path { get; set; }
    public DateTimeOffset? Expires { get; set; }
    public bool? Secure { get; set; }
    public bool? HttpOnly { get; set; }
    public SameSite SameSite { get; set; }
    public double? MaxAge { get; set; }
}