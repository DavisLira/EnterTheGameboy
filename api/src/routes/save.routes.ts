import { Router } from "express";
import { SaveController } from "../controllers/save.controller";

const saveRouter = Router();
const saveController = new SaveController();

// Save endpoints
saveRouter.get("/:steamId", saveController.getSaves.bind(saveController));
saveRouter.post("/create", saveController.createSave.bind(saveController));
saveRouter.post("/add-player", saveController.addPlayer.bind(saveController));
saveRouter.post("/delete", saveController.deleteSave.bind(saveController));
saveRouter.put("/update-whitelist", saveController.updateWhitelist.bind(saveController))

export default saveRouter;
