using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using System.Text.Json.Serialization;
using System.Text.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace HelloWorld;

public class Function
{

    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input">The event for the Lambda function handler to process.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    public string FunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
    {
        Console.WriteLine(JsonSerializer.Serialize(request));
        string name = string.Empty;

        if (request != null && request.QueryStringParameters != null)
        {
            request.QueryStringParameters.TryGetValue("name", out name);
        }
        else if (request != null && request.PathParameters != null)
        {
            request.PathParameters.TryGetValue("name", out name);
        }

        if (string.IsNullOrEmpty(name))
            name = "John Doe";

        var message = $"Hello {name}, from AWS Lambda";
        return message;
    }

}
