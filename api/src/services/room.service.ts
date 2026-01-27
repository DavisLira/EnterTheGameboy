import { randomBytes } from "crypto";
import { ActiveRoom, RoomStatus } from "../interfaces/active-room/active-room.interface";

// Armazenamento temporário em memória (será substituído por banco de dados)
const rooms: Map<string, ActiveRoom & { sessionId?: string; startedAt?: Date }> = new Map();

export class RoomService {
  generateRoomCode(): string {
    return randomBytes(2).toString("hex").toUpperCase();
  }

  generateSessionId(): string {
    return `sess_${randomBytes(8).toString("hex")}`;
  }

  createRoom(hostId: string, roomCode?: string): ActiveRoom & { sessionId?: string } {
    const code = roomCode || this.generateRoomCode();
    const room = new ActiveRoom(hostId, code);
    rooms.set(code, room);
    return room;
  }

  getAllRooms(): (ActiveRoom & { sessionId?: string })[] {
    return Array.from(rooms.values());
  }

  getRoomByCode(roomCode: string): (ActiveRoom & { sessionId?: string }) | undefined {
    return rooms.get(roomCode);
  }

  addPlayerToRoom(roomCode: string, playerId: string): (ActiveRoom & { sessionId?: string }) | undefined {
    const room = rooms.get(roomCode);
    if (!room) return undefined;

    if (!room.playersIds) {
      room.playersIds = [];
    }

    if (!room.playersIds.includes(playerId)) {
      room.playersIds.push(playerId);
    }

    return room;
  }

  startRoom(roomCode: string): { room: ActiveRoom & { sessionId?: string; startedAt?: Date }; sessionId: string } | undefined {
    const room = rooms.get(roomCode);
    if (!room) return undefined;

    if (room.status === RoomStatus.IN_PROGRESS) {
      return { room, sessionId: room.sessionId! };
    }

    const sessionId = this.generateSessionId();
    room.status = RoomStatus.IN_PROGRESS;
    room.sessionId = sessionId;
    room.startedAt = new Date();

    return { room, sessionId };
  }

  endRoom(roomCode: string): (ActiveRoom & { sessionId?: string }) | undefined {
    const room = rooms.get(roomCode);
    if (!room) return undefined;

    room.status = RoomStatus.ENDED;
    return room;
  }

  deleteRoom(roomCode: string): boolean {
    return rooms.delete(roomCode);
  }
}
