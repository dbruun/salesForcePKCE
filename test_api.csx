using OpenAI.Responses;
using Azure.AI.Extensions.OpenAI;

// Check what types are available
CreateResponseOptions opts = new();
Console.WriteLine(string.Join(", ", typeof(CreateResponseOptions).GetProperties().Select(p => p.Name)));
Console.WriteLine("---");
Console.WriteLine(string.Join(", ", typeof(ResponseResult).GetProperties().Select(p => p.Name)));
