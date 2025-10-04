Feature: JogoService

  Scenario: Adicionar um novo jogo valido
    Given um jogo com nome "The Witcher", empresa "CDPR", preco 199.9, classificacao 18 e genero 2
    When eu adicionar o jogo
    Then o repositorio deve ter recebido uma chamada para adicionar o jogo

  Scenario: Atualizar um jogo existente
    Given um jogo existente com id 1
    And um jogo DTO com nome "Cyberpunk", empresa "CDPR", preco 249.9, classificacao 18 e genero 2
    When eu atualizar o jogo com id 1
    Then o repositorio deve ter recebido uma chamada para atualizar o jogo

  Scenario: Deletar um jogo inexistente
    When eu tentar deletar o jogo com id 99
    Then uma excecao NotFoundException deve ser lancada
