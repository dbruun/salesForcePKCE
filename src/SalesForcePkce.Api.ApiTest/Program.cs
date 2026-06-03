using OpenAI.Responses;
using Azure.AI.Extensions.OpenAI;

// Discover API surface via reflection
var optType = typeof(CreateResponseOptions);
Console.WriteLine("=== CreateResponseOptions Properties ===");
foreach (var prop in optType.GetProperties())
    Console.WriteLine($"  {prop.PropertyType.Name} {prop.Name}");

Console.WriteLine("\n=== ResponseResult Properties ===");
foreach (var prop in typeof(ResponseResult).GetProperties())
    Console.WriteLine($"  {prop.PropertyType.Name} {prop.Name}");

Console.WriteLine("\n=== ResponseTool Methods ===");
foreach (var method in typeof(ResponseTool).GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
    Console.WriteLine($"  {method.ReturnType.Name} {method.Name}({string.Join(", ", method.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"))})");

Console.WriteLine("\n=== InternalOutputItemFunctionToolCall Properties ===");
foreach (var prop in typeof(InternalOutputItemFunctionToolCall).GetProperties())
    Console.WriteLine($"  {prop.PropertyType.Name} {prop.Name}");

Console.WriteLine("\n=== FunctionToolCallOutput Constructors ===");
foreach (var ctor in typeof(FunctionToolCallOutput).GetConstructors())
    Console.WriteLine($"  ({string.Join(", ", ctor.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"))})");
