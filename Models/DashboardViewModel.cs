using System.Collections.Generic;

namespace EDSG.Models {
    public class DashboardViewModel {
        public ModoDashboard Modo { get; set; } = ModoDashboard.Cliente;
        public List<Servico> ServicosPendentes { get; set; }
        public List<Servico> ServicosAtivos { get; set; }
        public List<Servico> ServicosConcluidos { get; set; }
        public List<Mensagem> MensagensRecebidas { get; set; }
        public List<ApplicationUser> Favoritos { get; set; }
        public List<Avaliacao> AvaliacoesRecebidas { get; set; }
        public EstatisticasProfissional Estatisticas { get; set; }
    }

    public enum ModoDashboard {
        Cliente,
        Profissional
    }

    public class EstatisticasProfissional {
        public int TotalServicos { get; set; }
        public int ServicosConcluidos { get; set; }
        public int ServicosPendentes { get; set; }
        public double AvaliacaoMedia { get; set; }
        public decimal ReceitaTotal { get; set; }
    }
}