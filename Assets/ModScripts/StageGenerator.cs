using System.Collections.Generic;
using System.Linq;
using static UnityEngine.Random;

public class StageGenerator
{
    public List<Stage> Stages;
    public List<int> CalculatedStages;

    public StageGenerator(List<Constant> assignedConstants, int stageCount)
    {
        Stages = Enumerable.Range(0, stageCount).Select(_ => new Stage(assignedConstants.PickRandom(), Range(0, 10))).ToList().Shuffle();
        CalculatedStages = CalculateStages(Stages);
    }

    private List<int> CalculateStages(List<Stage> stages)
    {
        var finalList = new List<int>();

        for (int i = 0; i < stages.Count; i++)
            finalList.Add((stages[i].GeneratedValue + stages[i].SelectedConstant.ConstantDigits[i % 100]) % 10);

        return finalList;
    }
}