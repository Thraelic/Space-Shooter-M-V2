﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;

namespace SpaceShooterV2
{
    internal class MainGame
    {
        //Declarations
        //Required resources
        private readonly SpriteBatch _spriteBatch;
        private readonly GameWindow _window;

        //Constants
        private const int ShipScale = 13;
        private const int BulletScale = 70;
        private const int ColumnNum = 10;
        private const int RowNum = 10;
        private const int ProbabilityShipSpawn = 120; // 1/ProbabilityShipSpawn
        private const int ProbabilityPowerUpSpawn = 370; // 1/ProbabilityPowerUpSpawn

        //Important bools
        private const bool Testing = false;
        private readonly bool _multiplayer;
        private bool _dead;

        //Main Resources
        private List<GameObject> _objectList;
        private List<Texture2D> _textureList;
        private SpriteFont _font;
        private int _score;
        private readonly string _settings;
        private readonly int _difficulty;
        private readonly Random _random = new Random();
        private readonly Stopwatch _aliveTimer = new Stopwatch();

        //FPS
        private int _previousFPS = 60;

        //Collision detection
        private List<int>[,] _objectCollisionList;
        private float _tileWidth;
        private float _tileHeight;


        //Public Procedures
        public MainGame(bool multiplayer, GameWindow window, SpriteBatch spriteBatch, string settings, int difficulty)
        {
            _multiplayer = multiplayer;
            _window = window;
            _spriteBatch = spriteBatch;
            _settings = settings;
            _difficulty = difficulty;
        }

        public void Initialize()
        {
            _objectList = new List<GameObject>();
            _textureList = new List<Texture2D>();

            #region ObjectCollisionList Set Up

            _objectCollisionList = new List<int>[ColumnNum, RowNum];

            for (var x = 0; x < ColumnNum; x++)
                for (var y = 0; y < RowNum; y++)
                    _objectCollisionList[x, y] = new List<int>();

            #endregion

            _tileWidth = (float) _window.ClientBounds.Width/ColumnNum;
            _tileHeight = (float) _window.ClientBounds.Height/RowNum;
        }

        public void LoadContent(List<Texture2D> texList, SpriteFont font)
        {
            //0 = collisionTex, 1 = Background, 2 = playerShip, 3 = Long Bullet, 4 = Round Bullet, 5 = Health bar piece
            _textureList = texList;
            _font = font;
            Debug.WriteLine(" Main Game - Game Assets loaded");

            #region Player SetUp
            // Obj: 3 a
            Debug.WriteLine(" Main Game - ShipScale:" +_window.ClientBounds.Height/ShipScale);

            string[] playerSetting = new string[1];
            playerSetting = _settings.Split(':');

            if (_multiplayer)
            {
                _objectList.Add(new PlayerShip((double)_textureList[2].Width / (double)_textureList[2].Height,
                    _window.ClientBounds.Height/ShipScale, 2, 1, playerSetting[0], _window.ClientBounds.Width,
                    _window.ClientBounds.Height, (double)_textureList[5].Width / _textureList[5].Height));
                _objectList.Add(new PlayerShip((double)_textureList[13].Width / (double)_textureList[13].Height,
                    _window.ClientBounds.Height/ShipScale, 13, 2, playerSetting[1], _window.ClientBounds.Width,
                    _window.ClientBounds.Height, (double)_textureList[5].Width / _textureList[5].Height));
            }
            else
            {
                _objectList.Add(new PlayerShip((double)_textureList[2].Width / (double)_textureList[2].Height,
                    _window.ClientBounds.Height / ShipScale, 2, 0, playerSetting[0], _window.ClientBounds.Width,
                    _window.ClientBounds.Height, (double)_textureList[5].Width / _textureList[5].Height));
            }

            #endregion

            #region Testing

            if (Testing)
            {
                //CreateShip(0);
                //CreateShip(1);
                //CreateShip(2);
                //CreatePowerUp(0);
                //CreatePowerUp(1);
            }

            #endregion

            _aliveTimer.Start();
        }

        public void Update(GameTime gameTime)
        {
            KeyboardState curKeyState = Keyboard.GetState();

            if (!_dead)
            {
                #region Update Objects
                // Obj: 1 b
                //Updates all of the objects in _objectList. 
                //How objects are updates is dependant on their type
                for (var i = 0; i < _objectList.Count; i++)
                    if (_objectList[i] != null)
                    {
                        if (_objectList[i].GetType() == typeof(PlayerShip))
                        {
                            #region Player Update

                            ((PlayerShip) _objectList[i]).Update(gameTime, curKeyState);

                            #region Firing

                            // Obj: 1.i 2
                            if (((PlayerShip) _objectList[i]).Firing)
                            {
                                if (((PlayerShip) _objectList[i]).BulDamage >= 2)
                                {
                                    _objectList.Add(
                                        new Bullet((double) _textureList[8].Width/(double) _textureList[8].Height,
                                            _window.ClientBounds.Height/BulletScale, 8,
                                            _window.ClientBounds.Height/(int) (0.4f*BulletScale),
                                            0,
                                            ((PlayerShip) _objectList[i]).getCenterPoint, true,
                                            ((PlayerShip) _objectList[i]).BulDamage));
                                    ((PlayerShip) _objectList[i]).Firing = false;
                                }
                                else
                                {
                                    _objectList.Add(
                                        new Bullet((double) _textureList[3].Width/(double) _textureList[3].Height,
                                            _window.ClientBounds.Height/BulletScale, 3,
                                            _window.ClientBounds.Height/(int) (0.4f*BulletScale),
                                            0,
                                            ((PlayerShip) _objectList[i]).getCenterPoint, true,
                                            ((PlayerShip) _objectList[i]).BulDamage));
                                    ((PlayerShip) _objectList[i]).Firing = false;
                                }
                            }

                            #endregion

                            #endregion
                        }
                        else if ((_objectList[i].GetType() == typeof(EnemyShip)) ||
                                 _objectList[i].GetType().IsSubclassOf(typeof(EnemyShip)))
                        {
                            #region Charger Update

                            if (_objectList[i].GetType() == typeof(Charger))
                            {
                                ((Charger) _objectList[i]).Update(gameTime);

                                #region Firing Logic

                                if (((Charger) _objectList[i]).WillFire)
                                {
                                    ((Charger) _objectList[i]).WillFire = false;

                                    if (_multiplayer)
                                    {

                                        double movementAngle;

                                        if ((((PlayerShip) _objectList[0]).Health != 0) &&
                                            (((PlayerShip) _objectList[1]).Health != 0))
                                        {
                                            movementAngle =
                                                ((Charger) _objectList[i]).GetAngleTwoPoints(
                                                    _objectList[i].getCenterPoint,
                                                    _objectList[_random.Next(0, 2)].getCenterPoint);

                                            _objectList.Add(
                                                new Bullet(
                                                    (double) _textureList[4].Width/(double) _textureList[4].Height,
                                                    _window.ClientBounds.Height/BulletScale, 4,
                                                    (int)
                                                        (((Charger) _objectList[i]).GetBulXVel*Math.Cos(movementAngle)*
                                                         -1),
                                                    (int)
                                                        (((Charger) _objectList[i]).GetBulXVel*Math.Sin(movementAngle)*
                                                         -1),
                                                    ((Charger) _objectList[i]).getCenterPoint, false, 1));
                                        }
                                        else if (((PlayerShip) _objectList[0]).Health != 0)
                                        {
                                            movementAngle =
                                                ((Charger) _objectList[i]).GetAngleTwoPoints(
                                                    _objectList[i].getCenterPoint,
                                                    _objectList[0].getCenterPoint);

                                            _objectList.Add(
                                                new Bullet(
                                                    (double) _textureList[4].Width/(double) _textureList[4].Height,
                                                    _window.ClientBounds.Height/BulletScale, 4,
                                                    (int)
                                                        (((Charger) _objectList[i]).GetBulXVel*Math.Cos(movementAngle)*
                                                         -1),
                                                    (int)
                                                        (((Charger) _objectList[i]).GetBulXVel*Math.Sin(movementAngle)*
                                                         -1),
                                                    ((Charger) _objectList[i]).getCenterPoint, false, 1));
                                        }
                                        else if (((PlayerShip) _objectList[1]).Health != 0)
                                        {
                                            movementAngle =
                                                ((Charger) _objectList[i]).GetAngleTwoPoints(
                                                    _objectList[i].getCenterPoint,
                                                    _objectList[1].getCenterPoint);

                                            _objectList.Add(
                                                new Bullet(
                                                    (double) _textureList[4].Width/(double) _textureList[4].Height,
                                                    _window.ClientBounds.Height/BulletScale, 4,
                                                    (int)
                                                        (((Charger) _objectList[i]).GetBulXVel*Math.Cos(movementAngle)*
                                                         -1),
                                                    (int)
                                                        (((Charger) _objectList[i]).GetBulXVel*Math.Sin(movementAngle)*
                                                         -1),
                                                    ((Charger) _objectList[i]).getCenterPoint, false, 1));
                                        }
                                    }
                                    else
                                    {
                                        var movementAngle =
                                            ((Charger) _objectList[i]).GetAngleTwoPoints(_objectList[i].getCenterPoint,
                                                _objectList[0].getCenterPoint);
                                        _objectList.Add(
                                            new Bullet((double) _textureList[4].Width/(double) _textureList[4].Height,
                                                _window.ClientBounds.Height/BulletScale, 4,
                                                (int) (((Charger) _objectList[i]).GetBulXVel*Math.Cos(movementAngle)*-1),
                                                (int) (((Charger) _objectList[i]).GetBulXVel*Math.Sin(movementAngle)*-1),
                                                ((Charger) _objectList[i]).getCenterPoint, false, 1));
                                    }
                                    ((Charger) _objectList[i]).UpdateCurCharge();
                                }
                            }

                            #endregion

                            #endregion

                            #region Shotgun Update

                            if (_objectList[i].GetType() == typeof(Shotgun))
                            {
                                #region Target Selection
                                // Obj: 1.ii.3 d

                                if (_multiplayer &&
                                    (((Shotgun) _objectList[i]).Target == -1 ||
                                     ((PlayerShip) _objectList[((Shotgun) _objectList[i]).Target]).Health == 0))
                                {
                                    if (((PlayerShip) _objectList[0]).Health != 0 &&
                                        ((PlayerShip) _objectList[1]).Health != 0)
                                    {
                                        ((Shotgun) _objectList[i]).Target = _random.Next(0, 2);
                                    }
                                    else if (((PlayerShip) _objectList[0]).Health != 0)
                                    {
                                        ((Shotgun) _objectList[i]).Target = 0;
                                    }
                                    else if (((PlayerShip) _objectList[1]).Health != 0)
                                    {
                                        ((Shotgun) _objectList[i]).Target = 1;
                                    }
                                }
                                else if (((Shotgun) _objectList[i]).Target == -1)
                                {
                                    ((Shotgun) _objectList[i]).Target = 0;
                                }

                                #endregion

                                if (((Shotgun)_objectList[i]).Target != -1)
                                ((Shotgun)_objectList[i]).Update(gameTime, _objectList[((Shotgun)_objectList[i]).Target].getCenterPoint);

                                #region Firing Logic
                                // Obj: 1.ii.3 b 

                                if (((Shotgun) _objectList[i]).WillFire)
                                {
                                    ((Shotgun) _objectList[i]).WillFire = false;

                                    _objectList.Add(
                                        new Bullet((double) _textureList[4].Width/(double) _textureList[4].Height,
                                            _window.ClientBounds.Height/BulletScale, 4,
                                            Convert.ToInt32(((Shotgun) _objectList[i]).GetBulXVel*Math.Sin(Math.PI/2)*-1),
                                            Convert.ToInt32(((Shotgun) _objectList[i]).GetBulXVel*Math.Cos(Math.PI/2)*-1),
                                            ((Shotgun) _objectList[i]).getCenterPoint, false, 1));

                                    _objectList.Add(
                                        new Bullet((double) _textureList[4].Width/(double) _textureList[4].Height,
                                            _window.ClientBounds.Height/BulletScale, 4,
                                            Convert.ToInt32(((Shotgun) _objectList[i]).GetBulXVel*Math.Sin(1.92)*-1),
                                            Convert.ToInt32(((Shotgun) _objectList[i]).GetBulXVel*Math.Cos(1.92)*-1),
                                            ((Shotgun) _objectList[i]).getCenterPoint, false, 1));

                                    _objectList.Add(
                                        new Bullet((double) _textureList[4].Width/(double) _textureList[4].Height,
                                            _window.ClientBounds.Height/BulletScale, 4,
                                            Convert.ToInt32(((Shotgun) _objectList[i]).GetBulXVel*Math.Sin(1.22)*-1),
                                            Convert.ToInt32(((Shotgun) _objectList[i]).GetBulXVel*Math.Cos(1.22)*-1),
                                            ((Shotgun) _objectList[i]).getCenterPoint, false, 1));
                                }

                                #endregion

                            }

                            #endregion

                            #region Bomber Update

                            if (_objectList[i].GetType() == typeof(Bomber))
                            {
                                ((Bomber)_objectList[i]).Update(gameTime);

                                #region Firing Logic

                                if (((Bomber) _objectList[i]).WillFire)
                                {
                                    ((Bomber) _objectList[i]).WillFire = false;

                                    _objectList.Add(
                                        new Bullet((double) _textureList[4].Width/(double) _textureList[4].Height,
                                            _window.ClientBounds.Height/BulletScale, 4,
                                            Convert.ToInt32(((Bomber) _objectList[i]).GetBulXVel*
                                                            Math.Sin(((Bomber) _objectList[i]).FireAngle)*-1),
                                            Convert.ToInt32(((Bomber) _objectList[i]).GetBulXVel*
                                                            Math.Cos(((Bomber) _objectList[i]).FireAngle)*-1),
                                            ((Bomber) _objectList[i]).getCenterPoint, false, 1));
                                }

                                #endregion

                            }

                            #endregion

                            #region Deletion of EnemyShips if they have collided
                            if (((Ship)_objectList[i]).Health <= 0)
                            {
                                if ((_objectList[i].GetType() == typeof(EnemyShip)) ||
                                    _objectList[i].GetType().IsSubclassOf(typeof(EnemyShip)))
                                {
                                    // Obj: 3 d
                                    _score += ((EnemyShip) _objectList[i]).Score;
                                    Debug.WriteLine(" Main Game - Cur Score: " + _score);
                                }
                                _objectList[i] = null;
                            }
                            #endregion
                        }
                        else if (_objectList[i].GetType() == typeof(Bullet) || _objectList[i].GetType() == typeof(PowerUp))
                        {
                            #region Bullet && PowerUp Update

                            _objectList[i].Update(gameTime);
                            // Obj: 1.iii 2 , Obj: 1.iv 2
                            if (_objectList[i].Collision)
                                _objectList[i] = null;

                            #endregion
                        }

                        #region Updating ObjectCollisionList
                        //Every object which has not collided with something and exits i.e. not null, adds a reference to itself in each box its corners are in
                        if ((_objectList[i] != null) && !_objectList[i].Collision)
                        {
                            if (!(_objectList[i].GetType() == typeof(PlayerShip) &&
                                  ((PlayerShip) _objectList[i]).Health == 0))
                            {
                                //add references to a object to each of the boxes the corners are in.
                                var curObjRec = _objectList[i].BoundingBox;
                                //Top Left
                                if ((curObjRec.X > 0) && (curObjRec.Y > 0) && (curObjRec.X < _window.ClientBounds.Width) &&
                                    (curObjRec.Y < _window.ClientBounds.Height))
                                    _objectCollisionList[
                                        (int) Math.Truncate(curObjRec.X/_tileWidth),
                                        (int) Math.Truncate(curObjRec.Y/_tileHeight)].Add(i);
                                //Top Right
                                if ((curObjRec.X + curObjRec.Width > 0) && (curObjRec.Y > 0) &&
                                    (curObjRec.X + curObjRec.Width < _window.ClientBounds.Width) &&
                                    (curObjRec.Y < _window.ClientBounds.Height))
                                    _objectCollisionList[
                                        (int) Math.Truncate((curObjRec.X + curObjRec.Width)/_tileWidth),
                                        (int) Math.Truncate(curObjRec.Y/_tileHeight)].Add(i);
                                //Bottom Left
                                if ((curObjRec.X > 0) && (curObjRec.Y + curObjRec.Height > 0) &&
                                    (curObjRec.X < _window.ClientBounds.Width) &&
                                    (curObjRec.Y + curObjRec.Height < _window.ClientBounds.Height))
                                    _objectCollisionList[
                                        (int) Math.Truncate(curObjRec.X/_tileWidth),
                                        (int) Math.Truncate((curObjRec.Y + curObjRec.Height)/_tileHeight)].Add(i);
                                //Buttom right
                                if ((curObjRec.X + curObjRec.Width > 0) && (curObjRec.Y + curObjRec.Height > 0) &&
                                    (curObjRec.X + curObjRec.Width < _window.ClientBounds.Width) &&
                                    (curObjRec.Y + curObjRec.Height < _window.ClientBounds.Height))
                                    _objectCollisionList[
                                        (int) Math.Truncate((curObjRec.X + curObjRec.Width)/_tileWidth),
                                        (int) Math.Truncate((curObjRec.Y + curObjRec.Height)/_tileHeight)].Add(i);
                                //Center point
                                if ((curObjRec.Width >= _tileWidth || curObjRec.Height >= _tileHeight)&&((curObjRec.X + curObjRec.Width/2 > 0) && (curObjRec.Y + curObjRec.Height/2 > 0) &&
                                    (curObjRec.X + curObjRec.Width/2 < _window.ClientBounds.Width) &&
                                    (curObjRec.Y + curObjRec.Height/2 < _window.ClientBounds.Height)))
                                    _objectCollisionList[
                                        (int)Math.Truncate((curObjRec.X + curObjRec.Width/2) / _tileWidth),
                                        (int)Math.Truncate((curObjRec.Y + curObjRec.Height/2) / _tileHeight)].Add(i);
                                //If a bullet or power up is off the screen delete it
                                // Obj: 1.iii 1, Obj: 1.iv 1
                                if ((_objectList[i].GetType() == typeof(Bullet) || _objectList[i].GetType() == typeof(PowerUp)) &&
                                    ((curObjRec.Y + curObjRec.Height < 0) || (curObjRec.X + curObjRec.Width < 0) ||
                                     (curObjRec.X > _window.ClientBounds.Width) ||
                                     (curObjRec.Y > _window.ClientBounds.Height)))
                                {
                                    Debug.WriteLine(" Main Game - Object {0} of type {1} left screen", i, _objectList[i].GetType().ToString());
                                    _objectList[i] = null;
                                }
                            }
                        }

                        #endregion
                    }

                #endregion

                #region Collision Check

                // Obj: 1 c

                for (int x = 0; x < ColumnNum; x++)
                    for (int y = 0; y < RowNum; y++)
                    {
                        //If there are things which can collide
                        if ((_objectCollisionList[x, y].Count >= 2) &&
                            (ContainsCompareTypes(typeof(PlayerShip), _objectCollisionList[x, y]) ||
                             ContainsCompareTypes(typeof(EnemyShip), _objectCollisionList[x, y])) &&
                            ContainsCompareTypes(typeof(Bullet), _objectCollisionList[x, y]) ||
                            (ContainsCompareTypes(typeof(PlayerShip), _objectCollisionList[x, y]) &&
                             ContainsCompareTypes(typeof(PowerUp), _objectCollisionList[x, y])))
                        {
                            //Do collision check
                            //Get the objects which are contained within that square
                            //Updating the filtered list will update the original list due to the way that C# handles lists as a collection of references
                            List<GameObject> filteredList = new List<GameObject>();
                            foreach (int i in _objectCollisionList[x, y])
                            {
                                if (!filteredList.Contains(_objectList[i]))
                                    filteredList.Add(_objectList[i]);
                            }

                            #region Bullet Collision

                            //Check if any bullets collide with ships
                            foreach (Bullet curBullet in filteredList.OfType<Bullet>())
                            {
                                //Dont check if the bullet has already collided with something
                                if (!curBullet.Collision)
                                {
                                    foreach (Ship curShip in filteredList.OfType<Ship>())
                                    {
                                        if (!curShip.Collision)
                                        {
                                            //Now do the collision check
                                            if ((((curShip.GetType() == typeof(PlayerShip)) && !curBullet.Owner) ||
                                                 (curShip.GetType().IsSubclassOf(typeof(EnemyShip)) && curBullet.Owner)) &&
                                                curShip.BoundingBox.Intersects(curBullet.BoundingBox))
                                            {
                                                Debug.WriteLine(" Main Game - Bullet Collision at ({0},{1})", x, y);
                                                curShip.Collision = true;
                                                curShip.dmgToTake = curBullet.Dmg;
                                                curBullet.Collision = true;
                                            }
                                        }
                                    }
                                }
                            }

                            #endregion

                            #region PowerUp Collision
                            //If two a player and a powerup can collide in this square
                            if (filteredList.Exists(item => item.GetType() == typeof(PlayerShip)) &&
                                filteredList.Exists(item => item.GetType() == typeof(PowerUp)))
                            {
                                //for each power up
                                foreach (PowerUp curPowerUp in filteredList.OfType<PowerUp>())
                                {
                                    //and for each player in the square
                                    foreach (PlayerShip curPlayer in filteredList.OfType<PlayerShip>())
                                    {
                                        //check if they collide
                                        if (curPowerUp.BoundingBox.Intersects(curPlayer.BoundingBox))
                                        {
                                            Debug.WriteLine(" Main Game - PowerUp Collision at ({0},{1})", x, y);
                                            //If they do:
                                            // Obj: 1.iv 3
                                            switch (curPowerUp.Type)
                                            {
                                                case 0:
                                                    curPlayer.Heal(2);
                                                    break;
                                                case 1:
                                                    curPlayer.BoostDamage();
                                                    break;
                                            }
                                            curPowerUp.Collision = true;
                                        }
                                    }
                                }
                            }

                            #endregion
                        //Clear the list for the next cycle
                        filteredList.Clear();
                        }
                        //Clear the list for the next cycle
                        _objectCollisionList[x, y].Clear();
                    }

                #endregion

                #region Deleted null from ObjectList
                //Removes deleted objects from the list
                //!_objectList.OfType<EnemyShip>().Any()
                if (_objectList.Contains(null))
                {
                    _objectList.RemoveAll(item => item == null);
                    Debug.WriteLine(" Main Game - Collapsed ObjectList");
                }

                #endregion

                if (!Testing)
                {
                    #region Create Ships at random
                    // Obj: 3 b
                    // All Subsequent objectives are shown in the code below but cannot be commented due to them all being on the same line
                    //Creates ships at random with a random starting Y value
                    if (_multiplayer)
                    {
                        if (_random.Next(0, (int)(ProbabilityShipSpawn / 1.2f) - (int)(_aliveTimer.Elapsed.Seconds / 2)) < 1)
                        {
                            CreateShip();
                        }
                    }
                    else if (_random.Next(0, ProbabilityShipSpawn - (int)(_aliveTimer.Elapsed.Seconds/2)) < 1)
                    {
                        CreateShip();
                    }

                    #endregion

                    #region Create Power ups at random

                    // Obj: 3 c
                    //Creates powerUps at random with a random Y value
                    
                    if (_random.Next(0, ProbabilityPowerUpSpawn) < 1)
                    {
                        CreatePowerUp();
                    }

                    #endregion
                }

                #region CheckAlive
                //Checks if the cumulative health of the players or player is 0
                if (_multiplayer)
                {
                    if (((PlayerShip) _objectList[0]).Health + ((PlayerShip) _objectList[1]).Health == 0)
                    {
                        _dead = true;
                    }
                }
                else
                {
                    if (((PlayerShip)_objectList[0]).Health == 0)
                    {
                        _dead = true;
                    }
                }

                #endregion
            }
        }

        public void Draw(GameTime gameTime)
        {
          
            #region Drawing Collision Boxes

            if (Testing)
                for (var x = 0; x < ColumnNum; x++)
                    for (var y = 0; y < RowNum; y++)
                        _spriteBatch.Draw(_textureList[0],
                            new Rectangle((int) (x*_tileWidth), (int) (y*_tileHeight), (int) _tileWidth,
                                (int) _tileHeight), Color.White);

            #endregion

            #region Drawing Objects
            // Obj: 1 a

            foreach (var curObj in _objectList.Where(item => item != null))
                if (!(curObj.GetType() == typeof(PlayerShip)))
                {
                    curObj.Draw(_spriteBatch, _textureList[curObj.TexNum]);
                }
                else if (((PlayerShip) curObj).Health == 0)
                {
                    ((PlayerShip) curObj).DrawDeath(_spriteBatch, _textureList[curObj.TexNum]);
                }
                else
                {
                    curObj.Draw(_spriteBatch, _textureList[curObj.TexNum]);
                    ((PlayerShip) curObj).DrawUI(_spriteBatch, _textureList[5], _textureList[12]);
                }

            #endregion

            #region Draw Score 

            _spriteBatch.DrawString(_font, "Score: " + Convert.ToString(_score),
                new Vector2(
                    _window.ClientBounds.Width / 2 - (_font.MeasureString("Score: " + Convert.ToString(_score)).X * ((float)_window.ClientBounds.Width / 1920f )) / 2, _window.ClientBounds.Height / 64),
                Color.Ivory, 0f, new Vector2(0, 0), new Vector2((float)_window.ClientBounds.Width / 1920f, 1f), SpriteEffects.None, 0f);

            #endregion

            #region Calculate FPS

            if (_previousFPS != Convert.ToInt32(1/gameTime.ElapsedGameTime.TotalSeconds))
            {
                Debug.WriteLine(" Main Game - Draw fps: {0}, No. Objects {1}",
                    Convert.ToInt32(1/gameTime.ElapsedGameTime.TotalSeconds), _objectList.Count);
                _previousFPS = Convert.ToInt32(1/gameTime.ElapsedGameTime.TotalSeconds);
                //Aprox 500 objects without fps drop
            }

            #endregion

        }

        //Public Accessors
        public bool IsDead
        {
            get { return _dead;}
        }

        public int TotalScore
        {
            get { return _score; }
        }

        //Private Methods
        private void CreateShip(int forcedShip = -1)
        {
            int shipChoice = forcedShip;
            if (shipChoice == -1)
            {
                shipChoice = _random.Next(0, 3);
            }

            switch (shipChoice)
                {
                    case 0:
                        Debug.WriteLine(" Main Game - Shotgun Created");
                        _objectList.Add(new Shotgun((double) _textureList[10].Width/(double) _textureList[10].Height,
                            (int) (_window.ClientBounds.Height/(0.6f*ShipScale)), 10,
                            _window.ClientBounds.Height/(int) (1.4f*BulletScale), 100,
                            _difficulty, _window.ClientBounds.Width, _window.ClientBounds.Height,
                            _random.Next(0, _window.ClientBounds.Height + 1)));
                        break;
                    case 1:
                        Debug.WriteLine(" Main Game - Charger Created");
                        _objectList.Add(new Charger((double) _textureList[9].Width/(double) _textureList[9].Height,
                            (int) (_window.ClientBounds.Height/(0.9f*ShipScale)), 9,
                            _window.ClientBounds.Height/(int) (0.8f*BulletScale), 50,
                            _difficulty, _window.ClientBounds.Width, _window.ClientBounds.Height,
                            _random.Next(0, _window.ClientBounds.Height + 1)));
                        break;
                    case 2:
                        Debug.WriteLine(" MainGame - Bomber Created");
                        _objectList.Add(new Bomber((double) _textureList[11].Width/(double) _textureList[11].Height,
                            (int) (_window.ClientBounds.Height/(1.1f*ShipScale)), 11,
                            _window.ClientBounds.Height/(int) (2.6f*BulletScale), 100,
                            _difficulty, _window.ClientBounds.Width, _window.ClientBounds.Height,
                            _random.Next(0, _window.ClientBounds.Height + 1)));
                        break;
                }
            }

        private void CreatePowerUp(int forcedPowerUp = -1)
        {
            int powerUpChoice = forcedPowerUp;
            if (powerUpChoice == -1)
            {
                powerUpChoice = _random.Next(0, 3);
            }

            switch (powerUpChoice)
            {
                //Heal
                case 0:
                    _objectList.Add(new PowerUp(_textureList[6].Width / _textureList[6].Height,
                        _window.ClientBounds.Height / BulletScale, 6, -_window.ClientBounds.Height / (int)(1.4f * BulletScale),
                        new Vector2(_window.ClientBounds.Width,
                            _random.Next(0, _window.ClientBounds.Height + 1)),
                        0));
                    break;
                //Damage
                case 1:
                    _objectList.Add(new PowerUp(_textureList[7].Width / _textureList[7].Height,
                        _window.ClientBounds.Height / BulletScale, 7, -_window.ClientBounds.Height / (int)(1.4f * BulletScale),
                        new Vector2(_window.ClientBounds.Width,
                            _random.Next(0, _window.ClientBounds.Height + 1)),
                        1));
                    break;
            }
        }

        //Private Functions 
        private bool ContainsCompareTypes(Type t, List<int> intList)
        {
            foreach (var i in intList)
                if (_objectList[i].GetType() == t || _objectList[i].GetType().IsSubclassOf(t))
                    return true;
            return false;
        }
    }
}