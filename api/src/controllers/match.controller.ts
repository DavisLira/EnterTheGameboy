import { Request, Response } from "express";
import { MatchService } from "../services/match.service";
import {
  SnapshotPayload,
  EndMatchPayload,
} from "../interfaces/match/match-state.interface";

const matchService = new MatchService();

export class MatchController {
  async saveSnapshot(req: Request, res: Response): Promise<void> {
    const { session_id, snapshot } = req.body as SnapshotPayload;

    if (!session_id || !snapshot) {
      res.status(400).json({ message: "Missing session_id or snapshot" });
      return;
    }

    const state = await matchService.saveSnapshot(session_id, snapshot);

    if (!state) {
      res.status(404).json({ message: "Match not found" });
      return;
    }

    res.json({ message: "Snapshot saved", state });
  }

  async getMatchState(req: Request, res: Response): Promise<void> {
    const sessionId = req.params.sessionId as string;

    const state = await matchService.getMatchState(sessionId);

    if (!state) {
      res.status(404).json({ message: "Match not found" });
      return;
    }

    const ttl = await matchService.getTTL(sessionId);

    res.json({ state, ttl_seconds: ttl });
  }

  async endMatch(req: Request, res: Response): Promise<void> {
    const { session_id, result } = req.body as EndMatchPayload;

    if (!session_id || !result) {
      res.status(400).json({ message: "Missing session_id or result" });
      return;
    }

    if (result !== "victory" && result !== "defeat") {
      res.status(400).json({ message: "Invalid result. Must be 'victory' or 'defeat'" });
      return;
    }

    const summary = await matchService.endMatch(session_id, result);

    if (!summary) {
      res.status(404).json({ message: "Match not found" });
      return;
    }

    res.json({ message: "Match ended", summary });
  }
}
