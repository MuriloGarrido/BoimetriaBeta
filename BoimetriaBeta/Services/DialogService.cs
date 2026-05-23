namespace BoimetriaBeta.Services;

public sealed class DialogService : IDialogService
{
    public Task AlertAsync(string title, string message, string accept = "OK")
    {
        var page = CurrentPage();
        return page is null ? Task.CompletedTask : page.DisplayAlertAsync(title, message, accept);
    }

    public Task<bool> ConfirmAsync(string title, string message, string accept = "Sim", string cancel = "Não")
    {
        var page = CurrentPage();
        return page is null ? Task.FromResult(false) : page.DisplayAlertAsync(title, message, accept, cancel);
    }

    static Page? CurrentPage()
        => Shell.Current?.CurrentPage
           ?? Application.Current?.Windows.FirstOrDefault()?.Page;
}
