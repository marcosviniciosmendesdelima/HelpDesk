using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace HelpDesk.Gateway.Hubs
{
    // Concentrador de conexões persistentes para o HelpDesk (SRP)
    public class TicketHub : Hub
    {
        // Método invocado pelo Frontend Angular ao abrir um chamado específico
        public async Task AssinarTicket(Guid ticketId)
        {
            string grupo = ObterNomeDoGrupo(ticketId);
            
            // Adiciona a conexão atual ao grupo específico deste ticket
            await Groups.AddToGroupAsync(Context.ConnectionId, grupo);
            
            Console.WriteLine($" [SignalR] Cliente '{Context.ConnectionId}' assinou o grupo: {grupo}");
            
            // Feedback de confirmação instantâneo para o cliente conectado
            await Clients.Caller.SendAsync("AssinaturaConfirmada", ticketId);
        }

        // Método invocado pelo Frontend Angular ao fechar/sair da tela do chamado
        public async Task CancelarAssinaturaTicket(Guid ticketId)
        {
            string grupo = ObterNomeDoGrupo(ticketId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, grupo);
            
            Console.WriteLine($" [SignalR] Cliente '{Context.ConnectionId}' removeu a assinatura do grupo: {grupo}");
        }

        // Limpeza automática se o usuário fechar a aba do navegador
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine($" [SignalR] Conexão encerrada com ID: {Context.ConnectionId}. Motivo: {exception?.Message ?? "Desconexão voluntária"}");
            await base.OnDisconnectedAsync(exception);
        }

        // Padronização estática para evitar colisão de chaves com outras entidades
        public static string ObterNomeDoGrupo(Guid ticketId) => $"ticket:{ticketId}";
    }
}