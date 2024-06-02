var analyzer = new OOAnalyzer();
await analyzer.AnalyzeSolution("D:\\Studies\\KPI\\Masters 2 term\\Khitsko\\Stockshare\\StockShare.Platform.Server.sln");
analyzer.PrintClassesMetrics();

Console.WriteLine(new string('-', 50));

analyzer.PrintSolutionMetrics();