using HelpDesk.Gateway.Models;

namespace HelpDesk.Gateway.Tests;

public class UnitTest1
{
    [Fact]
    public void Chamado_DefaultValues_AreValid()
    {
        var chamado = new Chamado();

        Assert.Equal(string.Empty, chamado.Titulo);
        Assert.Equal("Aberto", chamado.Status);
        Assert.Equal("Média", chamado.Prioridade);
        Assert.Equal(Guid.Empty, chamado.Id);
        Assert.True(chamado.DataCriacao <= DateTime.UtcNow);
    }
}
