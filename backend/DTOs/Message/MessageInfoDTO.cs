using Models;

namespace DTOs.MessageDTOs;


public record MessageInfoDTO(string Content, string Role)
{
    public static MessageInfoDTO FromMessage(Message message)
    {
        return new MessageInfoDTO(message.Content, message.Role);
    }
}