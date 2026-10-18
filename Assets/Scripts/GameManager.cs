using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    Unity.Mathematics.Random prng = new Unity.Mathematics.Random((uint)DateTime.Now.Second);

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
    List<GameObject> leftEmitters;
    List<GameObject> rightEmitters;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        emitters = new List<GameObject>();

        leftEmitters = new List<GameObject>();

        rightEmitters = new List<GameObject>();
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
    }


    public void Enqueue(TONE tone)
    {
        Game.Instance.Enqueue(tone);
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
            } else
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
                bool isChoiceLeft = (totalLeft < distribution.Length / 2) && isLeft;
                if (isChoiceLeft)
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
                emitter.SetRepititions(repetitions);
                emitters.Add(emitter.gameObject);
                if (isChoiceLeft)
                {
                    leftEmitters.Add(emitter.gameObject);
                }
                else
                {
                    rightEmitters.Add(emitter.gameObject);
                }
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
        var middleWall = GameObject.FindWithTag("middle_wall");
        float nearestDistance = 0.0f;
        bool isLeftPlayer = middleWall.transform.position.x > t.position.x;
        var emittersToCheck = isLeftPlayer ? leftEmitters : rightEmitters;
        foreach (var e in emittersToCheck) {
            var diff = t.position.x - e.transform.position.x;

            //var pos = transform.position;
            //pos.y ;
            //var leftCast = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y), Vector2.left);
            //var rightCast = Physics2D.Raycast(transform.position, Vector2.right);
            //var leftO = leftCast.collider.gameObject;
            //var rightO = rightCast.collider.gameObject;

            if (nearestEmitter == null || System.Math.Abs(nearestDistance) > System.Math.Abs(diff))
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
        Game.Instance.CheckAnswer();
    }

    public void ListenToSubmission() { 
        Game.Instance.PlaySubmission();
    }

    public void Reset()
    {
        Game.Instance.ResetAnswer();
    }
}
