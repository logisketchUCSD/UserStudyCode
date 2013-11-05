using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Office.Core;

namespace Basic
{
    /// <summary>
    /// I believe this class is for handling how to undo many operations, by storing
    /// the attributes of (for example) a removed shape on a stack.
    /// </summary>
    class ShapeAttributes
    {
        private float Left, Top, Width, Height, Rotation, Size;
        private MsoAutoShapeType Type;
        private int Color;
        private string Name, Text, Font;
        
        public ShapeAttributes(PowerPoint.Shape shape)
        {
            Left = shape.Left;
            Top = shape.Top;
            Width = shape.Width;
            Height = shape.Height;
            Rotation = shape.Rotation;
            Type = shape.AutoShapeType;
            Color = shape.Fill.ForeColor.RGB;
            Name = shape.Name;
            // FIXME: This doesn't work when you add text to an AutoShape
            if (shape.Type == MsoShapeType.msoTextBox)
            {
                Text = shape.TextFrame.TextRange.Text;
                Font = shape.TextFrame.TextRange.Font.Name;
                Size = shape.TextFrame.TextRange.Font.Size;
            }
             
        }

        #region Getters

        public float getLeft()
        {
            return this.Left;
        }

        public float getTop()
        {
            return this.Top;
        }

        public float getWidth()
        {
            return this.Width;
        }

        public float getHeight()
        {
            return this.Height;
        }

        public float getRotation()
        {
            return this.Rotation;
        }

        public MsoAutoShapeType getType()
        {
            return this.Type;
        }

        public int getColor()
        {
            return this.Color;
        }

        public string getName()
        {
            return this.Name;
        }

        public string getText()
        {
            return this.Text;
        }

        public string getFont()
        {
            return this.Font;
        }

        public float getSize()
        {
            return this.Size;
        }

        #endregion
    }
}
