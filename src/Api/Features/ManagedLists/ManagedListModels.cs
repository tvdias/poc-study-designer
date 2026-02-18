namespace Api.Features.ManagedLists;

// Create Managed List
public record CreateManagedListRequest(Guid ProjectId, string Name, string? Description);
public record CreateManagedListResponse(Guid Id, Guid ProjectId, string Name, string? Description, ManagedListStatus Status);

// Update Managed List
public record UpdateManagedListRequest(string Name, string? Description);
public record UpdateManagedListResponse(Guid Id, Guid ProjectId, string Name, string? Description, ManagedListStatus Status);

// Get Managed Lists
public record GetManagedListsResponse(
    Guid Id, 
    Guid ProjectId, 
    string Name, 
    string? Description, 
    ManagedListStatus Status,
    int ItemCount,
    int QuestionCount,
    DateTime CreatedOn,
    string? CreatedBy);

// Get Managed List by Id
public record GetManagedListByIdResponse(
    Guid Id, 
    Guid ProjectId, 
    string Name, 
    string? Description, 
    ManagedListStatus Status,
    DateTime CreatedOn,
    string? CreatedBy,
    DateTime? ModifiedOn,
    string? ModifiedBy,
    List<ManagedListItemDto> Items);

public record ManagedListItemDto(Guid Id, string Value, string Label, int SortOrder, bool IsActive);

// Managed List Items
public record CreateManagedListItemRequest(string Value, string Label, int SortOrder);
public record CreateManagedListItemResponse(Guid Id, Guid ManagedListId, string Value, string Label, int SortOrder, bool IsActive);

public record UpdateManagedListItemRequest(string Value, string Label, int SortOrder, bool IsActive);
public record UpdateManagedListItemResponse(Guid Id, Guid ManagedListId, string Value, string Label, int SortOrder, bool IsActive);

// Question Assignment
public record AssignManagedListToQuestionRequest(Guid QuestionnaireLineId, Guid ManagedListId);
public record AssignManagedListToQuestionResponse(Guid Id, Guid QuestionnaireLineId, Guid ManagedListId);
