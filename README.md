# BRANDED TO KILL

> Tóquio, 2h da manhã. Pegue o ouro da yakuza antes do tempo acabar.

Jogo 2D visto de cima feito em **Unity 6** para o *Projeto Introdução ao Unity* (Insper, Jogos Digitais 2026), a partir do tutorial "Introdução Rápida".

- **Autor:** Vinicius Montes Dib
- **Jogar no navegador (itch.io):** _link do itch.io aqui_

## Como jogar

Pegue as **12 moedas de ouro** espalhadas pelos becos de Kabukichō antes que o cronômetro zere, desviando ou atirando nos capangas da yakuza.

| Ação | Teclado + mouse | Controle Xbox |
|---|---|---|
| Andar | WASD ou setas | Analógico esquerdo |
| Atirar (na direção em que está olhando) | Espaço ou clique esquerdo | A ou RT |
| Navegar nos menus | Mouse | Analógico / D-pad + A |

- Você tem **3 vidas**. Encostar num capanga tira uma vida, e depois você fica alguns instantes piscando e invencível.
- Os **triângulos roxos** dão **+10 segundos**. Cada capanga derrotado dá **+50 pontos** e **+3 segundos**.
- O tempo que sobrar no final vira pontos extras.
- Capangas **cinza** patrulham um caminho fixo. Os **laranja** ficam parados e perseguem você quando chega perto.

### 🤫 Segredo
Existe um **bueiro escondido** num dos becos. Quem pisar nele cai na **fase secreta: o Cofre da Yakuza**, uma sala dourada com 8 moedas que valem **50 pontos cada** (as normais valem 10), vigiadas por guardas. Pontos, vidas e tempo continuam de onde estavam. Pegar todas as moedas do cofre vence o jogo com um final especial.

## O que foi feito (rubrica)

| Item | Como |
|---|---|
| Tutorial | Jogador com Rigidbody2D e movimento normalizado, coletáveis com trigger e a tag `Coletavel`, prefabs, som ao coletar, menu inicial e `GameManager` estático com tela de fim |
| Tempo | Cronômetro regressivo na tela, tempo final na tela de fim, e relógios e abates que dão segundos |
| Inimigos | `EnemyPatrol` (caminho fixo) e `EnemyChaser` (persegue quando o jogador chega perto); encostar tira vida |
| Visual | Formas do tutorial com cores neon (tema de Tóquio à noite) e 3 sprites próprios: jogador, capanga e moeda |
| Áudio | Música no menu e no jogo, e sons de moeda, relógio, tiro, dano, inimigo derrotado, vitória e derrota |
| UI | Pontos, moedas, tempo e vidas na tela |
| Controle | Controle Xbox pelo Input System, inclusive nos menus |
| Level design | Arena nova com becos entre os prédios de Kabukichō, tiro como desafio novo e uma fase secreta (o cofre) |
| Sem dead-end | O jogo sempre termina (todas as moedas, tempo zerado ou sem vidas), e a tela de fim sempre oferece jogar de novo ou voltar ao menu |

## Scripts (`Assets/Scripts`)

| Script | O que faz |
|---|---|
| `PlayerMovement` | Anda, gira para a direção do movimento, coleta moedas e relógios, e leva dano dos inimigos |
| `PlayerShoot` | Cria a bala (prefab) na direção em que o jogador olha |
| `SecretEntrance` | O bueiro secreto, que carrega a cena `FaseSecreta` |
| `Bullet` | Derruba o inimigo ou some ao bater na parede |
| `EnemyPatrol` / `EnemyChaser` | Os dois tipos de inimigo |
| `GameManager` | Dados estáticos (pontos, vidas, tempo), cronômetro e fim de jogo |
| `HUD` | Atualiza os textos da interface |
| `MainMenu` / `EndGame` | Menu inicial e tela de fim |

O script `Assets/Editor/GameBuilder.cs` monta as cenas e os prefabs automaticamente pelo menu **Branded to Kill → Montar jogo**. A pasta `Assets/Tests` tem 10 testes automáticos (Window → General → Test Runner → PlayMode).

## Créditos

- **Programação e design:** Vinicius Montes Dib
- **Sprites:** jogador, capanga e moeda desenhados por código (`Tools/gen_sprites.py`); formas básicas como no tutorial.
- **Música e sons:** originais, sintetizados por código em Python (NumPy/SciPy).
- **Fonte:** [Rajdhani](https://fonts.google.com/specimen/Rajdhani) (Indian Type Foundry), licença [SIL Open Font License](Assets/Fonts/OFL.txt).
- **Base:** tutorial "Introdução Rápida", Insper.
- **IA:** desenvolvido com auxílio do assistente Claude (Anthropic), como permitido pelo professor.
- **Título:** homenagem ao filme *Branded to Kill* (Seijun Suzuki, 1967).
