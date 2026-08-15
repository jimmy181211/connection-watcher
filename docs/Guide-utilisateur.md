# Guide d'utilisation du Moniteur de connexions TCP

## Objectif principal

Cet outil vous aide à **surveiller l'adresse IP ou le port que vous choisissez**. Il peut :

- Enregistrer automatiquement l'apparition d'une connexion
- Enregistrer les adresses IP et les ports locaux et distants
- Enregistrer, lorsqu'ils sont disponibles, le propriétaire de la connexion indiqué par Windows, le PID, le chemin de l'exécutable, les informations du fichier, les processus parents ou hôtes et les services Windows associés
- Enregistrer silencieusement, afficher une notification ou ouvrir une alerte selon vos paramètres
- Conserver les informations pour les consulter ou les transmettre à un service de cybersécurité
- Confirmer si une nouvelle connexion vers la même cible apparaît plus tard

## Fonctionnement

Créez d'abord une règle indiquant l'adresse IP ou le port à surveiller. Activez ensuite la règle et démarrez la surveillance. Par défaut, l'application consulte la liste des connexions TCP Windows une fois par seconde. Sur la page **Accueil**, vous pouvez régler l'intervalle de 0,5 à 10 secondes, par pas de 0,5 seconde. Un intervalle court détecte plus facilement les connexions brèves ; un intervalle long utilise moins de ressources, mais peut les manquer. Seules les connexions correspondant à une règle activée sont traitées ; les autres ne produisent ni enregistrement ni alerte.

Lorsqu'une connexion correspond à une règle, l'application exécute l'action choisie :

- **Enregistrer silencieusement :** écrit l'événement dans le journal CSV sans modifier l'icône de la zone de notification ni afficher de compteur.
- **Notification et journal :** n'ouvre aucune fenêtre. L'icône passe en état d'avertissement, effacé à l'ouverture du journal des événements.
- **Alerte contextuelle et journal :** ouvre une fenêtre dès la première correspondance. Tant qu'elle reste ouverte, les correspondances suivantes mettent à jour la même fenêtre. Après sa fermeture, l'intervalle défini dans la règle détermine quand une nouvelle alerte peut apparaître.

La page Accueil affiche un symbole compact pour chaque action. **Règles de surveillance** associe le symbole à un nom court, tandis que la colonne **Action** du journal n'affiche que le symbole :

- `1 ●` cercle gris : Enregistrer silencieusement
- `2 ▲` triangle orange : Notification et journal
- `3 ◆` losange rouge : Alerte contextuelle et journal

Le nombre et la forme permettent aussi de distinguer les actions sans se fier à la couleur. Placez le pointeur sur un symbole pour afficher le nom complet.

#### *Remarque importante :*

1. Une correspondance signifie seulement qu'une connexion choisie est apparue. Elle ne prouve pas que l'ordinateur est infecté.
2. Cet outil **enregistre les connexions et affiche des alertes uniquement**. Toute autre mesure de sécurité doit aussi tenir compte d'une analyse antivirus et de l'avis de professionnels qualifiés.

## Première utilisation

1. Choisissez l'une des sept langues proposées pendant l'installation ; la version portable demande également la langue au premier démarrage.
2. Ouvrez **Règles de surveillance**.
3. Sélectionnez **Nouvelle règle**.
4. Saisissez les conditions dans les champs du formulaire.
5. Vérifiez l'aperçu au bas du formulaire.
6. Enregistrez et activez la règle.
7. Revenez à **Accueil** et sélectionnez **Démarrer la surveillance**.

### Exemple

Pour surveiller si un port local se reconnecte à `103.1.40.235:1433`, créez cette règle :

- Type : Connexion TCP
- IP distante : `103.1.40.235`
- Port distant : `1433`
- Port local : Tous
- Action : Alerte contextuelle et journal
- Intervalle de répétition : 5 minutes

## Journaux

Les journaux sont stockés dans :

```text
%LOCALAPPDATA%\ConnectionWatcher\Logs\
```

Chaque nouvelle connexion correspondante apparaît sur une seule ligne du **journal des événements**. Une connexion ouverte pendant plusieurs heures n'est pas réenregistrée chaque seconde. **État** indique si elle est active ou terminée, et **Durée observée** se met à jour pendant son activité puis se fige à sa fin.

Pour faciliter la lecture, le tableau n'affiche que les champs principaux. Sa colonne **Application** utilise les informations disponibles sur le produit du fichier et, à défaut, le nom du processus. Double-cliquez sur une ligne pour ouvrir les **Détails de l’événement** et consulter le propriétaire de la connexion indiqué par Windows, le PID, le chemin, les informations sur le produit, jusqu'à trois processus parents ou hôtes, les services Windows associés et les autres champs de la connexion. L'état actif et la durée continuent de s'y actualiser, et **Copier les détails** copie l'enregistrement complet.

Ce contexte peut aider à repérer l'application liée à une connexion, mais ne prouve pas toujours quelle application l'a finalement déclenchée. Par exemple, un navigateur, un proxy, un VPN ou un composant Web intégré peut déjà fonctionner en arrière-plan.

La durée observée commence lorsque l'application voit la connexion pour la première fois ; elle peut donc être inférieure à sa durée réelle. Lorsque la surveillance est arrêtée, l'application ne sait pas si la connexion s'est interrompue. Un nouveau démarrage crée donc une nouvelle observation. Le CSV interne écrit les informations uniquement lors de la détection et de la fin ; l'application les regroupe sur une seule ligne du journal.

Une connexion n'est marquée comme terminée qu'après avoir été absente de la table des connexions Windows pendant deux secondes. Si elle réapparaît pendant ce délai, elle reste la même observation. L'heure de fin correspond au dernier moment où l'application a réellement vu la connexion. Une apparition ultérieure après ce délai crée un nouvel enregistrement.

Sélectionnez **Effacer l'affichage** pour désencombrer le journal des événements. Cette action masque les lignes existantes dans l'interface sans supprimer les journaux CSV. Les événements antérieurs restent masqués après le redémarrage de l'application, tandis que les nouveaux apparaissent normalement.

La limite totale est de 25 Mo par défaut et peut être réglée entre 5 et 500 Mo dans **Paramètres**. L'application utilise jusqu'à cinq fichiers et supprime les plus anciens lorsque la limite est atteinte.

## Centre d'aide

Dans **Paramètres**, sélectionnez **Ouvrir le centre d'aide** pour lire la présentation du projet et le guide. Les documents suivent la langue actuelle de l'interface.

## Mises à jour du logiciel

Dans **Paramètres**, sélectionnez **Vérifier maintenant** pour demander à GitHub la dernière version publique. L'application le fait uniquement à votre demande. Si une version plus récente existe, vous pouvez ouvrir sa page GitHub Release, lire les notes de version et la télécharger vous-même. L'application ne télécharge, n'installe et n'exécute aucune mise à jour automatiquement, et elle ne transmet ni règles ni journaux.

## Démarrage et son d'alerte

- **Lancer l'application à l'ouverture de session Windows :** ouvre l'application après la connexion, sans démarrer la surveillance.
- **Démarrer automatiquement la surveillance à l'ouverture :** démarre la surveillance avec les règles activées.
- **Son de l'alerte urgente :** utilise un bref son intégré, indépendant du modèle de sons Windows. Réglez le volume entre 10 % et 100 % (40 % par défaut). **Tester le son** apparaît à côté du réglage du volume ; le test et les alertes réelles utilisent le même niveau, et le volume Windows reste actif.

## Limites importantes

1. La vérification a lieu une fois par seconde par défaut. Même avec le réglage de 0,5 seconde, une connexion qui apparaît et disparaît entre deux vérifications peut être manquée.
2. La version 1 **surveille uniquement TCP**, pas UDP.
3. La table TCP Windows n'indique pas toujours de manière fiable qui a initié la connexion.
4. Les autorisations Windows ou l'arrêt rapide d'un processus peuvent empêcher la lecture d'un chemin, des informations du fichier, d'un processus parent ou d'un service associé. Le PID et tout nom disponible restent enregistrés. Le contexte des processus et services constitue un indice d'enquête, pas une conclusion garantie sur la cause première.
5. Aucune surveillance n'a lieu lorsque l'application est fermée, arrêtée ou que l'ordinateur est en veille.
6. La durée observée commence à la première détection. Sa précision dépend de l'intervalle choisi ; ce n'est pas une heure de début exacte fournie par Windows.
7. L'application ne ferme aucun programme, ne modifie pas le pare-feu et ne bloque aucune adresse IP.

## Confidentialité et autorisations

1. Aucun droit d'administrateur n'est requis.
2. Aucun compte, nom d'utilisateur, mot de passe ou courriel n'est requis.
3. L'application se connecte à GitHub uniquement lorsque vous sélectionnez manuellement **Vérifier maintenant**. Elle ne se connecte pas à un serveur du développeur et ne téléverse ni règles ni journaux.
4. Elle ne lit pas le contenu des paquets.
5. Les paramètres sont enregistrés dans `%LOCALAPPDATA%\ConnectionWatcher\config.json`.

## Désinstallation

Vous pouvez supprimer la version installée depuis **Applications installées** dans Windows. La désinstallation supprime le programme, mais conserve par défaut les paramètres et journaux dans `%LOCALAPPDATA%\ConnectionWatcher` afin d'éviter la perte accidentelle d'informations. Supprimez manuellement ce dossier lorsque vous êtes certain de ne plus en avoir besoin.
