using Microsoft.AspNetCore.SignalR;

namespace HelpDesk.Gateway.Hubs;

public class TicketsHub : Hub
{
    // O hub pode ficar vazio! Ele serve como o canal receptor e retransmissor (Broadcaster) das mensagens.
    public async Task EnviarAtualizacaoStatus(string ticketId, string status)
    {
        await Clients.All.SendAsync("OnTicketStatusChanged", new { ticketId, status });
    }
}