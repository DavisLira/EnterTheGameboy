// src/interfaces/player/player.interface.ts

export interface Player {
  id: string; // ID interno do Mongo
  steamId: string; // ID da Steam (Novo)
  username?: string; // Opcional
  kills: number; // Número de kills do jogador
  createdAt: Date;
  updatedAt: Date;
}

export interface CreatablePlayer {
  steamId: string;
  username?: string;
}
