namespace MissPaulingBot.Common.Menus.Views.Applications;

public class ButtonsResponseView : QuestionResponseModAppViewBase
{
    public override bool ResponseIsRequired => true;

    public override string Question => "A user is pushing everyone's buttons but in a way that doesn't break any rules." +
                                       " Nobody has directly come to you to report anything, but it's apparent that they" +
                                       " are a rule-abiding nuisance. Will you talk to them about this, and if so," +
                                       " what will you say?";

    public override string? GetCurrentResponse()
        => App.ButtonsResponse;

    public override void ModifyApp(string? response)
        => App.ButtonsResponse = response;
}