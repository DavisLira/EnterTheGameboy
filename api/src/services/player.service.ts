import { PlayerModel } from "../models/player.model";
import { redis } from "../redis/redis.client"; // Importa sua conexão existente
import { Player } from "../interfaces/player/player.interface";

export class PlayerService {
  // Função principal que a Unity vai chamar
  async findOrCreateBySteamId(
    steamId: string,
    username?: string,
  ): Promise<Player> {
    const cacheKey = `player:${steamId}`;

    // 1. Tenta pegar do Redis (Cache)
    const cachedPlayer = await redis.get(cacheKey);
    if (cachedPlayer) {
      console.log("Cache Hit! Retornando do Redis.");
      return JSON.parse(cachedPlayer);
    }

    // 2. Se não achou no cache, busca no MongoDB
    console.log("Cache Miss. Buscando no Mongo...");
    let player = await PlayerModel.findOne({ steamId });

    // 3. Se não existe no Mongo, cria um novo
    if (!player) {
      console.log("Player não existe. Criando novo...");
      player = await PlayerModel.create({
        steamId,
        username: username || `Player_${steamId.slice(-4)}`, // Nome padrão se não vier
      });
    }

    // 4. Salva no Redis para a próxima vez (Expira em 1 hora = 3600s)
    // Usamos o 'player.toJSON()' ou convertemos para objeto simples
    await redis.set(cacheKey, JSON.stringify(player), "EX", 3600);

    return player;
  }

  // Apenas busca (útil para outras coisas)
  async getPlayerById(id: string): Promise<Player | null> {
    return PlayerModel.findById(id);
  }

  async updateKills(steamId: string, kills: number) {
    // Opção A: Substituir o valor (Se o Unity manda o TOTAL de kills da vida toda)
    // Use essa opção se você carregar as kills no login e somar no Unity.
    return PlayerModel.findOneAndUpdate(
      { steamId },
      { $set: { kills: kills } },
      { new: true }, // Retorna o objeto atualizado
    );
  }
}
