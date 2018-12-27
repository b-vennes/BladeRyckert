using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

[Serializable]
public class Level : MonoBehaviour
{

    public Camera cam;

    public float xUnitLength = 0.8f;

	public float yUnitLength = 0.5f;

    public Player player;

    public List<Robot> robots;

    public List<Robot> superRobots;

    public Laser laser;

    public int[,] pathGrid;

    public AudioSource ambientBgm;

    public AudioSource actionBgm;

    public int robotsLeft;

    public List<Vector2Int> objectives;

    public virtual void Start()
    {
        
    }

    public Sprite pathCircle;

    public TextMeshProUGUI enemiesRemaining;
    public TextMeshProUGUI hackStatus;
    public TextMeshProUGUI hackProgress;
    public TextMeshProUGUI nextPrompt;
    public TextMeshProUGUI buttonToHack;

    public GameObject movementTile;

    public List<GameObject> movementTiles;

    public GameObject detectionTile;

    public List<GameObject> detectionTiles;

    public GameObject laserBeamTemplate;

    public GameObject laserBeamCopy;

    public List<GameObject> startDialogs;

    public List<GameObject> endDialogues;

    public Queue<GameObject> startDialogQueue;

    public int dialoguePhase;

    GameObject currentDialogue;
    
    AudioSource currentDialogAudioSource;

    public void Setup()
    {
        player.animator = player.obj.GetComponent<Animator>();
        player.sprite = player.obj.GetComponent<SpriteRenderer>();
        player.gridPosition = player.gridStartPosition;
        player.movingTo = player.sprite.transform.position;
        player.isMoving = false;
        player.isHidden = true;
        player.alive = true;

        foreach ( Robot basicBot in robots)
        {
            basicBot.animator = basicBot.obj.GetComponent<Animator>();
            basicBot.sprite = basicBot.obj.GetComponent<SpriteRenderer>();
            basicBot.gridPosition = basicBot.gridStartPosition;
            basicBot.movingTo = basicBot.sprite.transform.position;
            basicBot.isMoving = false;
            basicBot.targetingPlayer = false;
            basicBot.waitingForOpenSpace = false;
        }

        foreach( Robot superBot in superRobots)
        {
            superBot.animator = superBot.obj.GetComponent<Animator>();
            superBot.sprite = superBot.obj.GetComponent<SpriteRenderer>();
            superBot.gridPosition = superBot.gridStartPosition;
            superBot.movingTo = superBot.sprite.transform.position;
            superBot.isMoving = false;
            superBot.targetingPlayer = false;
            superBot.waitingForOpenSpace = false;
        }

        enemiesRemaining = GameObject.Find("EnemiesRemainingText").GetComponent<TextMeshProUGUI>();
        hackStatus = GameObject.Find("HackStatus").GetComponent<TextMeshProUGUI>();
        hackProgress = GameObject.Find("HackProgress").GetComponent<TextMeshProUGUI>();
        nextPrompt = GameObject.Find("NextPrompt").GetComponent<TextMeshProUGUI>();
        buttonToHack = GameObject.Find("RightButtonToHack").GetComponent<TextMeshProUGUI>();

        TileOverlays();

        startDialogQueue = new Queue<GameObject>();

        foreach(GameObject dialogue in startDialogs)
        {
            startDialogQueue.Enqueue(dialogue);
        }

        enemiesRemaining.enabled = false;
        hackStatus.enabled = false;
        hackProgress.enabled = false;
        buttonToHack.enabled = false;

        dialoguePhase = 1;

        NextDialogue();
    }

    public void NextDialogue()
    {
        if (dialoguePhase == 1)
        {
            if (startDialogQueue.Count == 0)
            {
                dialoguePhase = 0;
                enemiesRemaining.enabled = true;
                hackStatus.enabled = true;
                hackProgress.enabled = true;
                buttonToHack.enabled = true;
                nextPrompt.enabled = false;
            }
            else
            {
                currentDialogue = startDialogQueue.Dequeue();
                currentDialogue = Instantiate(currentDialogue);
                currentDialogAudioSource = currentDialogue.GetComponent<AudioSource>();
                currentDialogAudioSource.Play();
            }
        }
    }

    void Update()
    {
        CheckInputs();

        UpdateSorting();

        CountRobots();

        CheckLevelComplete();

        UpdateUi();
    }

    private void TileOverlays()
    {
        
        // Check player tile placements
        movementTiles = new List<GameObject>();
        detectionTiles = new List<GameObject>();

        Instantiate(movementTile, GetDestination(player.obj.transform.position, 1, 1), Quaternion.identity, player.obj.transform);
        Instantiate(movementTile, GetDestination(player.obj.transform.position, 1, -1), Quaternion.identity, player.obj.transform);
        Instantiate(movementTile, GetDestination(player.obj.transform.position, -1, 1), Quaternion.identity, player.obj.transform);
        Instantiate(movementTile, GetDestination(player.obj.transform.position, -1, -1), Quaternion.identity, player.obj.transform);

        foreach(Robot r in robots)
        {
            for (int i = 1; i < 3; i++)
            {
                Instantiate(detectionTile, GetDestination(r.obj.transform.position, i, i), Quaternion.identity, r.obj.transform);
                Instantiate(detectionTile, GetDestination(r.obj.transform.position, i, -i), Quaternion.identity, r.obj.transform);
                Instantiate(detectionTile, GetDestination(r.obj.transform.position, -i, i), Quaternion.identity, r.obj.transform);
                Instantiate(detectionTile, GetDestination(r.obj.transform.position, -i, -i), Quaternion.identity, r.obj.transform);
            }
        }

        foreach (Robot r in superRobots)
        {
            for (int i = 1; i < 5; i++)
            {
                Instantiate(detectionTile, GetDestination(r.obj.transform.position, i, i), Quaternion.identity, r.obj.transform);
                Instantiate(detectionTile, GetDestination(r.obj.transform.position, i, -i), Quaternion.identity, r.obj.transform);
                Instantiate(detectionTile, GetDestination(r.obj.transform.position, -i, i), Quaternion.identity, r.obj.transform);
                Instantiate(detectionTile, GetDestination(r.obj.transform.position, -i, -i), Quaternion.identity, r.obj.transform);
            }
        }

    }

    public void UpdateUi()
    {
        enemiesRemaining.SetText(">$ enemies_remaining=" + robotsLeft);

        if (laser.charged)
        {
            hackStatus.SetText(">$ hack_is_ready=true");
        }
        else
        {
            hackStatus.SetText(">$ hack_is_ready=false");
        }

        hackProgress.SetText(">$ hack_progress=" + laser.charge * 25);
    }

    void FixedUpdate()
    {
        UpdateAnimations();
    }

    void CountRobots()
    {
        int robotsCounted = 0;

        foreach(Robot r in robots)
        {
            robotsCounted++;
        }

        foreach (Robot r in superRobots)
        {
            robotsCounted++;
        }

        robotsLeft = robotsCounted;
    }

    public virtual void CheckLevelComplete()
    {

    }

    void CheckInputs()
    {
        if (dialoguePhase == 1)
        {
            if (Input.GetMouseButtonDown(0))
            {
                currentDialogAudioSource.Stop();
                Destroy(currentDialogue);
                NextDialogue();
            }
            return;
        }
        if (dialoguePhase == 2)
        {
            if (Input.GetMouseButtonDown(0))
            {
                currentDialogAudioSource.Stop();
                Destroy(currentDialogue);
                NextDialogue();
            }
            return;
        }
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 clickPoint = cam.ScreenToWorldPoint(Input.mousePosition);

            if (!(Math.Abs(clickPoint.x - player.sprite.transform.position.x) < 2f) || !(Math.Abs(clickPoint.y - player.sprite.transform.position.y) < 2f))
            {
                return;
            }

            if (clickPoint.x > player.sprite.transform.position.x && clickPoint.y > player.sprite.transform.position.y)
            {
                AttemptPlayerMove(0,1);
            }
            else if (clickPoint.x < player.sprite.transform.position.x && clickPoint.y < player.sprite.transform.position.y)
            {
                AttemptPlayerMove(0,-1);
            }
            else if (clickPoint.x > player.sprite.transform.position.x && clickPoint.y < player.sprite.transform.position.y)
            {
                AttemptPlayerMove(1,0);
            }
            else if (clickPoint.x < player.sprite.transform.position.x && clickPoint.y > player.sprite.transform.position.y)
            {
                AttemptPlayerMove(-1,0);
            }
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            AttemptPlayerMove(0,1);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            AttemptPlayerMove(0,-1);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            AttemptPlayerMove(-1,0);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            AttemptPlayerMove(1,0);
        }
        
        // fire the lazer
        if (Input.GetMouseButtonDown(1))
        {
            if (laser.charged && !player.isMoving)
            {
                player.animator.SetTrigger("Hack");
                StartCoroutine(FireAndMoveBots());
            }
        }
        
        // reset
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return;
        }
    }

    IEnumerator FireAndMoveBots()
    {
        laser.firing = true;
        FireLaser(laser);
        yield return new WaitForSeconds(2.5f);
        laser.firing = false;
        MoveBots();
        Destroy(laserBeamCopy);
    }

    public void FireLaser(Laser laser)
    {

        laser.FireLaser();

        AudioSource laserCharge = GameObject.Find("LaserCharge").GetComponent<AudioSource>();
        laserCharge.Play();

        laserBeamCopy = Instantiate(laserBeamTemplate);

        int xLane = -1;
        int yLane = 8;
        if (laser.position.x == -1)
        {
            yLane = laser.position.y;
        }
        else
        {
            xLane = laser.position.x;
        }

        List<Robot> robotsCopy = new List<Robot>(robots);

        foreach (Robot bot in robotsCopy)
        {
            if (xLane == -1)
            {
                if (bot.gridPosition.y == yLane)
                {
                    StartCoroutine(BotDestroyedByLaser(bot));
                }
            }
            else
            {
                if (bot.gridPosition.x == xLane)
                {
                    StartCoroutine(BotDestroyedByLaser(bot));
                }
            }
        }

        List<Robot> superRobotsCopy = new List<Robot>(superRobots);
        foreach (Robot super in superRobotsCopy)
        {
            if (xLane < 0)
            {
                if (super.gridPosition.y == yLane)
                {
                    StartCoroutine(SuperBotDestroyedByLaser(super));
                }
            }
            else
            {
                if (super.gridPosition.x == xLane)
                {
                    StartCoroutine(SuperBotDestroyedByLaser(super));
                }
            }
        }

        if (xLane < 0)
        {
            if (player.gridPosition.y == yLane)
            {
                StartCoroutine(PlayerKilled());
            }
        }
        else
        {
            if (player.gridPosition.x == xLane)
            {
                StartCoroutine(PlayerKilled());
            }
        }
    }

    private bool AttemptPlayerMove(int xGridDir, int yGridDir)
    {

        if (!isValid(player.gridPosition.x + xGridDir, player.gridPosition.y + yGridDir) ||
            pathGrid[player.gridPosition.x + xGridDir, player.gridPosition.y + yGridDir] != 1 || !player.alive)
        {
            return false;
        }

        bool robotsMoving = RobotsMoving();

        // make sure all animations completed
        if (!player.isMoving && !robotsMoving && !laser.firing)
        {
            Vector2Int movementVector = GridToBoard(new Vector2Int(xGridDir, yGridDir));

            // set movement values
            player.gridPosition = new Vector2Int(player.gridPosition.x + xGridDir, player.gridPosition.y + yGridDir);
            player.movingTo = GetDestination(player.sprite.transform.position, movementVector.x, movementVector.y);
            player.isMoving = true;
            player.timeStartedLerp = Time.time;
            player.lerpStartPosition = player.sprite.transform.position;

            // handle animations
            if (xGridDir == 1 && yGridDir == 0)
            {
                player.animator.SetTrigger("WalkDown");
                player.sprite.flipX = false;
            }
            else if (xGridDir == 0 && yGridDir == 1)
            {
                player.animator.SetTrigger("WalkUp");
                player.sprite.flipX = false;
            }
            else if (xGridDir == 0 && yGridDir == -1)
            {
                player.animator.SetTrigger("WalkDown");
                player.sprite.flipX = true;
            }
            else if (xGridDir == -1 && yGridDir == 0)
            {
                player.animator.SetTrigger("WalkUp");
                player.sprite.flipX = true;
            }

            AudioSource footsteps = GameObject.Find("Player/Footsteps").GetComponent<AudioSource>();
            footsteps.Play();
            Invoke("MoveBots", 0.25f);

            laser.IncreaseCharge();

            return true;
        }
        else
        {
            return false;
        }
    }

    bool RobotsMoving()
    {

        foreach (Robot r in robots)
        {
            if (r.isMoving)
            {
                return true;
            }
        }

        foreach (Robot r in superRobots)
        {
            if (r.isMoving)
            {
                return true;
            }
        }

        return false;
    }

    void StartActionMusic()
    {
        ambientBgm.Stop();
        actionBgm.Play();
    }

    void MoveBots()
    {

        // BASIC BOTS
        foreach (Robot basic in robots)
        {
            List<Vector2Int> stepsToPlayer = ShortestPathTowardsPlayer(basic.gridPosition.x, basic.gridPosition.y);

            Vector2Int firstStep = stepsToPlayer[0];

            if (basic.targetingPlayer)
            {

                basic.waitingForOpenSpace = false;
                // check to make sure not blocked
                foreach (Robot bot in robots)
                {
                    if (bot.gridPosition.x == basic.gridPosition.x + firstStep.x && bot.gridPosition.y == basic.gridPosition.y + firstStep.y)
                    {
                        basic.waitingForOpenSpace = true;
                        break;
                    }
                }
                // can move into grid cell
                if (!basic.waitingForOpenSpace)
                {
                    Vector2Int movementVector = GridToBoard(firstStep);

                    basic.gridPosition = new Vector2Int(basic.gridPosition.x + firstStep.x, basic.gridPosition.y + firstStep.y);
                    basic.movingTo = GetDestination(basic.sprite.transform.position, movementVector.x, movementVector.y);
                    basic.isMoving = true;
                    basic.timeStartedLerp = Time.time;
                    basic.lerpStartPosition = basic.sprite.transform.position;

                    // handle animations
                    HandleRobotMoveAnimation(basic, firstStep.x, firstStep.y);

                    AudioSource botWalk = GameObject.Find(basic.obj.name + "/RobotWalk").GetComponent<AudioSource>();
                    botWalk.Play();
                }
            }
            // patrol
            else 
            {
                Vector2Int currPatrol = basic.patrol[basic.patrolStage];
                Vector2Int movementVector = GridToBoard(basic.patrol[basic.patrolStage]);

                basic.waitingForOpenSpace = false;
                // check to make sure not blocked
                foreach (Robot bot in robots)
                {
                    if (bot.gridPosition.x == basic.gridPosition.x + currPatrol.x && bot.gridPosition.y == basic.gridPosition.y + currPatrol.y)
                    {
                        basic.waitingForOpenSpace = true;
                        break;
                    }
                }

                if (!basic.waitingForOpenSpace)
                {
                    basic.gridPosition = new Vector2Int(basic.gridPosition.x + currPatrol.x, basic.gridPosition.y + currPatrol.y);
                    basic.movingTo = GetDestination(basic.sprite.transform.position, movementVector.x, movementVector.y);
                    basic.isMoving = true;
                    basic.timeStartedLerp = Time.time;
                    basic.lerpStartPosition = basic.sprite.transform.position;

                    // handle animations
                    HandleRobotMoveAnimation(basic, currPatrol.x, currPatrol.y);

                    AudioSource botWalk = GameObject.Find(basic.obj.name + "/RobotWalk").GetComponent<AudioSource>();
                    botWalk.Play();

                    basic.patrolStage = (basic.patrolStage + 1) % basic.patrol.Count;
                }
            }

            HandleRobotAttack(basic);

            // check targeting
            if (!basic.targetingPlayer && CheckBotDetectionRange(basic))
            {
                basic.targetingPlayer = true;
                AudioSource threat = GameObject.Find(basic.obj.name + "/RobotThreatDetected").GetComponent<AudioSource>();
                threat.Play();
                
                if (player.isHidden)
                {
                    player.isHidden = false;
                    StartActionMusic();
                }
            }
        }

        // SUPER BOTS
        foreach (Robot super in superRobots)
        {
            List<Vector2Int> stepsToPlayer = ShortestPathTowardsPlayer(super.gridPosition.x, super.gridPosition.y);

            // player walked into bot
            if (stepsToPlayer.Count == 0)
            {
                StartCoroutine(PlayerKilled());
                return;
            }

            if (super.targetingPlayer)
            {
                Vector2Int firstStep = stepsToPlayer[0];

                Vector2Int movementVector = GridToBoard(firstStep);

                int cellMultiplier = 0;

                bool hitPlayer = false;

                List<Robot> friendlyFireBots = new List<Robot>();
                List<Robot> friendlyFireSupers = new List<Robot>();

                while (isValid(super.gridPosition.x + firstStep.x * (cellMultiplier), super.gridPosition.y + firstStep.y * (cellMultiplier+1)) &&
                    pathGrid[super.gridPosition.x + firstStep.x * (cellMultiplier), super.gridPosition.y + firstStep.y * (cellMultiplier+1)] == 1 &&
                    cellMultiplier < 2)
                {
                    cellMultiplier++;

                    if (player.gridPosition.x == super.gridPosition.x + firstStep.x * cellMultiplier 
                        && player.gridPosition.y == super.gridPosition.y + firstStep.y * cellMultiplier)
                    {
                        hitPlayer = true;
                    }

                    foreach(Robot r in robots)
                    {
                        if (r.gridPosition.x == super.gridPosition.x + firstStep.x * cellMultiplier 
                        && r.gridPosition.y == super.gridPosition.y + firstStep.y * cellMultiplier)
                        {
                            friendlyFireBots.Add(r);
                        }
                    }
                }

                //movement values
                super.gridPosition = new Vector2Int(super.gridPosition.x + cellMultiplier * firstStep.x, super.gridPosition.y + cellMultiplier * firstStep.y);
                super.movingTo = GetDestination(super.sprite.transform.position, cellMultiplier * movementVector.x, cellMultiplier * movementVector.y);
                super.isMoving = true;
                super.lerpStartPosition = super.sprite.transform.position;
                super.timeStartedLerp = Time.time;

                // handle animations
                HandleRobotMoveAnimation(super, firstStep.x, firstStep.y);

                AudioSource botWalk = GameObject.Find(super.obj.name + "/HeavyRobotWalk").GetComponent<AudioSource>();
                botWalk.Play();

                foreach(Robot r in friendlyFireBots)
                {
                    StartCoroutine(BotDestroyedBySuper(r));
                }

                foreach(Robot r in friendlyFireSupers)
                {
                    StartCoroutine(SuperBotDestroyedBySuper(r));
                }

                if (hitPlayer)
                {
                    // hit the player

                    // calculate animations
                    if (player.gridPosition.x > super.gridPosition.x && player.gridPosition.y > super.gridPosition.y)
                    {
                        super.animator.SetTrigger("RobotAttackUp");
                        super.sprite.flipX = false;
                    }
                    if (player.gridPosition.x < super.gridPosition.x && player.gridPosition.y > super.gridPosition.y)
                    {
                        super.animator.SetTrigger("RobotAttackUp");
                        super.sprite.flipX = true;
                    }
                    if (player.gridPosition.x > super.gridPosition.x && player.gridPosition.y < super.gridPosition.y)
                    {
                        super.animator.SetTrigger("RobotAttackDown");
                        super.sprite.flipX = false;
                    }
                    if (player.gridPosition.x < super.gridPosition.x && player.gridPosition.y < super.gridPosition.y)
                    {
                        super.animator.SetTrigger("RobotAttackDown");
                        super.sprite.flipX = true;
                    }

                    player.alive = false;
                    
                    StartCoroutine(PlayerKilled());
                    return;
                }
            }

            if (!super.targetingPlayer && CheckSuperBotDetectionRange(super))
            {
                super.targetingPlayer = true;
                AudioSource detected = GameObject.Find(super.obj.name + "/HeavyRobotThreatDetected").GetComponent<AudioSource>();
                detected.Play();

                if (player.isHidden)
                {
                    player.isHidden = false;
                    StartActionMusic();
                }
            }
        }
    }

    void HandleRobotMoveAnimation(Robot r, int xChange, int yChange)
    {
        // handle initial move direction
        if (xChange == 1 && yChange == 0)
        {
            r.animator.SetTrigger("RobotStepDown");
            r.sprite.flipX = false;
        }
        else if (xChange == -1 && yChange == 0)
        {
            r.animator.SetTrigger("RobotStepUp");
            r.sprite.flipX = true;
        }
        else if (xChange == 0 && yChange == -1)
        {
            r.animator.SetTrigger("RobotStepDown");
            r.sprite.flipX = true;
        }
        else if (xChange == 0 && yChange == 1)
        {
            r.animator.SetTrigger("RobotStepUp");
            r.sprite.flipX = false;
        }
    }

    void HandleRobotAttack(Robot r)
    {
        List<Vector2Int> pathToPlayer = ShortestPathTowardsPlayer(r.gridPosition.x, r.gridPosition.y);

        // stepped on player, leave animation as it is
        if (pathToPlayer.Count == 0)
        {
            player.alive = false;
            StartCoroutine(PlayerKilled());
            return;
        }

        // going to be outside our attack range when we move
        if (pathToPlayer.Count > 1)
        {
            return;
        }

        Vector2Int attackDirection = pathToPlayer[0];

        if (attackDirection.x == 1 && attackDirection.y == 0)
        {
            r.animator.SetTrigger("RobotAttackDown");
            r.sprite.flipX = false;
        }
        else if (attackDirection.x == 0 && attackDirection.y == 1)
        {
            r.animator.SetTrigger("RobotAttackUp");
            r.sprite.flipX = false;
        }
        else if (attackDirection.x == -1 && attackDirection.y == 0)
        {
            r.animator.SetTrigger("RobotAttackUp");
            r.sprite.flipX = true;
        }
        else if (attackDirection.x == 0 && attackDirection.y == -1)
        {
            r.animator.SetTrigger("RobotAttackDown");
            r.sprite.flipX = true;
        }

        player.alive = false;
        StartCoroutine(PlayerKilled());
    }

    Vector3 GetDestination(Vector3 start, int xChange, int yChange)
    {
        return new Vector3(start.x + xChange * xUnitLength, start.y + yChange * yUnitLength, start.z);
    }

    void UpdateSorting()
    {
        player.sprite.sortingOrder = -(int)(player.sprite.transform.position.y * 100);

        foreach( Robot r in robots)
        {
            r.sprite.sortingOrder = -(int)(r.sprite.transform.position.y * 100);
        }

        foreach(Robot r in superRobots)
        {
            r.sprite.sortingOrder = -(int)(r.sprite.transform.position.y * 100);
        }
    }

    IEnumerator PlayerKilled()
    {
        AudioSource squished = GameObject.Find(player.obj.name + "/RobotSqueeze").GetComponent<AudioSource>();
        squished.Play();
        yield return new WaitForSeconds(squished.clip.length);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    IEnumerator BotDestroyedByLaser(Robot r)
    {
        robots.Remove(r);
        AudioSource laserSound = GameObject.Find("LaserCharge").GetComponent<AudioSource>();
        yield return new WaitForSeconds(laserSound.clip.length);
        AudioSource destroyed = GameObject.Find(r.obj.name + "/RobotDestroyedExplosion").GetComponent<AudioSource>();
        destroyed.Play();
        Destroy(r.sprite);
        yield return new WaitForSeconds(destroyed.clip.length);
        Destroy(r.obj);
    }

    IEnumerator BotDestroyedBySuper(Robot r)
    {
        robots.Remove(r);
        AudioSource destroyed = GameObject.Find(r.obj.name + "/RobotDestroyedExplosion").GetComponent<AudioSource>();
        destroyed.Play();
        Destroy(r.sprite);
        yield return new WaitForSeconds(destroyed.clip.length);
        Destroy(r.obj);
    }

    IEnumerator SuperBotDestroyedByLaser(Robot r)
    {
        superRobots.Remove(r);
        AudioSource laserSound = GameObject.Find("LaserCharge").GetComponent<AudioSource>();
        yield return new WaitForSeconds(laserSound.clip.length);
        AudioSource destroyed = GameObject.Find(r.obj.name + "/HeavyRobotDestroyed").GetComponent<AudioSource>();
        destroyed.Play();
        Destroy(r.sprite);
        yield return new WaitForSeconds(destroyed.clip.length);
        Destroy(r.obj);
    }

    IEnumerator SuperBotDestroyedBySuper(Robot r)
    {
        superRobots.Remove(r);
        AudioSource destroyed = GameObject.Find(r.obj.name + "/HeavyRobotDestroyed").GetComponent<AudioSource>();
        destroyed.Play();
        Destroy(r.sprite);
        yield return new WaitForSeconds(destroyed.clip.length);
        Destroy(r.obj);
    }

    void UpdateAnimations()
    {
        if (player.isMoving)
        {
            // calculate lerp
            float timeSinceStart = Time.time - player.timeStartedLerp;
            float percentageComplete = timeSinceStart / player.lerpLength;

            if (player.sprite.transform.position == player.movingTo)
            {
                player.isMoving = false;
            }
            else
            {
                player.sprite.transform.position = Vector3.Lerp(player.lerpStartPosition, player.movingTo, percentageComplete);
            }
        }

        foreach(Robot robot in robots)
        {
            if (robot.isMoving)
            {
                // calculate lerp
                float timeSinceStart = Time.time - robot.timeStartedLerp;
                float percentageComplete = timeSinceStart / robot.lerpLength;
                

                if (robot.obj.transform.position == robot.movingTo)
                {
                    robot.isMoving = false;
                }
                else
                {
                    robot.obj.transform.position = Vector3.Lerp(robot.lerpStartPosition, robot.movingTo, percentageComplete);
                }
            }
        }

        foreach(Robot robot in superRobots)
        {
            if (robot.isMoving)
            {
                // calculate lerp
                float timeSinceStart = Time.time - robot.timeStartedLerp;
                float percentageComplete = timeSinceStart / robot.lerpLength;
                
                if (robot.obj.transform.position == robot.movingTo)
                {
                    robot.isMoving = false;
                }
                else
                {
                    robot.obj.transform.position = Vector3.Lerp(robot.lerpStartPosition, robot.movingTo, percentageComplete);
                }
            }
        }
    }

    Vector2Int GridToBoard(Vector2Int vector)
    {
        if (vector.x == 1 && vector.y == 0)
        {
            return new Vector2Int(1, -1);
        }
        else if (vector.x == 0 && vector.y == 1)
        {
            return new Vector2Int(1, 1);
        }
        else if (vector.x == -1 && vector.y == 0)
        {
            return new Vector2Int(-1, 1);
        }
        else
        {
            return new Vector2Int(-1, -1);
        }
    }

    int[] rowNum = {-1, 0, 0, 1};
    int[] colNum = {0, -1, 1, 0};

    List<Vector2Int> ShortestPathTowardsPlayer(int xStart, int yStart)
    {
        List<Vector2Int> shortestList = null;

        // default the visited array
        int[,] visited = new int[8,8];

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                visited[i,j] = 0;
            }
        }

        visited[xStart, yStart] = 1;

        Queue<QueueNode> q = new Queue<QueueNode>();

        QueueNode s = new QueueNode() {x = xStart, y = yStart, steps = new List<Vector2Int>()};
        // start it off
        q.Enqueue(s);

        while (q.Count != 0)
        {
            QueueNode curr = q.Dequeue();
            int x = curr.x;
            int y = curr.y;

            // found player
            if (x == player.gridPosition.x && y == player.gridPosition.y)
            {
                // check if shorter list
                if (shortestList == null || curr.steps.Count < shortestList.Count )
                {
                    shortestList = new List<Vector2Int>();
                    foreach(Vector2Int step in curr.steps)
                    {
                        shortestList.Add(step);
                    }
                }
            }
            // did not find player
            else 
            {
                for (int i = 0; i < 4; i++)
                {
                    int row  = x + rowNum[i];
                    int col = y + colNum[i];

                    if (isValid(row, col) && pathGrid[row, col] == 1 && visited[row, col] == 0)
                    {
                        visited[row,col] = 1;

                        QueueNode adj = new QueueNode();
                        adj.x = row;
                        adj.y = col;
                        
                        adj.steps = new List<Vector2Int>();

                        foreach(Vector2Int cell in curr.steps)
                        {
                            adj.steps.Add(cell);
                        }

                        adj.steps.Add(new Vector2Int(rowNum[i], colNum[i]));

                        q.Enqueue(adj);
                    }
                }
            }
        }

        return shortestList;
    }

    bool CheckBotDetectionRange(Robot r)
    {
        for (int i = 0; i < 3; i++)
        {
            if (player.gridPosition == new Vector2Int(r.gridPosition.x, r.gridPosition.y + i))
            {
                return true;
            }

            if (player.gridPosition == new Vector2Int(r.gridPosition.x + i, r.gridPosition.y))
            {
                return true;
            }

            if (player.gridPosition == new Vector2Int(r.gridPosition.x, r.gridPosition.y - i))
            {
                return true;
            }

            if (player.gridPosition == new Vector2Int(r.gridPosition.x - i, r.gridPosition.y))
            {
                return true;
            }
        }

        return false;
    }

    bool CheckSuperBotDetectionRange(Robot r)
    {
        for (int i = 0; i < 5; i++)
        {
            if (player.gridPosition == new Vector2Int(r.gridPosition.x, r.gridPosition.y + i))
            {
                return true;
            }

            if (player.gridPosition == new Vector2Int(r.gridPosition.x + i, r.gridPosition.y))
            {
                return true;
            }

            if (player.gridPosition == new Vector2Int(r.gridPosition.x, r.gridPosition.y - i))
            {
                return true;
            }

            if (player.gridPosition == new Vector2Int(r.gridPosition.x - i, r.gridPosition.y))
            {
                return true;
            }
        }

        return false;
    }

    bool isValid(int row, int col)
    {
        return (row >= 0) && (col >= 0) && (row < 8) && (col < 8); 
    }

}

public class QueueNode
{
    public int x {get; set;}
    public int y {get;set;}
    public List<Vector2Int> steps {get; set;}
}