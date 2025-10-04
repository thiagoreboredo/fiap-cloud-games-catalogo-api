using Application.DTOs;
using Application.Exceptions;
using Application.Helper;
using AutoMapper;
using Azure.Messaging.ServiceBus;
using Domain.Entity;
using Domain.Repository;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Application.Services
{
    // Classe que representa a mensagem a ser enviada (deve ser igual à da Function)
    public class JogoDocument
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Company { get; set; }
        public double Price { get; set; }
        public string Genre { get; set; }
        public string Rating { get; set; }
    }

    public class JogoService
    {
        private readonly IJogoRepository _jogoRepository;
        private readonly IMapper _mapper;
        private readonly IAppLogger<JogoService> _logger;
        private readonly ServiceBusClient _serviceBusClient;
        private readonly IConfiguration _configuration;

        public JogoService(IJogoRepository jogoRepository, IMapper mapper, IAppLogger<JogoService> logger, ServiceBusClient serviceBusClient, IConfiguration configuration)
        {
            _jogoRepository = jogoRepository;
            _mapper = mapper;
            _logger = logger;
            _serviceBusClient = serviceBusClient;
            _configuration = configuration;
        }

        public async Task DeleteGameByIdAsync(int id)
        {
            _logger.LogInformation($"Deletando jogo com id: {id}.");
            Jogo jogo = await _jogoRepository.GetByIdAsync(id);

            if (jogo == null)
                throw new NotFoundException("Não existe jogo com Id: " + id);

            await _jogoRepository.DeleteAsync(jogo);
            _logger.LogInformation($"Jogo com id {id} deletado.");
        }

        public async Task UpdateGameByIdAsync(int id, JogoDTO jogoDTO)
        {
            _logger.LogInformation($"Atualizando jogo com id: {id}.");

            ValidateGame(jogoDTO);

            Jogo jogo = await _jogoRepository.GetByIdAsync(id);

            if (jogo == null)
                throw new NotFoundException("Não existe jogo com Id: " + id);

            // Atualiza o objeto 'jogo' com os novos dados do DTO
            _mapper.Map(jogoDTO, jogo);

            await _jogoRepository.UpdateAsync(jogo);

            // Envia a mensagem para o Service Bus após a atualização
            await PublishJogoAtualizadoAsync(jogo);

            _logger.LogInformation($"Jogo com id {id} atualizado e mensagem enviada.");
        }

        public async Task AddGameAsync(JogoDTO jogoDTO)
        {
            _logger.LogInformation("Criando jogo.");

            ValidateGame(jogoDTO);
            Jogo jogo = _mapper.Map<Jogo>(jogoDTO);
            await _jogoRepository.AddAsync(jogo);

            // Envia a mensagem para o Service Bus após a criação
            await PublishJogoAtualizadoAsync(jogo);

            _logger.LogInformation("Jogo criado e mensagem enviada.");
        }

        private async Task PublishJogoAtualizadoAsync(Jogo jogo)
        {
            var topicName = _configuration["ServiceBus:TopicName"];
            ServiceBusSender sender = _serviceBusClient.CreateSender(topicName);

            // Criamos o objeto da mensagem no formato que a Function espera
            var jogoDocument = new JogoDocument
            {
                Id = jogo.Id,
                Name = jogo.Name,
                Company = jogo.Company,
                Price = jogo.Price,
                Genre = jogo.Genre.ToString(),
                Rating = jogo.Rating.ToString()
            };

            string messageBody = JsonSerializer.Serialize(jogoDocument);
            ServiceBusMessage message = new ServiceBusMessage(messageBody);

            try
            {
                await sender.SendMessageAsync(message);
                _logger.LogInformation($"Mensagem para o jogo {jogo.Id} enviada para o tópico {topicName}.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao enviar mensagem para o Service Bus: {ex.Message}");
                // Aqui você poderia adicionar uma lógica de resiliência, como colocar a mensagem em uma fila de "dead letter"
            }
            finally
            {
                await sender.DisposeAsync();
            }
        }

        private void ValidateGame(JogoDTO jogo)
        {
            string errorMessage = "";
            errorMessage = ValidationHelper.ValidaEmpties<JogoDTO>(jogo, errorMessage);

            if (!string.IsNullOrEmpty(errorMessage))
                throw new BadDataException(errorMessage.Trim());
        }

        public async Task<List<JogoDTO>> GetAllGamesAsync()
        {
            _logger.LogInformation("Buscando todos os jogos.");
            List<Jogo> jogos = (await _jogoRepository.GetAllAsync()).ToList();
            _logger.LogInformation($"{jogos.Count} jogos retonaram.");
            return _mapper.Map<List<JogoDTO>>(jogos);
        }
    }
}