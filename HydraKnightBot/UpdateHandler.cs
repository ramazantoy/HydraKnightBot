using System.Diagnostics;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace HydraKnightBot;

public class UpdateHandler : IUpdateHandler
{
    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        Console.WriteLine(update.Type);

        if (update.Type == UpdateType.Message && update.Message?.Text != null)
        {
            await HandleMessage(botClient, update.Message, cancellationToken);
        }
        else if (update.Type == UpdateType.ChatMember)
        {
            await HandleNewMember(botClient, update.ChatMember, cancellationToken);
        }
    }

    private async Task HandleMessage(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        var command = message.Text.Split(' ')[0].ToLower();
        var args = message.Text.Split(' ').Skip(1).ToArray();


        switch (command)
        {
            case "/ban":
                await BanUser(botClient, message, args, cancellationToken);
                break;
            case "/mute":
                await MuteUser(botClient, message, args, cancellationToken);
                break;
            case "/unmute":
                await UnmuteUser(botClient, message, args, cancellationToken);
                break;
            case "/unban":
                await UnbanUser(botClient, message, args, cancellationToken);
                break;
        }
    }


    private async Task<bool> IsAdmin(ITelegramBotClient botClient, long userId, long chatId)
    {
        try
        {
            var chatMember = await botClient.GetChatMemberAsync(chatId, userId);

            return chatMember.Status == ChatMemberStatus.Creator || chatMember.Status == ChatMemberStatus.Administrator;
        }
        catch (Exception e)
        {
            return false;
        }
    }


    private async Task UnbanUser(ITelegramBotClient botClient, Message message, string[] args,
        CancellationToken cancellationToken)
    {
        var isAdmin = message.From != null && await IsAdmin(botClient, message.From.Id, message.Chat.Id);

        if (!isAdmin)
        {
            await botClient.SendTextMessageAsync(message.Chat.Id,
                "Üzgünüm yalnızca yöneticiler tarafından kullanılabilirim.", cancellationToken: cancellationToken,
                replyToMessageId: message.MessageId);
            return;
        }

        long userId = 0;
        string? userName = "";

        if (message.ReplyToMessage?.From != null)
        {
            userId = message.ReplyToMessage.From.Id;
            userName = message.ReplyToMessage.From.Username ?? message.ReplyToMessage.From.FirstName;
        }
        else if (args.Length > 0)
        {
            userName = args[0].TrimStart('@');
            try
            {
                var chatMember = await botClient.GetChatMemberAsync(message.Chat.Id, userId, cancellationToken);
                userId = chatMember.User.Id;
            }
            catch (Exception)
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Kullanıcı bulunamadı.",
                    cancellationToken: cancellationToken);
                return;
            }
        }
        else
        {
            await botClient.SendTextMessageAsync(message.Chat.Id,
                "Lütfen bir kullanıcı adı belirtin veya bir mesajı yanıtlayın.", cancellationToken: cancellationToken);
            return;
        }

        try
        {
            await botClient.UnbanChatMemberAsync(message.Chat.Id, userId, cancellationToken: cancellationToken);
            await botClient.SendTextMessageAsync(message.Chat.Id, $"{userName} kullanıcısının banı kaldırıldı.",
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            await botClient.SendTextMessageAsync(message.Chat.Id, $"Kullanıcının banı kaldırılamadı: {ex.Message}",
                cancellationToken: cancellationToken);
        }

        Console.WriteLine(userName);
    }

    private async Task BanUser(ITelegramBotClient botClient, Message message, string[] args,
        CancellationToken cancellationToken)
    {
        var isAdmin = message.From != null && await IsAdmin(botClient, message.From.Id, message.Chat.Id);

        if (!isAdmin)
        {
            await botClient.SendTextMessageAsync(message.Chat.Id,
                "Üzgünüm yalnızca yöneticiler tarafından kullanılabilirim.", cancellationToken: cancellationToken,
                replyToMessageId: message.MessageId);
            return;
        }


        long userId = 0;
        var userName = "";

        if (message.ReplyToMessage?.From != null)
        {
            userId = message.ReplyToMessage.From.Id;
            userName = message.ReplyToMessage.From.Username;
        }
        else if (args.Length > 0)
        {
            try
            {
                var chatMember = await botClient.GetChatMemberAsync(message.Chat.Id, userId, cancellationToken);
                userId = chatMember.User.Id;
            }
            catch (Exception)
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Kullanıcı bulunamadı.",
                    cancellationToken: cancellationToken);
                return;
            }
        }
        else
        {
            await botClient.SendTextMessageAsync(message.Chat.Id,
                "Lütfen bir kullanıcı adı belirtin veya bir mesajı yanıtlayın.", cancellationToken: cancellationToken);
            return;
        }

        try
        {
            await botClient.BanChatMemberAsync(message.Chat.Id, userId, cancellationToken: cancellationToken);
            await botClient.SendTextMessageAsync(message.Chat.Id, $"{userName} banlandı.",
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            await botClient.SendTextMessageAsync(message.Chat.Id, $"Kullanıcı banlanamadı: {ex.Message}",
                cancellationToken: cancellationToken);
        }

        Console.WriteLine(userName);
    }


    private async Task UnmuteUser(ITelegramBotClient botClient, Message message, string[] args,
        CancellationToken cancellationToken)
    {
        var isAdmin = message.From != null && await IsAdmin(botClient, message.From.Id, message.Chat.Id);

        if (!isAdmin)
        {
            await botClient.SendTextMessageAsync(message.Chat.Id,
                "Üzgünüm yalnızca yöneticiler tarafından kullanılabilirim.", cancellationToken: cancellationToken,
                replyToMessageId: message.MessageId);
            return;
        }

        long userId = 0;
        var firstName = "";

        if (message.ReplyToMessage?.From != null)
        {
            userId = message.ReplyToMessage.From.Id;
            firstName = message.ReplyToMessage.From.FirstName;
        }
        else if (args.Length > 0)
        {
            firstName = args[0].TrimStart('@');
            try
            {
                var chatMember = await botClient.GetChatMemberAsync(message.Chat.Id, userId, cancellationToken);
                userId = chatMember.User.Id;
                firstName = chatMember.User.Username ?? chatMember.User.FirstName;
            }
            catch (Exception)
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, $"Kullanıcı bulunamadı.@{firstName}",
                    cancellationToken: cancellationToken);
                return;
            }
        }
        else
        {
            await botClient.SendTextMessageAsync(message.Chat.Id,
                "Lütfen bir kullanıcı adı belirtin veya bir mesajı yanıtlayın.", cancellationToken: cancellationToken);
            return;
        }

        try
        {
            var chat = await botClient.GetChatAsync(message.Chat.Id);
            var permissions = chat.Permissions;
            Console.WriteLine("W"+permissions);


            if (permissions != null)
            {
                await botClient.RestrictChatMemberAsync(
                    message.Chat.Id,
                    userId,
                    permissions,
                    cancellationToken: cancellationToken);

                await botClient.SendTextMessageAsync(message.Chat.Id,
                    $"{Extensions.GetUserMentionString(userId, firstName)} susturulması kaldırıldı.",
                    cancellationToken: cancellationToken, parseMode: ParseMode.Html);
            }
        }
        catch (Exception ex)
        {
            await botClient.SendTextMessageAsync(message.Chat.Id,
                $"Kullanıcının susturulması kaldırılamadı: {ex.Message}", cancellationToken: cancellationToken);
        }
    }

    private async Task MuteUser(ITelegramBotClient botClient, Message message, string[] args,
        CancellationToken cancellationToken)
    {
        var isAdmin = message.From != null && await IsAdmin(botClient, message.From.Id, message.Chat.Id);

        if (!isAdmin)
        {
            await botClient.SendTextMessageAsync(message.Chat.Id,
                "Üzgünüm yalnızca yöneticiler tarafından kullanılabilirim.", cancellationToken: cancellationToken,
                replyToMessageId: message.MessageId);
            return;
        }

        long userId = 0;
        var firstName = "";

        if (message.ReplyToMessage?.From != null)
        {
            userId = message.ReplyToMessage.From.Id;
            firstName = message.ReplyToMessage.From.FirstName;
        }
        else if (args.Length > 0)
        {
            try
            {
                var chatMember = await botClient.GetChatMemberAsync(message.Chat.Id, userId, cancellationToken);
                userId = chatMember.User.Id;
            }
            catch (Exception)
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Kullanıcı bulunamadı.",
                    cancellationToken: cancellationToken);
                return;
            }
        }
        else
        {
            await botClient.SendTextMessageAsync(message.Chat.Id,
                "Lütfen bir kullanıcı adı belirtin veya bir mesajı yanıtlayın.", cancellationToken: cancellationToken);
            return;
        }

        var muteDuration = TimeSpan.Zero;
        if (args.Length >= 1)
        {
            muteDuration = Extensions.ParseDuration(args[0]);
            if (muteDuration == TimeSpan.Zero)
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Geçersiz süre formatı. Örnek: 1m, 2h, 3d",
                    cancellationToken: cancellationToken);
                return;
            }
        }

        try
        {
            if (muteDuration == TimeSpan.Zero)
            {
                await botClient.RestrictChatMemberAsync(
                    message.Chat.Id,
                    userId,
                    new ChatPermissions
                    {
                        CanSendMessages = false,
                        CanSendAudios = false,
                        CanSendDocuments = false,
                        CanSendPhotos = false,
                        CanSendVideos = false,
                        CanSendVideoNotes = false,
                        CanSendVoiceNotes = false,
                        CanSendPolls = false,
                        CanSendOtherMessages = false,
                        CanAddWebPagePreviews = false,
                        CanChangeInfo = false,
                        CanInviteUsers = false,
                        CanPinMessages = false
                    },
                    cancellationToken: cancellationToken);
                await botClient.SendTextMessageAsync(message.Chat.Id,
                    $"{Extensions.GetUserMentionString(userId, firstName)} süresiz olarak susturuldu.",
                    cancellationToken: cancellationToken, parseMode: ParseMode.Html,
                    replyToMessageId: message.MessageId);
            }
            else
            {
                await botClient.RestrictChatMemberAsync(
                    message.Chat.Id,
                    userId,
                    new ChatPermissions
                    {
                        CanSendMessages = false,
                        CanSendAudios = false,
                        CanSendDocuments = false,
                        CanSendPhotos = false,
                        CanSendVideos = false,
                        CanSendVideoNotes = false,
                        CanSendVoiceNotes = false,
                        CanSendPolls = false,
                        CanSendOtherMessages = false,
                        CanAddWebPagePreviews = false,
                        CanChangeInfo = false,
                        CanInviteUsers = false,
                        CanPinMessages = false
                    },
                    untilDate: DateTime.UtcNow.Add(muteDuration),
                    cancellationToken: cancellationToken);
                await botClient.SendTextMessageAsync(message.Chat.Id,
                    $"{Extensions.GetUserMentionString(userId, firstName)} {Extensions.FormatDuration(muteDuration)} boyunca susturuldu.",
                    cancellationToken: cancellationToken, parseMode: ParseMode.Html,
                    replyToMessageId: message.MessageId);
            }
        }
        catch (Exception ex)
        {
            await botClient.SendTextMessageAsync(message.Chat.Id, $"Kullanıcı susturulamadı: {ex.Message}",
                cancellationToken: cancellationToken, replyToMessageId: message.MessageId);
        }
    }






    private async Task HandleNewMember(ITelegramBotClient botClient, ChatMemberUpdated chatMember,
        CancellationToken cancellationToken)
    {
        if (chatMember.NewChatMember.Status == ChatMemberStatus.Member)
        {
            await botClient.SendTextMessageAsync(
                chatMember.Chat.Id,
                $"Hoş geldin, {chatMember.NewChatMember.User.FirstName}! Grubumuza katıldığın için teşekkürler.",
                cancellationToken: cancellationToken);
        }
    }

    public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception,
        CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException =>
                $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(ErrorMessage);
        return Task.CompletedTask;
    }
}