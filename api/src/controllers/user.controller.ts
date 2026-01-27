import { Request, Response } from "express";
import { PlayerService } from "../services/player.service";

const playerService = new PlayerService();

export class PlayerController {
  
  // Endpoint que a Unity vai chamar
  async loginWithSteam(req: Request, res: Response): Promise<void> {
    try {
      const { steamId, username } = req.body;

      if (!steamId) {
        res.status(400).json({ message: "Steam ID is required" });
        return;
      }

      // Chama nossa lógica blindada de Cache+Banco
      const player = await playerService.findOrCreateBySteamId(steamId, username);

      res.status(200).json({ player });
    } catch (error) {
      console.error(error);
      res.status(500).json({ message: "Internal Server Error" });
    }
  }
  
  // ... mantenha os outros métodos se quiser, mas atualize para usar async/await
}