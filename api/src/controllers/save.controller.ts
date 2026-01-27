import { Request, Response } from "express";
import { SaveService } from "../services/save.services";

export class SaveController {
  private saveService = new SaveService();

  // GET /saves/:steamId
  async getSaves(req: Request, res: Response) {
    try {
      const { steamId } = req.params;
      const saves = await this.saveService.getHostSaves(steamId);
      return res.status(200).json({ saves });
    } catch (error) {
      return res.status(500).json({ error: String(error) });
    }
  }

  // POST /saves/create
  async createSave(req: Request, res: Response) {
    try {
      const { steamId, slotIndex, name } = req.body;
      const save = await this.saveService.createOrResetSave(steamId, slotIndex, name);
      return res.status(200).json(save);
    } catch (error) {
      return res.status(500).json({ error: String(error) });
    }
  }
  
  // POST /saves/add-player
  async addPlayer(req: Request, res: Response) {
      try {
          const { saveId, newSteamId } = req.body;
          const updated = await this.saveService.addPlayerToWhitelist(saveId, newSteamId);
          return res.status(200).json(updated);
      } catch (error) {
          return res.status(500).json({ error: String(error) });
      }
  }

  // DELETE /saves/delete
  async deleteSave(req: Request, res: Response) {
    try {
      const { saveId, steamId } = req.body;
      await this.saveService.deleteSave(saveId, steamId);
      return res.status(200).json({ message: "Deleted" });
    } catch (error) {
      return res.status(500).json({ error: String(error) });
    }
  }
}