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

## 🇮🇹 Italiano

Chutoki regola da solo le ventole della scheda madre: **silenzioso quando il PC riposa, più forte quando lavora**, con protezioni se scalda. Gratis, senza pubblicità, open source.
Installa una volta il driver gratuito [PawnIO](https://github.com/namazso/PawnIO.Setup/releases/latest), poi scarica Chutoki dalle [Releases](../../releases/latest) e avvia `Chutoki.exe`.

## 🇪🇸 Español

Chutoki ajusta solo los ventiladores de tu placa base: **silencioso cuando el PC descansa, más fuerte cuando trabaja**, con protecciones si se calienta. Gratis, sin anuncios, de código abierto.
Instala una vez el controlador gratuito [PawnIO](https://github.com/namazso/PawnIO.Setup/releases/latest), luego descarga Chutoki desde [Releases](../../releases/latest) y abre `Chutoki.exe`.

## 🇨🇳 中文

Chutoki 自动调节主板风扇：**电脑空闲时安静，工作时加强散热**，温度升高时有多重保护。免费、无广告、开源。
先安装一次免费驱动 [PawnIO](https://github.com/namazso/PawnIO.Setup/releases/latest)，然后从 [Releases](../../releases/latest) 下载 Chutoki 并运行 `Chutoki.exe`。

## 🇯🇵 日本語

Chutoki はマザーボードのファンを自動で調整します：**PCが休んでいるときは静かに、作業中はしっかり冷却**。熱くなったときの保護機能付き。無料・広告なし・オープンソース。
無料ドライバー [PawnIO](https://github.com/namazso/PawnIO.Setup/releases/latest) を一度インストールし、[Releases](../../releases/latest) から Chutoki をダウンロードして `Chutoki.exe` を起動してください。

## 🧪 Testeurs recherchés / Testers wanted

Chutoki a besoin de tests sur un maximum de cartes mères ! Dans l'appli : ⚙ → **📨 Envoyer un rapport de test** (matériel uniquement, rien de personnel).
Chutoki needs testing on as many motherboards as possible! In the app: ⚙ → **📨 Send a test report** (hardware only, nothing personal).
→ [Compatibilité / Compatibility](https://slimouzz.github.io/chutoki/compatibilite.html) · [Envoyer un rapport / Send a report](../../issues/new?template=test-report.yml)

Contact : contact-chutoki@proton.me

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

Signature du code / Code signing: free code signing provided by [SignPath.io](https://about.signpath.io), certificate by [SignPath Foundation](https://signpath.org) (application in progress / candidature en cours) — voir / see [CODE_SIGNING.md](CODE_SIGNING.md).
