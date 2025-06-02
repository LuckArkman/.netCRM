using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Hubs
{
    [Authorize]
    public class DashboardHub : Hub
    {
        // Este hub será usado para enviar atualizações de dados em tempo real para o dashboard
        // Exemplo de como o backend enviaria:
        // await _dashboardHub.Clients.All.SendAsync("UpdateKpi", "totalSolicitations", 123);
        // await _dashboardHub.Clients.All.SendAsync("UpdateRecentRequests", newRequestObject);
    }
}