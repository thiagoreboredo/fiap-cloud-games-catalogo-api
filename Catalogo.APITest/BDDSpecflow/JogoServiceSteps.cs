using Application.DTOs;
using Application.Exceptions;
using Application.Helper;
using Application.Services;
using AutoMapper;
using Domain.Entity;
using Domain.Entity.Enum;
using Domain.Repository;
using MassTransit;
using Moq;
using TechTalk.SpecFlow;

[Binding]
public class JogoServiceSteps
{
    private readonly ScenarioContext _context;
    private readonly Mock<IJogoRepository> _jogoRepositoryMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IAppLogger<JogoService>> _appLoggerMock = new();
    private readonly Mock<IPublishEndpoint> _publishEndpointMock = new();

    private JogoService _service;
    private JogoDTO _jogoDto;
    private Exception _exception;
    private Jogo _jogo;

    public JogoServiceSteps(ScenarioContext context)
    {
        _context = context;
        _service = new JogoService(
            _jogoRepositoryMock.Object,
            _mapperMock.Object,
            _appLoggerMock.Object,
            _publishEndpointMock.Object);
    }

    [Given(@"um jogo com nome ""(.*)"", empresa ""(.*)"", preco (.*), classificacao (.*) e genero (.*)")]
    public void GivenUmJogoComNome(string nome, string empresa, double preco, int classificacao, int genero)
    {
        _jogoDto = new JogoDTO { /* ... */ };
        _jogo = new Jogo { Id = 1, Name = nome, Company = empresa, Price = preco, Rating = (EClassificacao)classificacao, Genre = (EGenero)genero };
        _mapperMock.Setup(m => m.Map<Jogo>(_jogoDto)).Returns(_jogo);
    }

    [Given(@"um jogo existente com id (\d+)")]
    public void GivenUmJogoExistenteComId(int id)
    {
        _jogo = new Jogo { Id = id, /* ... */ };
        _jogoRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(_jogo);
    }

    [Given(@"um jogo DTO com nome ""(.*)"", empresa ""(.*)"", preco (.*), classificacao (.*) e genero (.*)")]
    public void GivenUmJogoDTOParaAtualizar(string nome, string empresa, double preco, int classificacao, int genero)
    {
        _jogoDto = new JogoDTO { /* ... */ };
    }

    [When(@"eu adicionar o jogo")]
    public async Task WhenEuAdicionarOJogo()
    {
        await _service.AddGameAsync(_jogoDto);
    }

    [When(@"eu atualizar o jogo com id (\d+)")]
    public async Task WhenEuAtualizarOJogoComId(int id)
    {
        await _service.UpdateGameByIdAsync(id, _jogoDto);
    }

    [When(@"eu tentar deletar o jogo com id (\d+)")]
    public async Task WhenEuTentarDeletarOJogoComId(int id)
    {
        try { await _service.DeleteGameByIdAsync(id); } catch (Exception ex) { _exception = ex; }
    }

    [Then(@"o repositorio deve ter recebido uma chamada para adicionar o jogo")]
    public void ThenRepositorioDeveReceberChamadaAdd()
    {
        _jogoRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Jogo>()), Times.Once);
    }

    [Then(@"o repositorio deve ter recebido uma chamada para atualizar o jogo")]
    public void ThenRepositorioDeveReceberChamadaUpdate()
    {
        _jogoRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Jogo>()), Times.Once);
    }

    // --- NOVA VERIFICAÇÃO DE MENSAGERIA ---
    [Then(@"uma mensagem deve ser enviada ao RabbitMQ")]
    public void ThenUmaMensagemDeveSerEnviadaAoRabbitMQ()
    {
        // Verifica se o método Publish do MassTransit foi chamado com o evento correto
        _publishEndpointMock.Verify(p => p.Publish(It.IsAny<JogoCriadoEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Then(@"uma excecao NotFoundException deve ser lancada")]
    public void ThenExcecaoNotFoundDeveSerLancada()
    {
        Assert.IsType<NotFoundException>(_exception);
    }
}