using EDSG.Models;
using System;
using System.Collections.Generic;

namespace EDSG.Models {
    public class MensagensViewModel {
        // Lista de mensagens para exibição (legacy)
        public List<Mensagem> Mensagens { get; set; } = new List<Mensagem>();

        // Mensagens da conversa atual (usado pela view)
        public List<Mensagem> MensagensConversa { get; set; } = new List<Mensagem>();

        // Mensagem atual para enviar (se aplicável)
        public string NovoTexto { get; set; }

        // IDs para envio de nova mensagem
        public string DestinatarioId { get; set; }
        public string DestinatarioNome { get; set; }

        // ID da conversa atualmente selecionada
        public string ConversaAtualId { get; set; }

        // ID do usuário logado (usado para diferenciar remetente/destinatário)
        public string UsuarioId { get; set; }

        // Para listagem de conversas
        public List<ConversaResumo> Conversas { get; set; } = new List<ConversaResumo>();

        // Filtros e paginação
        public string Filtro { get; set; }
        public int PaginaAtual { get; set; } = 1;
        public int TotalPaginas { get; set; }
        public int TotalMensagens { get; set; }

        // Status
        public bool TemMensagensNaoLidas { get; set; }
    }

    // Classe auxiliar para resumo de conversas
    public class ConversaResumo {
        // Outro usuário na conversa
        public string OutroUsuarioId { get; set; }
        public ApplicationUser OutroUsuario { get; set; }

        // Última mensagem da conversa
        public Mensagem UltimaMensagem { get; set; }

        // Contadores
        public bool TemNovaMensagem { get; set; }
        public int MensagensNaoLidas { get; set; }

        // Informações legadas (opcionais)
        public string UsuarioId { get; set; }
        public string UsuarioNome { get; set; }
        public string UltimaMensagemTexto => UltimaMensagem?.Texto;
        public DateTime? DataUltimaMensagem => UltimaMensagem?.DataEnvio;
    }
}