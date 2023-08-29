namespace MissPaulingBot.Common.Menus.Views.Applications
{
    public sealed class AbuseResponseView : QuestionResponseModAppViewBase
    {
        public override bool ResponseIsRequired => true;

        public override string Question => "You feel a moderator is abusing their powers or otherwise becoming unfit to moderate. What would you do to prevent this" +
                                           " from becoming a bigger problem?";

        public override string GetCurrentResponse()
            => App.AbuseResponse;

        public override void ModifyApp(string response)
            => App.AbuseResponse = response;
    }
}