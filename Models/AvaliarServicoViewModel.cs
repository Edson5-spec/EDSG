@model EDSG.Models.AvaliarServicoViewModel

@{
    ViewData["Title"] = "Avaliar Serviço";
    Layout = "~/Views/Shared/_Layout.cshtml";
}

< !DOCTYPE html >
< html >
< head >
    < meta charset = "utf-8" />
    < meta name = "viewport" content = "width=device-width, initial-scale=1.0" />
    < title > @ViewData["Title"] - EDSG </ title >


    < !--Bootstrap CSS-- >
    < link href = "https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/css/bootstrap.min.css" rel = "stylesheet" >
    < !--Font Awesome-- >
    < link rel = "stylesheet" href = "https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css" >


    < !--Estilos específicos desta página -->
    <style>
        .star-container {
            position: relative;
text - align: center;
        }
        
        .star - label {
cursor: pointer;
transition: all 0.2s ease;
display: block;
padding: 10px;
    border - radius: 8px;
}
        
        .star - label:hover {
            transform: scale(1.1);
background - color: rgba(255, 193, 7, 0.1);
        }
        
        input[type = "radio"]:checked + .star - label {
    background - color: rgba(255, 193, 7, 0.2);
transform: scale(1.1);
}
        
        .fa - star {
filter: drop - shadow(0 2px 4px rgba(0, 0, 0, 0.1));
}
        
        .rating - description {
    min - height: 24px;
    font - style: italic;
}
        
        .card {
    border - radius: 12px;
border: none;
    box - shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
}
        
        .card - header {
    border - radius: 12px 12px 0 0!important;
}
        
        .char-count - warning {
color: #dc3545;
            font - weight: bold;
}
        
        .badge - estado {
    font - size: 0.9em;
padding: 0.4em 0.8em;
}
        
        .badge - pendente {
    background - color: #ffc107; color: #000; }
        .badge - aceite {
        background - color: #17a2b8; color: #fff; }
        .badge - emprogresso {
            background - color: #007bff; color: #fff; }
        .badge - concluido {
                background - color: #28a745; color: #fff; }
        .badge - recusado {
                    background - color: #6c757d; color: #fff; }
        .badge - cancelado {
                        background - color: #dc3545; color: #fff; }
    </ style >
</ head >
< body >
    < div class= "container mt-4" >
        < div class= "row justify-content-center" >
            < div class= "col-md-8 col-lg-6" >
                < div class= "card shadow" >
                    < div class= "card-header bg-primary text-white" >
                        < h4 class= "mb-0" >
                            < i class= "fas fa-star me-2" ></ i > @ViewData["Title"]
                        </ h4 >
                    </ div >
                    < div class= "card-body" >
                        < div class= "alert alert-info mb-4" >
                            < div class= "d-flex" >
                                < div class= "flex-shrink-0" >
                                    < i class= "fas fa-info-circle fa-2x" ></ i >
                                </ div >
                                < div class= "flex-grow-1 ms-3" >
                                    < h5 class= "alert-heading" > Avaliando o serviço com:</ h5 >
                                    < p class= "mb-0" >< strong > Profissional:</ strong > @Model.ProfissionalNome </ p >
                                    < small class= "text-muted" > Serviço #@Model.ServicoId</small>
                                </ div >
                            </ div >
                        </ div >

                        < form asp - action = "AvaliarServico" method = "post" id = "avaliacaoForm" >
                            @Html.AntiForgeryToken()

                            < input type = "hidden" asp -for= "ServicoId" />
                            < input type = "hidden" asp -for= "ProfissionalNome" />

                            < !--Campo Nota-- >
                            < div class= "form-group mb-4" >
                                < label class= "form-label fw-bold mb-3" > Como classificaria este serviço? *</label>
                                
                                <div class= "star-rating-wrapper" >
                                    < div class= "d-flex justify-content-center mb-2" >
                                        @for(int i = 1; i <= 5; i++)
                                        {
                                            < div class= "star-container mx-2" >
                                                < input type = "radio" id = "star@(i)" name = "Nota" value = "@i"
                                                       class= "visually-hidden" @(Model.Nota == i ? "checked" : "") />
                                                < label for= "star@(i)" class= "star-label" >
                                                    < i class= "@(i <= Model.Nota ? "fas" : "far") fa-star fa-3x text-warning"></i>
                                                    <span class= "d-block text-center mt-1 small" > @i </ span >
                                                </ label >
                                            </ div >
                                        }
                                    </ div >
                                    < div class= "text-center mt-2" >
                                        < div class= "rating-description" id = "ratingDescription" >
                                            @if(Model.Nota > 0)
                                            {
    @GetRatingDescription(Model.Nota)
                                            }
                                            else {
                                                < span class= "text-muted" > Selecione uma classificação</span>
                                            }
                                        </ div >
                                    </ div >
                                </ div >


                                < span asp - validation -for= "Nota" class= "text-danger d-block text-center mt-2" ></ span >
                            </ div >

                            < !--Campo Comentário-- >
                            < div class= "form-group mb-4" >
                                < label asp -for= "Comentario" class= "form-label fw-bold" >
                                    < i class= "fas fa-comment me-1" ></ i > Deixe um comentário(opcional)
                                </ label >
                                < textarea asp -for= "Comentario" class= "form-control" rows = "6"
                                          placeholder = "Partilhe a sua experiência com este profissional...&#10;&#10;O que gostou mais?&#10;Há algo que poderia ser melhorado?"
                                          id = "comentarioTextarea" ></ textarea >
                                < span asp - validation -for= "Comentario" class= "text-danger" ></ span >
                                < div class= "text-end mt-1" >
                                    < small class= "text-muted" >
                                        < span id = "charCount" >@(Model.Comentario?.Length ?? 0) </ span >/ 1000 caracteres
                                    </ small >
                                </ div >
                            </ div >

                            < !--Dicas para uma boa avaliação -->
                            <div class= "alert alert-light border mb-4" >
                                < h6 class= "fw-bold" >< i class= "fas fa-lightbulb me-1 text-warning" ></ i > Dicas para uma boa avaliação:</ h6 >
                                < ul class= "mb-0 small" >
                                    < li > Seja específico sobre o que gostou ou não</li>
                                    <li>Mencione a pontualidade, qualidade do trabalho e comunicação</li>
                                    <li>A sua avaliação ajuda outros clientes a fazerem escolhas informadas</li>
                                </ul>
                            </div>

                            <!-- Botões -->
                            <div class= "d-grid gap-2 d-md-flex justify-content-md-between mt-4" >
                                < a asp - action = "DetalhesServico" asp - controller = "Dashboard"
                                   asp - route - id = "@Model.ServicoId" class= "btn btn-outline-secondary" >
                                    < i class= "fas fa-arrow-left me-1" ></ i > Voltar aos Detalhes
                                </a>
                                
                                <div class= "d-grid gap-2 d-md-flex" >
                                    < button type = "reset" class= "btn btn-outline-warning me-md-2" id = "resetBtn" >
                                        < i class= "fas fa-redo me-1" ></ i > Limpar
                                    </ button >
                                    < button type = "submit" class= "btn btn-success px-4" id = "submitBtn" >
                                        < i class= "fas fa-paper-plane me-1" ></ i > Enviar Avaliação
                                    </ button >
                                </ div >
                            </ div >
                        </ form >
                    </ div >
                </ div >
            </ div >
        </ div >
    </ div >

    < !--Scripts-- >
    < script src = "https://code.jquery.com/jquery-3.6.0.min.js" ></ script >
    < script src = "https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/js/bootstrap.bundle.min.js" ></ script >
    < script src = "//cdn.jsdelivr.net/npm/sweetalert2@11" ></ script >


    < script >
        $(document).ready(function() {
    // Contador de caracteres
    const comentarioTextarea = $('#comentarioTextarea');
    const charCount = $('#charCount');

    comentarioTextarea.on('input', function() {
        const length = $(this).val().length;
        charCount.text(length);

        if (length > 1000) {
            charCount.addClass('char-count-warning');
            comentarioTextarea.addClass('is-invalid');
        } else {
            charCount.removeClass('char-count-warning');
            comentarioTextarea.removeClass('is-invalid');
        }
    });

    // Sistema de estrelas com descrição
    function updateRatingDisplay(value) {
                // Atualizar estrelas
                $('.star-label i').each(function(index) {
            const starValue = index + 1;
            if (starValue <= value) {
                        $(this).removeClass('far').addClass('fas');
            } else {
                        $(this).removeClass('fas').addClass('far');
            }
        });

        // Atualizar descrição
        const descriptions = {
                    0: "Selecione uma classificação",
                    1: "Muito insatisfeito - O serviço não atendeu às expectativas mínimas",
                    2: "Insatisfeito - Houve problemas significativos",
                    3: "Neutro - O serviço foi aceitável, mas pode melhorar",
                    4: "Satisfeito - Bom serviço, atendeu às expectativas",
                    5: "Muito satisfeito - Excelente serviço, superou as expectativas"
                }
;
                
                $('#ratingDescription').text(descriptions[value] || '');
            }
            
            // Clique nas estrelas
            $('input[name="Nota"]').on('change', function() {
    const value = $(this).val();
    updateRatingDisplay(value);
});
            
            // Hover nas estrelas
            $('.star-label').on('mouseenter', function() {
    const value = $(this).find('input').val();
    updateRatingDisplay(value);
});
            
            $('.star-rating-wrapper').on('mouseleave', function() {
    const currentValue = $('input[name="Nota"]:checked').val() || 0;
    updateRatingDisplay(currentValue);
});

// Inicializar com valor existente
const initialValue = $('input[name="Nota"]:checked').val() || 0;
updateRatingDisplay(initialValue);
            
            // Validação do formulário
            $('#avaliacaoForm').on('submit', function(e) {
    const notaSelecionada = $('input[name="Nota"]:checked').val();
    const comentario = comentarioTextarea.val();

    // Validar nota
    if (!notaSelecionada) {
        e.preventDefault();
        Swal.fire({
        icon: 'warning',
                        title: 'Atenção',
                        text: 'Por favor, selecione uma classificação de 1 a 5 estrelas.',
                        confirmButtonColor: '#3085d6',
                        confirmButtonText: 'OK'
                    }).then(() => {
                        $('input[name="Nota"]').first().focus();
        });
        return false;
    }

    // Validar comprimento do comentário
    if (comentario.length > 1000) {
        e.preventDefault();
        Swal.fire({
        icon: 'error',
                        title: 'Comentário muito longo',
                        text: 'O comentário não pode exceder 1000 caracteres. Reduza para 1000 caracteres ou menos.',
                        confirmButtonColor: '#d33',
                        confirmButtonText: 'OK'
                    }).then(() => {
            comentarioTextarea.focus();
        });
        return false;
    }

    // Mostrar loading
    const submitBtn = $('#submitBtn');
    const originalText = submitBtn.html();
    submitBtn.prop('disabled', true).html('<i class="fas fa-spinner fa-spin me-1"></i>A enviar...');

    return true;
});
            
            // Reset do formulário
            $('#resetBtn').on('click', function() {
                $('input[name="Nota"]').prop('checked', false);
    updateRatingDisplay(0);
    comentarioTextarea.val('');
    charCount.text('0');
    comentarioTextarea.removeClass('is-invalid');
    charCount.removeClass('char-count-warning');
});

// Inicializar contador
comentarioTextarea.trigger('input');
        });
    </ script >
</ body >
</ html >

@functions {
    public string GetRatingDescription(int nota) {
    return nota switch {
        1 => "Muito insatisfeito - O serviço não atendeu às expectativas mínimas",
        2 => "Insatisfeito - Houve problemas significativos",
        3 => "Neutro - O serviço foi aceitável, mas pode melhorar",
        4 => "Satisfeito - Bom serviço, atendeu às expectativas",
        5 => "Muito satisfeito - Excelente serviço, superou as expectativas",
        _ => "Selecione uma classificação"
    };
}
}