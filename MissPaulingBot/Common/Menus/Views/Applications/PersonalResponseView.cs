namespace MissPaulingBot.Common.Menus.Views.Applications;

public class PersonalResponseView : QuestionResponseModAppViewBase
{
    public override bool ResponseIsRequired => false;

    public override string Question =>
        "Tells us something about yourself. Feel free to humble brag, or tell us a fun fact" +
        " about yourself or your life!";

    public override string? GetCurrentResponse()
        => App.PersonalResponse;

    public override void ModifyApp(string? response)
        => App.PersonalResponse = response;
}