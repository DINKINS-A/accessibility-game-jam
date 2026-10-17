using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    Game game;

    Unity.Mathematics.Random prng = new Unity.Mathematics.Random(1);

    //[System.Serializable]
    //public class ToneMapping
    //{
    //    public int toneId;
    //    public AudioSource audioSource;
    //}

    //[SerializeField] private ToneMapping[] toneMappings;

    //private Dictionary<int, AudioSource> toneAudioMap;

    [SerializeField] public Emitter emitterPrefab;

    public static GameManager Instance { get; private set; }

    List<GameObject> emitters;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        game = new Game();
        emitters = new List<GameObject>();
        //toneAudioMap = new Dictionary<int, AudioSource>();

        //foreach (ToneMapping mapping in toneMappings)
        //{
        //    if (mapping.audioSource != null)
        //        toneAudioMap[mapping.toneId] = mapping.audioSource;
        //}
        DontDestroyOnLoad(gameObject);
    }


    public void Enqueue(TONE tone)
    {
        game.Enqueue(tone);
    }
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
    {
        LoadLevel(scene);
    }

    public void LoadLevel(UnityEngine.SceneManagement.Scene scene)
    {
        //SceneManager.LoadScene("Levels/" + levelId);
        string path = Path.Combine(
            Application.dataPath,
            $"Levels/{scene.name}/answer.txt"
        );
        List<string> answerConfig = null;
        string answer = null;
        string distribution = null;
        if (File.Exists(path))
        {
            answerConfig = File.ReadLines(path).ToList();
            if (answerConfig.Count != 2)
            {
                Debug.LogError($"Expected 2 lines file: {path}");
            } else
            {
                answer = answerConfig[0];
                distribution = answerConfig[1];
            }
            Debug.Log(answerConfig);
        }
        else
        {
            Debug.LogError($"CSV not found: {path}");
        }
        GameObject obj = GameObject.FindWithTag("Ground");

        int totalEmitters = 0;

        if (answerConfig != null && distribution != null)
        {
            var pattern = answerConfig[0];
            game.SetAnswer(game.ParseAnswerLine(pattern));

            totalEmitters = distribution.Length;
        }

        var totalLeft = 0;
        var totalRight = 0;

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

            List<float> leftSlots = new List<float>();
            List<float> rightSlots = new List<float>();
            var emitterExtent = 2;


            float start = bounds.min.x + 1;
            // half size
            float xExtent = bounds.extents.x;
            float xmax = start + xExtent * 2;
            for (float i = 0; i < xExtent; i += emitterExtent)
            {
                leftSlots.Add(i);
                rightSlots.Add(i);
            }

            foreach (var c in distribution)
            {
                int repetitions = c - '0';

                // to keep balance
                

                Vector3 spawnPosition = new Vector3(
                    start,
                    bounds.max.y,
                    0f
                );

                var isLeft = prng.NextBool();
                float deltaY = 0.5f;
                float deltaX = 2;

                if (totalLeft < distribution.Length / 2 && isLeft)
                {
                    // forse spawn on the right side
                    var r = prng.NextInt(0, leftSlots.Count - 1);
                    deltaX = leftSlots[r];
                    leftSlots.RemoveAt(r);
                    totalLeft++;
                } else
                {
                    // forse spawn on the left side
                    var r = prng.NextInt(0, rightSlots.Count - 1);
                    deltaX = xExtent + rightSlots[r];
                    rightSlots.RemoveAt(r);
                    totalRight++;
                }
                spawnPosition.y += deltaY;
                spawnPosition.x += deltaX;
                var emitter = Instantiate<Emitter>(
                    emitterPrefab,
                    spawnPosition,
                    Quaternion.identity
                );
                emitter.SetRepititions((short)repetitions);
                emitters.Add(emitter.gameObject);
                float emitterHalfHeight = emitter.GetComponent<Collider2D>().bounds.extents.y / 2;
                emitter.transform.Translate(0, emitterHalfHeight, 0);

            }
        } else
        {   
            Debug.LogError("Not found Ground GameObject with tag 'Ground'. Add tag to detect Emitter spawn area.");
        }
    }

    public GameObject getNearestEmitter(Transform t)
    {
        GameObject nearestEmitter = null;
        float nearestDistance = 0.0f;
        foreach (var e in emitters) {
            var diff = t.position.x - e.transform.position.x;
            if (nearestEmitter == null || (nearestDistance < 0 ? -nearestDistance : nearestDistance) > (diff < 0 ? -diff : diff))
            {
                nearestDistance = diff;
                nearestEmitter = e;
            }
            ;
        }
        return nearestEmitter;
    }

    public void Test()
    {
        game.CheckAnswer();
    }

    public void Reset()
    {
        game.ResetAnswer();
    }
}
