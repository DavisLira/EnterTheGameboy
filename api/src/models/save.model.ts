import mongoose, { Schema, Document } from "mongoose";

export interface ISave extends Document {
  hostSteamId: string;      // Dono do Save
  slotIndex: number;        // 1, 2 ou 3
  name: string;             // Ex: "Mundo de Davis"
  createdAt: Date;
  
  // DADOS DO JOGO
  level: number;
  xp: number;
  currentScene: string;
  
  // WHITELIST (Quem pode entrar)
  allowedSteamIds: string[]; 
}

const SaveSchema = new Schema(
  {
    hostSteamId: { type: String, required: true },
    slotIndex: { type: Number, required: true }, // 0, 1 ou 2
    name: { type: String, default: "Novo Mundo" },
    
    level: { type: Number, default: 1 },
    xp: { type: Number, default: 0 },
    currentScene: { type: String, default: "Lobby" },
    
    // O Host sempre começa na lista
    allowedSteamIds: { type: [String], default: [] }
  },
  { timestamps: true, versionKey: false }
);

// Índice único: Um player só pode ter 1 save por slot
SaveSchema.index({ hostSteamId: 1, slotIndex: 1 }, { unique: true });

export const SaveModel = mongoose.model<ISave>("GameSave", SaveSchema);