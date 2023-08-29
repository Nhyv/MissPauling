using Disqord;

namespace MissPaulingBot.Utilities
{
    public class EmbedUtilities
    {
        public static LocalEmbed SuccessBuilder
            => new LocalEmbed().WithColor(0xD8BED8);

        public static LocalEmbed ErrorBuilder
            => new LocalEmbed().WithColor(0xEB4B4B);

        public static LocalEmbed LoggingBuilder
            => new LocalEmbed().WithColor(0xFFD700);

        public static LocalEmbed GameBuilder
            => new LocalEmbed().WithColor(0xFFFFF0);
    }
}