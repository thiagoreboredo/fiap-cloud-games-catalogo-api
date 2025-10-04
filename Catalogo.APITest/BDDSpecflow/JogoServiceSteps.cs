using Application.DTOs;
using Application.Exceptions;
using Application.Helper;
using Application.Services;
using AutoMapper;
using Azure.Messaging.ServiceBus;
using Domain.Entity;
using Domain.Entity.Enum;
using Domain.Repository;
using Microsoft.Extensions.Configuration;
using Moq;
using TechTalk.SpecFlow;

[Binding]
public class JogoServiceSteps
{
    private readonly ScenarioContext _context;
    private readonly Mock<IJogoRepository> _jogoRepositoryMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IAppLogger<JogoService>> _appLoggerMock = new();

    // --- NOVOS MOCKS ---
    private readonly Mock<IConfiguration> _configurationMock = new();
    private readonly Mock<ServiceBusClient> _serviceBusClientMock = new();
    private readonly Mock<ServiceBusSender> _serviceBusSenderMock = new();
    // --- FIM DOS NOVOS MOCKS ---

    private JogoService _service;
    private JogoDTO _jogoDto;
    private Exception _exception;
    private Jogo _jogo; // Adicionado para guardar a entidade Jogo

    public JogoServiceSteps(ScenarioContext context)
    {
        _context = context;

        // --- SETUP DOS NOVOS MOCKS ---
        _configurationMock.Setup(c => c["ServiceBus:TopicName"]).Returns("fake-topic");
        _serviceBusSenderMock = new Mock<ServiceBusSender>();
        _serviceBusClientMock.Setup(c => c.CreateSender(It.IsAny<string>())).Returns(_serviceBusSenderMock.Object);
        // --- FIM DO SETUP ---

        // --- ATUALIZAÇÃO DO CONSTRUTOR DO SERVIÇO ---
        _service = new JogoService(
            _jogoRepositoryMock.Object,
            _mapperMock.Object,
            _appLoggerMock.Object,
            _serviceBusClientMock.Object,
            _configurationMock.Object);
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

    // --- NOVA VERIFICAÇÃO ---
    [Then(@"uma mensagem deve ser enviada ao Service Bus")]
    public void ThenUmaMensagemDeveSerEnviadaAoServiceBus()
    {
        _serviceBusSenderMock.Verify(s => s.SendMessageAsync(It.IsAny<ServiceBusMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    // --- FIM DA NOVA VERIFICAÇÃO ---

    [Then(@"uma excecao NotFoundException deve ser lancada")]
    public void ThenExcecaoNotFoundDeveSerLancada()
    {
        Assert.IsType<NotFoundException>(_exception);
    }
}