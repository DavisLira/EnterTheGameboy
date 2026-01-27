import { Router } from "express";
import { RoomController } from "../controllers/room.controller";

const activeRoomRouter = Router();
const roomController = new RoomController();

activeRoomRouter.post("/", roomController.createRoom.bind(roomController));
activeRoomRouter.get("/", roomController.getAllRooms.bind(roomController));
activeRoomRouter.patch("/:roomCode", roomController.addPlayerToRoom.bind(roomController));
activeRoomRouter.get("/:roomCode", roomController.getRoomByCode.bind(roomController));
activeRoomRouter.post("/:roomCode/start", roomController.startRoom.bind(roomController));

export default activeRoomRouter;
