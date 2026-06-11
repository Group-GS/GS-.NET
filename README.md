# AgroSpace - Monitoramento Agrícola Espacial

**Global Solution 2026/1 - FIAP**
> *Conectando a exploração espacial a problemas reais na Terra.*

---

# Demonstração e Apresentação

## Vídeo Pitch
* https://www.youtube.com/watch?v=DtSzmFYKgts

## Vídeo Demonstrativo
* https://www.youtube.com/watch?v=Yc95m3ID53Q

---

## A Solução
O **AgroSpace** é uma plataforma de monitoramento agrícola baseada em IoT que auxilia produtores rurais no acompanhamento das condições climáticas e ambientais. Utilizando sensores conectados, processamos dados vitais (como temperatura e umidade) de diferentes biomas, prevenindo a perda de produtividade e gerando histórico para decisões estratégicas.

---

## Arquitetura do Projeto (.NET Core API)
A API foi desenvolvida em **C# (.NET Core)** seguindo os princípios RESTful e organizada em camadas lógicas para garantir a manutenibilidade:

* **Controllers:** Gerenciam as requisições HTTP e definem os *endpoints* da API.
* **Models:** Representam as entidades do domínio (Local, Sensor) e suas restrições (Data Annotations).
* **Data (DbContext):** Configuração do Entity Framework Core e mapeamento objeto-relacional.

**Por que essa arquitetura?** A separação de responsabilidades permite que a aplicação seja facilmente escalável. Se precisarmos adicionar uma nova funcionalidade (ex: uma tabela de `Alertas`), basta criar um novo Model, adicioná-lo ao DbContext e expor um novo Controller sem quebrar o código existente.

---

## 🗄️ Modelagem de Dados e Relacionamentos

O banco de dados utilizado é o **Oracle**. A estrutura foi criada utilizando a abordagem *Code-First* com o Entity Framework Core Migrations.

### Relacionamento Principal (1:N)
Implementamos um relacionamento de um-para-muitos (1:N) entre as entidades `Local` e `Sensor`. 
* **Por quê?** Um local físico (ex: Fazenda Verde no bioma Amazônia) precisa abrigar múltiplos sensores (temperatura, umidade, pressão) espalhados pela plantação. 
* **Comportamento de Exclusão:** Configuramos o comportamento em *Cascade* (Cascata). Se um `Local` for deletado, todos os `Sensores` vinculados a ele serão automaticamente excluídos do banco, evitando dados órfãos e inconsistências.

*<img width="1147" height="617" alt="image" src="https://github.com/user-attachments/assets/542470e8-14d6-4561-b6a9-2d998be602ed" />*

---

## 🚀 Como Executar o Projeto

### Pré-requisitos
* .NET 8 (ou superior) instalado.
* Acesso ao banco de dados Oracle (Credenciais configuradas no `appsettings.json`).

### Passo a Passo
1.  Clone o repositório:
    ```bash
    git clone [https://github.com/SeuUsuario/AgroSpace.Api.git](https://github.com/SeuUsuario/AgroSpace.Api.git)
    cd AgroSpace.Api
    ```
2.  Restaure as dependências do projeto:
    ```bash
    dotnet restore
    ```
3.  Aplique as Migrations para criar/atualizar o banco de dados:
    ```bash
    dotnet ef database update
    ```
4.  Execute a aplicação:
    ```bash
    dotnet run
    ```
5.  Acesse o **Swagger** para visualizar a documentação interativa e testar as rotas em: `http://localhost:<porta>/swagger`.

---

## 🧪 Testes e Validações

Nossa API trata dados inválidos de forma rigorosa, retornando os *HTTP Status Codes* apropriados:

* **Buscas Inválidas:** Retorna `404 Not Found` ao buscar um ID inexistente.
* **Entradas Inválidas (Business Rule):** Ao tentar criar (`POST`) ou atualizar (`PUT`) um Sensor enviando um `IdLocal` que não existe no banco, a API intercepta a requisição antes de salvar e retorna `400 Bad Request` com a mensagem: *"Não é possível cadastrar: O Local informado não existe."*

### Exemplo de Payload (POST /api/Sensores)
```json
{
  "tipoSensor": "Temperatura",
  "status": "ATIVO",
  "idLocal": 1
}
