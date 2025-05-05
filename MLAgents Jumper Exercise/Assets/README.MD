#  Jumper agent experiment


Voor deze opdracht maakte ik een jumper agent. Mijn agent springt om obstakels te ontwijken.
Deze obstakels worden continue ingespawned en krijgen elks een andere, random snelheid.


##  Set-up


###  Scripts


Om het project efficiënt, maar overzichtelijk, te maken, splitste ik het op in 3 scripts.


####  ObstacleSpawner.cs


Dit script spawned nieuwe obstakels met random intervallen.

Start():
    Checkt of de vereiste objecten zijn gerefereerd in de inspector.
    Valideert de interval waarden voor het spawnen.
    Initializeert de timer zodat de eerste spawn pas na een door de developer ingestelde delay gebeurt.

Update():
    Als de eerste spawn nog niet gebeurd is en de timer bereikt 0, roept deze functie SpawnObstacle() aan en berekent het interval voor de volgende spawn.
    Voor reguliere spawns: als de timer op de currentSpawnInterval staat, roept deze functie SpawnObstacle() aan, de timere wordt gerest en het interval voor de volgende spawn wordt berekend.

SetNextSpawnInterval():
    Helper functie die de pseudo-random-wachttijd voor de volgende spawn berekent.

SpawnObstacle():
    Spawned een nieuwe instantie van het obstakel prefab m.b.v. Instantiate().
    Plaatst het nieuwe obstakel op de exacte positie van het empty gameobject SpawnPoint.

ResetSpawner():
    Een publieke functie die gebruikt kan worden om de timing logic van de spawner te resetten. (Wordt momenteel niet direct aangeroepen door de andere scripts.).
    Vernietigt geen bestaande obstakels.


####  ObstacleController.cs


Dit script bestuurt het gedrag van een obstakel nadat het gespawned werd.

Start():
    Een random snelheid wordt gekozen en een safety check controleert of de minimum snelheid niet groter is dan de maximum snelheid.

Update():
    Deze functie zorgt voor de beweging van het obstakel. Het zorgt ervoor dat het obstakel verwijderd wordt indien endX (5) wordt bereikt.


####  JumperAgent.cs


Dit script is de agent. Het wordt gebruikt voor het trainen van de agent.

Initialize():
    Wordt aangeroepen wanneer de agent start. Krijgt de rigidbody component en de initiële positie.

OnEpisodeBegin():
    Reset de positie en snelheid van de agent. Vernietigt bestaande obstakels.

CollectObservations(VectorSensor sensor):
    Indien er problemen zijn met de ray perception sensor, kunnen de lijnen code hier geuncomment worden om de agent alles te laten weten.

OnActionReceived(ActionBuffers actions):
    Er wordt nagekeken of de agent op de grond staat.
    De actie wordt gelezen. De agent heeft twee mogelijke acties: niets doen en springen.
    Als de actie 1 is en de agent staat op de grond, dan wordt er een opwaardse kracht uitgevoerd.
    Er wordt in elke step een kleine beloning gegeven voor het overleven.

Heuristic(in ActionBuffers actionsOut):
    Er wordt hier gezorgd dat de agent manueel getest kan worden (spacebar om te springen).

OnCollisionEnter(Collision collision):
    Indien er een botsing plaatsvindt tussen de agent en het obstakel, wordt de episode beëindigd en krijgt de agent een straf.

OnDrawGizmosSelected():
    Helpt developers om tijdens het trainen te kijken of de agent op de grond staat.

###  Behaviour


behaviors:
  JumperAgent:
    trainer_type: ppo
    hyperparameters:
      batch_size: 512
      buffer_size: 20480
      learning_rate: 1.0e-5
      beta: 5.0e-4
      epsilon: 0.2
      lambd: 0.95
      num_epoch: 10
      learning_rate_schedule: linear
      beta_schedule: constant
      epsilon_schedule: linear
    network_settings:
      normalize: true
      hidden_units: 256
      num_layers: 3
    reward_signals:
      extrinsic:
        gamma: 0.99
        strength: 1.0
    max_steps: 5000000
    time_horizon: 256
    summary_freq: 2000


Om een zo optimaal mogelijke curve te bekomen experimenteerde ik wat met de behaviours.


###  Unity omgeving


Ik plaatste 6 trainingsomgevingen boven elkaar.


##  Resultaten en conclusies


We zien dat de grafieken wel degelijk steigen, maar er zit wel een instabiliteit in. Deze instabiliteit lijkt op een soort ruis die mee steigt met de grafiek.
Vermoedelijk komt de instabiliteit door, ondar andere, twee obstakels die elkaar kort opvolgen, waardoor de agent, die rays gebruikt om de omgeving te observeren, het tweede obstakel niet ziet aankomen en geen correcte timing van de jump kan inschatten.
De instabileit is waarschijnlijk ook te danken aan het feit dat een episode enkel eindigt wanneer de agent een obstakel aanraakt. Dit zorgt ervoor dat we de agent moeten belonen voor het overleven, wat moeilijker is dan gewoon de episode te beëindigen wanneer het obstakel op zijn bestemming komt zonder collision.