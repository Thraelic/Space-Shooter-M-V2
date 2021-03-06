﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SpaceShooterV2
{
    internal class MenuButton
    {
        //Variables
        private readonly int _width;
        private readonly int _height;
        private readonly int _texNum;
        private readonly Vector2 _position;
        private string _buttonText;
        private bool _clicked;
        private bool _isActive;
        private readonly bool _isClickable;
        private MouseState _preMouseState;

        //Public Procedures
        public MenuButton(double widthByHeight, int height, int texNum, Vector2 centerPosition, string buttonText, bool isClickable)
        {
            _width = (int)(widthByHeight*height);
            _height = height;
            _texNum = texNum;
            _position = new Vector2(centerPosition.X - _width/2,centerPosition.Y - _height/2);
            _buttonText = buttonText;
            _isClickable = isClickable;
        }

        public void Update(MouseState curMouseState)
        {
            if (curMouseState.LeftButton == ButtonState.Pressed && _clicked == false)
            {
                if (curMouseState.X >= _position.X && curMouseState.X < _position.X + _width && curMouseState.Y > _position.Y && curMouseState.Y < _position.Y + _height)
                {
                    if (_preMouseState.LeftButton == ButtonState.Released)
                    {
                        _clicked = true;
                    }
                }
            }
            _preMouseState = curMouseState;
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D tex, SpriteFont font,float fontScale)
        {
            spriteBatch.Draw(tex, new Rectangle((int) _position.X, (int) _position.Y, _width, _height), Color.White);
            Vector2 measuredString = font.MeasureString(_buttonText);
            spriteBatch.DrawString(font, _buttonText, new Vector2(_position.X + _width / 2 - (measuredString.X * fontScale) / 2, _position.Y + _height / 2 - (measuredString.Y) / 2), Color.White, 0f, new Vector2(0, 0), new Vector2(fontScale, 1f), SpriteEffects.None, 0f);
        }

        //Public Accessors
        public bool IsClicked
        {
            get { return _clicked; }
            set { _clicked = value; }
        }

        public bool IsActive
        {
            get { return _isActive; }
            set { _isActive = value; }
        }

        public int TexNum
        {
            get { return _texNum; }
        }

        public string Text
        {
            get { return _buttonText;}
            set { _buttonText = value; }
        }

        public bool IsClickable
        {
            get { return _isClickable;}
        }
    }
}
