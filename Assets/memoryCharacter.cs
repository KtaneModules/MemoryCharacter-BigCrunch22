using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

public class memoryCharacter : MonoBehaviour
{
	public KMBombModule module;
    public KMAudio Audio;
    public TextMesh kanjiDisplay;
    public KMSelectable yes;
    public KMSelectable no;
	public AudioClip[] SFX;

    int moduleId;
    static int moduleIdCounter = 1;
    string kanji = "一二三四五六七八九十百千上下左右中大小月日年早木林山川土空田天生花草虫犬人名女男子目耳口手足見音力気円入出立休先夕本文字学校村町森正水火玉王石竹糸貝車金雨赤青白";
    List<int> usedIndices = new List<int>();
    int curIndex, stageCounter = 0, forcedYes;
    bool solved;
	
	void Awake()
	{
		moduleId = moduleIdCounter++;
        yes.OnInteract += delegate {pressYes(); return false; };
        no.OnInteract += delegate { pressNo(); return false; };
	}
	
	void Start()
	{
        List<int> weights = new List<int>() {5, 5, 6, 6, 6, 6, 6, 7, 7, 7, 7, 7, 8, 8, 8, 8, 8, 8, 8, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, };
        forcedYes = weights.PickRandom();
        pickKanji(); 
    }

	void pickKanji()
	{
        stageCounter++;
        if (forcedYes == stageCounter) curIndex = usedIndices.PickRandom();       
        else
        {
            curIndex = rnd.Range(0, 80);
        }
        kanjiDisplay.text = kanji[curIndex].ToString();
        Debug.LogFormat("[Memory Character #{0}] Displayed character is {1}.", moduleId, kanji[curIndex]);
	}
	
    void pressYes()
    {
		yes.AddInteractionPunch(0.1f);
        if (solved)
		{
			Audio.PlaySoundAtTransform(SFX[0].name, transform);
			return;
		}
        if (usedIndices.Contains(curIndex))
        {
			Audio.PlaySoundAtTransform(SFX[1].name, transform);
            solved = true;
            module.HandlePass();
            Debug.LogFormat("[Memory Character #{0}] Yes button correctly pressed. Module solved.", moduleId);
            usedIndices.Add(curIndex);
        }
		else
		{
			Audio.PlaySoundAtTransform(SFX[0].name, transform);
			module.HandleStrike();
			Debug.LogFormat("[Memory Character #{0}] Yes button incorrectly pressed. Strike! Also, a complete reset.", moduleId);
			stageCounter = 0;
			usedIndices = new List<int>();
			Start();
		}
    }
	
    void pressNo()
    {
		Audio.PlaySoundAtTransform(SFX[0].name, transform);
		yes.AddInteractionPunch(0.1f);
        if (solved) return;
        if (usedIndices.Contains(curIndex))
        {
            module.HandleStrike();
            Debug.LogFormat("[Memory Character #{0}] No button incorrectly pressed. Strike! Also, a complete reset.", moduleId);
            stageCounter = 0;
			usedIndices = new List<int>();
			Start();
        }
		else
		{
			Debug.LogFormat("[Memory Character #{0}] No button correctly pressed.", moduleId);
			usedIndices.Add(curIndex);
			pickKanji();
		}
    }
	
	//twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} press yes/no [Presses the yes or no button.] | 'press' is optional. 'yes' and 'no' can be abbrevaited to 'y' or 'n'";
    #pragma warning restore 414
	IEnumerator ProcessTwitchCommand(string command)
	{
        var m = Regex.Match(command, @"^\s*(press\s+)?((?<yes>(yes|y))|(no|n))\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (!m.Success)
            yield break;
        yield return null;
        var btn = m.Groups["yes"].Success ? yes : no;
        btn.OnInteract();
	}
    
    private IEnumerator TwitchHandleForcedSolve()
    {
        while (!solved)
        {
            var btn = usedIndices.Contains(curIndex) ? yes : no;
            btn.OnInteract();
            if (!solved)
                yield return new WaitForSeconds(0.4f);
        }
    }
}