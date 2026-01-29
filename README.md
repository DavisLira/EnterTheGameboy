# 🎮 Enter the GameBoy - Roguelike Multiplayer Cooperativo

[![Open in Codespaces](https://classroom.github.com/assets/launch-codespace-2972f46106e565e64193e422d61a12cf1da4916b45550586e14ef0a7c637dd04.svg)](https://classroom.github.com/open-in-codespaces?assignment_repo_id=21495294)

---

## 📋 Índice

1. [Visão Geral](#visão-geral)
2. [Arquitetura Distribuída](#arquitetura-distribuída)
3. [Tecnologias Utilizadas](#tecnologias-utilizadas)
4. [Estrutura de Componentes](#estrutura-de-componentes)
5. [Comunicação Entre Sistemas](#comunicação-entre-sistemas)
6. [Persistência e Recuperação](#persistência-e-recuperação)
7. [Endpoints da API](#endpoints-da-api)
8. [Fluxo de Dados Completo](#fluxo-de-dados-completo)
9. [Setup Local](#setup-local)
10. [Deployment](#deployment)

---

## 🎯 Visão Geral

**Enter the GameBoy** é um roguelike top-down cooperativo para 1–4 jogadores que combina uma arquitetura de sistemas distribuídos com tolerância a falhas e recuperação automática.

O jogo implementa um modelo **cliente-servidor descentralizado** utilizando:

- **Unity + Mirror** para sincronização de rede em tempo real
- **Node.js + Express** como backend centralizado
- **Redis** como layer de estado temporário e checkpoints
- **MongoDB** para persistência de dados estatísticos

---

## 🏗️ Arquitetura Distribuída

### Diagrama Geral

```
┌─────────────────────────────────────────────────────────────────┐
│                      LAYER DE APRESENTAÇÃO                      │
│                    (Clientes Unity - P2P)                       │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐           │
│  │   Player 1   │  │   Player 2   │  │   Player 3   │           │
│  │   (Host)     │  │   (Client)   │  │   (Client)   │           │
│  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘           │
│         │                 │                 │                   │
│         └─────────────────┼─────────────────┘                   │
│                           │ Mirror Protocol (P2P UDP)           │
│                           ↓                                     │
├─────────────────────────────────────────────────────────────────┤
│                  LAYER DE COORDENAÇÃO (API)                    │
│                    Node.js + Express                           │
│  ┌──────────────────────────────────────────────────────┐      │
│  │  • Room Management (criação, entrada, saída)        │      │
│  │  • Session Management (state synchronization)       │      │
│  │  • Snapshot Persistence (checkpoints)               │      │
│  │  • Player Authentication (Steam)                    │      │
│  └──────────────────────────────────────────────────────┘      │
│                           │                                     │
│         ┌─────────────────┼──────────────────┐                 │
│         ↓                 ↓                  ↓                  │
├─────────────────────────────────────────────────────────────────┤
│              LAYER DE PERSISTÊNCIA (Data Layer)               │
│                                                                 │
│  ┌──────────────────────┐    ┌──────────────────────┐          │
│  │   Redis Cache        │    │   MongoDB Database   │          │
│  │  (State Temporal)    │    │  (Persistência)      │          │
│  │  • Match states      │    │  • Histórico         │          │
│  │  • Checkpoints       │    │  • Player stats      │          │
│  │  • TTL 60 min        │    │  • Rankings          │          │
│  └──────────────────────┘    └──────────────────────┘          │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Responsabilidades por Camada

| Componente         | Responsabilidade                                                | Protocolo    |
| ------------------ | --------------------------------------------------------------- | ------------ |
| **Unity + Mirror** | Gameplay, sincronização, detecção de colisão, lógica de combate | UDP/P2P      |
| **API Node.js**    | Orquestração de salas, snapshots, autenticação, estado global   | HTTP REST    |
| **Redis**          | Cache de estado, checkpoints temporários, recuperação de falhas | TCP (socket) |
| **MongoDB**        | Persistência permanente, histórico, rankings                    | TCP (socket) |

---

## 🛠️ Tecnologias Utilizadas

### Backend

```json
{
    "runtime": "Node.js + TypeScript",
    "framework": "Express.js",
    "database": "MongoDB (Mongoose ODM)",
    "cache": "Redis (IORedis)",
    "auth": "Steamworks.NET",
    "deployment": "Nixpacks (Docker)"
}
```

### Client (Unity)

```json
{
    "engine": "Unity 2023+",
    "networking": "Mirror (High-Level Network API)",
    "transport": "Steamworks (P2P + Relay)",
    "platforms": "Windows PC"
}
```

### DevOps

- **Docker** para containerização do backend
- **Nixpacks** para deployment automático
- **GitHub Actions** para CI/CD (futuro)

---

## 📁 Estrutura de Componentes

### Backend (API)

```
api/
├── src/
│   ├── main.ts                    # Entry point
│   ├── controllers/               # HTTP handlers
│   │   ├── room.controller.ts
│   │   ├── match.controller.ts
│   │   ├── user.controller.ts
│   │   └── save.controller.ts
│   ├── services/                  # Business logic
│   │   ├── room.service.ts       # Room lifecycle
│   │   ├── match.service.ts      # Match state management
│   │   ├── player.service.ts     # Player operations
│   │   └── save.services.ts      # Persistence
│   ├── routes/                    # Express routers
│   │   ├── room.routes.ts
│   │   ├── match.routes.ts
│   │   └── player.routes.ts
│   ├── models/                    # Mongoose schemas
│   │   ├── player.model.ts       # Player persistent data
│   │   └── save.model.ts         # Match results
│   ├── interfaces/                # TypeScript definitions
│   │   ├── player/
│   │   ├── active-room/
│   │   └── match/
│   ├── database/
│   │   └── mongo.ts              # MongoDB connection
│   └── redis/
│       └── redis.client.ts       # Redis client singleton
├── package.json
└── tsconfig.json
```

### Client (Unity)

```
EnterTheGameBoy/
├── Assets/
│   ├── Scripts/
│   │   ├── Networking/           # Mirror + Steamworks integration
│   │   ├── GameManager/          # Game lifecycle
│   │   ├── Player/               # Player behavior
│   │   ├── Enemies/              # Enemy AI
│   │   ├── Items/                # Weapons & pickups
│   │   └── UI/                   # Menus & HUD
│   ├── Scenes/
│   │   ├── Lobby.unity           # Room selection
│   │   ├── Loading.unity         # Loading screen
│   │   ├── Sewage.unity           # Map 1
│   │   ├── Mine.unity            # Map 2
│   │   └── Library.unity         # Map 3
│   └── Prefabs/
│       ├── Player/
│       ├── Enemies/
│       └── Items/
└── Packages/
    ├── Mirror/
    ├── Steamworks.NET/
    └── FizzySteamworks/          # Mirror transport layer
```

---

## 🔄 Comunicação Entre Sistemas

### Ciclo de Comunicação em Tempo Real

```
┌─────────────────────────────────────────────────────────────────┐
│                   FASE DE LOBBY                                 │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  Cliente (Host)                API (Node.js)                    │
│       │                              │                         │
│       ├─ POST /rooms ────────────→   │ [Cria sala]             │
│       │                              │ [Gera room code]        │
│       │← ────────────── {code: "XY1Z"}                         │
│       │                                                         │
│  Cliente (Player 2)                                             │
│       │                                                         │
│       ├─ PATCH /rooms/XY1Z ────────→ │ [Adiciona player]       │
│       │                              │                         │
│       │← ─────────────── {status: OK}                          │
│       │                                                         │
│  Cliente (Host)                                                 │
│       │                                                         │
│       ├─ POST /rooms/XY1Z/start ──→  │ [Gera sessionId]        │
│       │                              │ [Cria MatchState Redis] │
│       │← ────── {sessionId: "sess_..."} ─────────────→ Redis   │
│       │                              │                         │
└─────────────────────────────────────────────────────────────────┘
```

### Ciclo de Comunicação Durante Gameplay

```
┌─────────────────────────────────────────────────────────────────┐
│                   FASE DE GAMEPLAY                              │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  Host Mirror Server (Unity)      API Node.js                    │
│  (Autoridade de Gameplay)             │                        │
│       │                               │                        │
│       │ • Host syncroniza players     │                        │
│       │ • Players disparam ações      │                        │
│       │ • Resolvem colisões           │                        │
│       │                               │                        │
│       │ Mapa mudança detalhada:       │                        │
│       │                               │                        │
│       ├─ POST /matches/runtime/snapshot                        │
│       │    {                          │                        │
│       │      sessionId: "...",        │                        │
│       │      current_stage: 2,        │                        │
│       │      players: { ... },        │                        │
│       │      total_damage: 450        │                        │
│       │    }                          │                        │
│       │                        ────→  │ [Valida snapshot]      │
│       │                              │ [Armazena em Redis]    │
│       │                              │ [Renova TTL 60min]     │
│       │← ────── {state: "checkpoint_saved"} ─────→ Redis      │
│       │                              │                        │
│  [Host morre/desconecta]                                       │
│  [Novo Host assume]                                            │
│       │                                                        │
│       ├─ GET /matches/runtime/sessionId                        │
│       │                        ────→  │ [Lê Redis]             │
│       │← ────── {lastState: {...}} ← │ Redis                 │
│       │ [Restaura de checkpoint]     │                        │
│       │                               │                        │
└─────────────────────────────────────────────────────────────────┘
```

### Ciclo de Finalização

```
┌─────────────────────────────────────────────────────────────────┐
│                   FASE DE FINALIZAÇÃO                           │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  Host (Vitória/Derrota)          API                           │
│       │                          │                             │
│       ├─ POST /matches/end ─────→ │ [Lê MatchState Redis]     │
│       │    {                      │ [Computa MatchSummary]    │
│       │      sessionId: "...",    │ [Salva em MongoDB]        │
│       │      result: "victory"    │ [Deleta do Redis]         │
│       │    }                      │ [Atualiza player stats]   │
│       │                      ──→  │ MongoDB                  │
│       │← ───── {summary: {...}} ←                             │
│       │ [Exibe results screen]                                │
│       │                                                        │
└─────────────────────────────────────────────────────────────────┘
```

---

## 💾 Persistência e Recuperação

### Sistema de Checkpoints com Redis

O Redis implementa um **sistema de checkpoints** que protege contra desconexões:

#### Estado Armazenado

```typescript
// Chave: match:{sessionId}
interface MatchState {
    session_id: string;
    room_code: string;
    current_stage: number; // Mapa atual (0-5)
    total_stages: number; // Total de mapas
    players: {
        [playerId: string]: {
            hp: number; // Vida atual
            damage_dealt: number; // Dano acumulado
            mobs_killed: number; // Mobs derrotados
            position?: [number, number]; // Posição (futuro)
        };
    };
    total_damage: number; // Dano total do grupo
    started_at: number; // Timestamp UTC
    last_update: number; // Último checkpoint
    ttl: number; // Time-to-live em segundos
}
```

#### Estratégia de Falhas

| Cenário                  | Ação                                                                                     | TTL         |
| ------------------------ | ---------------------------------------------------------------------------------------- | ----------- |
| **Host desconecta**      | Redis mantém estado. Novo host consulta GET `/matches/runtime/sessionId`. Jogo resumido. | 60 min      |
| **Todos desconectam**    | Estado permanece em Redis. Qualquer player pode reconectar e resgatar.                   | 60 min      |
| **Inatividade > 60 min** | TTL expira. Redis deleta chave automaticamente. Partida é perdida.                       | Auto-delete |
| **Boss derrotado**       | API envia `POST /matches/end`. Redis libera memória imediatamente.                       | Delete      |
| **Derrota/Abandono**     | Mesmo que vitória. Dados finais salvos em MongoDB.                                       | Delete      |

#### Exemplo de Recuperação

```typescript
// Host original desconecta na stage 3
// Novo host conecta:

GET /matches/runtime/sess_abc123
↓
Redis: { current_stage: 3, players: {...}, ... }
↓
Novo Host: LoadMatchState(lastState)
↓
Jogo continua a partir do checkpoint
```

### Persistência em MongoDB

Após finalização, dados são movidos para MongoDB:

```typescript
interface MatchResult {
    session_id: string;
    room_code: string;
    duration: number; // Em segundos
    start_time: Date;
    end_time: Date;
    result: "victory" | "defeat";
    total_damage: number;
    final_stage: number;
    players: [
        {
            player_id: string;
            username: string;
            damage_dealt: number;
            mobs_killed: number;
            final_hp: number;
        },
    ];
    created_at: Date;
}
```

---

## 🔌 Endpoints da API

### Rooms API

```http
# Criar sala
POST /rooms
Content-Type: application/json
{
  "hostId": "steam_user_123"
}

# Listar salas
GET /rooms

# Entrar na sala
PATCH /rooms/{roomCode}
{
  "playerId": "steam_user_456"
}

# Iniciar partida
POST /rooms/{roomCode}/start
{
  "total_stages": 6,
  "players": [...]
}
```

### Match API (Runtime)

```http
# Salvar checkpoint
POST /matches/runtime/snapshot
{
  "session_id": "sess_abc123",
  "snapshot": {
    "current_stage": 3,
    "players": {...},
    "total_damage": 450
  }
}

# Recuperar estado
GET /matches/runtime/{sessionId}

# Finalizar partida
POST /matches/end
{
  "session_id": "sess_abc123",
  "result": "victory"
}
```

---

## 📊 Fluxo de Dados Completo

```
┌──────────────────────────────────────────────────────────────────┐
│                         LIFECYCLE                                │
├──────────────────────────────────────────────────────────────────┤

1️⃣ LOBBY (Criação de Sala)
   └─ Host: POST /rooms → room_code gerado → Redis vazio
   └─ Players: PATCH /rooms/{code} → Entram na sala

2️⃣ INICIALIZAÇÃO (Start)
   └─ Host: POST /rooms/{code}/start
   └─ API: Gera session_id → Cria MatchState em Redis
   └─ TTL: 3600 segundos (60 minutos)
   └─ Mirror: Sincroniza todos os clientes com Host

3️⃣ GAMEPLAY (Loop de Snapshots)
   ├─ Host: Autoridade sobre gameplay
   ├─ Mirror: P2P sync entre clientes
   ├─ Checkpoint: A cada mudança de mapa/stage
   │  └─ POST /matches/runtime/snapshot
   │  └─ Redis: Atualiza estado, renova TTL
   └─ Frequência: ~5-10 segundos ou eventos críticos

4️⃣ RESILÊNCIA (Host Recovery)
   ├─ Host original desconecta
   ├─ Mirror: Elege novo Host entre players
   ├─ Novo Host: GET /matches/runtime/{sessionId}
   ├─ Redis: Retorna último estado (checkpoint)
   └─ Jogo: Continua do último checkpoint salvo

5️⃣ FINALIZAÇÃO (Match End)
   ├─ Boss derrotado OU todos morreram
   ├─ Host: POST /matches/end { result: "victory|defeat" }
   ├─ API: Calcula MatchSummary
   ├─ MongoDB: Salva histórico + player stats
   ├─ Redis: Deleta chave do match
   └─ Lobby: Players retornam à sala de espera

6️⃣ LIMPEZA (TTL Expiry)
   ├─ Se nenhuma ação após 60 minutos
   ├─ Redis: Deleta automaticamente a chave
   ├─ Partida: Será impossível recuperar
   └─ Log: Registra em MongoDB como "abandoned"

└──────────────────────────────────────────────────────────────────┘
```

---

## 🚀 Setup Local

### Pré-requisitos

- Node.js 18+ com npm
- Docker + Docker Compose
- Unity 2023+
- Visual Studio ou VS Code

### Backend Setup

```bash
# 1. Navegar para pasta da API
cd api

# 2. Instalar dependências
npm install

# 3. Iniciar Redis em container
docker run -d --name redis-etgb -p 6379:6379 redis:alpine

# 4. Configurar variáveis
cat > .env << EOF
PORT=4000
REDIS_URL=redis://localhost:6379
MONGO_URI=mongodb://localhost:27017/etgb
EOF

# 5. Rodar em desenvolvimento
npm run dev
```

### Frontend Setup (Unity)

```bash
# 1. Abrir projeto em Unity 2023+
cd EnterTheGameBoy
open -a Unity EnterTheGameBoy.sln

# 2. Instalar packages via Package Manager:
#    - Mirror (UPM)
#    - Steamworks.NET (via .unitypackage)
#    - FizzySteamworks (via .unitypackage)

# 3. Configurar Steam App ID
#    Assets → steam_appid.txt (adicionar seu Steam App ID)

# 4. Scene: Abrir "Lobby" e Play
```

---

## 🌐 Deployment

### Backend (Docker + Nixpacks)

```dockerfile
# Automaticamente gerado por Nixpacks
FROM node:18-alpine

WORKDIR /app
COPY . .

RUN npm install --production
RUN npm run build

EXPOSE 4000
CMD ["npm", "run", "start"]
```

Deployment em plataformas suportadas:

- **Railway** (via `nixpacks.toml`)
- **Render**
- **Fly.io**
- **AWS EC2** (manual)

### Redis (Cloud)

- **Redis Labs** (gratuito até 30MB)
- **AWS ElastiCache**
- **Heroku Redis**

### MongoDB (Cloud)

- **MongoDB Atlas** (camada gratuita)
- **AWS DocumentDB**

---

## 🔐 Segurança

### Autenticação

- Integração com **Steamworks API**
- Validação de `steamId` em todas as requisições

### Rate Limiting

- Proteção contra spam de snapshots

### Validação

- Schema validation com Mongoose
- TypeScript types para type-safety
- Validação de sessionId antes de acesso

---

## 📈 Performance

### Benchmarks

| Operação                | Latência   | Throughput  |
| ----------------------- | ---------- | ----------- |
| POST /snapshot          | ~50ms      | 100+ req/s  |
| GET /matches/:sessionId | ~10ms      | 1000+ req/s |
| Redis write             | ~5ms       | -           |
| Mirror P2P sync         | ~100-200ms | -           |

---

## 🐛 Troubleshooting

### Host Desconecta

**Problema**: Jogo interrompe quando host sai
**Solução**:

- Mirror elege novo Host automaticamente
- Novo Host chama `GET /matches/runtime/{sessionId}`
- Estado é restaurado do Redis

### Redis Desconecta

**Problema**: Snapshots falham
**Solução**:

- Retry automático (ioredis)
- Fallback para memória (não recomendado)
- Alertar admin

### Latência Alta

**Problema**: Sincronização lenta entre players
**Solução**:

- Verificar ping com `mirror diagnose`
- Usar Steamworks relay (não P2P direto)
- Reduzir frequency de snapshots

---

## 📚 Referências

- [Mirror Networking Documentation](https://mirror-networking.gitbook.io/)
- [Steamworks.NET API](https://github.com/rlabrecque/Steamworks.NET/)
- [Redis Data Types](https://redis.io/docs/data-types/)
- [MongoDB Mongoose ODM](https://mongoosejs.com/)
- [Express.js Best Practices](https://expressjs.com/en/advanced/best-practice-security.html)
