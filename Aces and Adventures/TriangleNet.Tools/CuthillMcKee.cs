using System;

namespace TriangleNet.Tools;

public class CuthillMcKee
{
	private AdjacencyMatrix matrix;

	public int[] Renumber(Mesh mesh)
	{
		mesh.Renumber(NodeNumbering.Linear);
		return Renumber(new AdjacencyMatrix(mesh));
	}

	public int[] Renumber(AdjacencyMatrix matrix)
	{
		this.matrix = matrix;
		int num = matrix.Bandwidth();
		int[] columnPointers = matrix.ColumnPointers;
		Shift(columnPointers, up: true);
		int[] perm = GenerateRcm();
		int[] array = PermInverse(perm);
		int num2 = PermBandwidth(perm, array);
		if (Log.Verbose)
		{
			Log.Instance.Info($"Reverse Cuthill-McKee (Bandwidth: {num} > {num2})");
		}
		Shift(columnPointers, up: false);
		return array;
	}

	private int[] GenerateRcm()
	{
		int n = matrix.N;
		int[] array = new int[n];
		int iccsze = 0;
		int level_num = 0;
		int[] level_row = new int[n + 1];
		int[] array2 = new int[n];
		for (int i = 0; i < n; i++)
		{
			array2[i] = 1;
		}
		int num = 1;
		for (int i = 0; i < n; i++)
		{
			if (array2[i] != 0)
			{
				int root = i;
				FindRoot(ref root, array2, ref level_num, level_row, array, num - 1);
				Rcm(root, array2, array, num - 1, ref iccsze);
				num += iccsze;
				if (n < num)
				{
					return array;
				}
			}
		}
		return array;
	}

	private void Rcm(int root, int[] mask, int[] perm, int offset, ref int iccsze)
	{
		int[] columnPointers = matrix.ColumnPointers;
		int[] rowIndices = matrix.RowIndices;
		int[] array = new int[matrix.N];
		Degree(root, mask, array, ref iccsze, perm, offset);
		mask[root] = 0;
		if (iccsze <= 1)
		{
			return;
		}
		int num = 0;
		int num2 = 1;
		while (num < num2)
		{
			int num3 = num + 1;
			num = num2;
			for (int i = num3; i <= num; i++)
			{
				int num4 = perm[offset + i - 1];
				int num5 = columnPointers[num4];
				int num6 = columnPointers[num4 + 1] - 1;
				int num7 = num2 + 1;
				for (int j = num5; j <= num6; j++)
				{
					int num8 = rowIndices[j - 1];
					if (mask[num8] != 0)
					{
						num2++;
						mask[num8] = 0;
						perm[offset + num2 - 1] = num8;
					}
				}
				if (num2 <= num7)
				{
					continue;
				}
				int num9 = num7;
				while (num9 < num2)
				{
					int num10 = num9;
					num9++;
					int num8 = perm[offset + num9 - 1];
					while (num7 < num10)
					{
						int num11 = perm[offset + num10 - 1];
						if (array[num11 - 1] <= array[num8 - 1])
						{
							break;
						}
						perm[offset + num10] = num11;
						num10--;
					}
					perm[offset + num10] = num8;
				}
			}
		}
		ReverseVector(perm, offset, iccsze);
	}

	private void FindRoot(ref int root, int[] mask, ref int level_num, int[] level_row, int[] level, int offset)
	{
		int[] columnPointers = matrix.ColumnPointers;
		int[] rowIndices = matrix.RowIndices;
		int level_num2 = 0;
		GetLevelSet(ref root, mask, ref level_num, level_row, level, offset);
		int num = level_row[level_num] - 1;
		if (level_num == 1 || level_num == num)
		{
			return;
		}
		do
		{
			int num2 = num;
			int num3 = level_row[level_num - 1];
			root = level[offset + num3 - 1];
			if (num3 < num)
			{
				for (int i = num3; i <= num; i++)
				{
					int num4 = level[offset + i - 1];
					int num5 = 0;
					int num6 = columnPointers[num4 - 1];
					int num7 = columnPointers[num4] - 1;
					for (int j = num6; j <= num7; j++)
					{
						int num8 = rowIndices[j - 1];
						if (mask[num8] > 0)
						{
							num5++;
						}
					}
					if (num5 < num2)
					{
						root = num4;
						num2 = num5;
					}
				}
			}
			GetLevelSet(ref root, mask, ref level_num2, level_row, level, offset);
			if (level_num2 > level_num)
			{
				level_num = level_num2;
				continue;
			}
			break;
		}
		while (num > level_num);
	}

	private void GetLevelSet(ref int root, int[] mask, ref int level_num, int[] level_row, int[] level, int offset)
	{
		int[] columnPointers = matrix.ColumnPointers;
		int[] rowIndices = matrix.RowIndices;
		mask[root] = 0;
		level[offset] = root;
		level_num = 0;
		int num = 0;
		int num2 = 1;
		do
		{
			int num3 = num + 1;
			num = num2;
			level_num++;
			level_row[level_num - 1] = num3;
			for (int i = num3; i <= num; i++)
			{
				int num4 = level[offset + i - 1];
				int num5 = columnPointers[num4];
				int num6 = columnPointers[num4 + 1] - 1;
				for (int j = num5; j <= num6; j++)
				{
					int num7 = rowIndices[j - 1];
					if (mask[num7] != 0)
					{
						num2++;
						level[offset + num2 - 1] = num7;
						mask[num7] = 0;
					}
				}
			}
		}
		while (num2 - num > 0);
		level_row[level_num] = num + 1;
		for (int i = 0; i < num2; i++)
		{
			mask[level[offset + i]] = 1;
		}
	}

	private void Degree(int root, int[] mask, int[] deg, ref int iccsze, int[] ls, int offset)
	{
		int[] columnPointers = matrix.ColumnPointers;
		int[] rowIndices = matrix.RowIndices;
		int num = 1;
		ls[offset] = root;
		columnPointers[root] = -columnPointers[root];
		int num2 = 0;
		iccsze = 1;
		while (num > 0)
		{
			int num3 = num2 + 1;
			num2 = iccsze;
			for (int i = num3; i <= num2; i++)
			{
				int num4 = ls[offset + i - 1];
				int num5 = -columnPointers[num4];
				int num6 = Math.Abs(columnPointers[num4 + 1]) - 1;
				int num7 = 0;
				for (int j = num5; j <= num6; j++)
				{
					int num8 = rowIndices[j - 1];
					if (mask[num8] != 0)
					{
						num7++;
						if (0 <= columnPointers[num8])
						{
							columnPointers[num8] = -columnPointers[num8];
							iccsze++;
							ls[offset + iccsze - 1] = num8;
						}
					}
				}
				deg[num4] = num7;
			}
			num = iccsze - num2;
		}
		for (int i = 0; i < iccsze; i++)
		{
			int num4 = ls[offset + i];
			columnPointers[num4] = -columnPointers[num4];
		}
	}

	private int PermBandwidth(int[] perm, int[] perm_inv)
	{
		int[] columnPointers = matrix.ColumnPointers;
		int[] rowIndices = matrix.RowIndices;
		int num = 0;
		int num2 = 0;
		int n = matrix.N;
		for (int i = 0; i < n; i++)
		{
			for (int j = columnPointers[perm[i]]; j < columnPointers[perm[i] + 1]; j++)
			{
				int num3 = perm_inv[rowIndices[j - 1]];
				num = Math.Max(num, i - num3);
				num2 = Math.Max(num2, num3 - i);
			}
		}
		return num + 1 + num2;
	}

	private int[] PermInverse(int[] perm)
	{
		int n = matrix.N;
		int[] array = new int[n];
		for (int i = 0; i < n; i++)
		{
			array[perm[i]] = i;
		}
		return array;
	}

	private void ReverseVector(int[] a, int offset, int size)
	{
		for (int i = 0; i < size / 2; i++)
		{
			int num = a[offset + i];
			a[offset + i] = a[offset + size - 1 - i];
			a[offset + size - 1 - i] = num;
		}
	}

	private void Shift(int[] a, bool up)
	{
		int num = a.Length;
		if (up)
		{
			for (int i = 0; i < num; i++)
			{
				a[i]++;
			}
		}
		else
		{
			for (int j = 0; j < num; j++)
			{
				a[j]--;
			}
		}
	}
}
