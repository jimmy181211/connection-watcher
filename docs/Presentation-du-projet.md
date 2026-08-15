# Présentation du projet SocketSight

## Sommaire

- [Contexte et objectif](#contexte-et-objectif)
- [Présentation du projet](#présentation-du-projet)
- [Conception principale](#conception-principale)
- [Structure du projet](#structure-du-projet)
- [Démarrage, langue et centre d'aide](#démarrage-langue-et-centre-daide)
- [Compilation et vérification](#compilation-et-vérification)

## Contexte et objectif

Le Moniteur de ressources Windows affiche l'activité réseau actuelle, mais il faut le laisser ouvert et le surveiller. Une connexion brève peut disparaître avant d'être remarquée et il n'est pas pratique de conserver l'historique d'une cible précise.

SocketSight permet de définir des règles pour une IP distante, un port distant ou un port local. Il traite uniquement les connexions TCP correspondantes et enregistre l'heure, l'état, la durée observée, le processus indiqué par Windows et le contexte d'application disponible.

Ce n'est ni un remplacement du Moniteur de ressources ni un antivirus. L'objectif est de faciliter l'observation répétée d'une connexion choisie et sa consultation ultérieure, afin de permettre une analyse plus approfondie.

## Présentation du projet

SocketSight est un outil local d'observation des connexions TCP sous Windows, fondé sur des règles. Une fois la surveillance démarrée, il lit la table TCP de Windows selon l'intervalle choisi et traite les connexions correspondant aux règles activées.

L'intervalle par défaut est d'une seconde. Il peut être réglé de 0,5 à 10 secondes par pas de 0,5 seconde. Un intervalle court détecte mieux les connexions brèves mais effectue plus de vérifications ; un intervalle long utilise moins de ressources mais peut en manquer.

L'application n'enregistre ou ne signale que les connexions sélectionnées par les règles. Elle ne qualifie pas automatiquement les autres activités de réseau de suspectes. Cette version se concentre sur TCP ; UDP demanderait une conception de traçage bas niveau différente et une attribution des applications plus complexe.

## Conception principale

- **Les règles d'abord :** seules les connexions correspondant à des règles activées sont traitées.
- **Une observation par connexion :** une connexion continue n'est pas écrite chaque seconde.
- **Fin selon le temps réel :** elle se termine après deux secondes d'absence ; si elle revient pendant ce délai, c'est la même observation.
- **Le contexte est un indice :** les informations de processus, PID, fichier, processus parent et service Windows aident l'analyse sans prouver la cause finale.
- **Vue et données séparées :** **Effacer l'affichage** masque les anciennes lignes sans supprimer les CSV.
- **Fonctionnement local :** l'application ne lit pas le contenu des paquets et n'envoie ni règles ni journaux. GitHub n'est contacté que pour une vérification manuelle des mises à jour ou l'ouverture de la page de commentaires.

## Structure du projet

```text
connection-watcher/
├── ConnectionWatcher.sln
├── RELEASE_NOTES.md
├── src/
│   ├── ConnectionWatcher.Core/       # règles, surveillance, état, journaux et paramètres
│   └── ConnectionWatcher.App/        # interface WinForms, langues, zone de notification et démarrage
├── tests/
│   ├── ConnectionWatcher.Tests/      # tests fonctionnels et de compatibilité
│   └── ConnectionWatcher.UiSmoke/    # tests de langue, DPI et mise en page
├── docs/                             # présentations et guides utilisateur
├── learning/                         # tutoriel et supports d'apprentissage
├── scripts/build-release.ps1         # compilation, tests, empaquetage et préparation de publication
├── packaging/                        # définition de l'installateur Inno Setup
└── Final-Share/                      # fichiers finaux pour les utilisateurs
```

- `ConnectionWatcher.Core` contient les règles, la lecture TCP de Windows, le suivi des connexions, le contexte des processus, les journaux CSV et les paramètres.
- `ConnectionWatcher.App` contient l'interface, l'éditeur de règles, les détails d'événements, le centre d'aide, les alertes, les langues et l'écran de démarrage.
- `tests` protège le comportement principal et vérifie plusieurs langues et échelles d'affichage.
- `scripts` compile, teste, publie l'application autonome, crée l'installateur, copie les documents actuels et génère les sommes SHA-256.
- `artifacts` est la sortie de publication, `dist` celle de l'installateur et `Final-Share` le paquet final. Tous trois peuvent être recréés.

L'utilisateur télécharge un seul installateur : `SocketSight-Setup-win-x64.exe`. L'application installée est autonome et en plusieurs fichiers ; aucun runtime .NET séparé n'est nécessaire.

## Démarrage, langue et centre d'aide

L'installateur prend en charge sept langues. La langue choisie à l'installation devient aussi celle de l'interface SocketSight. Lors d'une mise à niveau, la nouvelle langue remplace une fois l'ancienne ; les règles, paramètres et journaux sont conservés.

Si le démarrage prend plus d'environ 0,5 seconde, SocketSight affiche un court écran local. Ses messages ne sont que des indications d'état : ils ne signifient ni connexion Internet ni analyse supplémentaire. L'écran se ferme lorsque la fenêtre principale est prête.

Le centre d'aide des paramètres affiche la présentation du projet et le guide d'utilisation dans la langue actuelle. Les mises à jour sont vérifiées manuellement et ne sont jamais téléchargées, installées ou exécutées automatiquement.

## Compilation et vérification

La compilation sous Windows nécessite le SDK .NET 8 et Inno Setup.

```powershell
dotnet build ConnectionWatcher.sln --configuration Release
dotnet run --project tests\ConnectionWatcher.Tests\ConnectionWatcher.Tests.csproj --configuration Release
```

Les mainteneurs peuvent exécuter :

```powershell
scripts\build-release.ps1
```

Le script compile, teste, publie, crée l'installateur, rassemble les documents actuels et génère les sommes SHA-256. Les destinataires peuvent utiliser `Get-FileHash` dans PowerShell.
