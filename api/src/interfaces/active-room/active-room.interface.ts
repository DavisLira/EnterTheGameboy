export enum RoomStatus {
  PENDING,
  IN_PROGRESS,
  ENDED
}

// interface de retorno do banco de dados
export interface IActiveRoom extends CreateActiveRoomBody {
  id: string;
  createdAt: Date;
  updatedAt: Date;
  status: RoomStatus;
  roomCode: string;
  playersIds: string[];
}

// interface do corpo da requisição de criação de uma sala
export interface CreateActiveRoomBody {
  hostId: string;
  roomCode: string;
}

// classe para criação/atualização de uma sala no banco de dados
export class ActiveRoom {
  hostId: string;
  roomCode: string;
  status?: RoomStatus;
  playersIds?: string[];

  constructor(hostId: string, roomCode: string) {
    this.hostId = hostId;
    this.roomCode = roomCode;
    this.status = RoomStatus.PENDING;
    this.playersIds = [hostId]; // começa com o host
  }
}
