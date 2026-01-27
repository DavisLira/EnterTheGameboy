import { Router } from "express";
import { MatchController } from "../controllers/match.controller";

const matchRouter = Router();
const matchController = new MatchController();

// Runtime endpoints
matchRouter.post("/runtime/snapshot", matchController.saveSnapshot.bind(matchController));
matchRouter.get("/runtime/:sessionId", matchController.getMatchState.bind(matchController));
matchRouter.post("/end", matchController.endMatch.bind(matchController));

export default matchRouter;
