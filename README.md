# FIAP Cloud Games - Catálogo API 🎮

Este microsserviço é o componente central da plataforma **FIAP Cloud Games (FCG)**, responsável pela gestão do inventário de jogos. Ele atua como o produtor de eventos principal, garantindo que todas as alterações no catálogo sejam propagadas de forma assíncrona para o restante ecossistema.

Na **Fase 4**, a API evoluiu para uma arquitetura orientada a eventos (*Event-Driven*) e foi totalmente preparada para a orquestração em larga escala com **Kubernetes (AKS)**.

## 🚀 Evoluções Técnicas (Fase 4)

Implementámos os requisitos mais avançados de arquitetura distribuída e escalabilidade:

- **Arquitetura Event-Driven & Event Sourcing**: A API agora implementa o padrão de *Event Sourcing* para mudanças de estado. Cada criação ou atualização de um jogo dispara um `JogoCriadoEvent`, garantindo a integridade e rastreabilidade dos dados em todo o sistema.
- **Mensageria com RabbitMQ**: Substituição do Azure Service Bus pelo **RabbitMQ** (via **MassTransit**), permitindo uma infraestrutura de mensageria resiliente e desacoplada dentro do cluster Kubernetes.
- **Otimização Docker (Hardening)**: Migração para a imagem base `aspnet:8.0-bookworm-slim`, focando na redução da superfície de ataque e na leveza do contentor.
- **Segurança de Execução**: Implementação de boas práticas de segurança com a execução do processo através de utilizador não-root (`USER $APP_UID`).
- **Kubernetes Nativo**: Preparação de manifestos para deploy no **Azure Kubernetes Service (AKS)** com suporte nativo a **HPA (Horizontal Pod Autoscaler)**.
- **Observabilidade (APM)**: Instrumentação profunda com **New Relic**, monitorizando o throughput de mensagens e a performance das transações de base de dados.

## 🛠 Tecnologias Utilizadas

- **Runtime**: .NET 8 (C#)
- **Mensageria**: RabbitMQ com MassTransit
- **Persistência**: Entity Framework Core com SQL Server
- **Conteinerização**: Docker (Multi-stage build)
- **Monitoramento**: New Relic APM
- **Orquestração**: Kubernetes (AKS)

## 🐳 Execução via Docker (Local)

Para testar o catálogo localmente com suporte a eventos, configure as variáveis do RabbitMQ e do Banco de Dados:

```bash
# Build da imagem otimizada
docker build -t fiap-cloud-games-catalogo-api .

# Execução (Exemplo de variáveis)
docker run -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Sua-String" \
  -e RabbitMQ__Host="localhost" \
  fiap-cloud-games-catalogo-api
```

## ⚓ Kubernetes e Escalabilidade

Este microsserviço foi desenhado para suportar alta carga no cluster AKS:
- **Liveness & Readiness Probes**: O Kubernetes monitoriza a saúde da API e a sua prontidão para receber tráfego, garantindo disponibilidade contínua.
- **HPA (Horizontal Pod Autoscaler)**: Configurado para escalar réplicas automaticamente com base no consumo de CPU e memória, permitindo que o catálogo suporte picos de tráfego sem degradação.

## 📈 Monitoramento (APM)

Através do **New Relic**, monitorizamos:
- Tempo de resposta dos endpoints de gestão de jogos.
- Taxa de sucesso na publicação de eventos no RabbitMQ.
- Saúde da conexão com o SQL Server e métricas de infraestrutura dos Pods.

---
**FIAP - Arquitetura de Sistemas .NET com Azure**
*Grupo 142*