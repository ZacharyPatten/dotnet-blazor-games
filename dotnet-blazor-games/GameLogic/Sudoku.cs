using System;
using System.Text;
using Towel;

namespace dotnet_blazor_games
{
	namespace GameLogic
	{
		internal class Sudoku
		{
			internal static Cell[,] Board = Generate();

			internal static Cell SelectedCell;

			internal class Cell
			{
				public int I;
				public int J;
				public int? Value;
				public string Locked;
				public string Selected;
				public string Blocking;
			}

			internal static Cell[,] Generate(
				Random random = null,
				int? blanks = null)
			{
				random ??= new Random();
				if (blanks.HasValue && blanks < 0 || 81 < blanks)
				{
					throw new ArgumentOutOfRangeException(nameof(blanks), blanks.Value, $"{nameof(blanks)} < 0 || 81 < {nameof(blanks)}");
				}
				else if (!blanks.HasValue)
				{
					blanks = random.Next(0, 82);
				}

				int?[,] board = new int?[9, 9];
				(int[] Values, int Count)[,] valids = new (int[] Values, int Count)[9, 9];
				for (int i = 0; i < 9; i++)
				{
					for (int j = 0; j < 9; j++)
					{
						valids[i, j] = (new int[9], -1);
					}
				}

				#region GetValidValues

				void GetValidValues(int row, int column)
				{
					bool SquareValid(int value, int row, int column)
					{
						for (int i = row - row % 3; i <= row; i++)
						{
							for (int j = column - column % 3; j <= column - column % 3 + 2 && !(i == row && j == column); j++)
							{
								if (board[i, j] == value)
								{
									return false;
								}
							}
						}
						return true;
					}

					bool RowValid(int value, int row, int column)
					{
						for (int i = 0; i < column; i++)
						{
							if (board[row, i] == value)
							{
								return false;
							}
						}
						return true;
					}

					bool ColumnValid(int value, int row, int column)
					{
						for (int i = 0; i < row; i++)
						{
							if (board[i, column] == value)
							{
								return false;
							}
						}
						return true;
					}

					valids[row, column].Count = 0;
					for (int i = 1; i <= 9; i++)
					{
						if (SquareValid(i, row, column) &&
							RowValid(i, row, column) &&
							ColumnValid(i, row, column))
						{
							valids[row, column].Values[valids[row, column].Count++] = i;
						}
					}
				}

				#endregion


				for (int i = 0; i < 9; i++)
				{
					for (int j = 0; j < 9; j++)
					{
						GetValidValues(i, j);
						while (valids[i, j].Count == 0)
						{
							board[i, j] = null;
							i = j == 0 ? i - 1 : i;
							j = j == 0 ? 8 : j - 1;
						}
						int index = random.Next(0, valids[i, j].Count);
						int value = valids[i, j].Values[index];
						valids[i, j].Values[index] = valids[i, j].Values[valids[i, j].Count - 1];
						valids[i, j].Count--;
						board[i, j] = value;
					}
				}

				foreach (int i in random.NextUnique(Math.Min(80, blanks.Value), 0, 81))
				{
					int row = i / 9;
					int column = i % 9;
					board[row, column] = null;
				}

				Cell[,] cellBoard = new Cell[board.GetLength(0), board.GetLength(1)];
				for (int i = 0; i < board.GetLength(0); i++)
				{
					for (int j = 0; j < board.GetLength(1); j++)
					{
						cellBoard[i, j] = new Cell()
						{
							I = i,
							J = j,
							Value = board[i, j],
							Locked = board[i, j].HasValue ? "locked" : default,
						};
					}
				}

				return cellBoard;
			}

			internal static void TryMove(int? value)
			{
				void Blocking(int a, int b)
				{
					Board[a, b].Blocking = "blocking";
					System.Timers.Timer timer = new System.Timers.Timer();
					timer.Elapsed += (s, e) =>
					{
						Board[a, b].Blocking = "";
						timer.Enabled = false;
						timer.Dispose();
					};
					timer.Interval = TimeSpan.FromSeconds(3).TotalMilliseconds;
					timer.Enabled = true;
					return;
				}

				if (SelectedCell is null)
				{
					return;
				}

				if (!value.HasValue)
				{
					if (string.IsNullOrWhiteSpace(SelectedCell.Locked))
					{
						SelectedCell.Value = null;
					}
					return;
				}

				int x = SelectedCell.I;
				int y = SelectedCell.J;

				// Square
				for (int i = x - x % 3; i <= x - x % 3 + 2; i++)
				{
					for (int j = y - y % 3; j <= y - y % 3 + 2; j++)
					{
						if (Board[i, j].Value == value)
						{
							Blocking(i, j);
							return;
						}
					}
				}
				// Row
				for (int i = 0; i < 9; i++)
				{
					if (Board[x, i].Value == value)
					{
						Blocking(x, i);
						return;
					}
				}
				// Column
				for (int i = 0; i < x; i++)
				{
					if (Board[i, y].Value == value)
					{
						Blocking(i, y);
						return;
					}
				}

				SelectedCell.Value = value;
			}

			public static string ToString(int?[,] board)
			{
				StringBuilder stringBuilder = new StringBuilder();
				for (int i = 0; i < board.GetLength(0); i++)
				{
					for (int j = 0; j < board.GetLength(1); j++)
					{
						stringBuilder.Append(board[i, j] + " ");
					}
					stringBuilder.AppendLine();
				}
				return stringBuilder.ToString();
			}
		}
	}
}
