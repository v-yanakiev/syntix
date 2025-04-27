using CommonAI;
using Microsoft.AspNetCore.Http;
using Models;
using Models.Mappings;
using OpenAI.Chat;

namespace OpenAIIntegration;

public partial class OpenAIResponseGetter
{
    private async Task<bool> CheckUserBalance(UserInfo user, HttpContext httpContext)
    {
        if (user.Balance > 0 || user.IsInValidTrial())
            return true;

        await CommonAITooling.SendMessageToUserChat(httpContext,
            "SYSTEM MESSAGE: The balance in your account has ran out! " +
            "If you want to continue, please fund your account.");

        return false;
    }

    private void UpdateUserBalance(
        StreamingChatCompletionUpdate update,
        UserInfo user,
        string languageModel)
    {
        if (update.Usage == null) return;

        var cachedInputTokenCount = update.Usage.InputTokenDetails.CachedTokenCount;
        var inputTokenCount = update.Usage.InputTokenCount - cachedInputTokenCount;
        var outputTokenCount = update.Usage.OutputTokenCount;

        var cachedInputCost = cachedInputTokenCount *
                              LLMNameToTokenPrice.CalculatePrice($"{languageModel}-in-cached");
        var inputCost = inputTokenCount *
                        LLMNameToTokenPrice.CalculatePrice($"{languageModel}-in");
        var outputCost = outputTokenCount *
                         LLMNameToTokenPrice.CalculatePrice($"{languageModel}-out");

        var newBalance = user.Balance - (cachedInputCost + inputCost + outputCost);
        _ = CommonAITooling.AdjustUserBalance(user, newBalance, contextFactory);
    }
}