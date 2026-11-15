# Code signing policy / Politique de signature

**Free code signing provided by [SignPath.io](https://about.signpath.io), certificate by [SignPath Foundation](https://signpath.org).**
*(Applies once the project has been accepted by SignPath Foundation — until then, releases are not signed.)*

## What is signed / Ce qui est signé
- `Chutoki.exe` and `Chutoki-Setup.exe` from the official releases of https://github.com/Slimouzz/chutoki
- Binaries are built automatically by GitHub Actions ([`.github/workflows/build.yml`](.github/workflows/build.yml)) from the public source code of this repository. Nothing built on a personal computer is signed.
- Third-party libraries (LibreHardwareMonitor, HidSharp, .NET packages) are included unmodified and are not re-signed as our own work. The PawnIO driver is not included.

## Team roles / Rôles
| Role | Member |
|---|---|
| Committer and reviewer | [Slimouzz](https://github.com/Slimouzz) (Radjouh Slim) |
| Approver (signing requests) | [Slimouzz](https://github.com/Slimouzz) (Radjouh Slim) |

All team members use multi-factor authentication on GitHub and SignPath.

## Privacy policy / Vie privée
This program will not transfer any information to other networked systems unless specifically requested by the user or the person installing or operating it, with one exception: once a day, Chutoki downloads a small public file (`https://slimouzz.github.io/chutoki/news.json`) to check for a new version or a message. No information about the user or the computer is sent. This check can be turned off in the advanced settings (⚙ → "Check for news once a day").

The optional "Send a test report" feature only opens a pre-filled GitHub page or email that the user reviews and sends themselves.

---
*FR — Chutoki est compilé automatiquement par GitHub à partir du code public de ce dépôt ; seule cette version officielle est signée. L'appli ne transmet aucune donnée, sauf la lecture quotidienne (désactivable) d'un petit fichier public pour vérifier les nouvelles versions.*
