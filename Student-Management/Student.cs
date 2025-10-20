using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentManagement
{
    [DynamoDBTable("students")]
    public class Student
    {
        [DynamoDBHashKey("id")]
        public string? Id { get; set; }
        [DynamoDBProperty("first_name")]
        public string? FirstName { get; set; }
        [DynamoDBProperty("last_name")]
        public string? LastName { get; set; }
        [DynamoDBProperty("class")]
        public int Class { get; set; }
    }
}
