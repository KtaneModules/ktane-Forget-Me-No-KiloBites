using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.Random;
using static UnityEngine.Debug;

public class ForgetMeNoScript : MonoBehaviour 
{

	public KMBombInfo Bomb;
	public KMAudio Audio;
	public KMBombModule Module;
	public KMBossModule Boss;

	public KMSelectable[] buttons;
    public MeshRenderer[] buttonLeds;

	public TextMesh mainDisplay, mainDisplayBig, stageDisplay;

	static int moduleIdCounter = 1;
	int moduleId;
	private bool moduleSolved;
    private bool isActivated, readyToSubmit;

    private int currentStage = 0, stageCount, enteredStages = 0;

	private static string[] ignoredModules;

    private List<Stage> stages;
    private ConstantGenerator generator;
    private List<Constant> assignedConstants;
    private StageGenerator stageGenerator;
    
    private List<int> answers;

    private Coroutine[] buttonAnims = new Coroutine[10];

    private enum SolveType
    {
        Regular,
        Accelerator,
        SlowStart,
        Triplets
    }

	void Awake()
    {

		moduleId = moduleIdCounter++;

        foreach (KMSelectable button in buttons)
            button.OnInteract += delegate () { ButtonPress(button); return false; };

        Module.OnActivate += Activate;

        mainDisplay.text = string.Empty;
        stageDisplay.text = string.Empty;
        mainDisplayBig.text = string.Empty;

    }

	
	void Start()
    {
        StartCoroutine(Init());
    }

    void Activate()
    {
        isActivated = true;

        if (stageCount == 0)
            return;

        DisplayStage();
    }

    IEnumerator Init()
    {
        yield return null;

        if (ignoredModules == null)
            ignoredModules = Boss.GetIgnoredModules("Forget Me No.", new[]
            {
                "14",
                "8",
                "Forget Enigma",
                "Forget Everything",
                "Forget It Not",
                "Forget Me Later",
                "Forget Me Not",
                "Forget Perspective",
                "Forget Them All",
                "Forget This",
                "Forget Us Not",
                "Forget Me No.",
                "Organization",
                "Purgatory",
                "Simon's Stages",
                "Souvenir",
                "Tallordered Keys",
                "The Time Keeper",
                "Timing is Everything",
                "The Troll",
                "Turn The Key",
                "Übermodule",
                "Ültimate Custom Night",
                "The Very Annoying Button"
            });

        stageCount = Bomb.GetSolvableModuleNames().Count(x => !ignoredModules.Contains(x));

        if (stageCount == 0)
        {
            Log($"[Forget Me No. #{moduleId}] No non-ignored modules were detected. Solving...");
            moduleSolved = true;
            stageDisplay.text = "--";
            Module.HandlePass();
            yield break;
        }

        generator = new ConstantGenerator(Bomb);
        assignedConstants = generator.Constants;
        stageGenerator = new StageGenerator(assignedConstants, stageCount);
        stages = stageGenerator.Stages;
        answers = stageGenerator.CalculatedStages;

        Log($"[Forget Me No. #{moduleId}] Constants assigned to each LED from 1 to 0 are as follows: {assignedConstants.Select(x => "π,e,√2,ln2,φ,γ,ρ,δ,λ,W(1)".Split(',')[(int)x.Type]).Join(", ")}");
        Log($"[Forget Me No. #{moduleId}] The following condition to determine the remaining constants is: {generator}");


        for (int i = 0; i < stageCount; i++)
            Log($"[Forget Me No. #{moduleId}] Stage {i + 1}: {stages[i].GeneratedValue}, LED {(assignedConstants.FindIndex(x => stages[i].SelectedConstant.Type == x.Type) + 1) % 10}");

        Log($"[Forget Me No. #{moduleId}] The final answer in groups of 3 is: {answers.Select((x, i) => new { Index = i, Value = x }).GroupBy(x => x.Index / 3).Select(x => x.Select(v => v.Value).Join("")).Join()}");

    }

    void ButtonPress(KMSelectable button)
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, button.transform);
        button.AddInteractionPunch(0.3f);

        var ix = Array.IndexOf(buttons, button);

        if (buttonAnims[ix] != null)
            StopCoroutine(buttonAnims[ix]);

        buttonAnims[ix] = StartCoroutine(ButtonAnimation(ix));

        if (moduleSolved || !isActivated)
            return;

        if (!readyToSubmit)
        {
            Log($"[Forget Me No. #{moduleId}] The module isn't ready to be submitted yet. Strike!");
            Module.HandleStrike();
        }

        var number = int.Parse(button.GetComponentInChildren<TextMesh>().text);

        if (number == answers[enteredStages])
        {
            enteredStages++;

            stageDisplay.text = "--";
            mainDisplayBig.text = string.Empty;

            DisplayInputScreen();

            if (enteredStages == stageCount)
            {
                Log($"[Forget Me No. #{moduleId}] All digits have been entered correctly. Solved!");
                moduleSolved = true;
                Module.HandlePass();
            }
        }
        else
        {
            Log($"[Forget Me No. #{moduleId}] The digit expected for stage {enteredStages + 1} is {answers[enteredStages]}, but inputted {number} instead. Strike!");
            Module.HandleStrike();
            mainDisplay.text = string.Empty;
            DisplayStage(enteredStages);
        }
    }

    void DisplayStage(int? stageToRecover = null)
    {
        var ledIx = assignedConstants.FindIndex(x => stages[stageToRecover ?? currentStage].SelectedConstant.Type == x.Type);
        mainDisplayBig.text = stages[stageToRecover ?? currentStage].GeneratedValue.ToString();

        var stage = ((stageToRecover ?? currentStage) + 1) % 100;

        stageDisplay.text = stage.ToString().PadLeft(2, '0');

        for (int i = 0; i < 10; i++)
            buttonLeds[i].material.color = i == ledIx ? Color.green : Color.black;
    }

    void InitializeSubmission()
    {
        for (int i = 0; i < 10; i++)
            buttonLeds[i].material.color = Color.black;

        mainDisplayBig.text = string.Empty;
        stageDisplay.text = "--";

        DisplayInputScreen();
    }

    void DisplayInputScreen()
    {
        var currentStage = enteredStages;
        var startingStage = 0;
        var finalStage = stageCount;

        var inputText = string.Empty;

        while (currentStage > 23 && finalStage != 24)
        {
            currentStage -= 12;
            finalStage -= 12;
            startingStage += 12;
        }

        for (int i = startingStage; i < Math.Min(startingStage + 24, stageCount); i++)
        {
            var digit = "-";

            if (i < enteredStages)
                digit = answers[i].ToString();

            if (i > startingStage)
                if (i % 3 == 0)
                    inputText += i % 12 == 0 ? "\n" : " ";

            inputText += digit;
        }

        mainDisplay.text = inputText;
    }

    IEnumerator ButtonAnimation(int pos)
    {
        var start = 0.0175f;
        var end = 0.015f;
        var elapsed = 0f;
        var duration = 0.075f;

        var orig = buttons[pos].transform.localPosition;

        buttons[pos].transform.localPosition = new Vector3(orig.x, start, orig.z);

        while (elapsed < duration)
        {
            yield return null;
            elapsed += Time.deltaTime;

            buttons[pos].transform.localPosition = new Vector3(orig.x, Mathf.Lerp(start, end, elapsed / duration), orig.z);
        }

        buttons[pos].transform.localPosition = new Vector3(orig.x, end, orig.z);

        elapsed = 0f;

        while (elapsed < duration)
        {
            yield return null;
            elapsed += Time.deltaTime;

            buttons[pos].transform.localPosition = new Vector3(orig.x, Mathf.Lerp(end, start, elapsed / duration), orig.z);
        }

        buttons[pos].transform.localPosition = new Vector3(orig.x, start, orig.z);

    }
	
	
	void Update()
    {
        if (moduleSolved || !isActivated || readyToSubmit)
            return;

        var solved = Bomb.GetSolvedModuleNames().Count(x => !ignoredModules.Contains(x));

        if (solved == stageCount)
        {
            readyToSubmit = true;
            InitializeSubmission();
            return;
        }

        if (solved > currentStage)
        {
            currentStage++;

            DisplayStage();
        }
    }

	// Twitch Plays


#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"!{0} press/submit 1234 [to submit your sequence]. The sequence length depends on how many modules were on the bomb. You may use spaces in the digit sequence";
#pragma warning restore 414

    private SolveType DetermineSolveType(int dlen, int slen)
    {
        if (dlen > slen)
            dlen = slen;

        if (dlen > 12 && value > 0.9)
            return SolveType.SlowStart;

        if (dlen > 4 && value > 0.75)
            return SolveType.Accelerator;

        if (value > 0.75)
            return SolveType.Triplets;

        return SolveType.Regular;
    }

    private bool GetMusicToggle(SolveType type, int curPos)
    {
        if (type == SolveType.SlowStart)
            return curPos == 1 || curPos == 12;

        return false;
    }

    private float GetDelay(SolveType type, int curPos, int slen, float time)
    {
        var allowance = (time / 0.05f) / slen;

        if (allowance < 0.05f)
            return allowance;

        switch (type)
        {
            case SolveType.SlowStart:
                if (curPos < 8)
                    return 0.5f + value * 2.5f;
                return 0.05f;
            case SolveType.Accelerator:
                return Math.Max(3f / (curPos + 1), 0.05f);
            case SolveType.Triplets:
                if (curPos % 3 == 0)
                    return 0.25f;
                return 0.05f;
        }


        return 0.05f;
    }

    private int GetDigit(char c) => "1234567890".IndexOf(c);

    private static readonly string[] placeNames =
    {
        "reddit",
        "discord",
        "twitch chat",
        "mom",
        "dad"
    };

    private bool firstTry = true;

	IEnumerator ProcessTwitchCommand(string command)
    {
		string[] split = command.ToUpperInvariant().Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
		yield return null;

        if (new[] { "PRESS", "SUBMIT" }.Any(x => x == split[0]))
        {
            if (split.Length == 1)
            {
                yield return "sendtochaterror Press specify what digits to press!";
                yield break;
            }

            var digits = new List<int>();

            foreach (var str in split.Skip(1).ToArray())
            {
                foreach (var c in str)
                {
                    if (!"0123456789 ".Contains(c))
                    {
                        yield return $"sendtochaterror Invalid character in sequence: '{c}'. Valid characters are 0-9, and space.";
                        yield break;
                    }

                    var d = GetDigit(c);

                    if (d != -1)
                        digits.Add(d);
                }
            }

            if (!readyToSubmit)
            {
                yield return "sendtochat DansGame A little early for that, don't you think?";
                buttons.PickRandom().OnInteract();
                yield return new WaitForSeconds(0.1f);
                yield break;
            }

            if (digits.Count > (answers.Count - enteredStages))
            {
                yield return "sendtochaterror NotLikeThis Too many digits submitted!";
                yield break;
            }

            yield return $"sendtochat {(firstTry ? "OhMyDog Here we go!" : "Maybe this time?")}";

            firstTry = false;

            yield return "multiple strikes";

            var solve = DetermineSolveType(digits.Count, answers.Count - enteredStages);

            if (Bomb.GetTime() / (answers.Count - enteredStages) < 0.1f)
                solve = SolveType.Regular;

            foreach (var digit in digits)
            {
                buttons[digit].OnInteract();

                if (buttonLeds.Any(x => x.material.color == Color.green))
                {
                    if (solve == SolveType.Regular && Bomb.GetTime() >= 45 && value > 0.95)
                    {
                        yield return new WaitForSeconds(2);
                        yield return $"sendtochat Kreygasm We did it {placeNames.PickRandom()}!";
                        yield return new WaitForSeconds(1);
                        yield return "sendtochat Kappa Nope, just kidding.";
                    }
                    else
                        yield return "sendtochat DansGame This isn't correct...";

                    yield return $"sendtochat Correct digits entered: {enteredStages}";

                    break;
                }

                if (enteredStages == answers.Count)
                {
                    yield return $"sendtochat Kreygasm We did it {placeNames.PickRandom()}!";
                    break;
                }

                if (GetMusicToggle(solve, enteredStages))
                    yield return "toggle waiting music";

                yield return new WaitForSeconds(GetDelay(solve, enteredStages, digits.Count, Bomb.GetTime()));
            }
            yield return "end multiple strikes";
        }
    }

	IEnumerator TwitchHandleForcedSolve()
    {
		while (!isActivated || !readyToSubmit)
        {
            if (moduleSolved)
                yield break;

            yield return true;
        }

        for (int i = enteredStages; i < answers.Count; i++)
        {
            buttons[GetDigit((char)(answers[i] + '0'))].OnInteract();
            yield return new WaitForSeconds(0.05f);
        }
    }


}





