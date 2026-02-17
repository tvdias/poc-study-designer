namespace DigTx.Designer.FunctionApp.Models;

using System.Text.Json.Serialization;
using DigTx.Designer.FunctionApp.Core.Converts;

[JsonConverter(typeof(CustomJsonStringEnumConverter<StandardOrCustomType>))]
public enum StandardOrCustomType
{
    Standard = 0,
    Custom = 1,
}
