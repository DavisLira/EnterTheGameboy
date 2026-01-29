import { Router } from "express";
import { PlayerController } from "../controllers/user.controller";

const playerRouter = Router();
const playerController = new PlayerController();

// Unity envia um POST com o steamId
playerRouter.post(
  "/steam-login",
  playerController.loginWithSteam.bind(playerController),
);

playerRouter.post(
  "/update-kills",
  playerController.updateKills.bind(playerController),
);

export default playerRouter;
