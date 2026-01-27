# *ETGB API - Roguelike Multiplayer Game Server*

API Node.js para gerenciamento de salas e estado de partidas do jogo roguelike multiplayer, utilizando Redis para estado temporário e recuperação de falhas.

## 📐 Arquitetura

```bash
Cliente Unity
   ↓ (Mirror)
Host (Mirror Server)
   ↓ HTTP
API Node.js (Express)
   ↓
Redis (estado temporário)
   ↓
Banco de Dados (estatísticas finais - futuro)
```

### Responsabilidades

| Componente | Função |
| -------- | ------ |
| **Unity + Mirror** | Gameplay, lógica de mapas, vida, dano, mobs. Decide quando enviar snapshots |
| **API Node.js** | Gerencia salas, recebe snapshots, persiste estatísticas, controla Redis |
| **Redis** | Estado temporário, progresso da run, checkpoints, recuperação de falha |
| **Banco de Dados** | Histórico de partidas, estatísticas finais, ranking (futuro) |

---

## 🚀 Quick Start

```bash
# 1. Instalar dependências
npm install

# 2. Iniciar Redis (Docker)
docker run -d --name redis-etgb -p 6379:6379 redis:alpine

# 3. Configurar .env
PORT=4000
REDIS_URL=redis://localhost:6379

# 4. Rodar em desenvolvimento
npm run dev
```

---

## 📁 Estrutura do Projeto

```bash
src/
├── controllers/          # Controladores HTTP
│   ├── room.controller.ts
│   ├── user.controller.ts
│   └── match.controller.ts
├── services/             # Lógica de negócio
│   ├── room.service.ts
│   ├── player.service.ts
│   └── match.service.ts
├── routes/               # Definição de rotas
│   ├── active-room.routes.ts
│   ├── player.routes.ts
│   └── match.routes.ts
├── interfaces/           # TypeScript interfaces
│   ├── active-room/
│   ├── player/
│   └── match/
├── redis/                # Cliente Redis
│   └── redis.client.ts
└── main.ts               # Entry point
```

---

## 🔌 Endpoints da API

### Players

| Método | Rota | Descrição |
| -------- | ------ | ----------- |
| `POST` | `/players` | Criar player |
| `GET` | `/players` | Listar todos players |
| `GET` | `/players/:id` | Buscar player por ID |

#### Criar Player

```http
POST /players
Content-Type: application/json

{
  "username": "gabriel",
  "password": "123456"
}
```

---

### Rooms (Salas)

| Método | Rota | Descrição |
| -------- | ------ | ----------- |
| `POST` | `/rooms` | Criar sala |
| `GET` | `/rooms` | Listar todas as salas |
| `GET` | `/rooms/:roomCode` | Buscar sala por código |
| `PATCH` | `/rooms/:roomCode` | Adicionar player na sala |
| `POST` | `/rooms/:roomCode/start` | **Iniciar partida** |

#### Criar Sala

```http
POST /rooms
Content-Type: application/json

{
  "hostId": "player_abc123"
}
```

**Response:**

```json
{
  "message": "Room created",
  "room": {
    "hostId": "player_abc123",
    "roomCode": "H7B2",
    "status": 0,
    "playersIds": ["player_abc123"]
  }
}
```

#### Adicionar Player na Sala

```http
PATCH /rooms/H7B2
Content-Type: application/json

{
  "playerId": "player_def456"
}
```

#### Iniciar Partida (Host)

```http
POST /rooms/H7B2/start
Content-Type: application/json

{
  "total_stages": 6,
  "players": [
    {
      "player_id": "p1",
      "username": "Gabriel",
      "skin_id": "knight_red"
    },
    {
      "player_id": "p2",
      "username": "Joao",
      "skin_id": "mage_blue"
    }
  ]
}
```

**Response:**

```json
{
  "message": "Room started",
  "session_id": "sess_a1b2c3d4e5f6g7h8",
  "room": { ... }
}
```

> ⚠️ O `session_id` retornado deve ser usado em todos os endpoints de match

---

### Matches (Partidas em Andamento)

| Método | Rota | Descrição |
| -------- | ------ | ----------- |
| `POST` | `/matches/runtime/snapshot` | **Salvar checkpoint (mudança de mapa)** |
| `GET` | `/matches/runtime/:sessionId` | Recuperar estado (host recovery) |
| `POST` | `/matches/end` | Finalizar partida |

#### Salvar Snapshot (Checkpoint de Mapa)

**Quando enviar?**

- ✅ A cada **mudança de mapa/stage** (checkpoint)
- ✅ Eventos críticos (boss, morte de player)
- ✅ Opcionalmente a cada 5-10 segundos

```http
POST /matches/runtime/snapshot
Content-Type: application/json

{
  "session_id": "sess_a1b2c3d4e5f6g7h8",
  "snapshot": {
    "current_stage": 3,
    "players": {
      "p1": { "hp": 40, "damage_dealt": 300, "mobs_killed": 15 },
      "p2": { "hp": 25, "damage_dealt": 260, "mobs_killed": 12 }
    },
    "total_damage": 560
  }
}
```

**Response:**

```json
{
  "message": "Snapshot saved",
  "state": {
    "current_stage": 3,
    "total_stages": 6,
    "players": { ... },
    "total_damage": 560,
    "started_at": 1700000000000,
    "last_update": 1700000450000
  }
}
```

#### Recuperar Estado (Host Recovery)

Usado quando o host original morre e um novo host assume a partida.

```http
GET /matches/runtime/sess_a1b2c3d4e5f6g7h8
```

**Response:**

```json
{
  "state": {
    "current_stage": 3,
    "total_stages": 6,
    "players": {
      "p1": { "hp": 40, "damage_dealt": 300, "mobs_killed": 15 },
      "p2": { "hp": 25, "damage_dealt": 260, "mobs_killed": 12 }
    },
    "total_damage": 560,
    "room_code": "H7B2",
    "players_meta": [ ... ]
  },
  "ttl_seconds": 3420
}
```

#### Finalizar Partida

```http
POST /matches/end
Content-Type: application/json

{
  "session_id": "sess_a1b2c3d4e5f6g7h8",
  "result": "victory"
}
```

**Response:**

```json
{
  "message": "Match ended",
  "summary": {
    "session_id": "sess_a1b2c3d4e5f6g7h8",
    "room_code": "H7B2",
    "started_at": 1700000000000,
    "ended_at": 1700001800000,
    "duration": 1800,
    "total_players": 2,
    "total_damage": 540,
    "final_stage": 6,
    "result": "victory",
    "players": [
      {
        "player_id": "p1",
        "username": "Gabriel",
        "skin_id": "knight_red",
        "final_hp": 20,
        "damage_dealt": 300,
        "mobs_killed": 18
      }
    ]
  }
}
```

---

## 🔄 Fluxo Completo do Jogo

```bash
┌─────────────────────────────────────────────────────────────────┐
│                        LOBBY PHASE                              │
├─────────────────────────────────────────────────────────────────┤
│  1. Host cria sala        →  POST /rooms                        │
│  2. Players entram        →  PATCH /rooms/:code                 │
│  3. Host inicia partida   →  POST /rooms/:code/start            │
│     └─→ Redis: Cria estado inicial (TTL 60 min)                 │
│     └─→ Retorna session_id                                      │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│                      GAMEPLAY PHASE                             │
├─────────────────────────────────────────────────────────────────┤
│  Loop: A cada mudança de mapa                                   │
│    └─→ POST /matches/runtime/snapshot                           │
│        └─→ Redis: Atualiza estado, renova TTL                   │
│                                                                 │
│  Se host morrer:                                                │
│    └─→ Novo host chama GET /matches/runtime/:sessionId          │
│    └─→ Estado recuperado do Redis                               │
│    └─→ Jogo continua do último checkpoint                       │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│                        END PHASE                                │
├─────────────────────────────────────────────────────────────────┤
│  Boss derrotado OU todos morreram                               │
│    └─→ POST /matches/end { result: "victory" | "defeat" }       │
│        └─→ Redis: Lê estado final, deleta chave                 │
│        └─→ Retorna MatchSummary completo                        │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🗂️ Estruturas de Dados

### RoomStatus

```typescript
enum RoomStatus {
  PENDING = 0,      // Aguardando players
  IN_PROGRESS = 1,  // Partida em andamento
  ENDED = 2         // Partida finalizada
}
```

### PlayerSnapshot (durante partida)

```typescript
interface PlayerSnapshot {
  hp: number;           // Vida atual
  damage_dealt: number; // Dano total causado
  mobs_killed: number;  // Mobs eliminados
}
```

### MatchState (Redis)

```typescript
interface MatchState {
  current_stage: number;  // Mapa/stage atual
  total_stages: number;   // Total de mapas
  players: Record<string, PlayerSnapshot>;
  total_damage: number;   // Dano total do grupo
  started_at: number;     // Timestamp início
  last_update: number;    // Último snapshot
}
```

### MatchSummary (fim da partida)

```typescript
interface MatchSummary {
  session_id: string;
  room_code: string;
  started_at: number;
  ended_at: number;
  duration: number;        // Em segundos
  total_players: number;
  total_damage: number;
  final_stage: number;
  result: "victory" | "defeat";
  players: MatchPlayerSummary[];
}
```

---

## ⚡ Redis: Sistema de Checkpoints

O Redis armazena o estado da partida como um **checkpoint** que é atualizado a cada mudança de mapa.

### Chave

```bash
match:{session_id}
```

### TTL

- **60 minutos** de expiração
- Renovado a cada snapshot

### Casos de Uso

| Cenário | Comportamento |
| --------- | -------------- |
| **Mudança de mapa** | Unity envia snapshot → Redis atualiza estado |
| **Host desconecta** | Novo host consulta API → Redis retorna último estado |
| **Todos morrem** | API envia `result: "defeat"` → Redis limpa dados |
| **Boss derrotado** | API envia `result: "victory"` → Redis limpa dados |
| **Partida abandonada** | TTL expira após 60 min → Redis limpa automaticamente |

---

## 🧠 Regra de Ouro

> **Unity joga** → Controla gameplay, vida, dano, mobs
> **API coordena** → Gerencia salas, recebe snapshots
> **Redis protege** → Guarda checkpoints, permite recovery
> **Banco registra** → Histórico final (futuro)

---

## 📜 Scripts

```bash
npm run dev        # Desenvolvimento com hot reload
npm run start      # Produção
npm run typecheck  # Verificação de tipos TypeScript
```

---

## 🔧 Variáveis de Ambiente

| Variável | Descrição | Default |
| --------- | ----------- | --------- |
| `PORT` | Porta do servidor | `4000` |
| `REDIS_URL` | URL de conexão Redis | `redis://localhost:6379` |
