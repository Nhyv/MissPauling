namespace MissPaulingBot.Common.Menus.Views.Applications;

public class ChangeResponseView : QuestionResponseModAppViewBase
{
    public override bool ResponseIsRequired => false;

    public override string Question => "As a moderator, what would you like to bring to this server? Do you have any concerns or desires to improve the state of the server?";

    public override string? GetCurrentResponse()
        => App.ChangeResponse;

    public override void ModifyApp(string? response)
        => App.ChangeResponse = response;
}