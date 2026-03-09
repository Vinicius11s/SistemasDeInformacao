using Agile360.API.Controllers;
using Agile360.Application.Processos.DTOs;
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
/// Bateria de testes QA — ProcessoController
///
/// Cobre os cenários solicitados:
///   1. Campos obrigatórios faltando (NumProcesso vazio)
///   2. Status inválido
///   3. Fase processual inválida
///   4. num_processo duplicado → 409 Conflict (dedup antecipado)
///   5. Criação bem-sucedida → 201 Created
/// </summary>
public class ProcessoControllerTests
{
    private readonly IProcessoRepository _repo = Substitute.For<IProcessoRepository>();
    private readonly ProcessoController _sut;

    private static readonly Guid ClienteId = Guid.NewGuid();

    public ProcessoControllerTests()
    {
        _sut = new ProcessoController(_repo);
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    // ════════════════════════════════════════════════════════════════════════
    //  CAMPOS OBRIGATÓRIOS FALTANDO
    // ════════════════════════════════════════════════════════════════════════

    [Fact(DisplayName = "Criar: NumProcesso vazio → 400 com mensagem amigável")]
    public async Task Criar_NumProcessoVazio_Retorna400()
    {
        var req = CriarRequest(numProcesso: "");

        var result = await _sut.Criar(req, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.StatusCode.Should().Be(400);
        await _repo.DidNotReceive().AddAsync(Arg.Any<Processo>());
    }

    [Fact(DisplayName = "Criar: NumProcesso apenas espaços → 400")]
    public async Task Criar_NumProcessoSoEspacos_Retorna400()
    {
        var req = CriarRequest(numProcesso: "   ");

        var result = await _sut.Criar(req, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
        await _repo.DidNotReceive().AddAsync(Arg.Any<Processo>());
    }

    // ════════════════════════════════════════════════════════════════════════
    //  STATUS INVÁLIDO
    // ════════════════════════════════════════════════════════════════════════

    [Theory(DisplayName = "Criar: status inválido → 400")]
    [InlineData("Pendente")]
    [InlineData("Cancelado")]
    [InlineData("")]
    [InlineData("ativo")]          // case-sensitive
    public async Task Criar_StatusInvalido_Retorna400(string statusInvalido)
    {
        var req = CriarRequest(status: statusInvalido);

        var result = await _sut.Criar(req, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
        await _repo.DidNotReceive().AddAsync(Arg.Any<Processo>());
    }

    [Theory(DisplayName = "Criar: fase processual inválida → 400")]
    [InlineData("Inicial")]
    [InlineData("conhecimento")]   // case-sensitive
    public async Task Criar_FaseInvalida_Retorna400(string faseInvalida)
    {
        var req = CriarRequest(faseProcessual: faseInvalida);

        var result = await _sut.Criar(req, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
        await _repo.DidNotReceive().AddAsync(Arg.Any<Processo>());
    }

    // ════════════════════════════════════════════════════════════════════════
    //  DUPLICATA — num_processo já existe (dedup antecipado)
    // ════════════════════════════════════════════════════════════════════════

    [Fact(DisplayName = "Criar: num_processo já existe → 409 Conflict antes de chamar o banco")]
    public async Task Criar_NumProcessoDuplicado_Retorna409SemChamarAddAsync()
    {
        var num = "1234567-89.2026.8.26.0100";
        _repo.GetByNumeroAsync(num, Arg.Any<CancellationToken>())
             .Returns(new Processo { NumProcesso = num });

        var req = CriarRequest(numProcesso: num);

        var result = await _sut.Criar(req, CancellationToken.None);

        result.Should().BeOfType<ConflictObjectResult>()
            .Which.StatusCode.Should().Be(409);

        // Nenhum insert deve ter sido chamado
        await _repo.DidNotReceive().AddAsync(Arg.Any<Processo>());
    }

    [Fact(DisplayName = "Criar: num_processo diferente → não gera 409 prematuramente")]
    public async Task Criar_NumProcessoNovo_NaoConflito()
    {
        const string num = "9999999-00.2026.8.26.0100";
        _repo.GetByNumeroAsync(num, Arg.Any<CancellationToken>()).ReturnsNull();
        _repo.AddAsync(Arg.Any<Processo>(), Arg.Any<CancellationToken>())
             .Returns(new Processo
             {
                 Id          = Guid.NewGuid(),
                 NumProcesso = num,
                 Status      = "Ativo",
                 ClienteId   = ClienteId,
                 CriadoEm   = DateOnly.FromDateTime(DateTime.UtcNow),
             });

        var req = CriarRequest(numProcesso: num);
        var result = await _sut.Criar(req, CancellationToken.None);

        result.Should().BeOfType<CreatedAtActionResult>()
            .Which.StatusCode.Should().Be(201);
        await _repo.Received(1).AddAsync(Arg.Any<Processo>(), Arg.Any<CancellationToken>());
    }

    // ════════════════════════════════════════════════════════════════════════
    //  CRIAÇÃO BEM-SUCEDIDA
    // ════════════════════════════════════════════════════════════════════════

    [Fact(DisplayName = "Criar: payload válido completo → 201 Created com dados retornados")]
    public async Task Criar_PayloadValido_Retorna201()
    {
        const string num = "0000001-01.2026.8.26.0001";
        var id = Guid.NewGuid();
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);

        _repo.GetByNumeroAsync(num, Arg.Any<CancellationToken>()).ReturnsNull();
        _repo.AddAsync(Arg.Any<Processo>(), Arg.Any<CancellationToken>())
             .Returns(new Processo
             {
                 Id              = id,
                 NumProcesso     = num,
                 Status          = "Ativo",
                 ClienteId       = ClienteId,
                 Assunto         = "Danos Morais",
                 Tribunal        = "TJSP",
                 FaseProcessual  = "Conhecimento",
                 CriadoEm       = hoje,
             });

        var req = new CreateProcessoRequest(
            IdCliente:           ClienteId,
            NumProcesso:         num,
            Status:              "Ativo",
            ParteContraria:      "Empresa XYZ",
            Tribunal:            "TJSP",
            ComarcaVara:         "1ª Vara Cível",
            Assunto:             "Danos Morais",
            ValorCausa:          50_000m,
            HonorariosEstimados: 5_000m,
            FaseProcessual:      "Conhecimento",
            DataDistribuicao:    new DateOnly(2026, 1, 15),
            Observacoes:         "Processo urgente"
        );

        var result = await _sut.Criar(req, CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.StatusCode.Should().Be(201);

        var response = created.Value.Should().BeOfType<ProcessoResponse>().Subject;
        response.NumProcesso.Should().Be(num);
        response.Status.Should().Be("Ativo");
        response.CriadoEm.Should().Be(hoje);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  OBTER / EXCLUIR
    // ════════════════════════════════════════════════════════════════════════

    [Fact(DisplayName = "Obter: id inexistente → 404 Not Found")]
    public async Task Obter_IdInexistente_Retorna404()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).ReturnsNull();

        var result = await _sut.Obter(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact(DisplayName = "Excluir: id inexistente → 404 Not Found, sem chamar RemoveAsync")]
    public async Task Excluir_IdInexistente_Retorna404()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).ReturnsNull();

        var result = await _sut.Excluir(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
        await _repo.DidNotReceive().RemoveAsync(Arg.Any<Processo>());
    }

    // ─── Helper ──────────────────────────────────────────────────────────────
    private static CreateProcessoRequest CriarRequest(
        string numProcesso    = "1234567-89.2026.8.26.0100",
        string status         = "Ativo",
        string? faseProcessual = null) => new(
        IdCliente:           ClienteId,
        NumProcesso:         numProcesso,
        Status:              status,
        ParteContraria:      null,
        Tribunal:            null,
        ComarcaVara:         null,
        Assunto:             null,
        ValorCausa:          null,
        HonorariosEstimados: null,
        FaseProcessual:      faseProcessual,
        DataDistribuicao:    null,
        Observacoes:         null);
}
