// Estado de um jogador durante a partida (snapshot)
export interface PlayerSnapshot {
  hp: number;
  damage_dealt: number;
  mobs_killed: number;
}

// Estado completo da partida armazenado no Redis
export interface MatchState {
  current_stage: number;
  total_stages: number;
  players: Record<string, PlayerSnapshot>;
  total_damage: number;
  started_at: number;
  last_update: number;
}

// Dados de um jogador no resumo final
export interface MatchPlayerSummary {
  player_id: string;
  username: string;
  skin_id: string;
  final_hp: number;
  damage_dealt: number;
  mobs_killed: number;
}

// Resultado da partida
export type MatchResult = "victory" | "defeat";

// Resumo final da partida (para persistência futura no banco)
export interface MatchSummary {
  session_id: string;
  room_code: string;
  started_at: number;
  ended_at: number;
  duration: number;
  total_players: number;
  total_damage: number;
  final_stage: number;
  result: MatchResult;
  players: MatchPlayerSummary[];
}

// Payload do snapshot enviado pelo Unity/Mirror
export interface SnapshotPayload {
  session_id: string;
  snapshot: {
    current_stage: number;
    total_stages?: number;
    players: Record<string, PlayerSnapshot>;
    total_damage: number;
  };
}

// Payload para iniciar a partida
export interface StartMatchPayload {
  room_code: string;
  total_stages: number;
  players: Array<{
    player_id: string;
    username: string;
    skin_id: string;
  }>;
}

// Payload para finalizar a partida
export interface EndMatchPayload {
  session_id: string;
  result: MatchResult;
  players_final?: Array<{
    player_id: string;
    username: string;
    skin_id: string;
  }>;
}
