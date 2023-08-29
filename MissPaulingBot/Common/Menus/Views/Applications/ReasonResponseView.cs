namespace MissPaulingBot.Common.Menus.Views.Applications
{
    public class ReasonResponseView : QuestionResponseModAppViewBase
    {
        public override bool ResponseIsRequired => true;

        public override string Question =>
            "Why do you want to be a moderator here?";

        public override string? GetCurrentResponse()
            => App.ReasonResponse;

        public override void ModifyApp(string? response)
            => App.ReasonResponse = response;
    }
}