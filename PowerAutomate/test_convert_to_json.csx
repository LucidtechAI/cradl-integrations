#r "nuget: Newtonsoft.Json, 13.0.3"

using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Collections.Generic;
using System.IO;

// Custom JSON writer that produces Python-style output: spaces after colons and commas
public class PythonStyleJsonWriter : JsonTextWriter
{
    private bool _propertyNameJustWritten = false;

    public PythonStyleJsonWriter(TextWriter writer) : base(writer)
    {
        // No special formatting needed
    }

    // Override to add space after comma between values
    protected override void WriteValueDelimiter()
    {
        WriteRaw(", ");
    }

    public override void WritePropertyName(string name)
    {
        base.WritePropertyName(name);
        _propertyNameJustWritten = true;
    }

    public override void WritePropertyName(string name, bool escape)
    {
        base.WritePropertyName(name, escape);
        _propertyNameJustWritten = true;
    }

    private void WriteSpaceAfterColonIfNeeded()
    {
        if (_propertyNameJustWritten)
        {
            WriteRaw(" ");
            _propertyNameJustWritten = false;
        }
    }

    public override void WriteValue(string value)
    {
        WriteSpaceAfterColonIfNeeded();
        base.WriteValue(value);
    }

    public override void WriteValue(int value)
    {
        WriteSpaceAfterColonIfNeeded();
        base.WriteValue(value);
    }

    public override void WriteValue(long value)
    {
        WriteSpaceAfterColonIfNeeded();
        base.WriteValue(value);
    }

    public override void WriteValue(double value)
    {
        WriteSpaceAfterColonIfNeeded();
        base.WriteValue(value);
    }

    public override void WriteValue(decimal value)
    {
        WriteSpaceAfterColonIfNeeded();
        base.WriteValue(value);
    }

    public override void WriteValue(bool value)
    {
        WriteSpaceAfterColonIfNeeded();
        base.WriteValue(value);
    }

    public override void WriteNull()
    {
        WriteSpaceAfterColonIfNeeded();
        base.WriteNull();
    }

    public override void WriteStartObject()
    {
        WriteSpaceAfterColonIfNeeded();
        base.WriteStartObject();
    }

    public override void WriteStartArray()
    {
        WriteSpaceAfterColonIfNeeded();
        base.WriteStartArray();
    }

    protected override void WriteIndent()
    {
        // Override to prevent any indentation/newlines
    }
}

// Helper method to serialize with Python-style formatting
static string SerializePythonStyle(JToken token)
{
    var sb = new StringBuilder();
    using (var sw = new StringWriter(sb))
    using (var writer = new PythonStyleJsonWriter(sw))
    {
        token.WriteTo(writer);
    }
    return sb.ToString();
}

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
    Console.WriteLine("Newtonsoft Compact (no spaces):");
    Console.WriteLine(compact);
    Console.WriteLine();

    // Using our custom Python-style writer
    string pythonStyle = SerializePythonStyle(testCase.Value);
    Console.WriteLine("Custom Python-style (space after ':' and ','):");
    Console.WriteLine(pythonStyle);
    Console.WriteLine();

    // Show byte representation for verification
    byte[] compactBytes = Encoding.UTF8.GetBytes(compact);
    byte[] pythonBytes = Encoding.UTF8.GetBytes(pythonStyle);
    Console.WriteLine($"Compact byte length: {compactBytes.Length}");
    Console.WriteLine($"Python-style byte length: {pythonBytes.Length}");
    Console.WriteLine($"Difference: {pythonBytes.Length - compactBytes.Length} bytes (added spaces)");
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

