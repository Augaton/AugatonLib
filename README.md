# AugatonLib

Bibliotheque partagee par les plugins EXILED de la communaute **Zone-Shilari**.

**EXILED 9.14.2** — `dotnet build -c Release src/AugatonLib.csproj`

## Deploiement

Ce n'est **pas** un plugin. Le fichier va dans :

```
/home/container/.config/EXILED/Plugins/dependencies/AugatonLib.dll
```

Jamais dans `Plugins/7777/` : EXILED tenterait de le charger comme plugin et
echouerait, faute de classe `Plugin`.

Il doit etre deploye **avant** les plugins qui en dependent, et mis a jour en
meme temps qu'eux.

## Contenu

| Espace de noms | Role |
|---|---|
| `Hints.HintChannel` | Canal de hint isole par plugin, adosse a HintServiceMeow, avec repli sur les hints natifs |
| `FriendlyFire.FriendlyFireArbiter` | Arbitrage de `Server.FriendlyFire` entre plugins, voir [FRIENDLYFIRE.md](../FRIENDLYFIRE.md) |
| `Caching.RoomCache` | Cache des salles eligibles, reconstruit au debut du round |
| `Caching.CooldownTracker` | Cooldowns indexes sur `UserId` |
| `Random.Chance` | Tirage pondere, melange Fisher-Yates, jets en pourcentage |
| `Text.SafeText` | Assainissement de tout texte controle par un joueur avant affichage |
| `Commands.StaffCommand` | Socle de commande staff : permission, bornage des arguments, capture d'exception, audit |
| `Commands.StaffParentCommand` | Commande parente listant ses sous-commandes automatiquement |
| `Commands.StatusCommand` | Sous-commande `status` uniforme |
| `Integrations.UncomplicatedBridge` | Pont vers UncomplicatedCustomItems, CustomRoles et CustomTeams |

## Assainissement des textes joueur

Un pseudo SCP:SL peut contenir des balises rich text. Diffuse tel quel dans un
broadcast ou un hint, il permet a n'importe quel joueur d'imposer une taille de
police, une couleur ou un `<link>` malforme a tous les autres — spam visuel au
mieux, crash client au pire.

`SafeText.Sanitize` retire les chevrons, les caracteres de controle, les
caracteres de formatage et les zero-width, borne la longueur, et renvoie un
repli si rien ne subsiste. Les emoji sont preserves.

```csharp
Announce(Text.Winner.Replace("%PLAYER%", SafeText.Nickname(player)));
```

**Regle** : tout texte venant d'un joueur et affiche a un autre joueur passe par
`SafeText`. Les logs serveur et les reponses de console staff gardent la valeur
brute, qui est ce que le staff a besoin de voir.

## Le pont Uncomplicated

Les trois plugins Uncomplicated ne sont pas distribues sur NuGet. Le pont les
atteint par reflexion, sur des signatures **verifiees contre les DLL publiees**
et non devinees :

| Cible | Catalogue | Application |
|---|---|---|
| Roles | `CustomRole.List` | `SummonedCustomRole.Summon(LabApi.Player, ICustomRole)` |
| Objets | `CustomItem.List` | `new SummonedCustomItem(ICustomItem, Exiled.Player)` |
| Equipes | `Team.List` | `TeamSpawner.SpawnSpecificTeam(Team, bool, bool)` |

Deux pieges qui ont ete traites :

- l'assembly des objets s'appelle **`UncomplicatedCustomItems-Exiled`**, pas
  `UncomplicatedCustomItems` ;
- les roles et les equipes travaillent avec `LabApi.Features.Wrappers.Player`,
  pas avec `Exiled.API.Features.Player`. La conversion passe par
  `LabApi.Player.Get(ReferenceHub)`.

Si un plugin Uncomplicated est absent, la partie correspondante se signale
comme indisponible et les fonctionnalites qui en dependent se desactivent
d'elles-memes.

## Compiler un plugin isolement

Les depots des plugins referencent ce projet par chemin relatif. Clonez-le a
cote du depot du plugin :

```
mon-dossier/
├── augatonlib/      (ou AugatonLib/, les deux sont reconnus)
└── Better914/
```

Ou passez le chemin explicitement :

```
dotnet build -c Release -p:CommonProject=/chemin/AugatonLib/src/AugatonLib.csproj
```

Sans cela, le build s'arrete avec un message explicite plutot qu'une erreur de
reference obscure.

## Releases

Chaque tag `v*` publie `AugatonLib.dll` ainsi qu'une archive prete a extraire
dans `.config/EXILED/`.

Les workflows des douze plugins recuperent ce depot par `actions/checkout` sur
`main` et embarquent la DLL compilee dans leur propre release. Une modification
poussee ici est donc reprise par la prochaine release de chaque plugin.

## Integration continue

`build` et `release` sont deux **workflows distincts**.

| Workflow | Declencheur | Produit |
|---|---|---|
| `build` | push sur `main`, pull request | Compile, cree le tag si besoin, declenche la release |
| `release` | tag `v*`, ou lancement manuel | Une **release** avec `AugatonLib.dll` |

### Publication automatique

Le job `tag` de `build` lit la balise `<Version>` du `.csproj`. Si le tag
`v<version>` n'existe pas encore, il le cree et declenche `release`.

Publier revient donc a **incrementer `<Version>` dans le `.csproj`** puis
pousser sur `main`. Sans changement de version, aucun tag n'est cree et aucune
release n'est publiee : les commits de correction ne generent pas de bruit.

La version affichee par `status` en jeu est lue dans l'assembly, elle-meme
issue de cette meme balise. Le tag, la DLL et l'affichage en jeu ne peuvent
donc pas diverger.

### Publication manuelle

```bash
git tag v1.0.0 && git push origin v1.0.0
```

Ou onglet Actions, workflow `release`, bouton **Run workflow** : le tag est cree
s'il n'existe pas.
