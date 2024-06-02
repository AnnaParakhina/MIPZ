public class ClassMetrics
{
    public string ClassName { get; }
    public string BaseType { get; set; }
    public int DIT { get; set; } = 0;
    public int NOC { get; set; } = 0;
    public List<string> MethodNames { get; } = [];
    public List<string> PropertyNames { get; } = [];
    public HashSet<string> InheritedMethods { get; } = [];
    public HashSet<string> HiddenMethods { get; } = [];
    public HashSet<string> InheritedAttributes { get; } = [];
    public HashSet<string> HiddenAttributes { get; } = [];
    public HashSet<string> CoupledClasses { get; } = [];
    public double MIF { get; set; }
    public double MHF { get; set; }
    public double AHF { get; set; }
    public double AIF { get; set; }
    public double POF { get; set; }

    public ClassMetrics(string className)
    {
        this.ClassName = className;
    }

    public override string ToString()
    {
        return $"Class: {this.ClassName}, DIT: {this.DIT}, NOC: {this.NOC}, MIF: {this.MIF}, MHF: {this.MHF}, AHF: {this.AHF}, AIF: {this.AIF}, POF: {this.POF}";
    }
}