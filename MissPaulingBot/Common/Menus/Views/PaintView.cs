using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Rest;

namespace MissPaulingBot.Common.Menus.Views
{
    public sealed class PaintView : ViewBase
    {
        private static readonly Dictionary<Snowflake, (string Name, LocalEmoji Emoji)> PaintRoles = new()
        {
            [673660723477610506] = ("A Color Similar to Slate", LocalEmoji.FromString("<:2F4F4F:673659626910253058>")),
            [673661192434089991] = ("A Deep Commitment to Purple", LocalEmoji.FromString("<:7D4071:673659626998333440>")),
            [673661256929902647] = ("A Distinctive Lack of Hue", LocalEmoji.FromString("<:141414:673659626968842268>")),
            [673661306481410089] = ("A Mann's Mint", LocalEmoji.FromString("<:BCDDB3:673659626884956165>")),
            [673661397447606300] = ("After Eight", LocalEmoji.FromString("<:2D2D24:673659626855858176>")),
            [673661441877868567] = ("Aged Moustache Grey", LocalEmoji.FromString("<:7E7E7E:673659627119968256>")),
            [673661574426263592] = ("An Extraordinary Abundance of Tinge", LocalEmoji.FromString("<:E6E6E6:673659626897539120>")),
            [673661664603799603] = ("Australium Gold", LocalEmoji.FromString("<:E7B53B:673659626952196110>")),
            [673661819415691285] = ("Color No. 216-190-216", LocalEmoji.FromString("<:D8BED8:673659626914316329>")),
            [673662043219427358] = ("Dark Salmon Injustice", LocalEmoji.FromString("<:E9967A:673659626939744256>")),
            [673662119140524042] = ("Drably Olive", LocalEmoji.FromString("<:808000:673659626880761897>")),
            //[673662183942520868] = ("Indubitably Green", LocalEmoji.FromString("<:729E42:673659627099127828>")),
            [673662258554994730] = ("Mann Co. Orange", LocalEmoji.FromString("<:CF7336:673659626536960021>")),
            [673662344685027368] = ("Muskelmannbraun", LocalEmoji.FromString("<:A57545:673659626897539154>")),
            [673662398586159121] = ("Noble Hatter's Violet", LocalEmoji.FromString("<:51384A:673659626868178997>")),
            [673662456907825193] = ("Peculiarly Drab Tincture", LocalEmoji.FromString("<:C5AF91:673659626767515649>")),
            [673662520476827669] = ("Pink as Hell", LocalEmoji.FromString("<:FF69B4:673659626994008082>")),
            [673662607474819084] = ("Radigan Conagher Brown", LocalEmoji.FromString("<:694D3A:673659627300454421>")),
            [673662652781953030] = ("The Bitter Taste of Defeat and Lime", LocalEmoji.FromString("<:32CD32:673659626901733436>")),
            [673662715855896576] = ("The Color of a Gentlemann's Business Pants", LocalEmoji.FromString("<:F0E68C:673659626964779028>")),
            [673662878397759488] = ("Ye Olde Rustic Colour", LocalEmoji.FromString("<:7C6C57:673659626931224592>")),
            [673662915097657366] = ("Zepheniah's Greed", LocalEmoji.FromString("<:424F3B:673659626658594837>"))
        };

        public PaintView() : base(x => (x as LocalInteractionMessageResponse).WithContent("Select your color below").WithIsEphemeral())
        {
            AddComponent(new SelectionViewComponent(PaintAsync)
            {
                Placeholder = "Choose your paint color...",
                Options = PaintRoles.Select(r => new LocalSelectionComponentOption
                {
                    Label = r.Value.Name,
                    Emoji = r.Value.Emoji,
                    Value = r.Key.ToString()
                }).ToList()
            });
        }

        private async ValueTask PaintAsync(SelectionEventArgs e)
        {
            _ = Snowflake.TryParse(e.SelectedOptions[0].Value.ToString(), out var roleId);

            if (e.Member.RoleIds.Contains(roleId)) return;

            foreach (var id in e.Member.RoleIds)
            {
                if (PaintRoles.ContainsKey(id))
                {
                    await Menu.Client.RevokeRoleAsync(e.GuildId.Value, e.AuthorId, id);
                    break;
                }
            }

            await Menu.Client.GrantRoleAsync(e.GuildId.Value, e.AuthorId, roleId);
            await e.Interaction.Response()
                .SendMessageAsync(new LocalInteractionMessageResponse().WithContent("Role added!").WithIsEphemeral());
            Menu.Stop();
        }
    }
}