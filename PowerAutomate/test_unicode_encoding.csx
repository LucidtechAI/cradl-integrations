#r "nuget: Newtonsoft.Json, 13.0.3"

using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Text;
using System.IO;

// Custom JSON writer that matches Python's json.dumps() behavior
public class PythonStyleJsonWriter : JsonTextWriter
{
    private bool _propertyNameJustWritten = false;

    public PythonStyleJsonWriter(TextWriter writer) : base(writer)
    {
        // Escape non-ASCII characters as \uXXXX to match Python's json.dumps() behavior
        this.StringEscapeHandling = StringEscapeHandling.EscapeNonAscii;
    }

    protected override void WriteValueDelimiter() => WriteRaw(", ");

    public override void WritePropertyName(string name) { base.WritePropertyName(name); _propertyNameJustWritten = true; }
    public override void WritePropertyName(string name, bool escape) { base.WritePropertyName(name, escape); _propertyNameJustWritten = true; }

    private void InjectSpaceAfterColon()
    {
        if (_propertyNameJustWritten) { WriteRaw(" "); _propertyNameJustWritten = false; }
    }

    public override void WriteValue(object value) { InjectSpaceAfterColon(); base.WriteValue(value); }
    public override void WriteValue(string value) { InjectSpaceAfterColon(); base.WriteValue(value); }
    public override void WriteValue(int value) { InjectSpaceAfterColon(); base.WriteValue(value); }
    public override void WriteValue(long value) { InjectSpaceAfterColon(); base.WriteValue(value); }
    public override void WriteValue(double value) { InjectSpaceAfterColon(); base.WriteValue(value); }
    public override void WriteValue(decimal value) { InjectSpaceAfterColon(); base.WriteValue(value); }
    public override void WriteValue(bool value) { InjectSpaceAfterColon(); base.WriteValue(value); }
    public override void WriteNull() { InjectSpaceAfterColon(); base.WriteNull(); }
    public override void WriteStartObject() { InjectSpaceAfterColon(); base.WriteStartObject(); }
    public override void WriteStartArray() { InjectSpaceAfterColon(); base.WriteStartArray(); }

    protected override void WriteIndent() { }
}

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

Console.WriteLine("=".PadRight(80, '='));
Console.WriteLine("Unicode Encoding Test - Matching Python's json.dumps()");
Console.WriteLine("=".PadRight(80, '='));
Console.WriteLine();

// Test case from Python example
var testObj = new JObject { ["foo"] = "Ø" };

Console.WriteLine("Test object: {\"foo\": \"Ø\"}");
Console.WriteLine();

// Serialize using our Python-style writer
string jsonString = SerializePythonStyle(testObj);
Console.WriteLine("C# Python-style serialization:");
Console.WriteLine(jsonString);
Console.WriteLine();

// Get the bytes
byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonString);
Console.WriteLine("Byte representation:");
Console.WriteLine(BitConverter.ToString(jsonBytes).Replace("-", " "));
Console.WriteLine();

Console.WriteLine("Expected from Python:");
Console.WriteLine(@"{""foo"": ""\u00d8""}");
Console.WriteLine();

Console.WriteLine("Expected bytes from Python:");
Console.WriteLine(@"7B 22 66 6F 6F 22 3A 20 22 5C 75 30 30 64 38 22 7D");
Console.WriteLine();

Console.WriteLine("Match: " + (jsonString == @"{""foo"": ""\u00d8""}"));
Console.WriteLine();

// More comprehensive test
Console.WriteLine("=".PadRight(80, '='));
Console.WriteLine("Comprehensive Unicode Test");
Console.WriteLine("=".PadRight(80, '='));
Console.WriteLine();

var norwegianTest = new JObject
{
    ["name"] = "Øyvind",
    ["city"] = "Oslo",
    ["letters"] = "ÆØÅæøå"
};

string norwegianJson = SerializePythonStyle(norwegianTest);
Console.WriteLine("Norwegian characters test:");
Console.WriteLine(norwegianJson);
Console.WriteLine();

byte[] norwegianBytes = Encoding.UTF8.GetBytes(norwegianJson);
Console.WriteLine("Length: " + norwegianBytes.Length + " bytes");
Console.WriteLine();

Console.WriteLine("This matches Python's json.dumps() behavior:");
Console.WriteLine("- Non-ASCII characters are escaped as \\uXXXX");
Console.WriteLine("- Spaces after colons and commas");
Console.WriteLine("- Compact format (no indentation)");
