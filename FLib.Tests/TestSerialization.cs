// ==================== qcbf@qq.com | 2026-03-26 ====================

#if DEBUG
using System.Text;
#endif
using FLib.WorldCores.TimeLogics;

namespace FLib.Tests;

public class TestSerialization
{
    private sealed class Json5ExistingObjectData
    {
        public int Id;
        public string Name = string.Empty;
        public Json5ExistingNestedData Nested = new();
        public List<int> Items = [9];
        public int[] Array = [8];
        public object Dynamic = 7;
    }

    private sealed class Json5ExistingNestedData
    {
        public int Value;
    }

#if DEBUG
    private sealed class DebugCycleNode
    {
        public string Name = string.Empty;
        public DebugCycleNode? Next;
    }

    private sealed class DebugSharedRoot
    {
        public DebugSharedNode? Left;
        public DebugSharedNode? Right;
    }

    private sealed class DebugSharedNode
    {
        public int Value;
    }

    private sealed class DebugReentrantSerializable : IJson5Serializable
    {
        public bool JsonSerialize(StringBuilder jsonText, object serializeObject, object? customData, int indent, Json5SerializeOptionData opData)
        {
            jsonText.Append(Json5.Serialize(serializeObject));
            return true;
        }
    }
#endif

    [Fact]
    public void TimeLogic()
    {
        var tl = new TimeLogic
        {
            Name = "tl",
            EndFrame = 10,
            Tracks = [new ScriptPackInstance(new TimeLogicTrack() { Name = "tlt1", Clips = [new ScriptPackInstance(new TimeLogicClip() { Name = "tlc11", BeginFrame = 0, EndFrame = 10 })] })]
        };

        var json = Json5.Serialize(tl);
        Assert.Equal(json, Json5.Serialize(Json5.Deserialize<TimeLogic>(json)));

        var bytes = BytesPack.Pack(tl);
        Assert.Equal(bytes, BytesPack.Pack(BytesPack.Unpack<TimeLogic>(bytes)));
    }


    [Fact]
    public void BytesPacks()
    {
        TypeAssistant.AddAssemblies(typeof(Team).Assembly);
        var instance = new Team { Value = 123 };
        var pack1 = new ScriptPackInstance(instance);
        Assert.Equal(pack1, Json5.Deserialize<ScriptPackInstance>(Json5.Serialize(pack1)));
        Assert.Equal(pack1, BytesPack.Unpack<ScriptPackInstance>(BytesPack.Pack(pack1)));

        var pack2 = new ScriptPackBytes(instance);
        var scriptPackBytes = Json5.Deserialize<ScriptPackBytes>(Json5.Serialize(pack2));
        Assert.Equal(pack2.Bytes, scriptPackBytes.Bytes);
        Assert.Equal(pack2.Bytes, BytesPack.Unpack<ScriptPackBytes>(BytesPack.Pack(pack2)).Bytes);
    }

    [Theory]
    [InlineData("\"\\\"\\'\\\\\\/\"", "\"'\\/")]
    [InlineData("'\\b\\f\\n\\r\\t\\v\\0'", "\b\f\n\r\t\v\0")]
    [InlineData("'\\x41\\u4F60\\u597d'", "A你好")]
    [InlineData("'\\uD83D\\uDE00'", "😀")]
    [InlineData("'\\\\n'", "\\n")]
    [InlineData("'\\q'", "q")]
    [InlineData("'  text  '", "  text  ")]
    [InlineData("'   '", "   ")]
    [InlineData("''", "")]
    [InlineData("path\\file", "path\\file")]
    public void Json5UnescapesQuotedStrings(string source, string expected)
    {
        Assert.Equal(expected, Json5.Deserialize<string>(source));
        Assert.Equal(expected, Json5.Deserialize(source));
    }

    [Theory]
    [InlineData("\n")]
    [InlineData("\r")]
    [InlineData("\r\n")]
    [InlineData("\u2028")]
    [InlineData("\u2029")]
    public void Json5UnescapesLineContinuations(string newline)
    {
        Assert.Equal("ab", Json5.Deserialize<string>("'a\\" + newline + "b'"));
    }

    [Theory]
    [InlineData("'\\x1'")]
    [InlineData("'\\u12X4'")]
    [InlineData("'\\1'")]
    [InlineData("'\\01'")]
    public void Json5RejectsInvalidEscapes(string source)
    {
        var exception = Assert.ThrowsAny<Exception>(() => Json5.Deserialize<string>(source));
        Assert.IsType<FormatException>(exception.GetBaseException());
    }

    [Fact]
    public void Json5StringEscapesRoundTrip()
    {
        const string value = "quote\"\\n\\\n  ";
        Assert.Equal(value, Json5.Deserialize<string>(Json5.Serialize(value)));
        Assert.Equal("a\n", Json5.Deserialize<Json5ExistingObjectData>("{'\\u004eame':'a\\n'}").Name);
        using var nodes = Json5.DeserializeToSyntaxNodes("'a\\n'");
        Assert.Equal("a\\n", nodes[0].ContentCopyString);
        var dict = new Dictionary<string, string> { ["path\\name"] = "value" };
        Assert.Equal(dict, Json5.Deserialize<Dictionary<string, string>>(Json5.Serialize(dict, EJson5SerializeOption.Compatible)));
    }

    [Fact]
    public void Json5DeserializeToExistingObject()
    {
        var data = new Json5ExistingObjectData
        {
            Id = 1,
            Name = "old",
            Nested = new Json5ExistingNestedData { Value = 2 },
            Items = [9],
            Array = [8],
            Dynamic = 7
        };
        var nested = data.Nested;
        var items = data.Items;
        var oldArray = data.Array;

        var result = Json5.Deserialize("{ Id: 3, Name: 'new', Nested: { Value: 4 }, Items: [1, 2, 3], Array: [5, 6], Dynamic: { A: 1 } }", data);

        Assert.Same(data, result);
        Assert.Equal(3, data.Id);
        Assert.Equal("new", data.Name);
        Assert.Same(nested, data.Nested);
        Assert.Equal(4, data.Nested.Value);
        Assert.Same(items, data.Items);
        Assert.Equal([1, 2, 3], data.Items);
        Assert.NotSame(oldArray, data.Array);
        Assert.Equal([5, 6], data.Array);
        var dynamicDict = Assert.IsType<Dictionary<string, object>>(data.Dynamic);
        Assert.Equal(1L, dynamicDict["A"]);
    }

    [Fact]
    public void Json5DeserializeToExistingDictionary()
    {
        var dict = new Dictionary<string, int> { ["Old"] = 1 };

        var result = Json5.Deserialize("{ New: 2 }", dict);

        Assert.Same(dict, result);
        Assert.Equal(1, dict["Old"]);
        Assert.Equal(2, dict["New"]);
    }

    [Fact]
    public void Json5DeserializeToExistingList()
    {
        var list = new List<int> { 9 };

        var result = Json5.Deserialize("[1, 2, 3]", list);

        Assert.Same(list, result);
        Assert.Equal([1, 2, 3], list);
    }

#if DEBUG
    [Fact]
    public void DebugSerializerReportsCircularReference()
    {
        var root = new DebugCycleNode { Name = "root" };
        root.Next = root;

        var exception = Assert.Throws<InvalidOperationException>(() => Json5.Serialize(root));

        Assert.Contains("Json5 serialization recursion exceeded", exception.Message);
        Assert.Contains(nameof(DebugCycleNode), exception.Message);
    }

    [Fact]
    public void DebugSerializerAllowsSharedNonCircularValue()
    {
        var shared = new DebugSharedNode { Value = 1 };
        var root = new DebugSharedRoot { Left = shared, Right = shared };

        var json = Json5.Serialize(root);

        Assert.Contains("Left", json);
        Assert.Contains("Right", json);
    }

    [Fact]
    public void DebugSerializerReportsCustomSerializerReentry()
    {
        var value = new DebugReentrantSerializable();

        var exception = Assert.Throws<InvalidOperationException>(() => Json5.Serialize(value));

        Assert.Contains("Json5 serialization recursion exceeded", exception.Message);
        Assert.Contains(nameof(DebugReentrantSerializable), exception.Message);
    }
#endif
}
