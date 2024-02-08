using Disqord.Bot.Commands.Application;
using Disqord.Gateway;
using MissPaulingBot.Common;
using MissPaulingBot.Common.Checks;
using Qmmands;

namespace MissPaulingBot.Modules.Utility;

[Name("Utility")]
[Description("Useful commands for this server.")]
public class UtilityCommands : DiscordApplicationModuleBase
{
    [SlashCommand("emotecount")]
    [RequireChannels(RequireChannels.ChannelMode.Utility)]
    [Description("Gets the number of static and animated emotes as well as how many are left.")]
    public IResult GetEmoteCounter()
    {
        var guild = Context.Bot.GetGuild(Constants.TF2_GUILD_ID);
        var emojiList = guild?.Emojis;
        var stickerList = guild?.Stickers;
        var gifEmoteCount = 0;

        foreach (var emoji in emojiList!.Values)
        {
            if (emoji.IsAnimated)
                ++gifEmoteCount;
        }

        return Response($"**Emote Stats**\n\n**Static Emotes:** {emojiList.Count - gifEmoteCount}/250\n**Animated Emotes:** {gifEmoteCount}/250" +
                        $"\n**Stickers:** {stickerList!.Count}/60");
    }

    /*[SlashCommand("createfiles")]
    [RequireAuthorRole(Constants.CO_OWNER_ROLE_ID)]
    [Description("Generate TF2 Custom Hit/KillSounds")]
    public async Task<IResult> CreateCustomFilesAsync([Description("Type of TF2 File")] Tf2FileType type,
        [SupportedFileExtensions("mp4", "mp3", "ogg")] [Description("File to convert")]
        IAttachment attachment)
    {

    }*/

}