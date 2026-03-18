#r "nuget: Newtonsoft.Json, 13.0.3"

using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Collections.Generic;

// Test objects - add more test cases here
var testCases = new Dictionary<string, JObject>
{
    ["Simple object"] = JObject.Parse(@"{""foo"": ""bar"", ""num"": 123}"),

    ["Nested object"] = JObject.Parse(@"{
        ""output"": {
            ""name"": ""John Doe"",
            ""age"": 30
        },
        ""context"": {
            ""actionId"": ""cradl:action:abc123"",
            ""runId"": ""run_456""
        }
    }"),

    ["Object with array"] = JObject.Parse(@"{
        ""items"": [""apple"", ""banana"", ""cherry""],
        ""count"": 3
    }"),

    ["Complex nested"] = JObject.Parse(@"{
        ""user"": {
            ""profile"": {
                ""firstName"": ""Jane"",
                ""lastName"": ""Smith""
            },
            ""settings"": {
                ""notifications"": true,
                ""theme"": ""dark""
            }
        },
        ""timestamp"": 1234567890
    }"),

    ["Mixed types"] = JObject.Parse(@"{
        ""string"": ""hello"",
        ""number"": 42,
        ""float"": 3.14,
        ""boolean"": true,
        ""null_value"": null,
        ""empty_object"": {},
        ""empty_array"": []
    }"),

    ["Webhook body example"] = JObject.Parse(@"{
        ""output"": {
            ""invoice_number"": ""INV-001"",
            ""total_amount"": 1500.50
        },
        ""context"": {
            ""actionId"": ""cradl:action:abc"",
            ""runId"": ""run_xyz"",
            ""documentId"": ""cradl:document:doc123""
        }
    }")
};

Console.WriteLine("=".PadRight(80, '='));
Console.WriteLine("JSON Serialization Test");
Console.WriteLine("=".PadRight(80, '='));
Console.WriteLine();

foreach (var testCase in testCases)
{
    Console.WriteLine($"Test: {testCase.Key}");
    Console.WriteLine("-".PadRight(80, '-'));

    // Using Newtonsoft.Json default (compact, no spaces)
    string compact = testCase.Value.ToString(Newtonsoft.Json.Formatting.None);
    Console.WriteLine("Compact (no spaces):");
    Console.WriteLine(compact);
    Console.WriteLine();

    // What we want: Python-style (space after colon and comma)
    // This is what Python's json.dumps() produces by default
    Console.WriteLine("Expected Python-style (space after ':' and ','):");
    string pythonStyle = Newtonsoft.Json.JsonConvert.SerializeObject(
        testCase.Value,
        Newtonsoft.Json.Formatting.None
    );
    // For now, just show what we're getting
    Console.WriteLine(pythonStyle);
    Console.WriteLine();

    // Show byte representation for verification
    byte[] bytes = Encoding.UTF8.GetBytes(pythonStyle);
    Console.WriteLine($"Byte length: {bytes.Length}");
    Console.WriteLine($"First 50 bytes as hex: {BitConverter.ToString(bytes, 0, Math.Min(50, bytes.Length)).Replace("-", " ")}");
    Console.WriteLine();

    Console.WriteLine("=".PadRight(80, '='));
    Console.WriteLine();
}

// Helper to show what Python's default format looks like
Console.WriteLine("REFERENCE: Python's json.dumps() default format:");
Console.WriteLine(@"Python uses separators=(', ', ': ') by default");
Console.WriteLine(@"Example: {""key"": ""value"", ""num"": 123}");
Console.WriteLine(@"         ^^^^^          ^^^^^");
Console.WriteLine(@"         Space after colon and comma");
Console.WriteLine();

// Show what we're currently getting vs what we want
var simpleTest = JObject.Parse(@"{""a"":""b"",""c"":123}");
Console.WriteLine("Current Newtonsoft output (compact):");
Console.WriteLine(simpleTest.ToString(Newtonsoft.Json.Formatting.None));
Console.WriteLine();
Console.WriteLine("Python default output (what we need):");
Console.WriteLine(@"{""a"": ""b"", ""c"": 123}");
Console.WriteLine();

Console.WriteLine("Press any key to exit...");
Console.ReadKey();
