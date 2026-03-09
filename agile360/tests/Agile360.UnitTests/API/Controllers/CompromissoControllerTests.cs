using Agile360.API.Controllers;
using Agile360.Application.Compromissos.DTOs;
using Agile360.Domain.Entities;
using Agile360.Domain.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Xunit;

namespace Agile360.UnitTests.API.Controllers;

/// <summary>
/// Bateria de testes QA — CompromissoController
///
/// Cobre:
///   1. TipoCompromisso inválido → 400
///   2. Status inválido → 400
///   3. Audiência sem id_processo → 400 (regra de negócio)
///   4. Criação bem-sucedida → 201
///   5. id inexistente em Obter/Excluir → 404
/// </summary>
public class CompromissoControllerTests
{
    private readonly ICompromissoRepository _repo = Substitute.For<ICompromissoRepository>();
    private readonly CompromissoController  _sut;

    private static readonly Guid ClienteId  = Guid.NewGuid();
    private static readonly Guid ProcessoId = Guid.NewGuid();

    private static readonly DateOnly HojeData = DateOnly.FromDateTime(DateTime.UtcNow);
    private static readonly TimeOnly HoraFixa = new(10, 0);

    public CompromissoControllerTests()
    {
        _sut = new CompromissoController(_repo);
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    // ════════════════════════════════════════════════════════════════════════
    //  TIPO DE COMPROMISSO INVÁLIDO
    // ════════════════════════════════════════════════════════════════════════

    [Theory(DisplayName = "Criar: tipo_compromisso inválido → 400")]
    [InlineData("Despacho")]
    [InlineData("audiência")]       // case-sensitive
    [InlineData("")]
    [InlineData("Diligência")]
    public async Task Criar_TipoInvalido_Retorna400(string tipoInvalido)
    {
        var req = CriarRequest(tipo: tipoInvalido);

        var result = await _sut.Criar(req, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.StatusCode.Should().Be(400);
        await _repo.DidNotReceive().AddAsync(Arg.Any<Compromisso>());
    }

    [Theory(DisplayName = "Criar: tipos válidos não retornam 400 de validação de tipo")]
    [InlineData("Audiência")]
    [InlineData("Atendimento")]
    [InlineData("Reunião")]
    [InlineData("Prazo")]
    public async Task Criar_TipoValido_PassaValidacaoTipo(string tipoValido)
    {
        // Audiência sem processo → retorna 400 por outra regra, não por tipo inválido
        // Os outros tipos com id_processo null são válidos
        var req = CriarRequest(tipo: tipoValido, idProcesso: tipoValido == "Audiência" ? ProcessoId : null);
        _repo.AddAsync(Arg.Any<Compromisso>(), Arg.Any<CancellationToken>())
             .Returns(CriarEntidade(tipoValido));

        var result = await _sut.Criar(req, CancellationToken.None);

        // Não deve ser BadRequest por tipo inválido
        result.Should().NotBeOfType<BadRequestObjectResult>("tipo_compromisso é válido");
    }

    // ════════════════════════════════════════════════════════════════════════
    //  STATUS INVÁLIDO
    // ════════════════════════════════════════════════════════════════════════

    [Theory(DisplayName = "Criar: tipo inválido (status string obsoleto) → 400 por tipo")]
    [InlineData("Pendente")]
    [InlineData("agendado")]         // case-sensitive
    [InlineData("Encerrado")]
    public async Task Criar_StatusInvalido_Retorna400(string tipoInvalido)
    {
        // Status foi migrado para IsActive (bool). O teste agora verifica tipo inválido.
        var req = CriarRequest(tipo: tipoInvalido);

        var result = await _sut.Criar(req, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
        await _repo.DidNotReceive().AddAsync(Arg.Any<Compromisso>());
    }

    // ════════════════════════════════════════════════════════════════════════
    //  REGRA DE NEGÓCIO: AUDIÊNCIA EXIGE ID_PROCESSO
    // ════════════════════════════════════════════════════════════════════════

    [Fact(DisplayName = "Criar Audiência sem id_processo → 400 com mensagem clara")]
    public async Task Criar_AudienciaSemProcesso_Retorna400()
    {
        var req = CriarRequest(tipo: "Audiência", idProcesso: null);

        var result = await _sut.Criar(req, CancellationToken.None);

        var bad = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        bad.StatusCode.Should().Be(400);
        await _repo.DidNotReceive().AddAsync(Arg.Any<Compromisso>());
    }

    [Fact(DisplayName = "Criar Audiência com id_processo → não bloqueia por regra de negócio")]
    public async Task Criar_AudienciaComProcesso_NaoBloqueia()
    {
        var req = CriarRequest(tipo: "Audiência", idProcesso: ProcessoId);
        _repo.AddAsync(Arg.Any<Compromisso>(), Arg.Any<CancellationToken>())
             .Returns(CriarEntidade("Audiência", idProcesso: ProcessoId));

        var result = await _sut.Criar(req, CancellationToken.None);

        result.Should().BeOfType<CreatedAtActionResult>().Which.StatusCode.Should().Be(201);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  CRIAÇÃO BEM-SUCEDIDA
    // ════════════════════════════════════════════════════════════════════════

    [Theory(DisplayName = "Criar: tipos não-Audiência sem id_processo → 201 (campo opcional)")]
    [InlineData("Atendimento")]
    [InlineData("Reunião")]
    [InlineData("Prazo")]
    public async Task Criar_TiposNaoAudiencia_SemProcesso_Retorna201(string tipo)
    {
        var req = CriarRequest(tipo: tipo, idProcesso: null);
        _repo.AddAsync(Arg.Any<Compromisso>(), Arg.Any<CancellationToken>())
             .Returns(CriarEntidade(tipo));

        var result = await _sut.Criar(req, CancellationToken.None);

        result.Should().BeOfType<CreatedAtActionResult>().Which.StatusCode.Should().Be(201);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  OBTER / EXCLUIR
    // ════════════════════════════════════════════════════════════════════════

    [Fact(DisplayName = "Obter: id inexistente → 404")]
    public async Task Obter_IdInexistente_Retorna404()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).ReturnsNull();

        var result = await _sut.Obter(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact(DisplayName = "Excluir: id inexistente → 404, sem chamar RemoveAsync")]
    public async Task Excluir_IdInexistente_Retorna404SemRemover()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).ReturnsNull();

        var result = await _sut.Excluir(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
        await _repo.DidNotReceive().RemoveAsync(Arg.Any<Compromisso>());
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static CriarCompromissoRequest CriarRequest(
        string tipo       = "Atendimento",
        bool   isActive   = true,
        Guid?  idProcesso = null) => new(
        TipoCompromisso: tipo,
        TipoAudiencia:   null,
        IsActive:        isActive,
        Data:            HojeData,
        Hora:            HoraFixa,
        Local:           null,
        IdCliente:       ClienteId,
        IdProcesso:      idProcesso,
        Observacoes:     null,
        LembreteMinutos: null);

    private static Compromisso CriarEntidade(string tipo, Guid? idProcesso = null) => new()
    {
        Id              = Guid.NewGuid(),
        TipoCompromisso = tipo,
        IsActive        = true,
        Data            = HojeData,
        Hora            = HoraFixa,
        ClienteId       = ClienteId,
        ProcessoId      = idProcesso,
        CriadoEm        = HojeData,
    };
}
