namespace MissPaulingBot.Common.Menus.Views.Applications
{
    public class ButtingHeadsResponseView : QuestionResponseModAppViewBase
    {
        public override bool ResponseIsRequired => true;

        public override string Question => "You cannot seem to get along with another moderator. You always tend to butt head with them, and it's very hard to hold" +
                                           "a professional conversation with them because you both just can't seem to agree on anything. What would you do in this situation?";

        public override string GetCurrentResponse()
            => App.ButtingHeadsResponse;

        public override void ModifyApp(string response)
            => App.ButtingHeadsResponse = response;
    }
}