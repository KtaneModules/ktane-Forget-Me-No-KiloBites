public struct Stage
{
    public Constant SelectedConstant { get; private set; }
    public int GeneratedValue { get; private set; }

    public Stage(Constant selectedConstant, int generatedValue)
    {
        SelectedConstant = selectedConstant;
        GeneratedValue = generatedValue;
    }
}