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

    public void LoadLevel(Scene scene)
    {
        // The previous scene's emitters are already destroyed. Drop the stale references.
        ClearEmitters();
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

        int totalEmitters = 0;

        if (answerConfig != null && distribution != null)
        {
            var pattern = answerConfig[0];
            Game.Instance.SetAnswer(Game.Instance.ParseAnswerLine(pattern));

            totalEmitters = distribution.Length;
        }

        GameObject ground = GameObject.FindWithTag("Ground");
        if (ground == null)
        {
            Debug.LogError("Not found Ground GameObject with tag 'Ground'. Add tag to detect Emitter spawn area.");
            return;
        }

        var groundCollider = ground.GetComponent<Collider2D>();
        if (groundCollider == null)
        {
            Debug.LogError("Ground GameObject has no Collider2D.");
            return;
        }

        // don't fly away
        Rigidbody2D rb = ground.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.constraints |= RigidbodyConstraints2D.FreezePositionY;
        }

        Bounds bounds = groundCollider.bounds;

        var leftSlots = new List<float>();
        var rightSlots = new List<float>();
        const int emitterExtent = 2;

        float start = bounds.min.x + 1;
        float xExtent = bounds.extents.x; // half size

        for (float i = 0; i < xExtent; i += emitterExtent)
        {
            leftSlots.Add(i);
            rightSlots.Add(i);
        }

        int totalLeft = 0;
        const float deltaY = 0.5f;

        foreach (char c in distribution)
        {
            int repetitions = c - '0';

            if (leftSlots.Count == 0 && rightSlots.Count == 0)
            {
                Debug.LogError("More emitters requested than available slots. Stopping spawn.");
                break;
            }

            bool wantLeft = prng.NextBool() && totalLeft < distribution.Length / 2;
            bool leftAvailable = leftSlots.Count > 0;
            bool rightAvailable = rightSlots.Count > 0;

            // Use the preferred side, falling back to the other if it has no free slots.
            bool isChoiceLeft = (wantLeft && leftAvailable) || !rightAvailable;

            float deltaX;
            if (isChoiceLeft)
            {
                int r = prng.NextInt(0, leftSlots.Count); // upper bound is exclusive
                deltaX = leftSlots[r];
                leftSlots.RemoveAt(r);
                totalLeft++;
            }
            else
            {
                int r = prng.NextInt(0, rightSlots.Count);
                deltaX = xExtent + rightSlots[r];
                rightSlots.RemoveAt(r);
            }

            Vector3 spawnPosition = new Vector3(
                start + deltaX,
                bounds.max.y + deltaY,
                0f
            );

            Emitter emitter = Instantiate(emitterPrefab, spawnPosition, Quaternion.identity);
            emitter.SetRepititions(repetitions);

            emitters.Add(emitter.gameObject);
            if (isChoiceLeft)
                leftEmitters.Add(emitter.gameObject);
            else
                rightEmitters.Add(emitter.gameObject);

            float emitterHalfHeight = emitter.GetComponent<Collider2D>().bounds.extents.y / 2;
            emitter.transform.Translate(0, emitterHalfHeight, 0);
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