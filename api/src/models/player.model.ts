import mongoose, { Schema, Document } from "mongoose";
import { Player } from "../interfaces/player/player.interface";

// Une a interface Player com o Documento do Mongoose
export interface PlayerDocument extends Player, Document {
  id: string; // Sobrescreve para garantir string
  kills: number;
}

const PlayerSchema = new Schema(
  {
    steamId: { type: String, required: true, unique: true },
    username: { type: String, required: false },
    // Removemos password obrigatório pois é login via Steam
    kills: { type: Number, default: 0 },
  },
  {
    timestamps: true, // Cria createdAt e updatedAt automaticamente
    versionKey: false,
  },
);

export const PlayerModel = mongoose.model<PlayerDocument>(
  "Player",
  PlayerSchema,
);
