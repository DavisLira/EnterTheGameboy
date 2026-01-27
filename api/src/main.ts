import "dotenv/config";
import express from "express";
import cors from "cors"; // <--- Recomendado instalar: npm install cors @types/cors -D
import { connectMongo } from "./database/mongo";
import playerRouter from "./routes/player.routes";
import saveRouter from "./routes/save.routes";

const app = express();

// --- ÁREA DE CONFIGURAÇÃO (MIDDLEWARES) ---

// 1. Permite ler JSON (CORREÇÃO DO SEU ERRO)
app.use(express.json()); 

// 2. Permite requisições externas (Evita dor de cabeça no futuro)
app.use(cors()); 

// -------------------------------------------

// ROTAS (Devem vir DEPOIS do app.use(express.json()))
app.use("/players", playerRouter);
app.use("/saves", saveRouter);


const startServer = async () => {
  try {
    await connectMongo();
    console.log("MongoDB connected successfully"); // Seu log confirmou isso

    app.listen(process.env.PORT || 3000, () => {
      console.log(`Server started on port ${process.env.PORT || 3000}`);
    });
  } catch (error) {
    console.error("Failed to start server:", error);
  }
};

startServer();