namespace DigTx.Designer.FunctionApp.Models;

using System.Text.Json.Serialization;
using DigTx.Designer.FunctionApp.Core.Converts;

[JsonConverter(typeof(CustomJsonStringEnumConverter<QuestionType>))]
public enum QuestionType
{
    DisplayScreen = 5,

    LargeTextInput = 6,

    Logic = 8,

    MultipleChoice = 4,

    MultipleChoiceMatrix = 12,

    NumericInput = 1,

    NumericMatrix = 7,

    SingleChoice = 0,

    SingleChoiceMatrix = 2,

    SmallTextInput = 3,

    TextInputMatrix = 13,
}
