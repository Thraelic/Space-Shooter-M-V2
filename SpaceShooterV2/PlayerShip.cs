﻿using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SpaceShooterV2
{
    internal class PlayerShip : Ship
    {
        //Variables
        //Controls
        private struct ControlScheme
        {
            public List<Keys> Controls;
            public List<bool> KeyStates;
        }

        private KeyboardState _previousKeyBoardState;
        private ControlScheme _controlScheme;

        //User interface
        private Vector2 _healthBarPos;
        private readonly int _UIUnitWidth;
        private readonly int _UIUnitHeight;
        private const int UIScale = 55;
        private const int UISpacingY = 2;
        private const int UISpacingX = 5;

        //Firing and bullet damage
        private int _bulletCoolDown;
        private const int BulletCdTotal = 18;
        private int _bulDamage = 1;
        private int _dmgBoostDuration;
        private const int DmgBoostDefaultDuration = 300;
        private bool _firing;

        //Animation
        private int _deathAnimationFrame;

        //Useful constants
        private readonly int _windowX;
        private readonly int _windowY;
        private const int VelocityScale = 14;
        private const int StartingHealth = 10;

        //Public Procedures
        public PlayerShip(double widthByHeight, int height, byte texNum, byte playerID, string keyStr, int winX, int winY, double UIWidthHeightRatio)
            : base(widthByHeight, height, texNum, 0, 0)
        {
            _health = StartingHealth;
            _windowX = winX;
            _windowY = winY;

            _xVelocity = _width/VelocityScale;
            _yVelocity = _xVelocity;

            #region Calculating Control Scheme
            // Obj: 1.i 4

            Debug.WriteLine(" Main Game - ID: {0}, Control Scheme: {1}", playerID,keyStr);

            _controlScheme.Controls = new List<Keys>();
            _controlScheme.KeyStates = new List<bool>();

            string[] _keys = keyStr.Split(',');

            foreach (string curStr in _keys)
            {
                _controlScheme.Controls.Add(GetKeyCode(curStr));
                _controlScheme.KeyStates.Add(false);
            }

            #endregion

            #region Calculating Starting Position and UI

            _UIUnitHeight = winY / UIScale;
            _UIUnitWidth = (int)(UIWidthHeightRatio * _UIUnitHeight);

            if (playerID == 1)
            {
                _healthBarPos = new Vector2(UISpacingX * _UIUnitWidth, UISpacingY * _UIUnitHeight);
                _position.Y = height;
            }
            else if (playerID == 2)
            {
                _healthBarPos = new Vector2(_windowX - (StartingHealth + UISpacingX) * _UIUnitWidth, UISpacingY * _UIUnitHeight);
                _position.Y = _windowY - 2*_height;
            }
            else
            {
                _healthBarPos = new Vector2(UISpacingX * _UIUnitWidth, UISpacingY * _UIUnitHeight);
                _position.Y = _windowY/2 - _height/2;
            }
            _position.X += 40;
            #endregion
        }

        public void Update(GameTime gameTime, KeyboardState curKeyboardState)
        {
            if (_health != 0)
            {
                #region User Input
                //Obj: 1.i 1

                #region Check Controls

                if (curKeyboardState.GetPressedKeys() != _previousKeyBoardState.GetPressedKeys())
                {
                    for (var i = 0; i < _controlScheme.Controls.Count; i++)
                        if (curKeyboardState.IsKeyDown(_controlScheme.Controls[i]))
                            _controlScheme.KeyStates[i] = true;
                        else
                            _controlScheme.KeyStates[i] = false;
                }
                _previousKeyBoardState = curKeyboardState;
                #endregion

                #region Action Based on Control State 

                for (var i = 0; i < _controlScheme.KeyStates.Count; i++)
                    if (_controlScheme.KeyStates[i])
                        switch (i)
                        {
                            case 0:
                                if (_position.Y - _yVelocity < 0)
                                    _position.Y = 0;
                                else
                                    _position.Y -= _yVelocity;
                                break;
                            case 1:
                                if (_position.X - _xVelocity < 0)
                                    _position.X = 0;
                                else
                                    _position.X -= _xVelocity;
                                break;
                            case 2:

                                if (_position.Y + _height >= _windowY)
                                    _position.Y = _windowY - _height;
                                else
                                    _position.Y += _yVelocity;
                                break;
                            case 3:
                                if (_position.X + _width >= _windowX)
                                    _position.X = _windowX - _width;
                                else
                                    _position.X += _xVelocity;
                                break;
                            case 4:
                                if (_bulletCoolDown >= BulletCdTotal)
                                {
                                    Debug.WriteLine(" Main Game - Firing");
                                    _bulletCoolDown = 0;
                                    _firing = true;
                                }
                                break;
                        }

                #endregion

                #endregion

                if (_collision)
                {
                    // Obj: 1.i 5
                    _health -= 1;
                    _collision = false;
                    Debug.WriteLine(" Main Game - Health: " + _health + " - " + ToString());
                }
                if (_bulletCoolDown != BulletCdTotal)
                {
                    // Obj: 1.i 3
                    _bulletCoolDown += 1;
                }

                if (_dmgBoostDuration > 0)
                {
                    _dmgBoostDuration -= 1;
                }
                else
                {
                    _bulDamage = 1;
                }
            }
        }

        public void DrawUI(SpriteBatch spriteBatch, Texture2D healthBarTex,Texture2D dmgBoostBarTex)
        {
            // Obj: 1.i 6
            for (int i = 0; i < _health; i++)
            {
                if (i < _health)
                {
                    spriteBatch.Draw(healthBarTex,
                        new Rectangle((int) _healthBarPos.X + _UIUnitWidth*i, (int) _healthBarPos.Y,
                            _UIUnitWidth, _UIUnitHeight), Color.White);
                }
            }
            for (int i = 0; i < _dmgBoostDuration/30; i++)
            {
                spriteBatch.Draw(dmgBoostBarTex,
                    new Rectangle((int) _healthBarPos.X + _UIUnitWidth*i,
                        (int) _healthBarPos.Y + (int) (1.2f*_UIUnitHeight), _UIUnitWidth, _UIUnitHeight),
                    Color.White);
                
            }
        }

        public void DrawDeath(SpriteBatch spriteBatch, Texture2D shipTex)
        {
            if (_deathAnimationFrame < 25)
            {
                spriteBatch.Draw(shipTex, new Rectangle((int) _position.X, (int) _position.Y, _width, _height),
                    Color.White);
                _deathAnimationFrame += 1;
            }
            else if (_deathAnimationFrame < 45)
            {
                spriteBatch.Draw(shipTex, new Rectangle((int)_position.X, (int)_position.Y, _width, _height),
      new Color(75,75,75,150));
                _deathAnimationFrame += 1;
            }
            else if (_deathAnimationFrame < 60)
            {
                spriteBatch.Draw(shipTex, new Rectangle((int)_position.X, (int)_position.Y, _width, _height),
                    Color.White);
                _deathAnimationFrame += 1;
            }
        }

        // Obj: 1.i 7
        public void Heal(int healthAdded)
        {
            if (_health + healthAdded > StartingHealth)
            {
                _health = StartingHealth;
            }
            else
            {
                _health += healthAdded;
            }
        }

        public void BoostDamage()
        {
            _bulDamage = 2;
            _dmgBoostDuration = DmgBoostDefaultDuration;
        }

        //Public Accessors
        public bool Firing
        {
            get { return _firing; }
            set { _firing = value; }
        }

        public int BulDamage
        {
            get { return _bulDamage; }
            set { _bulDamage = value; }
        }

        //Private Functions
        private Keys GetKeyCode(string curStr)
        {
            curStr = curStr.ToUpper();
            switch (curStr)
            {
                case "Q":
                    return Keys.Q;
                case "W":
                    return Keys.W;
                case "E":
                    return Keys.E;
                case "R":
                    return Keys.R;
                case "T":
                    return Keys.T;
                case "Y":
                    return Keys.Y;
                case "U":
                    return Keys.U;
                case "I":
                    return Keys.I;
                case "O":
                    return Keys.O;
                case "P":
                    return Keys.P;
                case "A":
                    return Keys.A;
                case "S":
                    return Keys.S;
                case "D":
                    return Keys.D;
                case "F":
                    return Keys.F;
                case "G":
                    return Keys.G;
                case "H":
                    return Keys.H;
                case "J":
                    return Keys.J;
                case "K":
                    return Keys.K;
                case "L":
                    return Keys.L;
                case "Z":
                    return Keys.Z;
                case "X":
                    return Keys.X;
                case "C":
                    return Keys.C;
                case "V":
                    return Keys.V;
                case "B":
                    return Keys.B;
                case "N":
                    return Keys.N;
                case "M":
                    return Keys.M;
                case "SPACE":
                    return Keys.Space;
                case "UP":
                    return Keys.Up;
                case "LEFT":
                    return Keys.Left;
                case "DOWN":
                    return Keys.Down;
                case "RIGHT":
                    return Keys.Right;
                case "ENTER":
                    return Keys.Enter;
                default:
                    return Keys.D0;
            }

        }
    }
}
