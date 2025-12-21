using Application.DTOs;
using Application.Exceptions;
using Application.Helper;
using AutoMapper;
using Domain.Entity;
using Domain.Repository;
using MassTransit;

namespace Application.Services
{
    // Evento de Mudança de Estado (Event Sourcing exigido)
    public class JogoCriadoEvent
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Company { get; set; }
        public double Price { get; set; }
        public int Genre { get; set; }
        public int Rating { get; set; }
    }

    public class JogoService
    {
        private readonly IJogoRepository _jogoRepository;
        private readonly IMapper _mapper;
        private readonly IAppLogger<JogoService> _logger;
        private readonly IPublishEndpoint _publishEndpoint;

        public JogoService(IJogoRepository jogoRepository, IMapper mapper, IAppLogger<JogoService> logger, IPublishEndpoint publishEndpoint)
        {
            _jogoRepository = jogoRepository;
            _mapper = mapper;
            _logger = logger;
            _publishEndpoint = publishEndpoint;
        }

        public async Task AddGameAsync(JogoDTO jogoDTO)
        {
            _logger.LogInformation("Iniciando criação de jogo.");
            ValidateGame(jogoDTO);

            Jogo jogo = _mapper.Map<Jogo>(jogoDTO);
            await _jogoRepository.AddAsync(jogo);

            // REGISTRO DE MUDANÇA DE ESTADO: Notifica o RabbitMQ
            await _publishEndpoint.Publish(new JogoCriadoEvent
            {
                Id = jogo.Id,
                Name = jogo.Name,
                Company = jogo.Company,
                Price = jogo.Price,
                Genre = (int)jogo.Genre,
                Rating = (int)jogo.Rating
            });

            _logger.LogInformation($"Jogo {jogo.Id} criado e evento publicado no RabbitMQ.");
        }

        public async Task UpdateGameByIdAsync(int id, JogoDTO jogoDTO)
        {
            _logger.LogInformation($"Atualizando jogo com id: {id}.");
            ValidateGame(jogoDTO);

            Jogo jogo = await _jogoRepository.GetByIdAsync(id);
            if (jogo == null) throw new NotFoundException("Não existe jogo com Id: " + id);

            _mapper.Map(jogoDTO, jogo);
            await _jogoRepository.UpdateAsync(jogo);

            // Notifica atualização (Event Sourcing)
            await _publishEndpoint.Publish(new JogoCriadoEvent
            {
                Id = jogo.Id,
                Name = jogo.Name,
                Company = jogo.Company,
                Price = jogo.Price,
                Genre = (int)jogo.Genre,
                Rating = (int)jogo.Rating
            });

            _logger.LogInformation($"Jogo {id} atualizado e evento de mudança enviado.");
        }

        public async Task DeleteGameByIdAsync(int id)
        {
            _logger.LogInformation($"Deletando jogo com id: {id}.");
            Jogo jogo = await _jogoRepository.GetByIdAsync(id);

            if (jogo == null) throw new NotFoundException("Não existe jogo com Id: " + id);

            await _jogoRepository.DeleteAsync(jogo);
            _logger.LogInformation($"Jogo {id} removido.");
        }

        public async Task<List<JogoDTO>> GetAllGamesAsync()
        {
            List<Jogo> jogos = (await _jogoRepository.GetAllAsync()).ToList();
            return _mapper.Map<List<JogoDTO>>(jogos);
        }

        private void ValidateGame(JogoDTO jogo)
        {
            string errorMessage = "";
            errorMessage = ValidationHelper.ValidaEmpties<JogoDTO>(jogo, errorMessage);
            if (!string.IsNullOrEmpty(errorMessage)) throw new BadDataException(errorMessage.Trim());
        }
    }
}