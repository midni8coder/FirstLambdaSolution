using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace StudentManagement;

public class Function
{
    private readonly DynamoDBContext _dbContext;

    public Function()
    {
        var client = new AmazonDynamoDBClient();
        var config = new DynamoDBContextConfig { ConsistentRead = true };
        _dbContext = new DynamoDBContext(client, config);
    }

    public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
    {
        if (request.RouteKey.Contains("GET /students/search"))
        {
            string? name = request.QueryStringParameters?["name"]?.ToLower();
            if (string.IsNullOrEmpty(name))
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = "Name parameter is required",
                    StatusCode = 400
                };
            }

            var config = new ScanOperationConfig
            {
                Filter = new ScanFilter()
            };

            config.Filter.AddCondition("first_name", ScanOperator.Contains, name);
            config.Filter.AddCondition("last_name", ScanOperator.Contains, name);

            var search = _dbContext.FromScanAsync<Student>(config);
            var searchResults = await search.GetRemainingAsync();

            if (!searchResults.Any())
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = "Record Not Found",
                    StatusCode = 404
                };
            }

            return new APIGatewayHttpApiV2ProxyResponse
            {
                Body = JsonSerializer.Serialize(searchResults),
                StatusCode = 200
            };
        }
        else if (request.RouteKey.Contains("GET /"))
        {
            var data = await _dbContext.ScanAsync<Student>(new List<ScanCondition>()).GetRemainingAsync();
            return new APIGatewayHttpApiV2ProxyResponse
            {
                Body = JsonSerializer.Serialize(data),
                StatusCode = 200
            };
        }
        else if (request.RouteKey.Contains("POST /") && request.Body != null)
        {
            var newStudent = JsonSerializer.Deserialize<Student>(request.Body);
            await _dbContext.SaveAsync(newStudent);
            return new APIGatewayHttpApiV2ProxyResponse
            {
                Body = $"Student with Id {newStudent?.Id} Created",
                StatusCode = 201
            };
        }
        else if (request.RouteKey.Contains("DELETE /students/"))
        {
            string? studentId = request.PathParameters?["id"];
            if (string.IsNullOrEmpty(studentId))
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = "Student ID is required",
                    StatusCode = 400
                };
            }

            // Check if student exists
            var existingStudent = await _dbContext.LoadAsync<Student>(studentId);
            if (existingStudent == null)
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = "Student not found",
                    StatusCode = 404
                };
            }

            // Delete the student
            await _dbContext.DeleteAsync<Student>(studentId);

            // Return 204 No Content - successful deletion without response body
            return new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = 204
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
