using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using AutochaveamentoACCA;

public class GenerateImage
{
    public static void GenerateBracketImage(List<Atleta> athletes, int numFreePasses, string outputPath)
    {
        // Define image and graphics settings
        int imageSizeX = 1754;
        int imageSizeY = 1240;
        int padding = 20;
        int athleteSpacing = 50;
        int lineWidth = 2;
        Font athleteFont = new Font(FontFamily.GenericSansSerif, 12);
        Font smallerFont = new Font(FontFamily.GenericSansSerif, 8);

        int longestValue = GetLongestName(athletes, athleteFont, smallerFont) + 80;

        using (Bitmap bracketImage = new Bitmap(imageSizeX, imageSizeY))
        using (Graphics graphics = Graphics.FromImage(bracketImage))
        {
            graphics.Clear(Color.White);

            // Draw bracket lines and athletes
            int y = padding;
            int lineY = y - 15 + athleteSpacing / 2;
            int freePassLineExtend = numFreePasses > 0 ? athleteSpacing * 2 : 0;

            for (int i = 0; i < athletes.Count; i++)
            {
                Atleta athlete = athletes[i];
                // Draw athlete's name, academy, and belt
                int xName = padding;
                int xAcademy = xName + graphics.MeasureString(athlete.nome, athleteFont).ToSize().Width + 25;
                int xBelt = xName + graphics.MeasureString(athlete.nome, athleteFont).ToSize().Width + 25;

                graphics.DrawString((i + 1).ToString() + ". " + athlete.nome, athleteFont, Brushes.Black, new PointF(xName, y));
                graphics.DrawString(athlete.agrem, smallerFont, Brushes.Black, new PointF(xAcademy, y - 5));
                graphics.DrawString(athlete.faixa, smallerFont, Brushes.Black, new PointF(xBelt, y + 15));
                
                // Draw box for athlete
                graphics.DrawRectangle(Pens.Black, 15, y - 10, longestValue - 15, 40);

                // Draw lines connecting athletes
                switch (numFreePasses)
                {
                    case 0:
                        if (i % 2 == 0)
                        {            
                            graphics.DrawLine(Pens.Black, longestValue, lineY, longestValue + 20, lineY); // Horizontal
                            graphics.DrawLine(Pens.Black, longestValue + 20, lineY, longestValue + 20, lineY - 25); // Vertical
                        }
                        else
                        {            
                            graphics.DrawLine(Pens.Black, longestValue, lineY, longestValue + 20, lineY); // Horizontal
                            graphics.DrawLine(Pens.Black, longestValue + 20, lineY, longestValue + 20, lineY + 25); // Vertical
                        }
                        break;
                    case 1:
                        if (i == 0)
                        {
                            graphics.DrawLine(Pens.Black, longestValue, lineY, longestValue + 80, lineY); // Horizontal
                            graphics.DrawLine(Pens.Black, longestValue + 80, lineY, longestValue + 80, lineY + 25); // Horizontal
                        }
                        else if (i % 2 == 0)
                        {            
                            graphics.DrawLine(Pens.Black, longestValue, lineY, longestValue + 20, lineY); // Horizontal
                            graphics.DrawLine(Pens.Black, longestValue + 20, lineY, longestValue + 20, lineY - 25); // Vertical
                        }
                        else
                        {            
                            graphics.DrawLine(Pens.Black, longestValue, lineY, longestValue + 20, lineY); // Horizontal
                            graphics.DrawLine(Pens.Black, longestValue + 20, lineY, longestValue + 20, lineY + 25); // Vertical
                        }
                        break;
                    case 2:
                        if (i == 0)
                        {
                            graphics.DrawLine(Pens.Black, longestValue, lineY, longestValue + 80, lineY); // Horizontal
                            graphics.DrawLine(Pens.Black, longestValue + 80, lineY, longestValue + 80, lineY + 25); // Horizontal
                        }
                        else if (i == athletes.Count-1)
                        {
                            graphics.DrawLine(Pens.Black, longestValue, lineY, longestValue + 80, lineY); // Horizontal
                            graphics.DrawLine(Pens.Black, longestValue + 80, lineY, longestValue + 80, lineY - 25); // Horizontal
                        }
                        else if (i % 2 == 0)
                        {            
                            graphics.DrawLine(Pens.Black, longestValue, lineY, longestValue + 20, lineY); // Horizontal
                            graphics.DrawLine(Pens.Black, longestValue + 20, lineY, longestValue + 20, lineY - 25); // Vertical
                        }
                        else
                        {            
                            graphics.DrawLine(Pens.Black, longestValue, lineY, longestValue + 20, lineY); // Horizontal
                            graphics.DrawLine(Pens.Black, longestValue + 20, lineY, longestValue + 20, lineY + 25); // Vertical
                        }
                        break;
                }

                y += athleteSpacing;
                lineY += athleteSpacing;
            }

            // Save the image
            bracketImage.Save(outputPath, ImageFormat.Png);
        }
    }

    private static void DrawSubBracketLines()
    {

    }

    private static int GetLongestName(List<Atleta> athletes, Font athleteFont, Font smallerFont)
    {        
        int imageSize = 1000;

        int longestNameValue = 0;
        int longestAgremValue = 0;

        using (Bitmap bracketImage = new Bitmap(imageSize, imageSize))
        using (Graphics graphics = Graphics.FromImage(bracketImage))
        {
            foreach (var atleta in athletes)
            {
                int curNameValue = graphics.MeasureString(atleta.nome, athleteFont).ToSize().Width;
                if (longestNameValue < curNameValue)
                {
                    longestNameValue = curNameValue;
                }
                int curAgremValue = graphics.MeasureString(atleta.agrem, smallerFont).ToSize().Width;
                if (longestAgremValue < curAgremValue)
                {
                    longestAgremValue = curAgremValue;
                }
            }
        }
        return longestNameValue + longestAgremValue;
    }
}