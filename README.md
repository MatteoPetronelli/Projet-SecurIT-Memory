# Projet SecurIT - Memory

## Contexte du Projet
Ce projet a été développé dans le cadre de la préparation du Salon de l'Innovation Tech pour la start-up **SecurIT**, spécialisée en cybersécurité.  
Le but de cette application est de proposer un mini-jeu interactif (Jeu de Memory) aux visiteurs du stand, afin de les sensibiliser aux concepts clés de la cybersécurité (Mots de passe, Pare-feu, Virus, etc.) tout en testant leur mémoire.

Ce projet a été réalisé en binôme en utilisant **C#** et le framework **WinForms**.

---

## Architecture du Projet

L'application respecte une séparation stricte entre la logique métier (Backend) et l'interface utilisateur (Frontend) :

- `Core/` : Gestionnaires principaux (Base de données, Logique du jeu, Thèmes, Paramètres)
- `Models/` : Objets de données métier (Carte, États de carte, Scores)
- `Views/` : Interface graphique utilisateur (WinForms)
- `Ressources/` : Assets du jeu (Audios et Images classées par thèmes)

### Arborescence

```text
Projet-SecurIT-Memory/
├── Core/
│   ├── DatabaseManager.cs
│   ├── GameManager.cs
│   ├── GameSettings.cs
│   └── ThemeManager.cs
├── Models/
│   ├── Card.cs
│   ├── CardState.cs
│   └── Score.cs
├── Views/
│   ├── GameForm.cs
│   ├── MainMenuForm.cs
│   └── OptionsForm.cs
├── Ressources/
│   ├── Audio/
│   └── Images/
│       ├── Crypto/
│       ├── Logiciel/
│       └── Materiel/
├── Program.cs
└── README.md
```

---

## Installation et Mise en Marche

### Étapes d'installation

1. **Cloner le dépôt**
```bash
git clone <url_du_depot>
```

2. **Ouvrir la solution**  
Double-cliquez sur le fichier `Projet-SecurIT-Memory.sln` pour ouvrir le projet dans Visual Studio Code.

3. **Vérification des ressources**  
Assurez-vous que les dossiers dans `Ressources/Images/` et `Ressources/Audio/` contiennent bien les fichiers nécessaires :
- Images : `.png` / `.jpg`
- Sons : `.wav`

4. **Base de données**  
La base SQL et la table `Scores` sont générées automatiquement au premier lancement via `DatabaseManager.cs`.

5. **Compilation et lancement**  
Appuyez sur **F5** ou cliquez sur **Run** puis **Start Debuggin** ou **Start Without Debugging** dans Visual Studio Code.

---

## Utilisation et Fonctionnement

### Menu Principal
Au lancement, le joueur peut :
- Démarrer une partie
- Accéder aux options
- Quitter l'application

### Options
Permet de configurer :
- La difficulté (taille de la grille)
- Le thème visuel (Matériel, Logiciel, Cryptographie)

### En Jeu

- Le joueur clique sur deux cartes pour les retourner
- Si elles correspondent → la paire reste visible
- Sinon → elles se retournent après un court délai
- Les clics sont temporairement bloqués pendant ce délai

### Mode Hardcore (Bonus)
Si activé :
- Toutes les 30 secondes, les cartes non trouvées changent de position aléatoirement

### Fin de Partie
- Une fois toutes les paires trouvées :
  - Le score est calculé (temps + nombre d'essais)
  - Il est enregistré dans la base de données SQL
  - Il alimente le **Leaderboard**

---

## Notes

- Projet pédagogique orienté cybersécurité
- Conçu pour une démonstration interactive en salon
- Architecture claire facilitant la maintenance et l'évolution
