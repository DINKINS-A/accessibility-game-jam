using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Seed must be non-zero for Unity.Mathematics.Random.
    private Unity.Mathematics.Random prng = new Unity.Mathematics.Random(MakeSeed());

    private static uint MakeSeed()
    {
        uint seed = (uint)Environment.TickCount;
        return seed == 0 ? 1u : seed;
    }

    [SerializeField] public Emitter emitterPrefab;

    private bool PlayerWon = false;
    private bool transitioning = false;
    private float Delay = 5f;

    public static GameManager Instance { get; private set; }

    // Initialised inline so they are never null, even on a duplicate instance
    // that is about to be destroyed.
    private readonly List<GameObject> emitters = new List<GameObject>();
    private readonly List<GameObject> leftEmitters = new List<GameObject>();
    private readonly List<GameObject> rightEmitters = new List<GameObject>();

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        Debug.Log("START WORKING");
    }
    void Update()
    {
        // Only start the transition once per level.

        Game.Instance.CheckWin();
        //{
        //    Debug.Log("UPDATE WORKING3");
        //    PlayerWon = true;
        //    Debug.Log("UPDATE WORKING4");
        //    TransitionTONext();
        //    Debug.Log("UPDATE WORKING5");
        //    Debug.Log("player won");
          
        //}
    }

    public void TransitionTONext()
    {
        if (transitioning) return;
        transitioning = true;
        StartCoroutine(Scenetransition());
    }

    IEnumerator Scenetransition()
    {
        yield return new WaitForSeconds(Delay);

        int next = SceneManager.GetActiveScene().buildIndex + 1;
        if (next < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(next);
        }
        else
        {
            SceneManager.LoadScene("menu");
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Enqueue(TONE tone)
    {
        Game.Instance.Enqueue(tone);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Ignore duplicate instances that are being destroyed.
        if (Instance != this) return;

        // This object persists across scenes, so reset per-level state here.
        transitioning = false;
        PlayerWon = false;

        LoadLevel(scene);
    }

    private void ClearEmitters()
    {
        emitters.Clear();
        leftEmitters.Clear();
        rightEmitters.Clear();
    }

    public void LoadLevel(UnityEngine.SceneManagement.Scene scene)
    {
        //SceneManager.LoadScene("Levels/" + levelId);
        TextAsset file = Resources.Load<TextAsset>($"{scene.name}");
        List<string> answerConfig = null;
        string answer = null;
        string distribution = null;

        if (file != null)
        {
            var lines = file.text.Split(
                new[] { "\r\n", "\n", "\r" },
                StringSplitOptions.None
            ).ToList();

            foreach (string line in lines)
            {
                Debug.Log(line);
            }

            answerConfig = lines;
            if (answerConfig.Count != 2)
            {
                Debug.LogError($"Expected 2 lines file: {scene.name}");
            }
            else
            {
                answer = answerConfig[0];
                distribution = answerConfig[1];
            }
            Debug.Log(answerConfig);
        }
        else
        {
            Debug.LogError($"{scene.name} textfile with pattern / answer not found");
        }
        GameObject obj = GameObject.FindWithTag("Ground");

        int totalEmitters = 0;

        if (answerConfig != null && distribution != null)
        {
            var pattern = answerConfig[0];
            Game.Instance.SetAnswer(Game.Instance.ParseAnswerLine(pattern));

            totalEmitters = distribution.Length;
        }


        if (obj != null)
        {
            var emitterCollider = obj.GetComponent<Collider2D>();
            // dont fly away
            Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.constraints |= RigidbodyConstraints2D.FreezePositionY;
            }
            Bounds bounds = emitterCollider.bounds;

            float leftBound = bounds.min.x;
            float rightBound = bounds.max.x;


            float start = bounds.min.x + 1;
            // half size
            float xExtent = bounds.extents.x;
            float xmax = start + xExtent * 2;

            var totalLeft = new List<char>();
            var totalRight = new List<char>();
            // determine left / right dist
            foreach (var c in distribution)
            {
                var isLeft = prng.NextBool();
                bool isForceLeft = totalRight.Count >= distribution.Length / 2;
                bool isForceRight = totalLeft.Count >= distribution.Length / 2;
                if (isForceLeft || (isLeft && !isForceRight))
                {
                    // forse spawn on the right side
                    totalLeft.Add(c);
                }
                else
                {
                    // forse spawn on the right side
                    totalRight.Add(c);
                }
            }

            float deltaLeft = xExtent / totalLeft.Count();

            // offset from left border
            float spawnDeltaLeft = 2;
            // spawn evenly distributed left
            for (int i = 0; i < totalLeft.Count(); i++)
            {
                var c = totalLeft[i];

                // offset from right border
                if (i == totalLeft.Count() - 1)
                {
                    spawnDeltaLeft -= 2;
                }
                // to keep balance
                Vector3 spawnPosition = new Vector3(
                    start + spawnDeltaLeft,
                    // offset y a bit
                    bounds.max.y,
                    0f
                );
                var emitter = Instantiate<Emitter>(
                    emitterPrefab,
                    spawnPosition,
                    Quaternion.identity
                );

                int repetitions = c - '0';
                emitter.SetRepititions(repetitions);
                emitters.Add(emitter.gameObject);
                leftEmitters.Add(emitter.gameObject);
                float emitterHalfHeight = emitter.GetComponent<Collider2D>().bounds.extents.y / 2;
                emitter.transform.Translate(0, emitterHalfHeight + 0.5f, 0);

                spawnDeltaLeft += deltaLeft;
            }

            float deltaRight = xExtent / totalRight.Count();
            // offset from border
            float spawnDeltaRight = 2 + xExtent;
            // spawn evenly distributed left
            for (int i = 0; i < totalRight.Count(); i++)
            {
                var c = totalRight[i];

                // offset from right border
                if (i == totalRight.Count() - 1)
                {
                    spawnDeltaRight -= 2;
                }
                // to keep balance
                Vector3 spawnPosition = new Vector3(
                    start + spawnDeltaRight,
                    bounds.max.y,
                    0f
                );
                var emitter = Instantiate<Emitter>(
                    emitterPrefab,
                    spawnPosition,
                    Quaternion.identity
                );

                int repetitions = c - '0';
                emitter.SetRepititions(repetitions);
                emitters.Add(emitter.gameObject);
                rightEmitters.Add(emitter.gameObject);
                float emitterHalfHeight = emitter.GetComponent<Collider2D>().bounds.extents.y / 2;
                // offset y
                emitter.transform.Translate(0, emitterHalfHeight + 0.5f, 0);

                spawnDeltaRight += deltaRight;
            }
        }
        else
        {
            Debug.LogError("Not found Ground GameObject with tag 'Ground'. Add tag to detect Emitter spawn area.");
        }
    }

    /// <summary>
    /// Returns the nearest emitter on the same side of the middle wall as t,
    /// or null if there is none. Callers must handle null.
    /// </summary>
    public GameObject getNearestEmitter(Transform t)
    {
        if (t == null) return null;

        var middleWall = GameObject.FindWithTag("middle_wall");
        if (middleWall == null) return null;

        bool isLeftPlayer = middleWall.transform.position.x > t.position.x;
        var emittersToCheck = isLeftPlayer ? leftEmitters : rightEmitters;

        GameObject nearest = null;
        float nearestDistance = float.MaxValue;

        for (int i = emittersToCheck.Count - 1; i >= 0; i--)
        {
            var e = emittersToCheck[i];

            // Unity's == null also catches destroyed objects.
            if (e == null)
            {
                emittersToCheck.RemoveAt(i);
                continue;
            }

            float dist = Mathf.Abs(t.position.x - e.transform.position.x);
            if (dist < nearestDistance)
            {
                nearestDistance = dist;
                nearest = e;
            }
        }
        return nearest;
    }

    public void Test()
    {
        Game.Instance.CheckAnswer();
    }

    public void ListenToSubmission()
    {
        Game.Instance.PlaySubmission();
    }

    public void Reset()
    {
        Game.Instance.ResetAnswer();
    }
}