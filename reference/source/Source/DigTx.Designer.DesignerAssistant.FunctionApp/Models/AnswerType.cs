namespace DigTx.Designer.FunctionApp.Models;

using System.Text.Json.Serialization;
using DigTx.Designer.FunctionApp.Core.Converts;

[JsonConverter(typeof(CustomJsonStringEnumConverter<AnswerType>))]
public enum AnswerType
{
    Column = 847610001,

    Row = 847610000,
}
