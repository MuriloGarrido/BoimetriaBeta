namespace BoimetriaBeta.Services;

public interface IDialogService
{
    Task AlertAsync(string title, string message, string accept = "OK");
    Task<bool> ConfirmAsync(string title, string message, string accept = "Sim", string cancel = "Não");
}
