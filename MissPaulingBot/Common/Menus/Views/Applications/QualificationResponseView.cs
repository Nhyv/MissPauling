namespace MissPaulingBot.Common.Menus.Views.Applications;

public class QualificationResponseView : QuestionResponseModAppViewBase
{
    public override bool ResponseIsRequired => true;

    public override string Question =>
        "What do you think qualifies you to be a moderator here?";

    public override string? GetCurrentResponse()
        => App.QualificationResponse;

    public override void ModifyApp(string? response)
        => App.QualificationResponse = response;
}