# 📘 **GDD — Enter the Gameboy**

## **1. Visão Geral**

### **1.1 Descrição do Jogo**

Enter the Gameboy é um **roguelite top-down cooperativo** para **1–4 jogadores**, com três mapas pré-definidos: **Esgoto**, **Mina de Mineração** e **Biblioteca**.
Cada mapa contém múltiplas salas com inimigos, armas diferentes, moedas para compra de upgrades e um chefe final.
O jogo utiliza **pixel art 16×16 em estilo 8-bit Game Boy** com tons de verde.

### **1.2 Objetivo**

Explorar salas, derrotar inimigos, coletar loot e enfrentar três chefes (um por mapa), além de um chefe final no último mapa. Completar os três mapas garante a vitória.

---

## **2. Gameplay**

### **2.1 Informações Gerais**

* **Nome:** Enter the Gameboy
* **Gênero:** Roguelite Top-down Shooter
* **Plataforma:** PC (Windows)
* **Multiplayer:** Coop online para 2–4 jogadores
* **Duração média da run:** 5–15 minutos
* **Resumo da experiência:** Jogo rápido, tenso, com foco em cooperação, movimentação estratégica, sobrevivência e progressão entre salas

---

### **2.2 Mecânicas de Jogo**

#### **2.2.1 Controles**

* Movimentação: WASD
* Atirar: Mouse
* Interagir: E
* Trocar arma: R
* Rolamento: Espaço
* (outros a definir)

#### **2.2.2 Dash / Rolamento**

* Cada jogador possui um **dash/rolamento** com cooldown.
* Permite atravessar pequenos perigos e esquivar tiros inimigos.

#### **2.2.3 Sistema de Mana**

* Armas consomem mana.
* Mana se regenera automaticamente com o tempo.
* (custos específicos a definir)

#### **2.2.4 Combate e Progressão**

* Inimigos surgem em cada sala (exceto sala de loja e inicial).
* Armas possuem atributos distintos.
* Jogadores coletam **moedas** para comprar upgrades em salas específicas.

#### **2.2.5 Cooperação**

* Ao entrar em uma nova sala, **todos os jogadores são teletransportados simultaneamente**.
* O **loot é compartilhado** entre todos.
* Jogador caído pode ser revivido se um aliado ficar em contato por **5 segundos**.

---

## **3. Estrutura do Jogo**

### **3.1 Mapas**

A ordem pode ser aleatória, mas cada mapa possui suas salas e um chefe:

#### **Mapa 1 — Esgoto**

* 7 salas + chefe
* Temática verde suja, goteiras, criaturas rastejantes
* Slimes, ratos, inimigos básicos

#### **Mapa 2 — Mina**

* 7 salas + chefe
* Temática cavernosa, carrinhos, explosivos
* Morcegos, mineradores corrompidos, inimigos intermediários

#### **Mapa 3 — Biblioteca**

* 7 salas + chefe + chefe final
* Temática arcana, estantes, magias
* Magos, livros encantados, inimigos avançados

---

### **3.2 Tipos de Sala**

* **Sala Inicial** — ponto de partida
* **Sala de Loja** — armas, upgrades, poções
* **Sala de Inimigos** — combate obrigatório
* **Sala do Chefe** — encerramento do mapa
* **Sala do Chefe Final** — apenas no último mapa

---

## **4. Personagens**

### **4.1 Jogador**

Atributos (iniciais, valores a definir):

* Vida
* Armadura (regenera com o tempo)
* Mana (regenera)
* Velocidade
* Arma inicial
* Dash/rolamento

### **4.2 Inimigos**

Tipos básicos (atributos a definir):

* Chaser (persegue)
* Shooter (atira)
* Tank (lento e resistente)
* Exploder (explode ao morrer)

### **4.3 Chefes**

* 1 chefe por mapa
* Chefe final após o terceiro mapa
* (designs e habilidades dos chefes a definir)

---

## **5. Armas e Itens**

* Armas básicas, intermediárias e raras
* Upgrades temporários ou permanentes durante a run
* Itens consumíveis
* (lista de armas e atributos a definir)

---

## **6. Interface**

* HUD minimalista estilo Game Boy
* Barras de vida, armadura, mana
* Indicadores de cooldown
* Nome dos jogadores
* Tela de loja
* Tela de estatísticas ao final

---

## **7. Arte e Estilo**

* Pixel art **16×16**
* Estética **8-bit estilo Game Boy**
* Paleta em tons de verde
* Baixa saturação, contraste alto
* Sprites minimalistas e animações curtas

---

## **8. Áudio**

* Efeitos retro 8-bit
* Sons de tiro, impacto, dash, itens
* Música ambiente por mapa
* Música especial para chefes

---

## **9. Fluxo de Jogo**

```
Menu Inicial
    ↓
Lobby
    ↓
Mapa 1 
    ↓
Mapa 2
    ↓
Mapa 3 (com 4° chefe)
    ↓
Vitória
    ↓
Tela de Estatísticas
```

---

## **10. Pós-jogo e Futuro**

*Tópico reservado para adicionar futuras expansões, modos extras, armas novas, inimigos adicionais, ajustes de dificuldade, etc.*
