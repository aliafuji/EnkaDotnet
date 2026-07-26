namespace EnkaDotNet.Enums.ZZZ
{
    public class StatSummary
    {
        public double FinalValue { get; set; }
        public double BaseValue { get; set; }
        public double AddedValue { get; set; }
    }

    public class FormattedStatValues
    {
        public string Final { get; set; }
        public string Base { get; set; }
        public string Added { get; set; }

        public static implicit operator string(FormattedStatValues value)
        {
            return value.Final;
        }

        public override string ToString()
        {
            return Final;
        }
    }
}
