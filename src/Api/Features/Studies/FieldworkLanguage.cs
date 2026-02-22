using Api.Features.FieldworkMarkets;
using Api.Features.Shared;

namespace Api.Features.Studies;

/// <summary>
/// Represents a language configuration for a Study's fieldwork market.
/// When a Study's FieldworkMarket changes, all related FieldworkLanguage records
/// for the previous market must be deleted (mirrors KTR_FieldworkLanguages in the POC).
/// </summary>
public class FieldworkLanguage : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid StudyId { get; set; }
    public Study Study { get; set; } = null!;
    public Guid FieldworkMarketId { get; set; }
    public FieldworkMarket FieldworkMarket { get; set; } = null!;
    public required string LanguageCode { get; set; }
    public required string LanguageName { get; set; }
    public bool IsActive { get; set; } = true;
}
