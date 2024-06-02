namespace Renju;

public class MatchGame
{
    private int[,] board;

    public int RowsCount { get; private set; }
    public int ColumnsCount { get; private set; }

    public MatchGame(int[,] board)
    {
        this.board = board;
        RowsCount = board.GetLength(0);
        ColumnsCount = board.GetLength(1);
    }

    public void DisplayBoard()
    {
        for (int i = 0; i < RowsCount; i++)
        {
            Console.Write("");
            for (int j = 0; j < ColumnsCount; j++)
            {
                Console.Write(board[i, j]);
                Console.Write(" ");
            }
            Console.WriteLine();
        }
        Console.WriteLine(new string('-', 50));
    }

    public WinningPosition GetWinner()
    {
        for (int row = 0; row < RowsCount; row++)
        {
            for (int col = 0; col < ColumnsCount; col++)
            {
                int val = board[row, col];
                if (val == 0) continue;

                if (col <= ColumnsCount - 5 && CheckDirection(row, col, 0, 1, val))
                    return new WinningPosition(val, row + 1, col + 1);

                if (row <= RowsCount - 5 && CheckDirection(row, col, 1, 0, val))
                    return new WinningPosition(val, row + 1, col + 1);

                if (row <= RowsCount - 5 && col <= ColumnsCount - 5 && CheckDirection(row, col, 1, 1, val))
                    return new WinningPosition(val, row + 1, col + 1);

                if (row <= RowsCount - 5 && col >= 4 && CheckDirection(row, col, 1, -1, val))
                    return new WinningPosition(val, row + 1, col + 1);
            }
        }

        return new WinningPosition(0, 0, 0);

        bool CheckDirection(int startRow, int startCol, int rowStep, int colStep, int val)
        {
            for (int i = 1; i < 5; i++)
            {
                if (board[startRow + i * rowStep, startCol + i * colStep] != val)
                    return false;
            }
            return true;
        }
    }
}

public struct WinningPosition
{
    public int Winner { get; }
    public int RowNumber { get; }
    public int ColNumber { get; }

    public WinningPosition(int winner, int rowNumber, int colNumber)
    {
        Winner = winner;
        RowNumber = rowNumber;
        ColNumber = colNumber;
    }
}
