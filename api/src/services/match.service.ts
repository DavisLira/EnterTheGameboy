import { redis } from "../redis/redis.client";
import {
  MatchState,
  MatchSummary,
  MatchResult,
  PlayerSnapshot,
  MatchPlayerSummary,
} from "../interfaces/match/match-state.interface";

const MATCH_TTL_SECONDS = 60 * 60; // 60 minutos

export class MatchService {
  private getMatchKey(sessionId: string): string {
    return `match:${sessionId}`;
  }

  async initializeMatchState(
    sessionId: string,
    roomCode: string,
    totalStages: number,
    players: Array<{ player_id: string; username: string; skin_id: string }>
  ): Promise<MatchState> {
    const now = Date.now();

    const playersState: Record<string, PlayerSnapshot> = {};
    for (const player of players) {
      playersState[player.player_id] = {
        hp: 100,
        damage_dealt: 0,
        mobs_killed: 0,
      };
    }

    const matchState: MatchState = {
      current_stage: 1,
      total_stages: totalStages,
      players: playersState,
      total_damage: 0,
      started_at: now,
      last_update: now,
    };

    // Armazena também o room_code e player metadata para uso posterior
    const stateToStore = {
      ...matchState,
      room_code: roomCode,
      players_meta: players,
    };

    await redis.setex(
      this.getMatchKey(sessionId),
      MATCH_TTL_SECONDS,
      JSON.stringify(stateToStore)
    );

    return matchState;
  }

  async saveSnapshot(
    sessionId: string,
    snapshot: {
      current_stage: number;
      total_stages?: number;
      players: Record<string, PlayerSnapshot>;
      total_damage: number;
    }
  ): Promise<MatchState | null> {
    const key = this.getMatchKey(sessionId);
    const existing = await redis.get(key);

    if (!existing) {
      return null;
    }

    const currentState = JSON.parse(existing);
    const now = Date.now();

    const updatedState = {
      ...currentState,
      current_stage: snapshot.current_stage,
      total_stages: snapshot.total_stages ?? currentState.total_stages,
      players: snapshot.players,
      total_damage: snapshot.total_damage,
      last_update: now,
    };

    await redis.setex(key, MATCH_TTL_SECONDS, JSON.stringify(updatedState));

    return updatedState;
  }

  async getMatchState(sessionId: string): Promise<(MatchState & { room_code: string; players_meta: Array<{ player_id: string; username: string; skin_id: string }> }) | null> {
    const key = this.getMatchKey(sessionId);
    const data = await redis.get(key);

    if (!data) {
      return null;
    }

    return JSON.parse(data);
  }

  async endMatch(
    sessionId: string,
    result: MatchResult
  ): Promise<MatchSummary | null> {
    const state = await this.getMatchState(sessionId);

    if (!state) {
      return null;
    }

    const endedAt = Date.now();
    const duration = Math.floor((endedAt - state.started_at) / 1000); // em segundos

    const playersSummary: MatchPlayerSummary[] = state.players_meta.map((meta) => {
      const playerState = state.players[meta.player_id] || {
        hp: 0,
        damage_dealt: 0,
        mobs_killed: 0,
      };

      return {
        player_id: meta.player_id,
        username: meta.username,
        skin_id: meta.skin_id,
        final_hp: playerState.hp,
        damage_dealt: playerState.damage_dealt,
        mobs_killed: playerState.mobs_killed,
      };
    });

    const summary: MatchSummary = {
      session_id: sessionId,
      room_code: state.room_code,
      started_at: state.started_at,
      ended_at: endedAt,
      duration,
      total_players: state.players_meta.length,
      total_damage: state.total_damage,
      final_stage: state.current_stage,
      result,
      players: playersSummary,
    };

    // Remove o estado do Redis
    await redis.del(this.getMatchKey(sessionId));

    return summary;
  }

  async getTTL(sessionId: string): Promise<number> {
    return redis.ttl(this.getMatchKey(sessionId));
  }
}
