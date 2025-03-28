﻿using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

public class CustomTableLayoutPanel : TableLayoutPanel
{
    private int selectedRow = -1;
    private int selectedColumn = -1;

    private static readonly Color SelectedTodayColor = Color.FromArgb(211, 225, 225);
    private static readonly Color SelectedColor = Color.FromArgb(241, 255, 255);
    private static readonly Color ActiveDayColor = Color.FromArgb(200, 200, 200);

    public CustomTableLayoutPanel()
    {
        this.DoubleBuffered = true;
        this.CellPaint += CustomTableLayoutPanel_CellPaint;
        this.MouseClick += CustomTableLayoutPanel_MouseClick;
    }

    private void CustomTableLayoutPanel_CellPaint(object sender, TableLayoutCellPaintEventArgs e)
    {
        int todayIndex = ((int)DateTime.Now.DayOfWeek + 6) % 7; // Pondělí = 0

        if (e.Row == todayIndex)
        {
            using (var brush = new SolidBrush(ActiveDayColor))
            {
                e.Graphics.FillRectangle(brush, e.CellBounds);
            }
        }

        // Kliknutá buňka (s jinou barvou pro aktivní den)
        if (e.Row == selectedRow && e.Column == selectedColumn)
        {
            Color highlightColor = (e.Row == todayIndex) ? SelectedTodayColor : SelectedColor;
            using (var brush = new SolidBrush(highlightColor))
            {
                e.Graphics.FillRectangle(brush, e.CellBounds);
            }
        }
    }

    private void CustomTableLayoutPanel_MouseClick(object sender, MouseEventArgs e)
    {
        (int row, int col) = GetCellFromPoint(e.Location);
        if (row != -1 && col != -1 && (row != selectedRow || col != selectedColumn))
        {
            selectedRow = row;
            selectedColumn = col;
            Invalidate();
        }
    }

    public void ClearSelection()
    {
        if (selectedRow != -1 || selectedColumn != -1)
        {
            selectedRow = -1;
            selectedColumn = -1;
            Invalidate();
        }
    }

    private (int row, int col) GetCellFromPoint(Point point)
    {
        int[] colWidths = GetColumnWidths();
        int[] rowHeights = GetRowHeights();

        int xSum = 0, ySum = 0;
        int clickedCol = -1, clickedRow = -1;

        for (int i = 0; i < colWidths.Length; i++)
        {
            if (point.X >= xSum && point.X < xSum + colWidths[i])
            {
                clickedCol = i;
                break;
            }
            xSum += colWidths[i];
        }

        for (int j = 0; j < rowHeights.Length; j++)
        {
            if (point.Y >= ySum && point.Y < ySum + rowHeights[j])
            {
                clickedRow = j;
                break;
            }
            ySum += rowHeights[j];
        }

        return (clickedRow, clickedCol);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        int[] colWidths = GetColumnWidths();
        int[] rowHeights = GetRowHeights();

        if (colWidths.Length == 0 || rowHeights.Length == 0) return;

        DateTime now = DateTime.Now;
        int todayIndex = ((int)now.DayOfWeek + 6) % 7; // Pondělí = 0
        int halfHourIndex = now.Hour * 2 + now.Minute / 30;

        // Kreslení mřížky
        using (var pen = new Pen(Color.Gray))
        {
            int x = 0, y = 0;
            foreach (var width in colWidths)
            {
                x += width;
                e.Graphics.DrawLine(pen, x, 0, x, Height);
            }
            foreach (var height in rowHeights)
            {
                y += height;
                e.Graphics.DrawLine(pen, 0, y, Width, y);
            }
        }

        // Červená čára podle aktuálního času
        if (todayIndex < rowHeights.Length && halfHourIndex < colWidths.Length)
        {
            int xPos = colWidths.Take(halfHourIndex).Sum();
            int yPos = rowHeights.Take(todayIndex).Sum();

            using (var redPen = new Pen(Color.Red, 2))
            {
                e.Graphics.DrawLine(redPen, xPos, yPos, xPos, yPos + rowHeights[todayIndex]);
            }
        }
    }
}
