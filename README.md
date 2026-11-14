<p align="center"><img src="docs/img/logo.png" width="140" alt="Chutoki"></p>

<h1 align="center">Chutoki</h1>
<p align="center"><b>Le ventilo qui fait chut. · The fan that goes shhh.</b><br>
Gratuit · Sans pub · Windows 10 / 11 — Free · No ads</p>

<p align="center"><a href="../../releases/latest"><b>⬇ Télécharger / Download</b></a></p>

---

## 🇫🇷 Français

Chutoki règle tout seul les ventilateurs de ta carte mère : **silencieux quand le PC se repose, plus fort quand il travaille**, avec des garde-fous si ça chauffe.

- **Calme** (par défaut) — chut, bébé dort · **Auto** — je m'occupe de tout · **Normal** · **Fort**
- Garde-fous : chauffe rapide, zone d'alerte, Calme renforcé, urgence à 100 %, capteur perdu, ventilateur bloqué, retour au BIOS à la fermeture
- Processeurs Intel et AMD, cartes graphiques NVIDIA, AMD et Intel
- 6 langues : français, anglais, italien, espagnol, chinois, japonais

### Pourquoi Chutoki ?
> J'ai créé Chutoki parce qu'aucune application ne rendait vraiment mon PC silencieux : le bruit des ventilateurs réveillait mon fils. Alors je l'ai fait moi-même, avec l'aide de l'intelligence artificielle.
> Si moi j'ai pu le faire, toi aussi : lance-toi et crée tes propres outils ! — *Slim*

### Installation
1. Installe le pilote gratuit [PawnIO](https://github.com/namazso/PawnIO.Setup/releases/latest) (une seule fois).
2. Télécharge Chutoki depuis les [Releases](../../releases/latest) et décompresse le dossier.
3. Lance `Chutoki.exe` et accepte la demande de Windows.

Détails : [LISEZMOI.txt](LISEZMOI.txt)

## 🇬🇧 English

Chutoki automatically adjusts your motherboard fans: **quiet when your PC rests, stronger when it works**, with safeguards if things heat up.

> I made Chutoki because no app could make my PC truly quiet: the fan noise kept waking up my son. So I built it myself, with the help of artificial intelligence. If I could do it, so can you! — *Slim*

Install: [PawnIO](https://github.com/namazso/PawnIO.Setup/releases/latest) driver once, then download from [Releases](../../releases/latest) and run `Chutoki.exe`. Details: [README.txt](README.txt)

## Compiler / Build

Aucun SDK nécessaire / No SDK needed (uses the C# compiler built into Windows):

```powershell
.\build.ps1
```

Le programme est créé dans `dist\` / Output goes to `dist\`.

## Licence / License

MIT © 2026 Radjouh Slim — voir / see [LICENSE.txt](LICENSE.txt).
Composants tiers / Third-party components: [THIRD-PARTY-NOTICES.txt](THIRD-PARTY-NOTICES.txt) (LibreHardwareMonitor MPL-2.0, HidSharp Apache-2.0, .NET MIT). PawnIO n'est pas redistribué / is not redistributed.

Fourni « tel quel », sans garantie / Provided "as is", without warranty.
