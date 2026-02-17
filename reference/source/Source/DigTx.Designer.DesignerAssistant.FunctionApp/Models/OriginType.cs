namespace DigTx.Designer.FunctionApp.Models;

using System.Text.Json.Serialization;
using DigTx.Designer.FunctionApp.Core.Converts;

[JsonConverter(typeof(CustomJsonStringEnumConverter<OriginType>))]
public enum OriginType
{
    QuestionBank = 1,
    New = 2,
}
