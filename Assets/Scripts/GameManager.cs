using System;
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

        string path = Path.Combine(
            Application.dataPath,
            $"Levels/{scene.name}/answer.txt"
        );

        // Scenes without an answer file (e.g. the menu) are not levels.
        if (!File.Exists(path))
        {
            Debug.Log($"No answer file for scene '{scene.name}' ({path}). Skipping level setup.");
            return;
        }

        List<string> answerConfig = File.ReadLines(path).ToList();
        if (answerConfig.Count != 2)
        {
            Debug.LogError($"Expected 2 lines in file: {path}");
            return;
        }

        string pattern = answerConfig[0];
        string distribution = answerConfig[1];

        Game.Instance.SetAnswer(Game.Instance.ParseAnswerLine(pattern));

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

            // Fall back to the other side if the chosen side has no free slots.
            bool isChoiceLeft = wantLeft ? leftSlots.Count > 0 : rightSlots.Count == 0;

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