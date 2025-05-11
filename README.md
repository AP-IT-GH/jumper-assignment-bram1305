# JumperAgent Performance analyse


Het doel van het project is om een agent aan te leren om over obstakels te springen. Deze obstakels volgen elkaar continue op met variabele snelheid en interval.


## Setup


### Scripts


We gebruiken 3 scripts om de omgeving te en de agent te laten werken.


###### JumperAgent.cs

Dit script implementeert de logica achter de agent.
Observations:
    isGrounded: observeert of de agent op de grond staat. Zo niet, dan kan de agent niet springen.
    rb.velicty.y / 10f: observeert de verticale snelheid.
Actions:
    We hebben een enkele discrete actie met twee branches:
        0: niets doen.
        1: springen (indien de agent op de grond staat), een opwaartse kracht uitvoeren.
Rewards:
    touchPunishment: een straf wordt gegeven wanneer de agent een obstakel aanraakt.
    obstacleClearedReward: een beloning wordt gegeven wanneer een obstakel met success wordt gepasseerd.
De episode wordt beëindigd wanneer de agent 3 obstakels geraakt heeft. We willen de agent meer kansen geven, aangezien deze anders nooit de kans zal krijgen om te leren dat het ontwijken van een obstakel een beloning oplevert.
Bij het begin van elke episode wordt de currentCollisions gereset naar wordt een instructie gestuurd naar de ObstacleSpawner om alle oude obstakels te verwijderen.


###### ObstacleSpawner.cs


Dit script beheert het spawning systeem van de obstakels.
Het script spawnt obstakels op een gespecifiëerd spawnPoint met een random timing tussen minSpawnInterval en maxSpawnInterval na een initialSpawnDelay.
Wanneer een obstakel gespawned wordt, zal de agent langs deze weg de obstakels volgen om melding te krijgen wanneer een obstakel het eindpunt bereikte. Hierdoor kan de agent beloond worden.
De ResetSpawner en ClearObstacles methoden worden door de agent aangeroepen bij OnEpisodeBegin.


###### ObstacleController.cs:


Dit script zorgt voor het gedrag van een individueel obstakel.
Het zorgt ervoor dat het obstakel aan een random gekozen snelheid binnen een minimum en een maximum, beweegt. Wanneer het obstakel op een eindpunt komt, zorgt het dat de agent een beloning krijgt en dan vernietigd het zichzelf.


### Behaviors

behaviors:
  JumperAgent:
    trainer_type: ppo
    hyperparameters:
      batch_size: 512
      buffer_size: 20480
      learning_rate: 3.0e-4
      beta: 5.0e-4
      epsilon: 0.2
      lambd: 0.95
      num_epoch: 4
      learning_rate_schedule: linear
      beta_schedule: linear
      epsilon_schedule: linear
    network_settings:
      normalize: true
      hidden_units: 256
      num_layers: 3
    reward_signals:
      extrinsic:
        gamma: 0.99
        strength: 1.0
    max_steps: 100000000
    time_horizon: 256
    summary_freq: 250

Bij het kiezen van de behaviors probeerde ik een stabiele leercurve te bekomen. Dit bleek helaas veel moeilijker dan gedacht.
Ik koos voor lineaire schedules om te proberen ervoor te zorgen dat de agent, naarmate dat deze meer bijleerde en dus dichter bij een optimum komt, minder random zal gaan zoeken.


### Omgeving


De unity omgeving bestaat uit 6 training areas.
De training area prefab bevat een groundplane, een agent, een obstakel-spawnpoint en een . Om de scripts te laten werken, voegde ik in de unity settings een 'ground' layer en een 'obstacle' layer toe, en maakte ik een 'Obstacle' tag aan.
Bij de agent voegde ik een maximum aantal steps van 3500 per episode toe, dit zorgt ervoor dat een agent tijdens het trainen niet in een eindeloze looping terecht komt.


## Resultaten


De grafieken werden als .png afbeeldingen bijgevoegd. Ik trainde mijn agent tot 20 miljoen steps.
Bij de cumulative rewards kunnen we zien dat de groei eerst goed zit, maar na een 4 miljoen steps bereikt het de hoogste top van de grafiek, om daarna volledig in te storten en niet meer eenzelfde hoogtepunt te bereiken. Deze trend kunnen we ook in de andere grafieken terugvinden.
De learning rate daalt eerst heel mooi, maar begint vervolgens terug te stijgen. Trends zoals deze kunnen we ook in andere grafieken terugvinden.
De entropie blijft een dalende lijn.


## Conclusie


Initieel leert de agent om over de obstakels te springen, dit loopt goed, maar de training stort na 4 miljoen steps volledig in elkaar en bereikt nooit meer eenzelfde hoogtepunt.
Deze resultaten geven mij de indruk dat er iets mis is met mijn reward systeem. Mijn vermoeden is dat het probleem zich stelt bij het feit dat een obstakel waar de agent mee botst nog altijd doorgaat naar het einde en een beloning veroorzaakt. Dit zou weleens een cruciale fout kunnen zijn die ervoor zorgt dat de agent leert dat hij niet gestraft wordt voor het botsen. Daarom moet er ervoor gezorgd worden dat in het script, de obstakeles bij een botsing verwijderd worden.
Aangezien de entropie blijft dalen, wordt onze agent steeds meer deterministisch. Doordat de agent na de 4 miljoen steps een slechte 'gedachtengang' heeft geleerd (dat een obstakel raken niet zo erg is), maar hij steeds minder random acties zal gaan uitvoeren, blijft de agent in deze slechte 'gedachtengang' zitten en kan het niet meer op een evenhoog of hoger hoogtepunt geraken.

We kunnen stellen dat de training gefaald is in het bereiken van een stabiele curve en consistent goede punten te halen. Het experiment zorgde er wel voor dat ik meer inzicht begin te krijgen in de betekenis van de verschillende grafieken en behaviors/hyperparameters. De resultaten van de finale training staan bij beta_3. U kan de video (16sec) van de training vinden in de assets folder.
