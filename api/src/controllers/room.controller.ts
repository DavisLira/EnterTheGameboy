import { Request, Response } from "express";
import { RoomService } from "../services/room.service";
import { MatchService } from "../services/match.service";
import { StartMatchPayload } from "../interfaces/match/match-state.interface";

interface CreateActiveRoomBody extends Request {
  body: {
    hostId: string;
    roomCode?: string;
  };
}

interface AddPlayerBody extends Request {
  body: {
    playerId: string;
  };
}

interface StartRoomBody extends Request {
  body: StartMatchPayload;
}

const roomService = new RoomService();
const matchService = new MatchService();

export class RoomController {
  getAllRooms(req: Request, res: Response): void {
    const rooms = roomService.getAllRooms();
    res.json({ rooms });
  }

  createRoom(req: CreateActiveRoomBody, res: Response): void {
    const { hostId, roomCode } = req.body;

    if (!hostId) {
      res.status(400).json({ message: "Missing hostId" });
      return;
    }

    const room = roomService.createRoom(hostId, roomCode);
    res.status(201).json({ message: "Room created", room });
  }

  getRoomByCode(req: Request, res: Response): void {
    const roomCode = req.params.roomCode as string;

    const room = roomService.getRoomByCode(roomCode);

    if (!room) {
      res.status(404).json({ message: "Room not found" });
      return;
    }

    res.json({ room });
  }

  addPlayerToRoom(req: AddPlayerBody, res: Response): void {
    const roomCode = req.params.roomCode as string;
    const { playerId } = req.body;

    if (!playerId) {
      res.status(400).json({ message: "Missing playerId" });
      return;
    }

    const room = roomService.addPlayerToRoom(roomCode, playerId);

    if (!room) {
      res.status(404).json({ message: "Room not found" });
      return;
    }

    res.json({ message: "Player added to room", room });
  }

  async startRoom(req: StartRoomBody, res: Response): Promise<void> {
    const roomCode = req.params.roomCode as string;
    const { total_stages, players } = req.body;

    if (!total_stages || !players || players.length === 0) {
      res.status(400).json({ message: "Missing total_stages or players" });
      return;
    }

    const result = roomService.startRoom(roomCode);

    if (!result) {
      res.status(404).json({ message: "Room not found" });
      return;
    }

    // Inicializa o estado da partida no Redis
    await matchService.initializeMatchState(
      result.sessionId,
      roomCode,
      total_stages,
      players
    );

    res.json({
      message: "Room started",
      session_id: result.sessionId,
      room: result.room,
    });
  }
}
