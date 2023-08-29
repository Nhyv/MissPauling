namespace MissPaulingBot.Common.Menus.Views.Applications
{
    public class AvailabilitiesResponseView : QuestionResponseModAppViewBase
    {
        public override bool ResponseIsRequired => true;

        public override string Question =>
            "Describe your availabilities and your timezone.\nWhat times and days are you normally available to moderate?";

        public override string? GetCurrentResponse()
            => App.AvailabilitiesResponse;

        public override void ModifyApp(string? response)
            => App.AvailabilitiesResponse = response;
    }
}