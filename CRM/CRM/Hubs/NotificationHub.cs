using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Hubs
{
    [Authorize] // Apenas usuários autenticados podem se conectar ao hub
    public class NotificationHub : Hub
    {
        // Este hub é primariamente para o servidor enviar mensagens aos clientes.
        // Os clientes se conectam e recebem notificações como "ReceiveNotification".
        // Exemplo:
        // await Clients.User(Context.UserIdentifier).SendAsync("ReceivePrivateNotification", "Sua solicitação foi atualizada.");
    }
}