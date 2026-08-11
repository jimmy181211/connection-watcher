# Moniteur de connexions TCP

## Contexte et objectif

Lors de l'analyse d'une connexion réseau inhabituelle, il faut souvent répondre à une question simple, mais difficile à confirmer à temps :

> Mon ordinateur s'est-il connecté à une adresse IP ou à un port précis ? Si oui, quand et par quel programme ?

Le Moniteur de ressources Windows affiche l'activité réseau actuelle, mais l'utilisateur doit l'ouvrir et continuer à le surveiller. Une connexion brève peut disparaître rapidement et il n'est pas pratique d'observer la fenêtre pendant longtemps. Il ne prévient pas non plus automatiquement lorsqu'une cible choisie apparaît et ne conserve pas d'historique continu.

Le Moniteur de connexions TCP répond à ce besoin. Une fois l'adresse IP ou le port sélectionné, l'application recherche en arrière-plan les connexions correspondantes. Elle enregistre l'heure, les adresses, les ports et, si disponibles, le programme et le PID, puis avertit l'utilisateur selon ses paramètres.

Cet outil ne remplace ni le Moniteur de ressources ni un antivirus. Il facilite la surveillance de cibles choisies, la conservation des informations et leur utilisation lors d'une enquête de sécurité ultérieure.

## Présentation du projet

Le Moniteur de connexions TCP est un petit **outil Windows de surveillance des connexions réseau basé sur des règles**. L'utilisateur choisit l'adresse IP distante, le port distant ou le port local qui l'intéresse. Lorsqu'une connexion TCP signalée par Windows correspond à une règle activée, l'application l'enregistre ou avertit l'utilisateur.

En termes simples, l'outil surveille une adresse IP ou un port précis. Par exemple, vous pouvez lui demander de surveiller `103.1.40.235:1433`. Lorsqu'une connexion vers cette cible apparaît, l'application enregistre l'heure, l'état actif ou terminé, la durée observée, le programme et le PID. Elle peut **enregistrer silencieusement, afficher une notification dans la zone de notification ou ouvrir une alerte contextuelle**.

L'application indique uniquement : « Une connexion que vous avez choisi de surveiller est apparue. » Elle ne classe pas les autres connexions comme suspectes et une connexion seule ne prouve pas que l'ordinateur est infecté. Les informations enregistrées peuvent être transmises à une équipe de cybersécurité.

## Structure du projet

```text
connection-watcher/
├── ConnectionWatcher.sln
├── src/
│   ├── ConnectionWatcher.Core/
│   └── ConnectionWatcher.App/
├── tests/
│   ├── ConnectionWatcher.Tests/
│   └── ConnectionWatcher.UiSmoke/
├── docs/
├── packaging/
└── Final-Share/
    ├── TCP-Connection-Watcher-Setup-win-x64.exe
    ├── SHA256SUMS.txt
    └── Docs/
```

- `ConnectionWatcher.sln` : fichier de solution du projet.
- `src/ConnectionWatcher.Core` : logique des paramètres, règles, lecture des connexions TCP Windows, déduplication et journaux CSV.
- `src/ConnectionWatcher.App` : interface Windows en sept langues, avec fenêtre principale, éditeur de règles, centre d'aide, notifications et alertes.
- `tests` : tests fonctionnels et d'interface ; la suite fonctionnelle comprend actuellement 16 tests.
- `docs` : présentations et guides dans les sept langues.
- `packaging` : définition du programme d'installation et notes de la version portable.
- `Final-Share` : dossier final avec un programme d'installation multilingue, les documents et les sommes SHA-256.

## Compilation et vérification

La compilation sous Windows nécessite le SDK .NET 8.

```powershell
dotnet build ConnectionWatcher.sln --configuration Release
dotnet run --project tests\ConnectionWatcher.Tests\ConnectionWatcher.Tests.csproj --configuration Release
```

Les paquets publiés contiennent `SHA256SUMS.txt`, que les destinataires peuvent vérifier avec `Get-FileHash` dans PowerShell.
