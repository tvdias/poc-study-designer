namespace DigTx.Designer.FunctionApp.Tests.Services;

using System.Reflection;
using DigTx.Designer.DesignerAssistant.FunctionApp.Models.Requests;
using DigTx.Designer.FunctionApp.Exceptions;
using DigTx.Designer.FunctionApp.Models;
using DigTx.Designer.FunctionApp.Services;

public partial class ProjectServiceTests
{
    private static void InvokeStaticPrivate(string methodName, params object[] args)
    {
        var mi = typeof(ProjectService).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static)
                 ?? throw new InvalidOperationException($"Method {methodName} not found.");
        mi.Invoke(null, args);
    }

    private static void InvokeInstancePrivate(object instance, string methodName, params object[] args)
    {
        var mi = typeof(ProjectService).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance)
                 ?? throw new InvalidOperationException($"Method {methodName} not found.");
        mi.Invoke(instance, args);
    }

    [Fact]
    public void ValidateSequencialQuestionDisplayOrder_Sequential_DoesNotThrow()
    {
        var list = new List<QuestionCreationRequest>
            {
                new()  {
                    Origin = OriginType.New,
                    Id = Guid.NewGuid(),
                    DisplayOrder = 1,
                    VariableName = "Q_VAR",
                    Title = "Question Title",
                    Text = "Question Text Long",
                    ScripterNotes = "Some notes",
                    QuestionRationale = "Rationale",
                    IsDummyQuestion = true,
                    QuestionType = QuestionType.DisplayScreen
                        },
                new()  {
                    Origin = OriginType.New,
                    Id = Guid.NewGuid(),
                    DisplayOrder = 2,
                    VariableName = "Q_VAR",
                    Title = "Question Title",
                    Text = "Question Text Long",
                    ScripterNotes = "Some notes",
                    QuestionRationale = "Rationale",
                    IsDummyQuestion = true,
                    QuestionType = QuestionType.DisplayScreen
                        }
            };

        var ex = Record.Exception(() =>
            InvokeStaticPrivate("ValidateSequencialQuestionDisplayOrder", list));

        Assert.Null(ex);
    }

    [Theory]
    [InlineData(new[] { 2, 3, 4 })]
    [InlineData(new[] { 1, 2, 2 })]
    public void ValidateSequencialQuestionDisplayOrder_Invalid_Throws(int[] orders)
    {
        var list = orders.Select(o =>
            new QuestionCreationRequest
            {
                Origin = OriginType.New,
                Id = Guid.NewGuid(),
                DisplayOrder = o,
                VariableName = "Q_VAR",
                Title = "Question Title",
                Text = "Question Text Long",
                ScripterNotes = "Some notes",
                QuestionRationale = "Rationale",
                IsDummyQuestion = true,
                QuestionType = QuestionType.DisplayScreen
            }).ToList();

        var ex = Assert.Throws<TargetInvocationException>(() =>
            InvokeStaticPrivate("ValidateSequencialQuestionDisplayOrder", list));

        Assert.IsType<InvalidRequestException>(ex.InnerException);
        Assert.Contains("DisplayOrder in Questions must be sequential", ex.InnerException!.Message);
    }

    [Fact]
    public void ValidateAnswersRequest_EmptyList_NoException()
    {
        var answers = new List<AnswerCreationRequest>();

        var ex = Record.Exception(() =>
            InvokeInstancePrivate(_projectService, "ValidateAnswersRequest", answers));

        Assert.Null(ex);
    }

    [Fact]
    public void ValidateAnswersRequest_NullList_NoException()
    {
        var ex = Record.Exception(() =>
            InvokeInstancePrivate(_projectService, "ValidateAnswersRequest", [null!]));

        Assert.Null(ex);
    }

    [Fact]
    public void ValidateAnswersRequest_MissingName_Throws()
    {
        var answers = new List<AnswerCreationRequest>
            {
                new() { Name = "", Text = "Txt" }
            };

        var ex = Assert.Throws<TargetInvocationException>(() =>
            InvokeInstancePrivate(_projectService, "ValidateAnswersRequest", answers));

        Assert.IsType<InvalidRequestException>(ex.InnerException);
        Assert.Contains("Answer Name is required", ex.InnerException!.Message);
    }

    [Fact]
    public void ValidateAnswersRequest_MissingText_Throws()
    {
        var answers = new List<AnswerCreationRequest>
            {
                new() { Name = "A1", Text = "" }
            };

        var ex = Assert.Throws<TargetInvocationException>(() =>
            InvokeInstancePrivate(_projectService, "ValidateAnswersRequest", answers));

        Assert.IsType<InvalidRequestException>(ex.InnerException);
        Assert.Contains("Answer Text is required", ex.InnerException!.Message);
    }

    [Fact]
    public void ValidateNewQuestions_StandardQuestion_Throws()
    {
        var questions = new List<QuestionCreationRequest>
            {
                new()
                {
                    DisplayOrder = 1,
                    Origin = OriginType.New,
                    StandardOrCustom = StandardOrCustomType.Standard,
                    VariableName = "VAR1",
                    Title = "Title",
                    Text = "Text",
                    ScripterNotes = "Notes",
                    QuestionRationale = "Rationale",
                    QuestionType = QuestionType.DisplayScreen,
                    Answers = new List<AnswerCreationRequest>(),
                }
            };

        var ex = Assert.Throws<TargetInvocationException>(() =>
            InvokeInstancePrivate(_projectService, "ValidateNewQuestions", questions));

        Assert.IsType<InvalidRequestException>(ex.InnerException);
        Assert.Contains("Only Custom Questions can be created", ex.InnerException!.Message);
    }

    [Fact]
    public void ValidateNewQuestions_CustomClosedQuestionMissingAnswers_Throws()
    {
        var questions = new List<QuestionCreationRequest>
            {
                new()
                {
                    DisplayOrder = 1,
                    Origin = OriginType.New,
                    StandardOrCustom = StandardOrCustomType.Custom,
                    VariableName = "VARX",
                    Title = "Some Title",
                    Text = "Some Text",
                    ScripterNotes = "Some Notes",
                    QuestionRationale = "Because",
                    QuestionType = (QuestionType)999, // Fallback to force not contained if enum value unknown
                    Answers = [] // empty, should trigger
                }
            };

        var ex = Assert.Throws<TargetInvocationException>(() =>
            InvokeInstancePrivate(_projectService, "ValidateNewQuestions", questions));

        Assert.IsType<InvalidRequestException>(ex.InnerException);
        Assert.Contains("Answers are required", ex.InnerException!.Message);
    }
}
