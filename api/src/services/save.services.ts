import { SaveModel } from "../models/save.model";

export class SaveService {

  // Retorna os 3 slots (alguns podem ser null se não existirem, trataremos no Unity ou aqui)
  async getHostSaves(hostSteamId: string) {
    const saves = await SaveModel.find({ hostSteamId });
    return saves;
  }

  // Cria ou Sobrescreve um Save (Reset)
  async createOrResetSave(hostSteamId: string, slotIndex: number, name: string) {
    // Apaga se já existir nesse slot
    await SaveModel.deleteOne({ hostSteamId, slotIndex });

    // Cria novo
    const newSave = await SaveModel.create({
      hostSteamId,
      slotIndex,
      name,
      allowedSteamIds: [hostSteamId] // O dono já nasce permitido
    });
    
    return newSave;
  }

  // Adiciona um amigo na whitelist do save
  async addPlayerToWhitelist(saveId: string, newSteamId: string) {
    return await SaveModel.findByIdAndUpdate(
      saveId,
      { $addToSet: { allowedSteamIds: newSteamId } }, // $addToSet evita duplicados
      { new: true }
    );
  }
  
  // Atualiza progresso (Level, XP...)
  async updateProgress(saveId: string, data: any) {
      return await SaveModel.findByIdAndUpdate(saveId, data, { new: true });
  }

  async deleteSave(saveId: string, hostSteamId: string) {
    // A segurança é importante: Só deleta se o ID bater E o dono for quem está pedindo
    return await SaveModel.deleteOne({ _id: saveId, hostSteamId });
  }
}