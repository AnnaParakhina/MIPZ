using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;

public class OOAnalyzer
{
    private readonly Dictionary<string, ClassMetrics> _classMetrics = [];

    public async Task AnalyzeSolution(string solutionPath)
    {
        var workspace = MSBuildWorkspace.Create();
        var solution = await workspace.OpenSolutionAsync(solutionPath);

        foreach (var project in solution.Projects)
        {
            foreach (var document in project.Documents)
            {
                await this.AnalyzeDocument(document);
            }
        }

        this.CalculateDITAndNOC();
        this.CalculateMoodMetrics();
    }

    private async Task AnalyzeDocument(Document document)
    {
        var syntaxTree = await document.GetSyntaxTreeAsync();
        var root = await syntaxTree.GetRootAsync();
        var semanticModel = await document.GetSemanticModelAsync();

        var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
        foreach (var classDeclaration in classes)
        {
            var className = classDeclaration.Identifier.Text;
            if (!this._classMetrics.ContainsKey(className))
            {
                this._classMetrics[className] = new ClassMetrics(className);
            }

            // Process base types (for DIT)
            var baseType = classDeclaration.BaseList?.Types.FirstOrDefault()?.ToString();
            if (baseType != null)
            {
                this._classMetrics[className].BaseType = baseType;
            }

            // Process methods (for MOOD metrics)
            var methods = classDeclaration.Members.OfType<MethodDeclarationSyntax>();
            foreach (var method in methods)
            {
                this._classMetrics[className].MethodNames.Add(method.Identifier.Text);

                // Check if method is inherited (for MIF)
                var methodSymbol = semanticModel.GetDeclaredSymbol(method);
                if (methodSymbol?.OverriddenMethod != null)
                {
                    this._classMetrics[className].InheritedMethods.Add(method.Identifier.Text);
                }

                // Check if method is hidden (for MHF)
                if (method.Modifiers.Any(SyntaxKind.PrivateKeyword))
                {
                    this._classMetrics[className].HiddenMethods.Add(method.Identifier.Text);
                }

                // Track method calls for COF
                var calledMethods = method.DescendantNodes().OfType<InvocationExpressionSyntax>();
                foreach (var calledMethod in calledMethods)
                {
                    if (semanticModel.GetSymbolInfo(calledMethod).Symbol is IMethodSymbol symbol)
                    {
                        var calledClass = symbol.ContainingType.Name;
                        if (!this._classMetrics[className].CoupledClasses.Contains(calledClass))
                        {
                            this._classMetrics[className].CoupledClasses.Add(calledClass);
                        }
                    }
                }
            }

            // Process properties (for MOOD metrics)
            var properties = classDeclaration.Members.OfType<PropertyDeclarationSyntax>();
            foreach (var property in properties)
            {
                this._classMetrics[className].PropertyNames.Add(property.Identifier.Text);

                // Check if property is inherited (for AIF)
                var propertySymbol = semanticModel.GetDeclaredSymbol(property);
                if (propertySymbol?.OverriddenProperty != null)
                {
                    this._classMetrics[className].InheritedAttributes.Add(property.Identifier.Text);
                }

                // Check if property is hidden (for AHF)
                if (property.Modifiers.Any(SyntaxKind.PrivateKeyword))
                {
                    this._classMetrics[className].HiddenAttributes.Add(property.Identifier.Text);
                }

                // Track property accesses for COF
                var accessedProperties = property.DescendantNodes().OfType<MemberAccessExpressionSyntax>();
                foreach (var accessedProperty in accessedProperties)
                {
                    if (semanticModel.GetSymbolInfo(accessedProperty).Symbol is IPropertySymbol symbol)
                    {
                        var accessedClass = symbol.ContainingType.Name;
                        if (!this._classMetrics[className].CoupledClasses.Contains(accessedClass))
                        {
                            this._classMetrics[className].CoupledClasses.Add(accessedClass);
                        }
                    }
                }
            }
        }
    }

    private void CalculateDITAndNOC()
    {
        foreach (var classMetrics in this._classMetrics.Values)
        {
            var currentClass = classMetrics;
            while (currentClass.BaseType != null && this._classMetrics.ContainsKey(currentClass.BaseType))
            {
                classMetrics.DIT++;
                currentClass = this._classMetrics[currentClass.BaseType];
            }

            if (classMetrics.BaseType != null && this._classMetrics.ContainsKey(classMetrics.BaseType))
            {
                this._classMetrics[classMetrics.BaseType].NOC++;
            }
        }
    }

    private void CalculateMoodMetrics()
    {
        foreach (var classMetrics in this._classMetrics.Values)
        {
            // Method Inheritance Factor (MIF)
            classMetrics.MIF = classMetrics.MethodNames.Count == 0 ? 0 : (double)classMetrics.InheritedMethods.Count / classMetrics.MethodNames.Count;

            // Method Hiding Factor (MHF)
            classMetrics.MHF = classMetrics.MethodNames.Count == 0 ? 0 : (double)classMetrics.HiddenMethods.Count / classMetrics.MethodNames.Count;

            // Attribute Hiding Factor (AHF)
            classMetrics.AHF = classMetrics.PropertyNames.Count == 0 ? 0 : (double)classMetrics.HiddenAttributes.Count / classMetrics.PropertyNames.Count;

            // Attribute Inheritance Factor (AIF)
            classMetrics.AIF = classMetrics.PropertyNames.Count == 0 ? 0 : (double)classMetrics.InheritedAttributes.Count / classMetrics.PropertyNames.Count;

            // Polymorphism Factor (POF)
            classMetrics.POF = classMetrics.MethodNames.Count == 0 ? 0 : (double)classMetrics.InheritedMethods.Count / classMetrics.MethodNames.Count;
        }
    }

    public void PrintClassesMetrics()
    {
        foreach (var classMetrics in this._classMetrics.Values)
        {
            Console.WriteLine(classMetrics);
        }
    }

    public void PrintSolutionMetrics()
    {
        int totalClasses = this._classMetrics.Count;
        int totalDIT = this._classMetrics.Values.Max(c => c.DIT);
        int totalNOC = this._classMetrics.Values.Sum(c => c.NOC);
        int totalMethods = this._classMetrics.Values.Sum(c => c.MethodNames.Count);
        int totalInheritedMethods = this._classMetrics.Values.Sum(c => c.InheritedMethods.Count);
        int totalHiddenMethods = this._classMetrics.Values.Sum(c => c.HiddenMethods.Count);
        int totalProperties = this._classMetrics.Values.Sum(c => c.PropertyNames.Count);
        int totalInheritedAttributes = this._classMetrics.Values.Sum(c => c.InheritedAttributes.Count);
        int totalHiddenAttributes = this._classMetrics.Values.Sum(c => c.HiddenAttributes.Count);
        int totalCouplings = this._classMetrics.Values.Sum(c => c.CoupledClasses.Count);

        double avgDIT = totalClasses == 0 ? 0 : (double)totalDIT / totalClasses;
        double avgNOC = totalClasses == 0 ? 0 : (double)totalNOC / totalClasses;
        double mif = totalMethods == 0 ? 0 : (double)totalInheritedMethods / totalMethods;
        double mhf = totalMethods == 0 ? 0 : (double)totalHiddenMethods / totalMethods;
        double ahf = totalProperties == 0 ? 0 : (double)totalHiddenAttributes / totalProperties;
        double aif = totalProperties == 0 ? 0 : (double)totalInheritedAttributes / totalProperties;
        double pof = totalMethods == 0 ? 0 : (double)totalInheritedMethods / totalMethods;

        Console.WriteLine("Overall Metrics for the Solution:");
        Console.WriteLine($"Total Classes: {totalClasses}");
        Console.WriteLine($"Total DIT: {totalDIT}");
        Console.WriteLine($"Average DIT: {avgDIT}");
        Console.WriteLine($"Total NOC: {totalNOC}");
        Console.WriteLine($"Average NOC: {avgNOC}");
        Console.WriteLine($"Total Methods: {totalMethods}");
        Console.WriteLine($"Total Inherited Methods: {totalInheritedMethods}");
        Console.WriteLine($"Total Hidden Methods: {totalHiddenMethods}");
        Console.WriteLine($"Total Properties: {totalProperties}");
        Console.WriteLine($"Total Inherited Attributes: {totalInheritedAttributes}");
        Console.WriteLine($"Total Hidden Attributes: {totalHiddenAttributes}");
        Console.WriteLine($"Total Couplings: {totalCouplings}");
        Console.WriteLine($"MIF: {mif}");
        Console.WriteLine($"MHF: {mhf}");
        Console.WriteLine($"AHF: {ahf}");
        Console.WriteLine($"AIF: {aif}");
        Console.WriteLine($"POF: {pof}");
    }
}
