using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class LevelTwo : Level
{

    public Vector2Int playerGridStart;

    public Vector2Int robotOneGridStart;

    public Vector2Int robotTwoGridStart;

    public Vector2Int robotThreeGridStart;

    public Vector2Int superRobotOneGridStart;

    public Vector2Int superRobotTwoGridStart;

    public List<Vector2Int> robotOnePatrol;
    public int robotOnePatrolStart;

    public List<Vector2Int> robotTwoPatrol;
    public int robotTwoPatrolStart;

    public List<Vector2Int> robotThreePatrol;
    public int robotThreePatrolStart;

    public override void Start()
    {
        cam = Camera.main;

        ambientBgm = GameObject.Find("LevelTwoAmbientBgm").GetComponent<AudioSource>();
        actionBgm = GameObject.Find("LevelTwoActionBgm").GetComponent<AudioSource>();

        ambientBgm.Play();

        player = new Player();

        robots = new List<Robot>();

        superRobots = new List<Robot>();

        player = new Player();
        player.obj = GameObject.Find("Player");
        player.gridStartPosition = playerGridStart;
        player.lerpLength = 0.3f;

        Robot robotOne = new Robot();
        robotOne.obj = GameObject.Find("RobotOne");
        robotOne.gridStartPosition = robotOneGridStart;
        robotOne.lerpLength = 0.3f;
        robotOne.patrol = robotOnePatrol;
        robotOne.patrolStage = robotOnePatrolStart;

        Robot robotTwo = new Robot();
        robotTwo.obj = GameObject.Find("RobotTwo");
        robotTwo.gridStartPosition = robotTwoGridStart;
        robotTwo.lerpLength = 0.3f;
        robotTwo.patrol = robotTwoPatrol;
        robotTwo.patrolStage = robotTwoPatrolStart;

        Robot robotThree = new Robot();
        robotThree.obj = GameObject.Find("RobotThree");
        robotThree.gridStartPosition = robotThreeGridStart;
        robotThree.lerpLength = 0.3f;
        robotThree.patrol = robotThreePatrol;
        robotThree.patrolStage = robotThreePatrolStart;

        robots.Add(robotOne);
        robots.Add(robotTwo);
        robots.Add(robotThree);

        Robot superRobotOne = new Robot();
        superRobotOne.obj = GameObject.Find("SuperRobotOne");
        superRobotOne.gridStartPosition = superRobotOneGridStart;
        superRobotOne.lerpLength = 0.3f;

        Robot superRobotTwo = new Robot();
        superRobotTwo.obj = GameObject.Find("SuperRobotTwo");
        superRobotTwo.gridStartPosition = superRobotTwoGridStart;
        superRobotTwo.lerpLength = 0.3f;

        superRobots.Add(superRobotOne);
        superRobots.Add(superRobotTwo);

        pathGrid = new int[,]
        {
            {0, 0, 0, 0, 0, 0, 1, 0},
            {0, 0, 0, 1, 1, 1, 1, 0},
            {0, 0, 0, 1, 0, 0, 1, 0},
            {0, 1, 1, 1, 1, 1, 1, 0},
            {0, 1, 0, 1, 0, 0, 0, 0},
            {1, 1, 1, 1, 1, 0, 0, 0},
            {1, 0, 0, 1, 0, 0, 0, 0},
            {1, 1, 1, 1, 1, 0, 0, 0}
        };

        laser = new Laser();
        laser.position = new Vector2Int(-1,3);

        objectives.Add(new Vector2Int(0,6));

        Setup();
    }

    public override void CheckLevelComplete()
    {
        if (robotsLeft == 0 && player.gridPosition.x == 0 && player.gridPosition.y == 6)
        {
            SceneManager.LoadScene("Outro");
        }
    }
}