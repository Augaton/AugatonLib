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
| `Commands.StaffCommand` | Socle de commande staff : permission, bornage des arguments, capture d'exception, audit |
| `Commands.StaffParentCommand` | Commande parente listant ses sous-commandes automatiquement |
| `Commands.StatusCommand` | Sous-commande `status` uniforme |
| `Integrations.UncomplicatedBridge` | Pont vers UncomplicatedCustomItems, CustomRoles et CustomTeams |

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
| `build` | push sur `main`, pull request | Un artefact de run, visible dans l'onglet Actions |
| `release` | push d'un **tag `v*`**, ou lancement manuel | Une **release** avec `AugatonLib.dll` |

Un push sur `main` ne declenche que `build`. Pour obtenir une DLL telechargeable
sans passer par l'onglet Actions :

```bash
git tag v1.0.0
git push origin v1.0.0
```

Ou depuis l'onglet Actions, workflow `release`, bouton **Run workflow** en
saisissant le tag : il sera cree s'il n'existe pas.

Comme les plugins compilent contre `main`, **une rupture d'API ici casse les
douze CI a la fois**. Les changements incompatibles meritent un tag et un
`augatonlib_ref` fixe cote plugins.
