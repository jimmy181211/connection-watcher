# Guide d'utilisation de SocketSight

## Sommaire

- [Qu'est-ce que cet outil ?](#quest-ce-que-cet-outil)
- [Installation et démarrage rapide](#installation-et-démarrage-rapide)
- [Intervalle de vérification](#intervalle-de-vérification)
- [Après une correspondance](#après-une-correspondance)
- [Consulter les événements](#consulter-les-événements)
- [Comprendre un enregistrement](#comprendre-un-enregistrement)
- [Centre d'aide et mises à jour](#centre-daide-et-mises-à-jour)
- [Journaux, son et autres paramètres](#journaux-son-et-autres-paramètres)
- [Confidentialité, droits et désinstallation](#confidentialité-droits-et-désinstallation)

## Qu'est-ce que cet outil ?

SocketSight vous aide à surveiller une adresse IP ou un port précis.

Lorsqu'une connexion TCP correspond à une règle, l'application enregistre l'heure, l'IP, le port et les informations de processus disponibles dans Windows, puis applique le mode d'alerte choisi.

Elle observe, enregistre et alerte uniquement. Elle ne ferme pas les programmes, ne modifie pas le pare-feu et ne bloque pas d'adresse IP.

## Installation et démarrage rapide

La langue choisie pendant l'installation est aussi celle de l'application. Lors d'une mise à niveau, choisir une autre langue la modifie une fois ; les règles, paramètres et journaux restent présents.

Si le démarrage prend plus d'environ 0,5 seconde, SocketSight affiche un court écran qui disparaît lorsque la fenêtre principale est prête.

1. Ouvrez **Règles de surveillance**.
2. Sélectionnez **Nouvelle règle**.
3. Saisissez l'IP ou le port à surveiller.
4. Enregistrez et activez la règle.
5. Revenez à **Accueil** et choisissez **Démarrer la surveillance**.

Exemple pour surveiller `103.1.40.235:1433` :

- IP distante : `103.1.40.235`
- Port distant : `1433`
- Port local : Tous
- Action : alerte contextuelle et journal
- Intervalle de répétition : 5 minutes

## Intervalle de vérification

L'intervalle par défaut est d'une seconde. Dans **Accueil**, choisissez 0,5 à 10 secondes par pas de 0,5 seconde.

Un intervalle court détecte mieux les connexions brèves mais consomme davantage de ressources. Même à 0,5 seconde, une connexion apparaissant et disparaissant entre deux vérifications peut être manquée.

Seules les règles activées créent des enregistrements ou des alertes.

## Après une correspondance

- **Journal silencieux :** écrit dans le journal sans alerte.
- **Notification de zone et journal :** l'icône de notification passe en avertissement ; ouvrir le journal des événements efface la notification.
- **Alerte contextuelle et journal :** affiche une fenêtre pour la première correspondance ; les suivantes mettent à jour cette même fenêtre.

Les nombres et les formes de l'accueil et de la liste d'événements aident à distinguer ces trois actions.

## Consulter les événements

Une même connexion n'apparaît que dans une ligne, pas dans une nouvelle ligne chaque seconde.

- Une connexion présente est **Active**.
- Une connexion terminée est **Terminée**.
- La **durée observée** se met à jour pendant l'activité puis reste fixe.
- **Application** affiche le nom de produit du fichier s'il est disponible, sinon le nom du processus.
- Double-cliquez sur une ligne pour voir le processus, le PID, le chemin, les processus parents, les services Windows et les autres détails. Vous pouvez aussi copier l'enregistrement.

Une connexion est marquée terminée après deux secondes d'absence dans la liste Windows. Si elle revient dans ces deux secondes, elle reste la même observation ; un retour plus tard crée une nouvelle ligne.

La durée commence lorsque l'application voit la connexion pour la première fois et peut donc différer de sa durée réelle. La surveillance arrêtée n'est pas observée ; son redémarrage crée une nouvelle observation.

## Comprendre un enregistrement

Une correspondance signifie seulement qu'une connexion que vous vouliez surveiller est apparue. Cela ne prouve pas la présence d'un logiciel malveillant.

Un navigateur, un proxy, un VPN ou un composant web peut déjà fonctionner en arrière-plan. Les informations de processus aident à identifier une application liée, mais ne prouvent pas laquelle a finalement déclenché la connexion.

La table TCP ne permet pas de déterminer de façon fiable quel côté a initié la connexion. Les droits Windows peuvent aussi empêcher la lecture de certains chemins, fichiers, processus parents ou services.

Pour évaluer un problème de sécurité, combinez ces informations avec un antivirus ou l'avis d'un professionnel.

## Centre d'aide et mises à jour

Dans **Paramètres**, cliquez sur **Ouvrir** à côté du centre d'aide pour lire la présentation du projet et le guide d'utilisation. Les documents suivent la langue de l'interface.

Cliquez sur **Vérifier maintenant** pour demander à GitHub la dernière version publique. L'application ne télécharge, n'installe ni n'exécute automatiquement les mises à jour.

Dans **Paramètres**, ouvrez **Commentaires** pour écrire une suggestion ou un problème. Le navigateur ouvre une Issue GitHub préremplie ; vérifiez-la puis envoyez-la vous-même. Les journaux et connexions ne sont pas joints par défaut.

## Journaux, son et autres paramètres

Les journaux sont enregistrés dans :

```text
%LOCALAPPDATA%\ConnectionWatcher\Logs\
```

Le CSV est écrit à la découverte d'une connexion et à la fin de son observation, pas chaque seconde. Le journal des événements regroupe une même connexion sur une ligne.

**Effacer l'affichage** masque les lignes sans supprimer les fichiers CSV. Elles restent masquées après un redémarrage ; les nouveaux événements apparaissent normalement.

La limite par défaut est de 25 Mo, réglable de 5 à 500 Mo dans **Paramètres**. Jusqu'à cinq fichiers sont conservés et le plus ancien est supprimé lorsque la limite est atteinte.

**Lancer l'application à l'ouverture de session Windows** ouvre seulement l'application. **Démarrer automatiquement la surveillance à l'ouverture** démarre la surveillance avec les règles activées.

Le son d'alerte urgente sert aux alertes contextuelles. Réglez son volume dans **Paramètres** ; **Tester le son** utilise le même volume et le volume Windows s'applique également.

## Confidentialité, droits et désinstallation

- Aucun droit administrateur, compte ou mot de passe n'est nécessaire.
- L'application ne lit pas le contenu des paquets.
- Les règles et journaux ne sont pas envoyés.
- GitHub n'est contacté que lors d'une vérification manuelle ou de l'ouverture de la page de commentaires.

La désinstallation conserve les paramètres et journaux par défaut. Si vous n'en avez plus besoin, supprimez manuellement :

```text
%LOCALAPPDATA%\ConnectionWatcher
```
