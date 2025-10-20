using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace StudentManagement;

public class Function
{
    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input">The event for the Lambda function handler to process.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
    {
        AmazonDynamoDBClient client = new AmazonDynamoDBClient();
        DynamoDBContext dbContext = new DynamoDBContext(client);

        if (request.RouteKey.Contains("GET /"))
        {
            var data = await dbContext.ScanAsync<Student>(new List<ScanCondition>()).GetRemainingAsync();
            return new APIGatewayHttpApiV2ProxyResponse
            {
                Body = JsonSerializer.Serialize(data),
                StatusCode = 200
            };
        }
        else if (request.RouteKey.Contains("POST /") && request.Body != null)
        {
            var newStudent = JsonSerializer.Deserialize<Student>(request.Body);
            await dbContext.SaveAsync(newStudent);
            return new APIGatewayHttpApiV2ProxyResponse
            {
                Body = $"Student with Id {newStudent?.Id} Created",
                StatusCode = 201
            };
        }
        else
        {
            return new APIGatewayHttpApiV2ProxyResponse
            {
                Body = "Bad Request",
                StatusCode = 400
            };
        }
    }

}
