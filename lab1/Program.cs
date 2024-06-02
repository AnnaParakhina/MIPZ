namespace Renju;

class Program
{
    private const string FileRelativePath = "./data/test-data.txt";
    private const int MatrixSize = 19;

    static async Task Main(string[] args)
    {
        var fullFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FileRelativePath);

        var matrices = await ReadMatricesFromFileAsync(fullFilePath);
        if (matrices == null)
        {
            return;
        }

        var games = matrices.Select(matrix => new MatchGame(matrix)).ToList();

        games.ForEach(game => game.DisplayBoard());

        games.ForEach(game =>
        {
            var res = game.GetWinner();
            Console.WriteLine(res.Winner);
            if (res.Winner != 0)
            {
                Console.WriteLine($"{res.RowNumber} {res.ColNumber}");
            }
            Console.WriteLine();
        });
    }

    static async Task<List<int[,]>> ReadMatricesFromFileAsync(string filePath)
    {
        try
        {
            var fileLines = await File.ReadAllLinesAsync(filePath);
            if (!int.TryParse(fileLines.First().Trim(), out var numberOfTests))
            {
                Console.WriteLine("Invalid file content. First line should contain the number of test cases.");
                return null;
            }

            var matrices = new List<int[,]>(numberOfTests);
            int lastRowRead = 0;

            for (int i = 0; i < numberOfTests; i++)
            {
                var matrix = new int[MatrixSize, MatrixSize];
                for (int j = 0; j < MatrixSize; j++)
                {
                    var rowValues = fileLines[lastRowRead + 1 + j].Trim().Split(' ').Select(int.Parse).ToArray();
                    for (int k = 0; k < MatrixSize; k++)
                    {
                        matrix[j, k] = rowValues[k];
                    }
                }
                matrices.Add(matrix);
                lastRowRead += MatrixSize + 1;
            }

            Console.WriteLine("All matrices have been successfully loaded.");
            return matrices;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading or processing the file: {ex.Message}");
            return null;
        }
    }
}
