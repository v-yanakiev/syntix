namespace DTOs.MessageDTOs;

public record GetCompletionDTO(
    List<CreateMessageDTO> Messages,
    Guid ChatId,
    string LanguageModel,
    long SelectedTemplateId);
    