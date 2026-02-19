namespace Api.Features.ManagedLists;

// Request/Response models for subset operations

public record SaveQuestionSelectionRequest(
    Guid ProjectId,
    Guid QuestionnaireLineId,
    Guid ManagedListId,
    List<Guid> SelectedManagedListItemIds
);

public record SaveQuestionSelectionResponse(
    Guid QuestionnaireLineId,
    Guid ManagedListId,
    bool IsFullSelection,
    Guid? SubsetDefinitionId,
    string? SubsetName
);

public record GetSubsetDetailsResponse(
    Guid Id,
    Guid ProjectId,
    Guid ManagedListId,
    string ManagedListName,
    string Name,
    string SignatureHash,
    SubsetStatus Status,
    List<SubsetMembershipDto> Members,
    DateTime CreatedOn,
    string CreatedBy
);

public record SubsetMembershipDto(
    Guid ManagedListItemId,
    string Value,
    string Label,
    int SortOrder
);

public record GetSubsetsForProjectResponse(
    List<SubsetSummaryDto> Subsets
);

public record SubsetSummaryDto(
    Guid Id,
    Guid ManagedListId,
    string ManagedListName,
    string Name,
    int MemberCount,
    DateTime CreatedOn
);

// Delete subset
public record DeleteSubsetResponse(
    Guid SubsetDefinitionId,
    List<Guid> AffectedQuestionIds
);

// Project subset summary (for AC-SYNC-02)
public record ProjectSubsetSummaryResponse(
    Guid ProjectId,
    List<SubsetDetailSummaryDto> Subsets
);

public record SubsetDetailSummaryDto(
    Guid Id,
    Guid ManagedListId,
    string ManagedListName,
    string Name,
    int MemberCount,
    int TotalItemsInList,
    bool IsFull,
    List<string> MemberLabels,
    int QuestionCount,
    DateTime CreatedOn
);
