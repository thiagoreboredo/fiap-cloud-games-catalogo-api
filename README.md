# Microsserviço de Catálogo - FIAP Cloud Games 🚀

Este repositório contém o código-fonte do **Microsserviço de Catálogo**, parte da arquitetura do projeto FIAP Cloud Games da Pós-Graduação em Arquitetura de Sistemas .NET com Azure.

Este serviço é responsável por gerenciar todas as operações relacionadas ao catálogo de jogos da plataforma, incluindo o CRUD (Create, Read, Update, Delete) de jogos e a gestão de promoções.

---

### 🎯 Responsabilidades do Serviço

-   **Gerenciamento de Jogos (CRUD)**: Fornece endpoints para criar, listar, atualizar e deletar jogos no catálogo. Essas operações são restritas a usuários com perfil de "Administrador".
-   **Consulta ao Catálogo**: Permite que qualquer usuário autenticado liste e visualize os jogos disponíveis.
-   **Gerenciamento de Promoções**: (Futuramente) Irá gerenciar a criação e associação de promoções aos jogos.
-   **Publicação de Eventos**: Notifica outros serviços (via mensageria) quando um jogo é criado ou atualizado, para fins de indexação na busca.

---

### 🛠️ Tecnologias Utilizadas

-   **.NET 8**: Plataforma de desenvolvimento.
-   **ASP.NET Core (Minimal API)**: Framework para construção da API.
-   **Entity Framework Core**: ORM para acesso a dados.
-   **PostgreSQL**: Banco de dados relacional.
-   **JWT (JSON Web Tokens)**: Para autorização de endpoints.
-   **xUnit**: Framework para testes de unidade.
-   **Docker**: Para conteinerização da aplicação.
-   **New Relic**: Para monitoramento e observabilidade (APM).

---

### 📂 Estrutura do Projeto

-   **Domain**: Contém as entidades `Jogo`, `Promocao` e as regras de negócio do catálogo.
-   **Application**: Orquestra os casos de uso (Services e DTOs).
-   **Infrastructure**: Implementa a persistência de dados com EF Core e o `DbContext`.
-   **Catalogo.API**: Expõe os endpoints RESTful (`/jogo`).
-   **FIAP-Cloud-GamesTest**: Contém os testes de unidade para o serviço.

---

### ▶️ Como Executar

1.  **Pré-requisitos**: .NET 8 SDK e Docker.
2.  **Configuração**: Ajuste a `ConnectionString` no arquivo `appsettings.Development.json` para apontar para o seu banco de dados PostgreSQL.
3.  **Execução**: Rode o projeto `Catalogo.API` a partir da sua IDE ou via linha de comando com `dotnet run`.